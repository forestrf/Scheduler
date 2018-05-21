using System.Diagnostics;

namespace Ashkatchap {
	/// <summary>
	/// Time counter used to measure time offsets between snapshots.
	/// There is a limit on how much time can pass between 2 snapshots for this to work correctly.
	/// It is recommended to not try to measure timespans longers than one hour to be safe, and it should be enough for the purpose of this library.
	/// </summary>
	public static class TimeCounter {
		private static readonly double Period = 1d / Stopwatch.Frequency;

		/// <summary>
		/// Get the number of seconds elapsed since <paramref name="from"/> to <paramref name="to"/>
		/// </summary>
		/// <param name="from">Oldest timestamp</param>
		/// <param name="to">Newest timestamp</param>
		public static double ElapsedSeconds(long from, long to) {
			// Rollover expected and harmless
			long offset = to - from;
			return offset * Period;
		}

		/// <summary>
		/// Capture the current timestamp. Use <see cref="ElapsedSeconds"/> to obtain the seconds elapsed between two of them
		/// Not compatible between different machines
		/// It is possible for two consecutive timestamps to not be timedly ordered
		/// </summary>
		public static long GetTimestamp() {
			return Stopwatch.GetTimestamp();
		}
	}
}
