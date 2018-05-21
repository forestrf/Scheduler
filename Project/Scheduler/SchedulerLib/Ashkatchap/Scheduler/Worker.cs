using Ashkatchap.Scheduler.Collections;
using System;
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

			internal ThreadSafeRingBuffer_SingleProducer_SingleConsumerInt jobsToDo;

			private void ThreadMethod() {
				jobsToDo = new ThreadSafeRingBuffer_SingleProducer_SingleConsumerInt(15, executor.jobDistributor, Thread.CurrentThread);
				while (true) {
					while (ThreadedJobs.FORCE_SINGLE_THREAD) {
						Thread.Sleep(30);
					}
					
					int indexToDo;
					if (jobsToDo.TryDequeue(out indexToDo)) {
						Job job = executor.jobsForWorkers[indexToDo];
						if (0 != job.jobId) {
							executor.jobsForWorkers[indexToDo].Execute();
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

	internal class Job {
		private static int lastId = 0;

		private enum STATE { WAITING, STARTED, FINISHED }
		private int state;
		private Action job;
		private Action<Exception> onException;
		internal int jobId;
		internal int jobArrayIndex;

		private AutoResetEvent are = new AutoResetEvent(false);

		internal void Set(Action job, Action<Exception> onException, int jobArrayIndex) {
			this.jobArrayIndex = jobArrayIndex;
			this.onException = onException;
			this.job = job;
			this.jobId = 0;
			while (0 == jobId) // Don't allow 0 as id because it is the default value and the value of not valid jobs
				jobId = Interlocked.Increment(ref lastId);
			state = (int) STATE.WAITING;
			are.Reset();
		}

		internal void Execute() {
			if ((int) STATE.WAITING != state) return;
			if ((int) STATE.WAITING != Interlocked.CompareExchange(ref state, (int) STATE.STARTED, (int) STATE.WAITING)) return;

			try {
				job();
			}
			catch (Exception e) {
				Console.WriteLine(e);
				if (null != onException) onException(e);
			}
			jobId = 0;
			are.Set();
			state = (int) STATE.FINISHED;
		}

		public bool WaitForFinish() {
			switch (state) {
				default:
					return false;
				case (int) STATE.WAITING:
				case (int) STATE.STARTED:
					are.WaitOne();
					return true;
				case (int) STATE.FINISHED:
					return true;
			}
		}

		public bool Equals(QueuedJob other) {
			return jobId == other.jobId;
		}
	}
}
