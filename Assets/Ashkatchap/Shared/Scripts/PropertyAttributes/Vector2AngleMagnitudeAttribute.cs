using UnityEngine;

public class Vector2AngleMagnitudeAttribute : PropertyAttribute {
	public readonly float snap;

	public Vector2AngleMagnitudeAttribute() {
		snap = 1;
	}

	public Vector2AngleMagnitudeAttribute(float snap) {
		this.snap = snap;
	}
}
