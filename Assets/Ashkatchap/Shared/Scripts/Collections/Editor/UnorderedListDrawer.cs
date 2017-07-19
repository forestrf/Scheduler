using UnityEngine;
using UnityEditor;

namespace Ashkatchap.Shared.Collections {
	public class UnorderedListDrawer<T> : PropertyDrawer {
		bool visible = true;
		int lastSize = 18;
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return lastSize;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			int startY = (int) position.y;
			position.height = 18;
			if (visible = EditorGUI.Foldout(position, visible, label)) {
				EditorGUI.indentLevel++;

				position.y += position.height;
				int previousSize = property.FindPropertyRelative("Size").intValue;
				EditorGUI.DelayedIntField(position, property.FindPropertyRelative("Size"));
				int newSize = property.FindPropertyRelative("Size").intValue;
				if (newSize != previousSize) {
					int blockSize = property.FindPropertyRelative("step_increment").intValue;
					property.FindPropertyRelative("elements").arraySize = Mathf.CeilToInt(newSize / (float) blockSize) * blockSize;
				}
				for (int i = 0; i < property.FindPropertyRelative("Size").intValue; i++) {
					position.y += 18;
					EditorGUI.PropertyField(position, property.FindPropertyRelative("elements").GetArrayElementAtIndex(i), true);
					position.y += -18 + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("elements").GetArrayElementAtIndex(i));
				}

				EditorGUI.indentLevel--;
			}
			lastSize = (int) (position.y - startY + 18);
			//base.OnGUI(position, property, label);
		}
	}
}
