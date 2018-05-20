﻿using System;
using System.Threading;

namespace Ashkatchap.Scheduler {
	internal partial class FrameUpdater {
		internal class Worker : IDisposable {
			private readonly Thread thread;
			internal readonly AutoResetEvent waiter = new AutoResetEvent(false);
			private WorkerManager executor;

			public Worker(WorkerManager executor, int index) {
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

			int lastExecutorPriorityStamp;
			
			internal QueuedJob pendingJob;

			private void ThreadMethod() {
				while (true) {
					while (ThreadedJobs.FORCE_SINGLE_THREAD) {
						Thread.Sleep(30);
					}

					if (null != pendingJob) {
						var currentPendingJob = pendingJob;
						QueuedJob job = Interlocked.CompareExchange(ref pendingJob, null, currentPendingJob);
						if (null != job) {
							job.Execute();
							var currentExecutorPriorityStamp = executor.lastActionStamp;
							if (currentExecutorPriorityStamp != lastExecutorPriorityStamp) {
								lastExecutorPriorityStamp = currentExecutorPriorityStamp;
								continue;
							}
						}
					} else {
						executor.waiter.Set();
						waiter.WaitOne();
					}
				}
			}

			internal static void SecureLaunchThread(Action action, string name) {
				try {
					action();
				}
				catch (Exception e) {
					Console.WriteLine(e);
					if (e.GetType() != typeof(ThreadInterruptedException) && e.GetType() != typeof(ThreadAbortException))
						Console.WriteLine(e.ToString());
				}
			}

			public void Dispose() {
				waiter.Close();
			}
		}
	}
}
