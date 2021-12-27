using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pinion.Utility;

namespace Pinion.Unity
{
	// Supports "pause until async operation is done" logic.
	// This can be used to support Unity logic that returns asynchronously, while still treating it as "synchronous" for the purposes of the script itself.
	public class PinionContainerAsync : PinionContainer
	{
		// Currently supports only waiting for one async operation at a time.
		// How to improve this? Complicated without losing some other guarantees.
		private AsyncOperation currentAsyncOperation = null;

		public string ProgressMessage
		{
			get;
			private set;
		}

		public void SleepUntilDone(AsyncOperation asyncOperation, string message = null)
		{
			currentAsyncOperation = asyncOperation;
			ProgressMessage = message;
			Sleep();
		}

		protected override void OnSleep()
		{
			base.OnSleep();

			// Passing a wrapper function, so we can (un)subscribe it as a unique item, instead of accidentally (un)subscribing RunInternal.
			UnityEventCaller.BindUpdate(SleepContinueHandler);
		}

		private void SleepContinueHandler()
		{
			if (currentAsyncOperation == null || currentAsyncOperation.isDone)
			{
				currentAsyncOperation = null;
				RunInternal();
			}
		}

		// This will be called the next time RunInternal is called.
		protected override void OnSleepResume()
		{
			base.OnSleepResume();

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
