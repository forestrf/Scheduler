using Ashkatchap.Scheduler;
using UnityEngine;

namespace Ashkatchap.UnityScheduler {
	public class TimerScaled : ITimer {
		private double cachedTime;

		public TimerScaled() {
			cachedTime = Time.time;
		}

		public void UpdateCurrentTime() {
			cachedTime = Time.time;
		}

		public double GetCurrentTime() {
			return cachedTime;
		}
	}
}
