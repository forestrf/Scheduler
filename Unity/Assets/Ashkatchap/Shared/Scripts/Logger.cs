//#define LOGLEVEL_TRACE_VERBOSE
//#define LOGLEVEL_TRACE
//#define LOGLEVEL_DEBUG
#define LOGLEVEL_INFO
//#define LOGLEVEL_WARN
//#define LOGLEVEL_ERROR

using System.Diagnostics;
using UnityEngine;

public static class Logger {
#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE || LOGLEVEL_DEBUG || LOGLEVEL_INFO || LOGLEVEL_WARN || LOGLEVEL_ERROR
	public static void Error(string message, Object context) {
		UnityEngine.Debug.LogError(message, context);
	}
	public static void Error(string message) {
		UnityEngine.Debug.LogError(message);
	}
	public static void ErrorAssert(bool check, string message, Object context) {
		if (check) {
			UnityEngine.Debug.LogError(message, context);
		}
	}
	public static void ErrorAssert(bool check, string message) {
		if (check) {
			UnityEngine.Debug.LogError(message);
		}
	}
#else
	[Conditional("LOGLEVEL_NEVER")] public static void Error(string message, Object context) { }
	[Conditional("LOGLEVEL_NEVER")] public static void Error(string message) { }
	[Conditional("LOGLEVEL_NEVER")] public static void ErrorAssert(bool check, string message, Object context) { }
	[Conditional("LOGLEVEL_NEVER")] public static void ErrorAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE || LOGLEVEL_DEBUG || LOGLEVEL_INFO || LOGLEVEL_WARN
	public static void Warn(string message, Object context) {
		UnityEngine.Debug.LogWarning(message, context);
	}
	public static void Warn(string message) {
		UnityEngine.Debug.LogWarning(message);
	}
	public static void WarnAssert(bool check, string message, Object context) {
		if (check) {
			UnityEngine.Debug.LogError(message, context);
		}
	}
	public static void WarnAssert(bool check, string message) {
		if (check) {
			UnityEngine.Debug.LogError(message);
		}
	}
#else
	[Conditional("LOGLEVEL_NEVER")] public static void Warn(string message, Object context) { }
	[Conditional("LOGLEVEL_NEVER")] public static void Warn(string message) { }
	[Conditional("LOGLEVEL_NEVER")] public static void WarnAssert(bool check, string message, Object context) { }
	[Conditional("LOGLEVEL_NEVER")] public static void WarnAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE || LOGLEVEL_DEBUG || LOGLEVEL_INFO
	public static void Info(string message, Object context) {
		UnityEngine.Debug.Log(message, context);
	}
	public static void Info(string message) {
		UnityEngine.Debug.Log(message);
	}
	public static void InfoAssert(bool check, string message, Object context) {
		if (check) {
			UnityEngine.Debug.LogError(message, context);
		}
	}
	public static void InfoAssert(bool check, string message) {
		if (check) {
			UnityEngine.Debug.LogError(message);
		}
	}
#else
	[Conditional("LOGLEVEL_NEVER")] public static void Info(string message, Object context) { }
	[Conditional("LOGLEVEL_NEVER")] public static void Info(string message) { }
	[Conditional("LOGLEVEL_NEVER")] public static void InfoAssert(bool check, string message, Object context) { }
	[Conditional("LOGLEVEL_NEVER")] public static void InfoAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE || LOGLEVEL_DEBUG
	public static void Debug(string message, Object context) {
		UnityEngine.Debug.Log(message, context);
	}
	public static void Debug(string message) {
		UnityEngine.Debug.Log(message);
	}
	public static void DebugAssert(bool check, string message, Object context) {
		if (check) {
			UnityEngine.Debug.LogError(message, context);
		}
	}
	public static void DebugAssert(bool check, string message) {
		if (check) {
			UnityEngine.Debug.LogError(message);
		}
	}
#else
	[Conditional("LOGLEVEL_NEVER")] public static void Debug(string message, Object context) { }
	[Conditional("LOGLEVEL_NEVER")] public static void Debug(string message) { }
	[Conditional("LOGLEVEL_NEVER")] public static void DebugAssert(bool check, string message, Object context) { }
	[Conditional("LOGLEVEL_NEVER")] public static void DebugAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE
	public static void Trace(string message, Object context) {
		UnityEngine.Debug.Log(message, context);
	}
	public static void Trace(string message) {
		UnityEngine.Debug.Log(message);
	}
	public static void TraceAssert(bool check, string message, Object context) {
		if (check) {
			UnityEngine.Debug.LogError(message, context);
		}
	}
	public static void TraceAssert(bool check, string message) {
		if (check) {
			UnityEngine.Debug.LogError(message);
		}
	}
#else
	[Conditional("LOGLEVEL_NEVER")] public static void Trace(string message, Object context) { }
	[Conditional("LOGLEVEL_NEVER")] public static void Trace(string message) { }
	[Conditional("LOGLEVEL_NEVER")] public static void TraceAssert(bool check, string message, Object context) { }
	[Conditional("LOGLEVEL_NEVER")] public static void TraceAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE
	public static void TraceVerbose(string message, Object context) {
		UnityEngine.Debug.Log(message, context);
	}
	public static void TraceVerbose(string message) {
		UnityEngine.Debug.Log(message);
	}
	public static void TraceVerboseAssert(bool check, string message, Object context) {
		if (check) {
			UnityEngine.Debug.LogError(message, context);
		}
	}
	public static void TraceVerboseAssert(bool check, string message) {
		if (check) {
			UnityEngine.Debug.LogError(message);
		}
	}
#else
	[Conditional("LOGLEVEL_NEVER")] public static void TraceVerbose(string message, Object context) { }
	[Conditional("LOGLEVEL_NEVER")] public static void TraceVerbose(string message) { }
	[Conditional("LOGLEVEL_NEVER")] public static void TraceVerboseAssert(bool check, string message, Object context) { }
	[Conditional("LOGLEVEL_NEVER")] public static void TraceVerboseAssert(bool check, string message) { }
#endif
}
