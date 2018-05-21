using System;

namespace Ashkatchap.Scheduler {
	public struct QueuedJob : IEquatable<QueuedJob> {
		public readonly int jobId;
		private readonly int jobArrayIndex;
		private readonly Job[] jobsArrayRef;

		internal QueuedJob(int jobId, int jobArrayIndex, Job[] jobsArrayRef) {
			this.jobId = jobId;
			this.jobArrayIndex = jobArrayIndex;
			this.jobsArrayRef = jobsArrayRef;
		}

		public void WaitForFinish() {
			if (0 == jobId) return;
			if (jobId != jobsArrayRef[jobArrayIndex].jobId) return;
			jobsArrayRef[jobArrayIndex].WaitForFinish();
		}

		public bool Equals(QueuedJob other) {
			return jobId == other.jobId;
		}
	}
}
