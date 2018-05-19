using System;
using System.Threading;

namespace Ashkatchap.Scheduler {
	internal class QueuedJob {
		public enum STATE : byte { WORKING, WAIT_FINISH, FINISHED, ERROR, DESTROYED }
		public STATE GetState() {
			return (STATE) state;
		}
		private int state;
		private static int lastId = 0;

		#region EXECUTOR_RW WORKER_RW
		private int length;
		private Job job;
		private Action<Exception> onException;
		#endregion

		#region EXECUTOR_RW WORKER_R
		private int temporalId;
		#endregion

		#region EXECUTOR_RW
		private int nextIndex;
		private int doneIndices;
		internal int priority;
		#endregion


		public QueuedJob() { }

		internal bool Init(Job job, ushort length, byte priority, Action<Exception> onException) {
			if (!Scheduler.InMainThread()) return false;

			this.onException = onException;
			this.job = job;
			this.temporalId = lastId++;
			this.priority = priority;
			this.state = (int) STATE.WORKING;
			this.nextIndex = 0;
			this.doneIndices = 0;
			this.length = length;
			Thread.MemoryBarrier();
			return true;
		}

		internal bool TryExecute() {
			if (state != (int) STATE.WORKING) return false;
			int indexToExecute = Interlocked.Increment(ref nextIndex);
			if (length - 1 == indexToExecute)
				Interlocked.CompareExchange(ref state, (int) STATE.WAIT_FINISH, (int) STATE.WORKING);
			else if (indexToExecute >= length)
				return false;

			try {
				job(indexToExecute);
				int doneIndex = Interlocked.Increment(ref doneIndices);
				if (length - 1 == doneIndex) {
					var originalState = Interlocked.CompareExchange(ref state, (int) STATE.FINISHED, (int) STATE.WAIT_FINISH);
					if ((int) STATE.WORKING == originalState)
						Interlocked.CompareExchange(ref state, (int) STATE.FINISHED, (int) STATE.WORKING);
				}
				return true;
			} catch (Exception e) {
				Interlocked.Exchange(ref state, (int) STATE.ERROR);
				Interlocked.Exchange(ref nextIndex, length);
				if (null != onException) onException(e);
				return false;
			}
		}

		public bool WaitForFinish() {
			if (!Scheduler.InMainThread()) return false;
			if (null == Scheduler.executor) return false;
			Scheduler.executor.SetJobToAllThreads(this);
			while (STATE.WORKING == (STATE) state) { TryExecute(); }
			while (STATE.WAIT_FINISH == (STATE) state) Thread.Sleep(0);
			return STATE.FINISHED == (STATE) state;
		}

		public void Destroy() {
			Interlocked.Exchange(ref state, (int) STATE.DESTROYED);
		}
		
		/// <returns>0 when finished, more than 0 otherwise</returns>
		public int Remaining() {
			int count = doneIndices;
			return length - count;
		}

		public bool ChangePriority(byte newPriority) {
			if (!Scheduler.InMainThread()) return false;
			if (null == Scheduler.executor) return false;

			Scheduler.executor.JobPriorityChange(this, newPriority);
			return true;
		}

		public bool CheckId(int id) {
			return temporalId == id;
		}
		public int GetId() {
			return temporalId;
		}
	}
}
