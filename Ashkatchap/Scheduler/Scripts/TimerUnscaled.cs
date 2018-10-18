using Ashkatchap.Scheduler;
using UnityEngine;

namespace Ashkatchap.UnityScheduler {
	public class TimerUnscaled : ITimer {
		private double cachedTime;

		public TimerUnscaled() {
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
