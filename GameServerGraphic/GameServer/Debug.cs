using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace GameServer
{
	/// <summary>
	///   <para>Class containing methods to ease debugging while developing a game.</para>
	/// </summary>
	public sealed class Debug
	{
		/// <summary>
		///   <para>Logs message to the Unity Console.</para>
		/// </summary>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void Log(object message, bool printStackTrace = false)
		{
			//AttachConsole(ATTACH_PARENT_PROCESS);
			if (printStackTrace)
			{
				Console.Write(message + " at Position ");
				Console.Write(GetStackTrace());
			}
			else
			{
				Console.Write(message + " at Position ");
				Console.Write(GetStackTrace());
			}
			//Debug.logger.Log(LogType.Log, message);
		}

		static string GetStackTrace()
		{
			System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
			return t.GetFrame(2).ToString();
		}

		static int GetNthIndex(string s, char t, int n)
		{
			int count = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == t)
				{
					count++;
					if (count == n)
					{
						return i;
					}
				}
			}
			return -1;
		}

		/// <summary>
		///   <para>Logs message to the Unity Console.</para>
		/// </summary>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void Log(object message, Object context)
		{
			//Debug.logger.Log(LogType.Log, message, context);
		}

		/// <summary>
		///   <para>Logs a formatted message to the Unity Console.</para>
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">Format arguments.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogFormat(string format, params object[] args)
		{
			//Debug.logger.LogFormat(LogType.Log, format, args);
		}

		/// <summary>
		///   <para>Logs a formatted message to the Unity Console.</para>
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">Format arguments.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogFormat(Object context, string format, params object[] args)
		{
			//Debug.logger.LogFormat(LogType.Log, context, format, args);
		}

		/// <summary>
		///   <para>A variant of Debug.Log that logs an error message to the console.</para>
		/// </summary>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogError(object message)
		{
			//AttachConsole(ATTACH_PARENT_PROCESS);
			Console.WriteLine(message + " ERROR " + GetStackTrace());
			//Debug.logger.Log(LogType.Error, message);
		}

		/// <summary>
		///   <para>A variant of Debug.Log that logs an error message to the console.</para>
		/// </summary>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogError(object message, Object context)
		{
			//AttachConsole(ATTACH_PARENT_PROCESS);
			Console.WriteLine(message + "ERROR");
			//Debug.logger.Log(LogType.Error, message, context);
		}

		/// <summary>
		///   <para>Logs a formatted error message to the Unity console.</para>
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">Format arguments.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogErrorFormat(string format, params object[] args)
		{
			//Debug.logger.LogFormat(LogType.Error, format, args);
		}

		/// <summary>
		///   <para>Logs a formatted error message to the Unity console.</para>
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">Format arguments.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogErrorFormat(Object context, string format, params object[] args)
		{
			//Debug.logger.LogFormat(LogType.Error, context, format, args);
		}
		/// <summary>
		///   <para>A variant of Debug.Log that logs an error message to the console.</para>
		/// </summary>
		/// <param name="context">Object to which the message applies.</param>
		/// <param name="exception">Runtime Exception.</param>
		public static void LogException(Exception exception)
		{
			//	Debug.logger.LogException(exception, (Object)null);
		}

		/// <summary>
		///   <para>A variant of Debug.Log that logs an error message to the console.</para>
		/// </summary>
		/// <param name="context">Object to which the message applies.</param>
		/// <param name="exception">Runtime Exception.</param>
		public static void LogException(Exception exception, Object context)
		{
			//Debug.logger.LogException(exception, context);
		}

		/// <summary>
		///   <para>A variant of Debug.Log that logs a warning message to the console.</para>
		/// </summary>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogWarning(object message)
		{
			//Debug.logger.Log(LogType.Warning, message);
		}

		/// <summary>
		///   <para>A variant of Debug.Log that logs a warning message to the console.</para>
		/// </summary>
		/// <param name="message">String or object to be converted to string representation for display.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogWarning(object message, Object context)
		{
			//Debug.logger.Log(LogType.Warning, message, context);
		}

		/// <summary>
		///   <para>Logs a formatted warning message to the Unity Console.</para>
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">Format arguments.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogWarningFormat(string format, params object[] args)
		{
			//Debug.logger.LogFormat(LogType.Warning, format, args);
		}

		/// <summary>
		///   <para>Logs a formatted warning message to the Unity Console.</para>
		/// </summary>
		/// <param name="format">A composite format string.</param>
		/// <param name="args">Format arguments.</param>
		/// <param name="context">Object to which the message applies.</param>
		public static void LogWarningFormat(object context, string format, params object[] args)
		{
			//Debug.logger.LogFormat(LogType.Warning, context, format, args);
		}

		//[DllImport("kernel32.dll")]
		//static extern bool AttachConsole(int dwProcessId);
		//private const int ATTACH_PARENT_PROCESS = -1;
	}
}

