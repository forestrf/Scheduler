using System;
using System.Threading;

namespace Ashkatchap.Updater {
	public partial class FrameUpdater {
		private class Worker {
			private readonly Thread thread;
			internal readonly AutoResetEvent waiter = new AutoResetEvent(false);
			private WorkerManager executor;
			private int index, count;

			public Worker(WorkerManager executor, int index, int count) {
				this.executor = executor;
				this.index = index;
				this.count = count;
				thread = new Thread(() => { SecureLaunchThread(ThreadMethod, "Worker FrameUpdater [" + index + "]"); });
				thread.Name = "Worker FrameUpdater [" + index + "]";
				thread.Priority = ThreadPriority.AboveNormal;
				thread.IsBackground = true;
				thread.Start();
			}

			public void OnDestroy() {
				thread.Interrupt();
				thread.Abort();
			}
			
			void ThreadMethod() {
				while (true) {
					while (FORCE_SINGLE_THREAD) {
						Thread.Sleep(32);
					}

					// Try run jobs until there are no more jobs
					short lastExecutorPriorityStamp = -1; // it will always fail the first time
					int p = 0;
					int i = 0;
					bool workDone = false;
					do {
						var currentExecutorPriorityStamp = executor.lastActionStamp;
						if (currentExecutorPriorityStamp != lastExecutorPriorityStamp) {
							p = 0;
							i = 0;
							lastExecutorPriorityStamp = currentExecutorPriorityStamp;
						}

						// The executor may replace this array from another thread
						// Because of that, we cache a reference to the array
						var array = executor.jobsToDo[p].array; // cache a reference
						var queuedJob = i < array.Length ? array[i] : null;
						if (queuedJob == null) {
							// We reached the last element of the "dynamic" array
							p++;
							i = 0;
						} else if (queuedJob.TryExecute()) {
							workDone = true;
						} else {
							i++;
						}
					} while (p < executor.jobsToDo.Length);

					// We want to check for work to do until we make a full inspection of jobsToDo and find nothing 
					if (!workDone) {
						// If we reach this point, then we wait for more work:
						waiter.WaitOne();
					}
				}
			}

			internal static void SecureLaunchThread(Action action, string name) {
				try {
					action();
				}
				catch (Exception e) {
					if (e.GetType() != typeof(ThreadInterruptedException) && e.GetType() != typeof(ThreadAbortException))
						Logger.Error(e.ToString());
				}
			}
		}
	}
}
