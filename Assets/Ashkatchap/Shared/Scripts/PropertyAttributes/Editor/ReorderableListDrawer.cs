using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomPropertyDrawer(typeof(SimpleReorderableList), true)]
public class ReorderableListDrawer : PropertyDrawer {
	private ReorderableList list;
	
	private ReorderableList getList(SerializedProperty property) {
		if (list == null) {
			list = new ReorderableList(property.serializedObject, property, true, true, true, true);
			list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
				rect.width -= 40;
				rect.x += 20;
				EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index), true);
			};
		}
		return list;
	}
	
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
		return getList(property.FindPropertyRelative("List")).GetHeight();
	}
	
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		var listProperty = property.FindPropertyRelative("List");
		var list = getList(listProperty);
		var height = 0f;
		for (var i = 0; i < listProperty.arraySize; i++) {
			height = Mathf.Max(height, EditorGUI.GetPropertyHeight(listProperty.GetArrayElementAtIndex(i)));
		}
		list.elementHeight = height;
		list.DoList(position);
	}
}
