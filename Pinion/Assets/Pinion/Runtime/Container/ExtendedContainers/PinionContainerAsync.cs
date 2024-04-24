using System.Collections.Generic;
using UnityEngine;
using Pinion.Utility;

namespace Pinion.ExtendedContainers
{
	// Supports "pause until async operation is done" logic.
	// This can be used to support Unity logic that returns asynchronously, while still treating it as "synchronous" for the purposes of the script itself.
	public class PinionContainerAsync : PinionContainer
	{
		// Small default capacity because there are presumably relatively few of these. Capacity will auto-expand if needed anyway.
		private List<System.Func<bool>> waitConditions = new List<System.Func<bool>>(8);
		private System.Func<string> progressMessageGetter = null;
		private string fixedProgressMessage = string.Empty;

		public string ProgressMessage
		{
			get
			{
				if (progressMessageGetter != null)
				{
					return progressMessageGetter();
				}
				else
				{
					return fixedProgressMessage;
				}
			}
		}

		public void SleepUntilDone(AsyncOperation asyncOperation, string message = null)
		{
			SleepWhile(() => !asyncOperation.isDone, message);
		}

		public void SleepWhile(System.Func<bool> condition, string message = null)
		{
			waitConditions.Add(condition);
			fixedProgressMessage = message;
			progressMessageGetter = null;
			Sleep();
		}

		public void SleepWhile(System.Func<bool> condition, System.Func<string> progressMessageGetter = null)
		{
			waitConditions.Add(condition);
			fixedProgressMessage = string.Empty;
			this.progressMessageGetter = progressMessageGetter;
			Sleep();
		}

		protected override void OnSleep()
		{
			base.OnSleep();

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Debug.LogWarning("At edit time, all Sleep commands behave as base Sleep(), regardless of container type. Script will continue here on the next run. This may have unintended consequences.");
				waitConditions.Clear();
				fixedProgressMessage = string.Empty;
				progressMessageGetter = null;
				return;
			}
#endif

			// Passing a wrapper function, so we can (un)subscribe it as a unique item, instead of accidentally (un)subscribing RunInternal.
			UnityEventCaller.BindUpdate(SleepContinueHandler);
		}

		private void SleepContinueHandler()
		{
			// Check for and remove any wait conditions that return false.
			for (int i = waitConditions.Count - 1; i >= 0; i--)
			{
				System.Func<bool> waitCondition = waitConditions[i];

				// Logic is "sleep while condition is true" so false means stop sleeping.
				if (waitCondition.Invoke() == false)
				{
					waitConditions.Remove(waitCondition);
				}
			}

			// If no more wait conditions, continue execution.
			if (waitConditions == null || waitConditions.Count < 1)
			{
				RunInternal();
			}
		}

		// This will be called the next time RunInternal is called.
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

		// It's been started and it's now no longer executing.
		public bool IsDone()
		{
			return HasStateFlag(InternalState.Initialized) && !HasStateFlag(InternalState.Executing);
		}
	}
}