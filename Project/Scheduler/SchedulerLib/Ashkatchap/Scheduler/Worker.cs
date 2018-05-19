using System;
using System.Threading;

namespace Ashkatchap.Scheduler {
	internal partial class FrameUpdater {
		internal class WorkerBase {
			public readonly int index;
			public readonly int[] tmp = new int[Scheduler.AVAILABLE_CORES];

			public WorkerBase(int index) {
				this.index = index;
			}
		}
		internal class Worker : WorkerBase {
			private readonly Thread thread;
			internal readonly AutoResetEvent waiter = new AutoResetEvent(false);
			private WorkerManager executor;

			public Worker(WorkerManager executor, int index) : base(index) {
				this.executor = executor;
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
					while (Scheduler.FORCE_SINGLE_THREAD) {
						Thread.Sleep(30);
					}

					bool workDone = false;
					int p = 0, i = 0;

					// The executor may replace this array from another thread
					// Because of that, we cache a reference to the array
					QueuedJob[] array;
					do {
						array = executor.jobsToDo[p].array;
						var queuedJob = i < array.Length ? array[i++] : null;
						if (queuedJob == null) {
							i = 0;
							p++;
							continue;
						}

						while (queuedJob.TryExecute()) {
							workDone = true;
							var currentExecutorPriorityStamp = executor.lastActionStamp;
							if (currentExecutorPriorityStamp != lastExecutorPriorityStamp) {
								lastExecutorPriorityStamp = currentExecutorPriorityStamp;

								p = i = 0;
								break;
							}
						}
					} while (p < executor.jobsToDo.Length);

					// We want to check for work to do until we make a full inspection of jobsToDo and find nothing 
					if (!workDone) {
						// If we reach this point, then we wait for more work:
						waiter.WaitOne();
					}
				}
			}

			private short lastExecutorPriorityStamp = -1;
			private bool IsUpToDate() {
				var currentExecutorPriorityStamp = executor.lastActionStamp;
				if (currentExecutorPriorityStamp != lastExecutorPriorityStamp) {
					lastExecutorPriorityStamp = currentExecutorPriorityStamp;
					return false;
				}
				return true;
			}

			internal static void SecureLaunchThread(Action action, string name) {
				try {
					action();
				}
				catch (Exception e) {
					if (e.GetType() != typeof(ThreadInterruptedException) && e.GetType() != typeof(ThreadAbortException))
						Console.WriteLine(e.ToString());
				}
			}
		}
	}
}
