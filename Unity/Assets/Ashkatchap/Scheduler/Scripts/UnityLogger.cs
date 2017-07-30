namespace Ashkatchap.Updater {
	public class UnityLogger : ILogger {
		public void Error(string message) {
			UnityEngine.Debug.LogError(message);
		}

		public void Warn(string message) {
			UnityEngine.Debug.LogWarning(message);
		}

		public void Info(string message) {
			UnityEngine.Debug.Log(message);
		}

		public void Debug(string message) {
			UnityEngine.Debug.Log(message);
		}

		public void Trace(string message) {
			UnityEngine.Debug.Log(message);
		}

		public void TraceVerbose(string message) {
			UnityEngine.Debug.Log(message);
		}
	}
}
