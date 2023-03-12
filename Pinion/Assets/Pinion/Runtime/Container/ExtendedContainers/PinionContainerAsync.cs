using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinion.Utility;

namespace Pinion.ExtendedContainers
{
	// Supports "pause until async operation is done" logic.
	// This can be used to support Unity logic that returns asynchronously, while still treating it as "synchronous" for the purposes of the script itself.
	public class PinionContainerAsync : PinionContainer
	{
		private List<System.Func<bool>> waitConditions = new List<System.Func<bool>>();

		public string ProgressMessage
		{
			get;
			private set;
		}

		public void SleepUntilDone(AsyncOperation asyncOperation, string message = null)
		{
			SleepWhile(() => !asyncOperation.isDone, message);
		}

		public void SleepWhile(System.Func<bool> condition, string message = null)
		{
			waitConditions.Add(condition);
			ProgressMessage = message;
			Sleep();
		}

		protected override void OnSleep()
		{
			base.OnSleep();

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				Debug.LogWarning("Can only use regular Sleep() instruction at edit time. Script will simply continue instead. This may have unintended consequences.");
				waitConditions.Clear();
				SleepContinueHandler();
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
