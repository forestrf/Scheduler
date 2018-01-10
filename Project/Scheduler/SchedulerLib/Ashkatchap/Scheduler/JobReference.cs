using Ashkatchap.Scheduler.Logging;

namespace Ashkatchap.Scheduler {
	public struct JobReference {
		private QueuedJob job;
		private int id;

		internal JobReference(QueuedJob job) {
			this.job = job;
			this.id = job != null ? job.GetId() : -1;
		}

		public bool WaitForFinish() {
			Logger.WarnAssert(!Scheduler.InMainThread(), "WaitForFinish can only be called from the main thread");
			if (!Scheduler.InMainThread()) return true;
			if (job == null) return true;
			if (job.CheckId(id)) job.WaitForFinish();
			return !job.HasErrors();
		}

		public void Destroy() {
			Logger.WarnAssert(!Scheduler.InMainThread(), "Destroy can only be called from the main thread");
			if (!Scheduler.InMainThread()) return;
			if (job == null) return;
			if (job.CheckId(id)) job.Destroy();
		}
	}
}
