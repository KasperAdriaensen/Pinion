using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinion.Utility;
using System.IO;

namespace Pinion
{
	public class PinionContainerLooping : PinionContainer
	{
		// For a looping script, InstantOnce doesn't really make sense, but this way,
		// the child class can still acts as an extension of the base class, rather than being a loop-only "replacement".
		// "Never" is simply there to facilitate flag checking (InstantOnce == 0 would always trigger.)
		[System.Flags]
		public enum ExecuteScheduling
		{
			Never = 0,
			InstantOnce = 1,
			Update = 2,
			FixedUpdate = 4
		}

		public enum ExecuteLoop
		{
			DontLoop = 0,
			EveryTime = 1,
			TimedInterval = 2,
			TimedIntervalUnscaled = 3
		}

		// TODO: Make these configurable
		private ExecuteScheduling executeScheduling = ExecuteScheduling.InstantOnce;
		private ExecuteLoop executeLoop = ExecuteLoop.DontLoop;
		private double loopInterval = 2;

		private double lastExecuteTime = double.NegativeInfinity;       // any timestamp should be > than this value, so first iteration will always run
		private double lastExecuteTimeFixed = double.NegativeInfinity;  // any timestamp should be > than this value, so first iteration will always run
		private const char metaDataSeparator = ':';


		public override void Run(System.Action<LogType, string> logHandler = null, params System.ValueTuple<string, object>[] externalVariables)
		{
			this.logHandler = logHandler;
			this.externalVariables = externalVariables;

			if (HasSchedulingFlag(ExecuteScheduling.InstantOnce))
				RunInternal();

			if (HasSchedulingFlag(ExecuteScheduling.Update))
				UnityEventCaller.BindUpdate(OnUpdate);

			if (HasSchedulingFlag(ExecuteScheduling.FixedUpdate))
				UnityEventCaller.BindFixedUpdate(OnFixedUpdate);
		}

		public override void Stop()
		{
			base.Stop();
			UnityEventCaller.UnbindUpdate(OnUpdate);
			UnityEventCaller.UnbindFixedUpdate(OnFixedUpdate);
			lastExecuteTime = double.NegativeInfinity;
			lastExecuteTimeFixed = double.NegativeInfinity;
		}

		private void OnUpdate()
		{
			if (executeLoop == ExecuteLoop.TimedInterval)
			{
				if (Time.time >= lastExecuteTime + loopInterval)
					lastExecuteTime = Time.time;
				else
					return;
			}

			if (executeLoop == ExecuteLoop.TimedIntervalUnscaled)
			{
				if (Time.unscaledTime >= lastExecuteTime + loopInterval)
					lastExecuteTime = Time.unscaledTime;
				else
					return;
			}

			RunInternal();

			if (executeLoop == ExecuteLoop.DontLoop)
				UnityEventCaller.UnbindUpdate(OnUpdate);
		}

		private void OnFixedUpdate()
		{
			if (executeLoop == ExecuteLoop.TimedInterval)
			{
				if (Time.fixedTime >= lastExecuteTimeFixed + loopInterval)
					lastExecuteTimeFixed = Time.fixedTime;
				else
					return;
			}

			if (executeLoop == ExecuteLoop.TimedIntervalUnscaled)
			{
				if (Time.fixedUnscaledTime >= lastExecuteTimeFixed + loopInterval)
					lastExecuteTimeFixed = Time.fixedUnscaledTime;
				else
					return;
			}

			RunInternal();

			if (executeLoop == ExecuteLoop.DontLoop)
				UnityEventCaller.UnbindFixedUpdate(OnFixedUpdate);
		}

		private bool HasSchedulingFlag(ExecuteScheduling flag)
		{
			return (executeScheduling & flag) == flag;
		}

		public override void ParseMetaData(string metaData, System.Action<string> errorMessageReceiver)
		{
			base.ParseMetaData(metaData, errorMessageReceiver);

			Debug.Log(metaData);

			if (metaData == null)
				throw new System.ArgumentNullException(nameof(metaData));


			using (StringReader reader = new StringReader(metaData))
			{
				string line = string.Empty;
				string[] splitData = null;

				do
				{
					line = reader.ReadLine();

					if (string.IsNullOrEmpty(line)) // Ignore blank lines or end of file.
						continue;

					if (line.StartsWith("//"))
						continue;

					splitData = line.Split(metaDataSeparator);
					ParseMetaDataLine(splitData, errorMessageReceiver);
				}
				while (line != null);
			}
		}


		private void ParseMetaDataLine(string[] splitData, System.Action<string> errorMessageReceiver)
		{
			if (splitData == null)
			{
				DisplayError(errorMessageReceiver, $"Invalid data in meta block.");
				return;
			}

			if (splitData.Length < 2)
			{
				DisplayError(errorMessageReceiver, $"Invalid data in meta block. Data needs to follow format 'key:value'.");
				return;
			}

			string key = splitData[0].Trim();
			string value = splitData[1].Trim();

			if (splitData.Length > 2)
			{
				DisplayError(errorMessageReceiver, $"Invalid data in meta block: {splitData[2]}");
				return;
			}

			switch (key)
			{
				case "scheduling":

					string[] flags = value.Split(',');

					// sensible default so it doesn't have to be explicitly listed to just run once
					executeScheduling = ExecuteScheduling.InstantOnce;

					ExecuteScheduling parsedFlag = ExecuteScheduling.Never;

					for (int i = 0; i < flags.Length; i++)
					{
						string flag = flags[i];

						if (System.Enum.TryParse<ExecuteScheduling>(flag, true, out parsedFlag))
						{
							executeScheduling |= parsedFlag;
						}
						else
						{
							DisplayError(errorMessageReceiver, $"Could not parse '{flag}' to a valid scheduling value.");
						}
					}
					break;

				case "loop":
					if (!System.Enum.TryParse<ExecuteLoop>(value, true, out executeLoop))
					{
						DisplayError(errorMessageReceiver, $"Could not parse '{value}' to a valid loop type.");
					}
					break;

				case "interval":
					if (!double.TryParse(value, out loopInterval))
					{
						DisplayError(errorMessageReceiver, $"Could not parse '{value}' to a valid loop interval value.");
					}
					break;

				default:
					DisplayError(errorMessageReceiver, $"Invalid meta block content: '{key}:{value}'");
					break;
			}
		}

		// This is called when a script instruction calls Sleep()

		protected override void OnSleep()
		{
			base.OnSleep();

			// Passing a wrapper function, so we can (un)subscribe it as a unique item, instead of accidentally (un)subscribing RunInternal.
			UnityEventCaller.BindUpdate(SleepContinueHandler);
		}

		// This will be called the next time RunInternal is called, either because it was hooked to Update in OnSleep or because this script runs every Update.
		// Whichever happens first.
		protected override void OnSleepResume()
		{
			base.OnSleepResume();

			// Passing a wrapper function, so we can (un)subscribe it as a unique item, instead of accidentally (un)subscribing RunInternal.
			UnityEventCaller.UnbindUpdate(SleepContinueHandler);
		}

		private void SleepContinueHandler()
		{
			RunInternal();
		}

		private void DisplayError(System.Action<string> errorMessageReceiver, string message)
		{
			if (errorMessageReceiver != null)
				errorMessageReceiver(message);
		}
	}
}

