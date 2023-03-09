using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinion.Utility;
using System.IO;
using Pinion.ContainerMemory;

namespace Pinion.ExtendedContainers
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

		private const char metaDataSeparator = ':';

		// TODO: Make these configurable
		private ExecuteScheduling executeScheduling = ExecuteScheduling.InstantOnce;
		private ExecuteLoop executeLoop = ExecuteLoop.DontLoop;
		private float loopInterval = 2;
		private float sleepResumeTime = -1;
		private float lastExecuteTime = float.NegativeInfinity;       // any timestamp should be > than this value, so first iteration will always run
		private float lastExecuteTimeFixed = float.NegativeInfinity;  // any timestamp should be > than this value, so first iteration will always run

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
			UnityEventCaller.UnbindUpdate(SleepContinueHandler);
			lastExecuteTime = float.NegativeInfinity;
			lastExecuteTimeFixed = float.NegativeInfinity;
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
				DisplayError($"Invalid data in meta block.");
				return;
			}

			if (splitData.Length < 2)
			{
				DisplayError($"Invalid data in meta block. Data needs to follow format 'key:value'.");
				return;
			}

			string key = splitData[0].Trim();
			string value = splitData[1].Trim();

			if (splitData.Length > 2)
			{
				DisplayError($"Invalid data in meta block: {splitData[2]}");
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
							DisplayError($"Could not parse '{flag}' to a valid scheduling value.");
						}
					}
					break;

				case "loop":
					if (!System.Enum.TryParse<ExecuteLoop>(value, true, out executeLoop))
					{
						DisplayError($"Could not parse '{value}' to a valid loop type.");
					}
					break;

				case "interval":
					if (!float.TryParse(value, out loopInterval))
					{
						DisplayError($"Could not parse '{value}' to a valid loop interval value.");
					}
					break;

				default:
					DisplayError($"Invalid meta block content: '{key}:{value}'");
					break;
			}

			// Local method just to prevent some boilerplate
			void DisplayError(string message)
			{
				if (errorMessageReceiver != null)
					errorMessageReceiver(message);
			}
		}

		// This is called when a script instruction calls Sleep()

		protected override void OnSleep()
		{
			base.OnSleep();

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Debug.LogWarning("Can only use regular Sleep() instruction at edit time. Script will simply continue instead. This may have unintended consequences.");
				SleepContinueHandler();
				return;
			}
#endif

			// Passing a wrapper function, so we can (un)subscribe it as a unique item, instead of accidentally (un)subscribing RunInternal.
			UnityEventCaller.BindUpdate(SleepContinueHandler);
		}

		private void SleepContinueHandler()
		{

			// Time.time is weird in editor. Let's just always have succeed it immediately.
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				RunInternal();
				return;
			}
#endif
			if (Time.time >= sleepResumeTime) // If called without a time variable, this means next frame.
				RunInternal();
		}

		// This will be called the next time RunInternal is called, either because it was hooked to Update in OnSleep or because this script runs every Update.
		// Whichever happens first.
		protected override void OnSleepResume()
		{
			base.OnSleepResume();

#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			// Passing a wrapper function, so we can (un)subscribe it as a unique item, instead of accidentally (un)subscribing RunInternal.
			UnityEventCaller.UnbindUpdate(SleepContinueHandler);
		}

		public void SleepForTime(float seconds)
		{
			sleepResumeTime = Time.time + seconds;
			Sleep();
		}
	}
}
