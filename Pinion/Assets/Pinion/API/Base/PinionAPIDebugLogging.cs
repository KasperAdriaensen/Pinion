using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pinion
{
	[APISource]
	public static class PinionAPIDebugLogging
	{
		[APIMethod]
		public static void Log(string message)
		{
			LogInternal(message);
		}

		[APIMethod]
		public static void Log(int value)
		{
			LogInternal(value);
		}

		[APIMethod]
		public static void Log(float value)
		{
			LogInternal(value);
		}

		[APIMethod]
		public static void Log(bool value)
		{
			LogInternal(value);
		}

		private static void LogInternal(object value)
		{
			Debug.Log(value);
		}
	}
}