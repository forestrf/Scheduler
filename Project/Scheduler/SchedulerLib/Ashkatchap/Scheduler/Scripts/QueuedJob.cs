using Ashkatchap.Shared.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ashkatchap.Updater {
	internal class QueuedJob {
		private static int lastId = 0;
		private static int CLEAN_STATE = new Range.PaddedRange(1, 0).state;
		internal AutoResetEvent finishEvent = new AutoResetEvent(false);

		private int hasErrors;

		#region EXECUTOR_RW WORKER_RW
		private Job job;
		private volatile int doneIndices;
		private int length;
		#endregion

		private readonly Range[] indices;
		private readonly Volatile.PaddedInt[] executedIndicesPerThread;

		#region EXECUTOR_RW WORKER_R
		private int temporalId;
		private int minimumRangeToSteal = 0;
		#endregion

		#region EXECUTOR_RW
		internal int priority;
		#endregion


		public QueuedJob() {
			indices = new Range[Scheduler.AVAILABLE_CORES];
			for (int i = 0; i < indices.Length; i++) indices[i] = new Range();
			executedIndicesPerThread = new Volatile.PaddedInt[Scheduler.AVAILABLE_CORES];
		}


		internal void Init(Job job, ushort length, byte priority, ushort minimumRangeToSteal) {
			Logger.WarnAssert(!Scheduler.InMainThread(), "Init can only be called from the main thread");
			if (!Scheduler.InMainThread()) return;

			finishEvent.Reset();

			Interlocked.Exchange(ref this.job, job);
			Interlocked.Exchange(ref this.temporalId, lastId++);
			Interlocked.Exchange(ref this.priority, priority);
			Interlocked.Exchange(ref this.minimumRangeToSteal, minimumRangeToSteal);
			Interlocked.Exchange(ref this.hasErrors, 0);

			if (length == 0) indices[0].range.Set(1, 0);
			else indices[0].range.Set(0, (ushort) (length - 1));

			for (int i = 1; i < indices.Length; i++) {
				indices[i].range.Set(1, 0);
			}
			for (int i = 0; i < executedIndicesPerThread.Length; i++) {
				Interlocked.Exchange(ref executedIndicesPerThread[i].value, 0);
			}
			
			this.doneIndices = 0; // volatile
			Interlocked.Exchange(ref this.length, length);

			//Logger.TraceVerbose("[" + temporalId + "] job created");
		}


		internal bool TryExecute(int workerIndex, ref KeyValuePair<int, int>[] tmp) {
			var rangeWrapper = indices[workerIndex];

			Range.PaddedRange rangeToExecute = rangeWrapper.range;


			// get an index to execute
			if (!rangeWrapper.range.GetIndexAndIncrease(out rangeToExecute.index)) {
				// This range is finished so we want a new range, if there is any available
				

				// Prepare tmp array
				if (tmp == null || tmp.Length != indices.Length - 1) {
					tmp = new KeyValuePair<int, int>[indices.Length - 1];
				}

				// First we fetch and sort the remaining ranges
				bool foundRange = false;
				for (int i = 0, j = 0; i < indices.Length; i++) {
					if (i == workerIndex) continue;
					int range = indices[i].range.GetRemainingRange();
					tmp[j++] = new KeyValuePair<int, int>(i, range);
					if (range > minimumRangeToSteal) foundRange = true;
				}
				
				if (foundRange) {
					// sort by range
					Array.Sort(tmp, sortByRange);

					bool rangeObtained = false;
					for (int i = 0; i < tmp.Length; i++) {
						Range.PaddedRange obtainedRange;
						if (indices[tmp[i].Key].GetFreeRange(out obtainedRange, (ushort) minimumRangeToSteal)) {
							// We were able to get a new range
							Logger.TraceVerbose("[" + temporalId + "] Range obtained: " + obtainedRange);
							rangeToExecute = obtainedRange;

							// Next set state, that can change by all threads. Right now only this thread wants to change it
							++obtainedRange.index;
							Interlocked.Exchange(ref rangeWrapper.range.state, obtainedRange.state);
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
			executedIndicesPerThread[workerIndex].value++;
									
			return true;
		}

		bool ReturnFinished(int workerIndex) {
			Commit(workerIndex);
			return false;
		}

		public void Commit(int workerIndex) {
			if (executedIndicesPerThread[workerIndex].value > 0) {
				Interlocked.Add(ref doneIndices, executedIndicesPerThread[workerIndex].value);
				if (doneIndices == length) {
					Logger.TraceVerbose("[" + temporalId + "] job finished");
					finishEvent.Set();
				}
				executedIndicesPerThread[workerIndex].value = 0;
			}
		}

		// DESC order
		private static Comparison<KeyValuePair<int, int>> sortByRange = (a, b) => {
			return b.Value - a.Value;
		};

		private KeyValuePair<int, int>[] tmpForMainThread;
		public void WaitForFinish() {
			Logger.ErrorAssert(!Scheduler.InMainThread(), "WaitForFinish can only be called from the main thread");
			if (!Scheduler.InMainThread()) return;

			if (Scheduler.executor == null) {
				Logger.WarnAssert(!Scheduler.InMainThread(), "Multithreading is not enabled right now");
				return;
			}
			Scheduler.executor.SetJobToAllThreads(this);
			while (TryExecute(0, ref tmpForMainThread)) ;
			while (!IsFinished()) finishEvent.WaitOne();
		}

		public void Destroy() {
			Interlocked.Exchange(ref doneIndices, length);
			finishEvent.Set();

			for (int i = 0; i < indices.Length; i++) {
				Interlocked.Exchange(ref indices[i].range.state, CLEAN_STATE);
			}
		}

		public bool HasErrors() {
			return hasErrors == 1;
		}

		public bool IsFinished() {
			return doneIndices == length;
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

			public bool GetFreeRange(out PaddedRange obtainedRange, ushort minimumRange = 0) {
				obtainedRange.state = 0;
				int availableRange;
				PaddedRange copiedRange = new PaddedRange(range.index, range.lastIndex);
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

					if (range.SetNewState(rangeToSet.state, comparandState, out copiedRange.state)) {
						//Logger.TraceVerbose(tmpCopied.ToString() + "-> " + rangeToSet.ToString() + " | " + obtainedRange.ToString());
						return true;
					}
				}
				obtainedRange.index = obtainedRange.lastIndex = 0;
				return false;
			}



			private const int CacheLineSize = 64;

			[StructLayout(LayoutKind.Explicit, Size = CacheLineSize * 2)]
			public struct PaddedRange {
				// We want to use 32 bits to store both index and lastIndex so we can use the 
				// Interlocked API with "state", that contains index and lastIndex
				[FieldOffset(CacheLineSize + 0)] public int state;
				[FieldOffset(CacheLineSize + 0)] public ushort index;
				[FieldOffset(CacheLineSize + 2)] public ushort lastIndex;

				public PaddedRange(ushort index, ushort lastIndex) {
					this.state = 0;
					this.index = index;
					this.lastIndex = lastIndex;
				}
				public void Set(ushort index, ushort lastIndex) {
					Interlocked.Exchange(ref state, new PaddedRange(index, lastIndex).state);
				}

				public int GetRemainingRange() {
					return index > lastIndex ? 0 : lastIndex - index + 1;
				}



				public bool GetIndexAndIncrease(out ushort indexToExecute) {
					var toSet = this;
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
		}
	}
}
