using Ashkatchap.Scheduler;
using UnityEngine;

namespace Ashkatchap.UnityScheduler {
	public class Timer : ITimer {
		private double cachedTime;

		public Timer() {
			cachedTime = Time.realtimeSinceStartup;
		}

		public void UpdateCurrentTime() {
			cachedTime = Time.realtimeSinceStartup;
		}

		public double GetCurrentTime() {
			return cachedTime;
		}
	}
}
