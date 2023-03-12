
namespace Pinion
{
	using Pinion.Documentation;
	using UnityEngine;

	[APISource]
	[DocSourceDisplayName("Debugging")]
	public static class PinionAPIDebugLogging
	{
		#region LOG
		/// Writes log message $0 to the player log file. Use with caution, can impact performance at high frequency.
		[APIMethod]
		public static void Log(string message)
		{
			LogInternal(message);
		}

		/// Writes log message $1 to the player log file. Use with caution, can impact performance at high frequency.
		/// If $2 is true, will also write to the in-game debug output.
		[APIMethod]
		public static void Log(PinionContainer container, string message, bool displayInGame)
		{
			LogInternal(message);

			if (displayInGame)
				container.Log(message);
		}

		/// Writes log message $0 to the player log file. Use with caution, can impact performance at high frequency.
		[APIMethod]
		public static void Log(int value)
		{
			LogInternal(value);
		}

		/// Writes log message $1 to the player log file. Use with caution, can impact performance at high frequency.
		/// If $2 is true, will also write to the in-game debug output.
		[APIMethod]
		public static void Log(PinionContainer container, int value, bool displayInGame)
		{
			LogInternal(value);

			if (displayInGame)
				container.Log(value.ToString());
		}

		/// Writes log message $0 to the player log file. Use with caution, can impact performance at high frequency.
		[APIMethod]
		public static void Log(float value)
		{
			LogInternal(value);
		}

		/// Writes log message $1 to the player log file. Use with caution, can impact performance at high frequency.
		/// If $2 is true, will also write to the in-game debug output.
		[APIMethod]
		public static void Log(PinionContainer container, float value, bool displayInGame)
		{
			LogInternal(value);

			if (displayInGame)
				container.Log(value.ToString());
		}

		/// Writes log message $0 to the player log file. Use with caution, can impact performance at high frequency.
		[APIMethod]
		public static void Log(bool value)
		{
			LogInternal(value);
		}

		/// Writes log message $1 to the player log file. Use with caution, can impact performance at high frequency.
		/// If $2 is true, will also write to the in-game debug output.
		[APIMethod]
		public static void Log(PinionContainer container, bool value, bool displayInGame)
		{
			LogInternal(value);

			if (displayInGame)
				container.Log(value.ToString());
		}

		private static void LogInternal(object value)
		{
			Debug.Log(value);
		}
		#endregion

		#region LOGWARNING
		/// Writes warning message $0 to the player log file. Use with caution, can impact performance at high frequency.
		[APIMethod]
		public static void LogWarning(string message)
		{
			LogWarningInternal(message);
		}

		/// Writes warning message $1 to the player log file. Use with caution, can impact performance at high frequency.
		/// If $2 is true, will also write to the in-game debug output.
		[APIMethod]
		public static void LogWarning(PinionContainer container, string message, bool displayInGame)
		{
			LogWarningInternal(message);

			if (displayInGame)
				container.LogWarning(message);
		}

		/// Writes warning message $0 to the player log file. Use with caution, can impact performance at high frequency.
		[APIMethod]
		public static void LogWarning(int value)
		{
			LogWarningInternal(value);
		}

		/// Writes warning message $1 to the player log file. Use with caution, can impact performance at high frequency.
		/// If $2 is true, will also write to the in-game debug output.
		[APIMethod]
		public static void LogWarning(PinionContainer container, int value, bool displayInGame)
		{
			LogWarningInternal(value);

			if (displayInGame)
				container.LogWarning(value.ToString());
		}

		/// Writes warning message $0 to the player log file. Use with caution, can impact performance at high frequency.
		[APIMethod]
		public static void LogWarning(float value)
		{
			LogWarningInternal(value);
		}

		/// Writes warning message $1 to the player log file. Use with caution, can impact performance at high frequency.
		/// If $2 is true, will also write to the in-game debug output.
		[APIMethod]
		public static void LogWarning(PinionContainer container, float value, bool displayInGame)
		{
			LogWarningInternal(value);

			if (displayInGame)
				container.LogWarning(value.ToString());
		}

		/// Writes warning message $0 to the player log file. Use with caution, can impact performance at high frequency.
		[APIMethod]
		public static void LogWarning(bool value)
		{
			LogWarningInternal(value);
		}

		/// Writes warning message $1 to the player log file. Use with caution, can impact performance at high frequency.
		/// If $2 is true, will also write to the in-game debug output.
		[APIMethod]
		public static void LogWarning(PinionContainer container, bool value, bool displayInGame)
		{
			LogWarningInternal(value);

			if (displayInGame)
				container.LogWarning(value.ToString());
		}

		private static void LogWarningInternal(object value)
		{
			Debug.LogWarning(value);
		}
		#endregion

		#region LOGERROR
		/// Writes error message $0 to the player log file. Use with caution, can impact performance at high frequency.
		[APIMethod]
		public static void LogError(string message)
		{
			LogErrorInternal(message);
		}

		/// Writes error message $1 to the player log file. Use with caution, can impact performance at high frequency.
		/// If $2 is true, will also write to the in-game debug output.
		[APIMethod]
		public static void LogError(PinionContainer container, string message, bool displayInGame)
		{
			LogErrorInternal(message);

			if (displayInGame)
				container.LogError(message);
		}

		/// Writes error message $0 to the player log file. Use with caution, can impact performance at high frequency.
		[APIMethod]
		public static void LogError(int value)
		{
			LogErrorInternal(value);
		}

		/// Writes error message $1 to the player log file. Use with caution, can impact performance at high frequency.
		/// If $2 is true, will also write to the in-game debug output.
		[APIMethod]
		public static void LogError(PinionContainer container, int value, bool displayInGame)
		{
			LogErrorInternal(value);

			if (displayInGame)
				container.LogError(value.ToString());
		}

		/// Writes error message $0 to the player log file. Use with caution, can impact performance at high frequency.
		[APIMethod]
		public static void LogError(float value)
		{
			LogErrorInternal(value);
		}

		/// Writes error message $1 to the player log file. Use with caution, can impact performance at high frequency.
		/// If $2 is true, will also write to the in-game debug output.
		[APIMethod]
		public static void LogError(PinionContainer container, float value, bool displayInGame)
		{
			LogErrorInternal(value);

			if (displayInGame)
				container.LogError(value.ToString());
		}

		/// Writes error message $0 to the player log file. Use with caution, can impact performance at high frequency.
		[APIMethod]
		public static void LogError(bool value)
		{
			LogErrorInternal(value);
		}

		/// Writes error message $1 to the player log file. Use with caution, can impact performance at high frequency.
		/// If $2 is true, will also write to the in-game debug output.
		[APIMethod]
		public static void LogError(PinionContainer container, bool value, bool displayInGame)
		{
			LogErrorInternal(value);

			if (displayInGame)
				container.LogError(value.ToString());
		}

		private static void LogErrorInternal(object value)
		{
			Debug.LogError(value);
		}
		#endregion
	}
}