//#define LOGLEVEL_TRACE_VERBOSE
//#define LOGLEVEL_TRACE
//#define LOGLEVEL_DEBUG
//#define LOGLEVEL_INFO
//#define LOGLEVEL_WARN
#define LOGLEVEL_ERROR

using System.Diagnostics;

namespace Ashkatchap.Updater {
	public static class Logger {
		public static ILogger logger = new InternalLogger();

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE || LOGLEVEL_DEBUG || LOGLEVEL_INFO || LOGLEVEL_WARN || LOGLEVEL_ERROR
		public static void Error(string message) {
			logger.Error(message);
		}
		public static void ErrorAssert(bool check, string message) {
			if (check) {
				logger.Error(message);
			}
		}
#else
		[Conditional("LOGLEVEL_NEVER")] public static void Error(string message) { }
		[Conditional("LOGLEVEL_NEVER")] public static void ErrorAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE || LOGLEVEL_DEBUG || LOGLEVEL_INFO || LOGLEVEL_WARN
		public static void Warn(string message) {
			logger.Warn(message);
		}
		public static void WarnAssert(bool check, string message) {
			if (check) {
				logger.Warn(message);
			}
		}
#else
		[Conditional("LOGLEVEL_NEVER")] public static void Warn(string message) { }
		[Conditional("LOGLEVEL_NEVER")] public static void WarnAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE || LOGLEVEL_DEBUG || LOGLEVEL_INFO
		public static void Info(string message) {
			logger.Info(message);
		}
		public static void InfoAssert(bool check, string message) {
			if (check) {
				logger.Info(message);
			}
		}
#else
		[Conditional("LOGLEVEL_NEVER")] public static void Info(string message) { }
		[Conditional("LOGLEVEL_NEVER")] public static void InfoAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE || LOGLEVEL_DEBUG
		public static void Debug(string message) {
			logger.Debug(message);
		}
		public static void DebugAssert(bool check, string message) {
			if (check) {
				logger.Debug(message);
			}
		}
#else
		[Conditional("LOGLEVEL_NEVER")] public static void Debug(string message) { }
		[Conditional("LOGLEVEL_NEVER")] public static void DebugAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE
		public static void Trace(string message) {
			logger.Trace(message);
		}
		public static void TraceAssert(bool check, string message) {
			if (check) {
				logger.Trace(message);
			}
		}
#else
		[Conditional("LOGLEVEL_NEVER")] public static void Trace(string message) { }
		[Conditional("LOGLEVEL_NEVER")] public static void TraceAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE
		public static void TraceVerbose(string message) {
			logger.TraceVerbose(message);
		}
		public static void TraceVerboseAssert(bool check, string message) {
			if (check) {
				logger.TraceVerbose(message);
			}
		}
#else
		[Conditional("LOGLEVEL_NEVER")] public static void TraceVerbose(string message) { }
		[Conditional("LOGLEVEL_NEVER")] public static void TraceVerboseAssert(bool check, string message) { }
#endif
	}
}
