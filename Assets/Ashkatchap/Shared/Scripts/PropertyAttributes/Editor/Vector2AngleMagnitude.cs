using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Vector2AngleMagnitudeAttribute))]
public class Vector2AngleMagnitude : PropertyDrawer {
	private static Vector2 mousePosition;
	private static Texture2D Knob = Resources.Load("knob") as Texture2D;
	private static float height = 60;

	private Vector2AngleMagnitudeAttribute angleAttribute {
		get { return (Vector2AngleMagnitudeAttribute) attribute; }
	}

	public override void OnGUI(Rect r, SerializedProperty property, GUIContent label) {
		r.x += EditorGUI.indentLevel * 15;
		r.width -= EditorGUI.indentLevel * 15;
		GUI.Label(r, label);
		r.y += 20;
		property.vector2Value = AngleToVector2(
			FloatAngle(r, Vector2ToAngle(property.vector2Value), angleAttribute.snap)
		, EditorGUI.FloatField(new Rect(r.x + 36, r.y + 20, r.width - 36, 18), new GUIContent("Speed"), Mathf.Round(property.vector2Value.magnitude * 10000) / 10000));
	}

	public static Vector2 AngleToVector2(float angle, float magnitude) {
		return new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * magnitude;
	}

	public static float Vector2ToAngle(Vector2 v) {
		Vector2 b = v.normalized;
		Vector2 a = new Vector2(0, 1);
		float angle = Mathf.Atan2(a.x * b.y - a.y * b.x, a.x * b.x + a.y * b.y) * Mathf.Rad2Deg + 90;
		if (angle < 0) angle += 360;
		if (angle == 360) angle = 0;
		return angle;
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return height;
	}

	public static float FloatAngle(Rect rect, float value, float snap) {
		int id = GUIUtility.GetControlID(FocusType.Passive, rect);

		Rect knobRect = new Rect(rect.x + 4, rect.y + 2, 32, 32);

		float delta = 1;

		if (Event.current != null) {
			if (Event.current.type == EventType.MouseDown && knobRect.Contains(Event.current.mousePosition)) {
				GUIUtility.hotControl = id;
				mousePosition = Event.current.mousePosition;
			} else if (Event.current.type == EventType.MouseUp && GUIUtility.hotControl == id)
				GUIUtility.hotControl = 0;
			else if (Event.current.type == EventType.MouseDrag && GUIUtility.hotControl == id) {
				Vector2 move = mousePosition - Event.current.mousePosition;
				value += delta * (-move.x - move.y);

				if (snap > 0) {
					float mod = value % snap;

					if (mod < (delta * 3) || Mathf.Abs(mod - snap) < (delta * 3))
						value = Mathf.Round(value / snap) * snap;
				}

				mousePosition = Event.current.mousePosition;
				GUI.changed = true;
			}
		}
		
		Matrix4x4 matrix = GUI.matrix;

		GUIUtility.RotateAroundPivot(-value, knobRect.center);

		GUI.DrawTexture(knobRect, Knob);
		GUI.matrix = matrix;

		Rect label = new Rect(rect.x + 36, rect.y, rect.width - 36, 18);
		value = EditorGUI.FloatField(label, new GUIContent("Degrees"), Mathf.Round(value * 10000) / 10000);

		return value;
	}
}
