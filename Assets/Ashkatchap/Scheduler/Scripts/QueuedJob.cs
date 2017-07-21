using Ashkatchap.Shared.Collections;
using System;
using System.Threading;

namespace Ashkatchap.Updater {
	public partial class FrameUpdater {
		internal class QueuedJob {
			private static int lastId = 0;


			#region EXECUTOR_RW WORKER_RW
			private Job job;
			private Volatile.PaddedInt index;
			private ushort length;
			#endregion

			#region EXECUTOR_RW WORKER_R
			private int temporalId;
			#endregion

			#region EXECUTOR_RW
			internal byte priority;
			internal byte wantedPriority;
			#endregion


			internal void Init(Job job, ushort length, byte priority) {
				if (!IsFinished()) {
					Logger.Warn("Thread safety broken. Errors may appear, like executing the same job index more than one time.");
				}
				this.job = job;
				temporalId = lastId++;
				this.length = length;
				this.priority = priority;
				Thread.MemoryBarrier();
				index.value = 0;
				Thread.MemoryBarrier();
			}

			internal bool TryExecute() {
				if (index.value < length) {
					int indexToRun = Interlocked.Increment(ref index.value) - 1;
					// check to see if it repeats a number
					if (indexToRun < length) {
						try {
							job(indexToRun);
						} catch (Exception e) {
							Logger.Error(e.ToString());
						}
						
						return true;
					}
				}
				return false;
			}

			public void WaitForFinish() {
				executor.SetJobToAllThreads(this);
				while (true) if (!TryExecute()) break;
			}

			public void Destroy() {
				Interlocked.Exchange(ref index.value, int.MaxValue);
			}

			public bool IsFinished() {
				Thread.MemoryBarrier();
				return index.value >= length;
			}

			public void ChangePriority(byte newPriority) {
				executor.JobPriorityChange(this, newPriority);
			}

			public bool CheckId(int id) {
				return temporalId == id;
			}
			public int GetId() {
				return temporalId;
			}
		}
	}
}
