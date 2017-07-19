using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ashkatchap.Shared {
	/// <summary>
	/// Detect hot reloading thanks to the fact that componentes are enabled and disabled when it happens
	/// </summary>
	public class HotReloadDetector : MonoBehaviour {
		static HotReloadDetector _instance;
		bool ready = false;

		void Awake() {
			if (_instance != null && _instance != this) {
				Destroy(this);
				return;
			}
			_instance = this;

			ready = true;
			DontDestroyOnLoad(this);
		}

		void OnEnable() {
			Awake();
			if (ready) {
				foreach (var c in subscriptionsAfter) {
					c.Invoke();
				}
			}
		}
		void OnDisable() {
			if (ready) {
				foreach (var c in subscriptionsBefore) {
					c.Invoke();
				}
			}
		}

		/////////////////////////////////////////////////
		// SUBSCRIPTIONS
		/////////////////////////////////////////////////

		static HashSet<Action> subscriptionsBefore = new HashSet<Action>();
		static HashSet<Action> subscriptionsAfter = new HashSet<Action>();

		public static void Subscribe(Action before, Action after) {
			if (subscriptionsBefore.Contains(before)) return;
			if (_instance == null) {
				var go = new GameObject("HotReloadDetector (Automatically created)");
				go.hideFlags = HideFlags.DontSave;
				go.AddComponent<HotReloadDetector>();
			}
			subscriptionsBefore.Add(before);
			subscriptionsAfter.Add(after);
		}
		public static void Unsubscribe(Action before, Action after) {
			if (!subscriptionsBefore.Contains(before)) return;
			subscriptionsBefore.Remove(before);
			subscriptionsAfter.Remove(after);
		}
	}
}
