using UnityEngine;
using System.Collections;
using System.Linq;

namespace Pinion.Utility
{
	public class UnityEventCaller : MonoBehaviour
	{
		private static UnityEventCaller instance = null;
		private static UnityEventCaller Instance
		{
			get
			{
#if UNITY_EDITOR
				if (!UnityEngine.Application.isPlaying)
				{
					throw new System.NotSupportedException("Can only be used at runtime, not in the editor.");
				}
#endif
				if (instance == null)
				{
					GameObject instanceGO = new GameObject("_UnityEventCaller");
					instance = instanceGO.AddComponent<UnityEventCaller>();
					instanceGO.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
					GameObject.DontDestroyOnLoad(instanceGO);  // Object will always persist.
				}

				return instance;
			}
		}


		private event System.Action onUpdate = null;
		private event System.Action onFixedUpdate = null;

		public static void BindUpdate(System.Action callback)
		{
			if (callback == null)
				return;

			// The easy way to ensure only one subscription.
			Instance.onUpdate -= callback;
			Instance.onUpdate += callback;
		}

		public static void UnbindUpdate(System.Action callback)
		{
			if (callback == null)
				return;

			Instance.onUpdate -= callback;
		}

		public static void BindFixedUpdate(System.Action callback)
		{
			if (callback == null)
				return;

			// The easy way to ensure only one subscription.
			Instance.onFixedUpdate -= callback;
			Instance.onFixedUpdate += callback;
		}

		public static void UnbindFixedUpdate(System.Action callback)
		{
			if (callback == null)
				return;

			Instance.onFixedUpdate -= callback;
		}

		private void Update()
		{
			if (onUpdate != null)
				onUpdate();
		}

		private void FixedUpdate()
		{
			if (onFixedUpdate != null)
				onFixedUpdate();
		}

		// Normally this Monobehaviour not be destroyed between scenes, but should it happen anyway (or when exiting play mode): this cleanly clears all subscribed events.
		private void OnDestroy()
		{
			onUpdate = null;
			onFixedUpdate = null;
		}
	}
}

