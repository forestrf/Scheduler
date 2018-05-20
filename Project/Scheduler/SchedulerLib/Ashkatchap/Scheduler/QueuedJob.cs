using System;
using System.Threading;

namespace Ashkatchap.Scheduler {
	public class QueuedJob : IEquatable<QueuedJob> {
		private static int lastId = 0;

		private enum STATE { WAITING, STARTED, FINISHED, ERROR, DESTROYED }
		private STATE state;
		private Action job;
		private Action<Exception> onException;
		public readonly int jobId;

		internal QueuedJob(Action job, Action<Exception> onException) {
			this.onException = onException;
			this.job = job;
			this.jobId = Interlocked.Increment(ref lastId);
			if (0 == jobId) // Don't allow 0 as id because it is the default value and the value of not valid jobs
				jobId = Interlocked.Increment(ref lastId);
			state = STATE.WAITING;
		}

		internal bool Execute() {
			if (STATE.WAITING != state) return false;
			state = STATE.STARTED;

			try {
				job();
				state = STATE.FINISHED;
				return true;
			} catch (Exception e) {
				Console.WriteLine(e);
				state = STATE.ERROR;
				if (null != onException) onException(e);
				return false;
			}
		}

		public bool WaitForFinish() {
			check_again:
			switch (state) {
				case STATE.DESTROYED:
				case STATE.ERROR:
				default:
					return false;
				case STATE.WAITING:
				case STATE.STARTED:
					Thread.Sleep(0);
					goto check_again;
				case STATE.FINISHED:
					return true;
			}
		}

		public void Destroy() {
			state = STATE.DESTROYED;
		}

		public bool Equals(QueuedJob other) {
			return jobId == other.jobId;
		}
	}
}
