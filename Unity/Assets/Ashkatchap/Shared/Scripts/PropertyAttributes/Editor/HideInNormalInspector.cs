using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HideInNormalInspectorAttribute))]
public class HideInNormalInspector : PropertyDrawer {
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return -2;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { }
}
