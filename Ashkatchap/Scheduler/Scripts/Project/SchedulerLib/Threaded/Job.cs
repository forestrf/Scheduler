using System;
using System.Threading;
using UnityEngine.Profiling;

namespace Ashkatchap.Scheduler {
	internal class Job {
		private static int lastId = 0;

		private enum STATE { WAITING, STARTED, FINISHED }
		private int state;
		private Action job;
		private Action<Exception> onException;
		internal int jobId;

		private AutoResetEvent are = new AutoResetEvent(false);

		internal void Set(Action job, Action<Exception> onException) {
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

			Profiler.BeginSample("Job");
			try {
				job();
			}
			catch (Exception e) {
				if (null != onException) onException(e);
				else if (null != ThreadedJobs.OnException) ThreadedJobs.OnException(e);
				else Console.WriteLine(e);
			}
			Profiler.EndSample();
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
