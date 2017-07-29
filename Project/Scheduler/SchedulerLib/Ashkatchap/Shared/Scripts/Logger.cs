//#define LOGLEVEL_TRACE_VERBOSE
//#define LOGLEVEL_TRACE
//#define LOGLEVEL_DEBUG
#define LOGLEVEL_INFO
//#define LOGLEVEL_WARN
//#define LOGLEVEL_ERROR

using System;
using System.Diagnostics;
using UnityEngine;

public static class Logger {
#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE || LOGLEVEL_DEBUG || LOGLEVEL_INFO || LOGLEVEL_WARN || LOGLEVEL_ERROR
	public static void Error(string message) {
		Console.WriteLine(message);
	}
	public static void ErrorAssert(bool check, string message) {
		if (check) {
			Console.WriteLine(message);
		}
	}
#else
	[Conditional("LOGLEVEL_NEVER")] public static void Error(string message) { }
	[Conditional("LOGLEVEL_NEVER")] public static void ErrorAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE || LOGLEVEL_DEBUG || LOGLEVEL_INFO || LOGLEVEL_WARN
	public static void Warn(string message) {
		Console.WriteLine(message);
	}
	public static void WarnAssert(bool check, string message) {
		if (check) {
			Console.WriteLine(message);
		}
	}
#else
	[Conditional("LOGLEVEL_NEVER")] public static void Warn(string message) { }
	[Conditional("LOGLEVEL_NEVER")] public static void WarnAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE || LOGLEVEL_DEBUG || LOGLEVEL_INFO
	public static void Info(string message) {
		Console.WriteLine(message);
	}
	public static void InfoAssert(bool check, string message) {
		if (check) {
			Console.WriteLine(message);
		}
	}
#else
	[Conditional("LOGLEVEL_NEVER")] public static void Info(string message) { }
	[Conditional("LOGLEVEL_NEVER")] public static void InfoAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE || LOGLEVEL_DEBUG
	public static void Debug(string message) {
		Console.WriteLine(message);
	}
	public static void DebugAssert(bool check, string message) {
		if (check) {
			Console.WriteLine(message);
		}
	}
#else
	[Conditional("LOGLEVEL_NEVER")] public static void Debug(string message) { }
	[Conditional("LOGLEVEL_NEVER")] public static void DebugAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE || LOGLEVEL_TRACE
	public static void Trace(string message) {
		Console.WriteLine(message);
	}
	public static void TraceAssert(bool check, string message) {
		if (check) {
			Console.WriteLine(message);
		}
	}
#else
	[Conditional("LOGLEVEL_NEVER")] public static void Trace(string message) { }
	[Conditional("LOGLEVEL_NEVER")] public static void TraceAssert(bool check, string message) { }
#endif

#if LOGLEVEL_TRACE_VERBOSE
	public static void TraceVerbose(string message) {
		Console.WriteLine(message);
	}
	public static void TraceVerboseAssert(bool check, string message) {
		if (check) {
			Console.WriteLine(message);
		}
	}
#else
	[Conditional("LOGLEVEL_NEVER")] public static void TraceVerbose(string message) { }
	[Conditional("LOGLEVEL_NEVER")] public static void TraceVerboseAssert(bool check, string message) { }
#endif
}
