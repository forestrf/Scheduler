#pragma warning disable 420

using System;
using System.Threading;

namespace Ashkatchap.Updater {
	internal class QueuedJob {
		private static int lastId = 0;


		#region EXECUTOR_RW WORKER_RW
		private Job job;
		private volatile int index;
		private volatile int doneIndices;
		private ushort length;
		#endregion

		#region EXECUTOR_RW WORKER_R
		private int temporalId;
		#endregion

		#region EXECUTOR_RW
		internal byte priority;
		#endregion


		internal void Init(Job job, ushort length, byte priority) {
			Logger.WarnAssert(!Scheduler.InMainThread(), "Init can only be called from the main thread");
			if (!Scheduler.InMainThread()) return;
				
			Thread.MemoryBarrier();
			this.job = job;
			temporalId = lastId++;
			this.priority = priority;
			Thread.MemoryBarrier();
			index = 0;
			doneIndices = 0;
			Thread.MemoryBarrier();
			this.length = length;
			Thread.MemoryBarrier();

			Logger.TraceVerbose("[" + temporalId + "] job created");
		}

		internal bool TryExecute() {
			int indexToRun = Interlocked.Increment(ref index) - 1;
			if (indexToRun < length) {
				try {
					job(indexToRun);
				} catch (Exception e) {
					Logger.Error(e.ToString());
				} finally {
					int f = Interlocked.Increment(ref doneIndices);
					if (f == length) {
						Logger.TraceVerbose("[" + temporalId + "] job finished");
					}
				}
						
				return true;
			}
				
			return false;
		}

		public void WaitForFinish() {
			Logger.ErrorAssert(!Scheduler.InMainThread(), "WaitForFinish can only be called from the main thread");
			if (!Scheduler.InMainThread()) return;

			if (Scheduler.executor == null) {
				Logger.WarnAssert(!Scheduler.InMainThread(), "Multithreading is not enabled right now");
				return;
			}
			Scheduler.executor.SetJobToAllThreads(this);
			while (true) if (!TryExecute()) break;
			while (!IsFinished()) {
				Thread.SpinWait(20);
			}
		}

		public void Destroy() {
			Logger.ErrorAssert(!Scheduler.InMainThread(), "Init can only be called from the main thread");
			if (!Scheduler.InMainThread()) return;

			doneIndices = length;
			index = length;
			Thread.MemoryBarrier();
		}

		public bool IsFinished() {
			int f = doneIndices;
			if (f == length) {
				return true;
			} else if (f > length) {
				Logger.Error("doneIndices is greater than length");
				return true;
			}
			return false;
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
	}
}
