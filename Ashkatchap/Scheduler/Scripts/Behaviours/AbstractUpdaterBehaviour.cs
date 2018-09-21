using System;
using UnityEngine;

namespace Ashkatchap.UnityScheduler.Behaviours {
	public abstract class AbstractUpdaterBehaviour : MonoBehaviour {
		private Action fixedUpdate, update, lateUpdate;

		public void SetQueues(Action fixedUpdate, Action update, Action lateUpdate) {
			this.fixedUpdate = fixedUpdate;
			this.update = update;
			this.lateUpdate = lateUpdate;
		}

		private void FixedUpdate() {
			fixedUpdate();
		}

		private void Update() {
			update();
		}

		private void LateUpdate() {
			lateUpdate();
		}
	}
}
