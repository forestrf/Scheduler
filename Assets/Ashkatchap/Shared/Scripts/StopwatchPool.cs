using System.Collections.Generic;
using System.Diagnostics;

namespace Ashkatchap.Shared {
	public static class StopwatchPool {
		static Stack<Stopwatch> pool = new Stack<Stopwatch>();
		public static Stopwatch StartClock() {
			Stopwatch s;
			if (pool.Count > 0) {
				s = pool.Pop();
			} else {
				s = new Stopwatch();
			}
			s.Reset();
			s.Start();
			return s;
		}
		public static float StopClockGetMS(Stopwatch clock) {
			clock.Stop();
			pool.Push(clock);
			return (float) clock.Elapsed.TotalMilliseconds;
		}
	}
}
