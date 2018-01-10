using System.Diagnostics;

namespace Ashkatchap.Scheduler.Logging {
	internal static class Logger {
		private const string KEYWORD = "SCHEDULER_";
		public static ILogger logger = new InternalLogger();

		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE"), Conditional(KEYWORD + "DEBUG"), Conditional(KEYWORD + "INFO"), Conditional(KEYWORD + "WARN"), Conditional(KEYWORD + "ERROR")]
		public static void Error(string message) {
			logger.Error(message);
		}
		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE"), Conditional(KEYWORD + "DEBUG"), Conditional(KEYWORD + "INFO"), Conditional(KEYWORD + "WARN"), Conditional(KEYWORD + "ERROR")]
		public static void ErrorAssert(bool check, string message) {
			if (check) {
				logger.Error(message);
			}
		}

		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE"), Conditional(KEYWORD + "DEBUG"), Conditional(KEYWORD + "INFO"), Conditional(KEYWORD + "WARN")]
		public static void Warn(string message) {
			logger.Warn(message);
		}
		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE"), Conditional(KEYWORD + "DEBUG"), Conditional(KEYWORD + "INFO"), Conditional(KEYWORD + "WARN")]
		public static void WarnAssert(bool check, string message) {
			if (check) {
				logger.Warn(message);
			}
		}

		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE"), Conditional(KEYWORD + "DEBUG"), Conditional(KEYWORD + "INFO")]
		public static void Info(string message) {
			logger.Info(message);
		}
		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE"), Conditional(KEYWORD + "DEBUG"), Conditional(KEYWORD + "INFO")]
		public static void InfoAssert(bool check, string message) {
			if (check) {
				logger.Info(message);
			}
		}

		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE"), Conditional(KEYWORD + "DEBUG")]
		public static void Debug(string message) {
			logger.Debug(message);
		}
		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE"), Conditional(KEYWORD + "DEBUG")]
		public static void DebugAssert(bool check, string message) {
			if (check) {
				logger.Debug(message);
			}
		}

		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE")]
		public static void Trace(string message) {
			logger.Trace(message);
		}
		[Conditional(KEYWORD + "TRACEVERBOSE"), Conditional(KEYWORD + "TRACE")]
		public static void TraceAssert(bool check, string message) {
			if (check) {
				logger.Trace(message);
			}
		}

		[Conditional(KEYWORD + "TRACEVERBOSE")]
		public static void TraceVerbose(string message) {
			logger.TraceVerbose(message);
		}
		[Conditional(KEYWORD + "TRACEVERBOSE")]
		public static void TraceVerboseAssert(bool check, string message) {
			if (check) {
				logger.TraceVerbose(message);
			}
		}
	}
}
