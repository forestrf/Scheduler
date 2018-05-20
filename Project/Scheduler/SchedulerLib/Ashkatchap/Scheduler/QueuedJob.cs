using System;
using System.Threading;

namespace Ashkatchap.Scheduler {
	public class QueuedJob : IEquatable<QueuedJob> {
		private static int lastId = 0;
		
		private enum STATE { WAITING, STARTED, FINISHED, ERROR, DESTROYED }
		private STATE state;

		#region EXECUTOR_RW WORKER_RW
		private Action job;
		private Action<Exception> onException;
		#endregion

		#region EXECUTOR_RW WORKER_R
		private int temporalId;
		#endregion


		internal QueuedJob(Action job, Action<Exception> onException) {
			this.onException = onException;
			this.job = job;
			this.temporalId = Interlocked.Increment(ref lastId);
			if (0 == temporalId) // Don't allow 0 as id because it is the default value and the value of not valid jobs
				temporalId = Interlocked.Increment(ref lastId);
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
				
		public int GetId() {
			return temporalId;
		}

		public bool Equals(QueuedJob other) {
			return GetId() == other.GetId();
		}
	}
}
