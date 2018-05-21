using System;

namespace Ashkatchap.Scheduler {
	public struct QueuedJob : IEquatable<QueuedJob> {
		public readonly int jobId;
		private readonly Job job;

		internal QueuedJob(Job job) {
			this.jobId = job.jobId;
			this.job = job;
		}

		public void WaitForFinish() {
			if (0 == jobId) return;
			if (jobId != job.jobId) return;
			job.WaitForFinish();
		}

		public bool Equals(QueuedJob other) {
			return jobId == other.jobId;
		}
	}
}
