using UnityEngine;

namespace Ashkatchap.Annotation {
	[ExecuteInEditMode]
	public class Annotation : MonoBehaviour {
		public string text;

		private void Awake() {
			this.hideFlags = HideFlags.DontSaveInBuild;
		}
	}
}
