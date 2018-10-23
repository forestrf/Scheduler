using System;

namespace Ashkatchap.Scheduler {
	internal class Timer : ITimer {
		private readonly DateTime start;
		public Timer() {
			start = DateTime.UtcNow;
		}

		private double cachedTime;
		public void UpdateCurrentTime() {
			cachedTime = (DateTime.UtcNow - start).TotalSeconds;
		}

		public double GetCurrentTime() {
			return cachedTime;
		}
	}
}
