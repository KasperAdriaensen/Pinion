using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion
{
	[APISource]
	public static class PinionAPIDebugLogging
	{
		#region LOG
		[APIMethod]
		public static void Log(string message)
		{
			LogInternal(message);
		}

		[APIMethod]
		public static void Log(PinionContainer container, string message, bool displayInGame)
		{
			LogInternal(message);

			if (displayInGame)
				container.Log(message);
		}

		[APIMethod]
		public static void Log(int value)
		{
			LogInternal(value);
		}

		[APIMethod]
		public static void Log(PinionContainer container, int value, bool displayInGame)
		{
			LogInternal(value);

			if (displayInGame)
				container.Log(value.ToString());
		}

		[APIMethod]
		public static void Log(float value)
		{
			LogInternal(value);
		}

		[APIMethod]
		public static void Log(PinionContainer container, float value, bool displayInGame)
		{
			LogInternal(value);

			if (displayInGame)
				container.Log(value.ToString());
		}

		[APIMethod]
		public static void Log(bool value)
		{
			LogInternal(value);
		}

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
		[APIMethod]
		public static void LogWarning(string message)
		{
			LogWarningInternal(message);
		}

		[APIMethod]
		public static void LogWarning(PinionContainer container, string message, bool displayInGame)
		{
			LogWarningInternal(message);

			if (displayInGame)
				container.LogWarning(message);
		}

		[APIMethod]
		public static void LogWarning(int value)
		{
			LogWarningInternal(value);
		}

		[APIMethod]
		public static void LogWarning(PinionContainer container, int value, bool displayInGame)
		{
			LogWarningInternal(value);

			if (displayInGame)
				container.LogWarning(value.ToString());
		}

		[APIMethod]
		public static void LogWarning(float value)
		{
			LogWarningInternal(value);
		}

		[APIMethod]
		public static void LogWarning(PinionContainer container, float value, bool displayInGame)
		{
			LogWarningInternal(value);

			if (displayInGame)
				container.LogWarning(value.ToString());
		}

		[APIMethod]
		public static void LogWarning(bool value)
		{
			LogWarningInternal(value);
		}

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
		[APIMethod]
		public static void LogError(string message)
		{
			LogErrorInternal(message);
		}

		[APIMethod]
		public static void LogError(PinionContainer container, string message, bool displayInGame)
		{
			LogErrorInternal(message);

			if (displayInGame)
				container.LogError(message);
		}

		[APIMethod]
		public static void LogError(int value)
		{
			LogErrorInternal(value);
		}

		[APIMethod]
		public static void LogError(PinionContainer container, int value, bool displayInGame)
		{
			LogErrorInternal(value);

			if (displayInGame)
				container.LogError(value.ToString());
		}

		[APIMethod]
		public static void LogError(float value)
		{
			LogErrorInternal(value);
		}

		[APIMethod]
		public static void LogError(PinionContainer container, float value, bool displayInGame)
		{
			LogErrorInternal(value);

			if (displayInGame)
				container.LogError(value.ToString());
		}

		[APIMethod]
		public static void LogError(bool value)
		{
			LogErrorInternal(value);
		}

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