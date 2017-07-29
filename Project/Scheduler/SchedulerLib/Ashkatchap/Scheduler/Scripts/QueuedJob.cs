using Ashkatchap.Shared.Collections;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ashkatchap.Updater {
	internal class QueuedJob {
		private static int lastId = 0;
		private static int CLEAN_STATE = new Range.SimpleRange(1, 0).state;


		#region EXECUTOR_RW WORKER_RW
		private int hasErrors;
		private int length;
		private Job job;
		#endregion

		private readonly Volatile.PaddedInt[] doneIndices;
		private readonly Range[] todoIndices;

		#region EXECUTOR_RW WORKER_R
		private int minimumRangeToSteal = 0;
		private int temporalId;
		#endregion

		#region EXECUTOR_RW
		internal int priority;
		#endregion


		public QueuedJob() {
			todoIndices = new Range[Scheduler.AVAILABLE_CORES];
			doneIndices = new Volatile.PaddedInt[Scheduler.AVAILABLE_CORES];
			for (int i = 0; i < todoIndices.Length; i++) todoIndices[i] = new Range();
		}


		internal void Init(Job job, ushort length, byte priority, ushort minimumRangeToSteal) {
			Logger.WarnAssert(!Scheduler.InMainThread(), "Init can only be called from the main thread");
			if (!Scheduler.InMainThread()) return;

			Interlocked.Exchange(ref this.job, job);
			Interlocked.Exchange(ref this.temporalId, lastId++);
			Interlocked.Exchange(ref this.priority, priority);
			Interlocked.Exchange(ref this.minimumRangeToSteal, minimumRangeToSteal);
			Interlocked.Exchange(ref this.hasErrors, 0);

			if (length == 0) todoIndices[0].range.value.SetThreadSafe(1, 0);
			else todoIndices[0].range.value.SetThreadSafe(0, (ushort) (length - 1));

			for (int i = 1; i < todoIndices.Length; i++) {
				todoIndices[i].range.value.SetThreadSafe(1, 0);
			}
			for (int i = 0; i < doneIndices.Length; i++) {
				Interlocked.Exchange(ref doneIndices[i].value, 0);
			}

			Interlocked.Exchange(ref this.length, length);

			//Logger.TraceVerbose("[" + temporalId + "] job created");
		}


		internal bool TryExecute(int workerIndex, ref IndexCount[] tmp) {
			var rangeWrapper = todoIndices[workerIndex];

			Range.SimpleRange rangeToExecute = new Range.SimpleRange(ref rangeWrapper.range.value);


			// get an index to execute
			if (!rangeWrapper.range.value.GetIndexAndIncrease(out rangeToExecute.index)) {
				// This range is finished so we want a new range, if there is any available


				// Prepare tmp array
				if (tmp == null || tmp.Length != todoIndices.Length - 1) {
					tmp = new IndexCount[todoIndices.Length - 1];
				}

				// First we fetch and sort the remaining ranges
				bool foundRange = false;
				for (int i = 0, j = 0; i < todoIndices.Length; i++) {
					if (i == workerIndex) continue;
					int range = todoIndices[i].range.value.GetRemainingRange();
					tmp[j++] = new IndexCount(i, range);
					if (range > minimumRangeToSteal) foundRange = true;
				}

				if (foundRange) {
					// sort by range
					HeapSort(tmp);

					bool rangeObtained = false;
					for (int i = 0; i < tmp.Length; i++) {
						Range.SimpleRange obtainedRange;
						if (todoIndices[tmp[i].index].GetFreeRange(out obtainedRange, (ushort) minimumRangeToSteal)) {
							// We were able to get a new range
							Logger.TraceVerbose("[" + temporalId + "] Range obtained: " + obtainedRange);
							rangeToExecute.state = obtainedRange.state;

							// Next set state, that can change by all threads. Right now only this thread wants to change it
							++obtainedRange.index;
							Interlocked.Exchange(ref rangeWrapper.range.value.state, obtainedRange.state);
							rangeObtained = true;
							break;
						}
					}
					if (!rangeObtained) return ReturnFinished(workerIndex);
				} else {
					return ReturnFinished(workerIndex);
				}
			}

			if (rangeToExecute.index > rangeToExecute.lastIndex) return true;

			try {
				job(rangeToExecute.index);
			} catch (Exception e) {
				Logger.Error(e.ToString());
				Interlocked.Exchange(ref hasErrors, 1);
				Destroy();
				return false;
			}
			Interlocked.Increment(ref doneIndices[workerIndex].value);

			return true;
		}

		bool ReturnFinished(int workerIndex) {
			return false;
		}

		private IndexCount[] tmpForMainThread;
		public void WaitForFinish() {
			Logger.ErrorAssert(!Scheduler.InMainThread(), "WaitForFinish can only be called from the main thread");
			if (!Scheduler.InMainThread()) return;

			if (Scheduler.executor == null) {
				Logger.WarnAssert(!Scheduler.InMainThread(), "Multithreading is not enabled right now");
				return;
			}
			Scheduler.executor.SetJobToAllThreads(this);
			while (TryExecute(0, ref tmpForMainThread)) ;
			while (!IsFinished()) Thread.Sleep(0);
		}

		public void Destroy() {
			for (int i = 0; i < doneIndices.Length; i++) {
				Interlocked.Exchange(ref doneIndices[i].value, int.MaxValue);
			}

			for (int i = 0; i < todoIndices.Length; i++) {
				Interlocked.Exchange(ref todoIndices[i].range.value.state, CLEAN_STATE);
			}
		}

		public bool HasErrors() {
			return hasErrors == 1;
		}

		private int GetFinishedCount() {
			int count = 0;
			for (int i = 0; i < doneIndices.Length; i++) {
				count += doneIndices[i].value;
			}
			return count;
		}

		public bool IsFinished() {
			int count = GetFinishedCount();
			if (count > length) Logger.Error("[" + temporalId + "] Expected done indices (" + length + "), counted (" + count + ")");
			return count == length;
		}

		public void ChangePriority(byte newPriority) {
			Logger.ErrorAssert(!Scheduler.InMainThread(), "ChangePriority can only be called from the main thread");
			if (!Scheduler.InMainThread()) return;

			if (Scheduler.executor == null) {
				Logger.WarnAssert(!Scheduler.InMainThread(), "Multithreading is not enabled right now");
				return;
			}

			Scheduler.executor.JobPriorityChange(this, newPriority);
		}

		public bool CheckId(int id) {
			return temporalId == id;
		}
		public int GetId() {
			return temporalId;
		}






		public class Range {
			public PaddedRange range;

			public override string ToString() {
				return range.ToString();
			}

			public bool GetFreeRange(out SimpleRange obtainedRange, ushort minimumRange = 0) {
				obtainedRange.state = 0;
				int availableRange;
				SimpleRange copiedRange = new SimpleRange(ref range.value);
				while ((availableRange = copiedRange.GetRemainingRange()) > minimumRange) {
					var tmpCopied = copiedRange; // debug

					var comparandState = copiedRange.state;
					obtainedRange.index = (ushort) (copiedRange.index + availableRange / 2);
					obtainedRange.lastIndex = copiedRange.lastIndex;

					if (availableRange == 1) {
						copiedRange.index = 1;
						copiedRange.lastIndex = 0;
					} else {
						copiedRange.lastIndex = (ushort) (copiedRange.index + availableRange / 2 - 1);
					}
					var rangeToSet = copiedRange;

					if (range.value.SetNewState(rangeToSet.state, comparandState, out copiedRange.state)) {
						//Logger.TraceVerbose(tmpCopied.ToString() + "-> " + rangeToSet.ToString() + " | " + obtainedRange.ToString());
						return true;
					}
				}
				obtainedRange.index = obtainedRange.lastIndex = 0;
				return false;
			}




			[StructLayout(LayoutKind.Explicit)]
			public struct SimpleRange {
				[FieldOffset(0)] public int state;
				[FieldOffset(0)] public ushort index;
				[FieldOffset(2)] public ushort lastIndex;

				public SimpleRange(ushort index, ushort lastIndex) {
					this.state = 0;
					this.index = index;
					this.lastIndex = lastIndex;
				}
				public SimpleRange(ref SimpleRange other) {
					index = lastIndex = 0;
					state = other.state;
				}
				public void SetThreadSafe(ushort index, ushort lastIndex) {
					Interlocked.Exchange(ref state, new SimpleRange(index, lastIndex).state);
				}

				public int GetRemainingRange() {
					return index > lastIndex ? 0 : lastIndex - index + 1;
				}
				

				public bool GetIndexAndIncrease(out ushort indexToExecute) {
					var toSet = new SimpleRange(ref this);
					while (index <= lastIndex) {
						var comparand = toSet.state;
						++toSet.index;

						Logger.ErrorAssert(toSet.index == 0, "WTF overflow? when? why?");

						if (TTAS(ref state, toSet.state, comparand, out toSet.state)) {
							indexToExecute = toSet.index;
							return true;
						}
					}

					indexToExecute = 0;
					return false;
				}

				public bool SetNewState(int newState, int oldState, out int reportedOldState) {
					return TTAS(ref state, newState, oldState, out reportedOldState);
				}
				
				/// <summary>
				/// Test and Test and Set
				/// </summary>
				/// <param name="location"></param>
				/// <param name="value"></param>
				/// <param name="comparand"></param>
				/// <param name="originalValue"></param>
				/// <returns>true if we were able to set value because the previous value was comparand</returns>
				private bool TTAS(ref int location, int value, int comparand, out int originalValue) {
					return comparand == (originalValue = Interlocked.CompareExchange(ref location, value, comparand));
				}
				

				public override string ToString() {
					return "{I:" + index + ", LI:" + lastIndex + "}";
				}
			}


			private const int CacheLineSize = 64;

			[StructLayout(LayoutKind.Explicit, Size = CacheLineSize * 2)]
			public struct PaddedRange {
				// We want to use 32 bits to store both index and lastIndex so we can use the 
				// Interlocked API with "state", that contains index and lastIndex
				[FieldOffset(CacheLineSize)] public SimpleRange value;

				public PaddedRange(ushort index, ushort lastIndex) {
					value = new SimpleRange(index, lastIndex);
				}
				
				public override string ToString() {
					return value.ToString();
				}
			}
		}


		// https://begeeben.wordpress.com/2012/08/21/heap-sort-in-c/
		private static void HeapSort(IndexCount[] input) {
			//Build-Max-Heap
			int heapSize = input.Length;
			for (int p = (heapSize - 1) / 2; p >= 0; p--)
				MaxHeapify(input, heapSize, p);

			for (int i = input.Length - 1; i > 0; i--) {
				//Swap
				var temp = input[i];
				input[i] = input[0];
				input[0] = temp;

				heapSize--;
				MaxHeapify(input, heapSize, 0);
			}
		}
		private static void MaxHeapify(IndexCount[] input, int heapSize, int index) {
			int left = (index + 1) * 2 - 1;
			int right = (index + 1) * 2;
			int smallest = 0;

			if (left < heapSize && input[left].count < input[index].count)
				smallest = left;
			else
				smallest = index;

			if (right < heapSize && input[right].count < input[smallest].count)
				smallest = right;

			if (smallest != index) {
				var temp = input[index];
				input[index] = input[smallest];
				input[smallest] = temp;

				MaxHeapify(input, heapSize, smallest);
			}
		}

		internal struct IndexCount {
			public int index, count;
			public IndexCount(int index, int count) {
				this.index = index;
				this.count = count;
			}
		}
	}
}
