using System;
using UnityEngine;

namespace Ashkatchap.Updater.Behaviours {
	public abstract class AbstractUpdaterBehaviour : MonoBehaviour {
		private Action update, lateUpdate;

		public void SetQueues(Action update, Action lateUpdate) {
			this.update = update;
			this.lateUpdate = lateUpdate;
		}

		private void Update() {
			update();
		}

		private void LateUpdate() {
			lateUpdate();
		}
	}
}
