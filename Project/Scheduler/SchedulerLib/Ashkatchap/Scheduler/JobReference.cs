namespace Ashkatchap.Scheduler {
	public struct JobReference {
		private QueuedJob job;
		private int id;

		internal JobReference(QueuedJob job) {
			this.job = job;
			this.id = job != null ? job.GetId() : -1;
		}

		public bool WaitForFinish() {
			if (!Scheduler.InMainThread()) return true;
			if (job == null) return true;
			if (job.CheckId(id)) job.WaitForFinish();
			return QueuedJob.STATE.FINISHED == job.GetState();
		}

		public void Destroy() {
			if (!Scheduler.InMainThread()) return;
			if (job == null) return;
			if (job.CheckId(id)) job.Destroy();
		}
	}
}
