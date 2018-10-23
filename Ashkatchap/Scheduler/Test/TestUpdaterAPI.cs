using System;
using UnityEngine;

namespace Ashkatchap.UnityScheduler.Tests {
	public class TestUpdaterAPI : MonoBehaviour {
		FrameUpdateReference[] nothingUpdate;

		public int arraySize = 10000;

		public int count = 0;

		Action DoNothingCached;
		private void Awake() {
			DoNothingCached = DoNothing;
		}

		private bool started = false;
		void OnEnable() {
			if (nothingUpdate == null || nothingUpdate.Length != arraySize) {
				nothingUpdate = new FrameUpdateReference[arraySize];
			}
			for (int i = 0; i < nothingUpdate.Length; i++) {
				nothingUpdate[i] = UpdaterAPI.AddUpdateCallback(DoNothingCached, QueueOrder.PostUpdate);
			}
			started = true;
		}
		void OnDisable() {
			for (int i = 0; i < nothingUpdate.Length; i++) {
				UpdaterAPI.RemoveUpdateCallback(nothingUpdate[i]);
			}
			started = false;
		}

		private void OnGUI() {
			GUILayout.Label("array Size");
			arraySize = int.Parse(GUILayout.TextField(arraySize.ToString()));
			if (!started && GUILayout.Button("START")) {
				OnEnable();
			}
			if (started && GUILayout.Button("END")) {
				OnDisable();
			}
		}

		void Update() {
			count = 0;
		}

		void DoNothing() {
			count++;
		}
	}
}
