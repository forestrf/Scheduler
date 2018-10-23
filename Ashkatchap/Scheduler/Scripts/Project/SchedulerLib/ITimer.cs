namespace Ashkatchap.Scheduler {
	/// <summary>
	/// Used to get the number of seconds. Precision is not important, and should be the cheapest possible.
	/// </summary>
	public interface ITimer {
		/// <summary>
		/// Update the cached time. Cache the value of Time.realtimeSinceStartup.
		/// </summary>
		void UpdateCurrentTime();

		/// <summary>
		/// Equivalent to the Unity Time.realtimeSinceStartup. Returns a cached value generated when calling <see cref="UpdateCurrentTime"/>
		/// </summary>
		double GetCurrentTime();
	}
}
