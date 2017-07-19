#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Ashkatchap.Shared {
	public static class SerializedUtility {
		public static void Add(this Object obj, string arrayVariableName, Object addedElement) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(arrayVariableName);
			sp.InsertArrayElementAtIndex(sp.arraySize);
			sp.GetArrayElementAtIndex(sp.arraySize - 1).objectReferenceValue = addedElement;
			so.ApplyModifiedProperties();
		}

		// http://answers.unity3d.com/questions/555724/serializedpropertydeletearrayelementatindex-leaves.html
		public static void RemoveAt(this Object obj, string arrayVariableName, int indexToRemove) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(arrayVariableName);
			if (sp.GetArrayElementAtIndex(indexToRemove).objectReferenceValue != null)
				sp.DeleteArrayElementAtIndex(indexToRemove);
			sp.DeleteArrayElementAtIndex(indexToRemove);
			so.ApplyModifiedProperties();
		}

		public static void Remove(this Object obj, string arrayVariableName, Object objectToRemove) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(arrayVariableName);
			for (int i = 0; i < sp.arraySize; i++) {
				if (sp.GetArrayElementAtIndex(i).objectReferenceValue == objectToRemove) {
					if (sp.GetArrayElementAtIndex(i).objectReferenceValue != null)
						sp.DeleteArrayElementAtIndex(i);
					sp.DeleteArrayElementAtIndex(i);
				}
			}
			so.ApplyModifiedProperties();
		}

		public static void ClearArray(this Object obj, string arrayVariableName) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(arrayVariableName);
			sp.arraySize = 0;
			so.ApplyModifiedProperties();
		}

		private static void RemoveArrayElement(SerializedProperty sp, int index) {
			if (sp.GetArrayElementAtIndex(index).objectReferenceValue != null)
				sp.DeleteArrayElementAtIndex(index);
			sp.DeleteArrayElementAtIndex(index);
		}

		public static void SetValue(this Object obj, string variable, Object value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.objectReferenceValue = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, AnimationCurve value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.animationCurveValue = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, bool value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.boolValue = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, Bounds value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.boundsValue = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, Color value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.colorValue = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, double value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.doubleValue = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, float value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.floatValue = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, int value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.intValue = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, long value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.longValue = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, Quaternion value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.quaternionValue = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, Rect value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.rectValue = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, string value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.stringValue = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, Vector2 value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.vector2Value = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, Vector3 value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.vector3Value = value;
			so.ApplyModifiedProperties();
		}
		public static void SetValue(this Object obj, string variable, Vector4 value) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			sp.vector4Value = value;
			so.ApplyModifiedProperties();
		}

		public static void LayoutPropertyField(this Object obj, string variable, GUIContent label = null) {
			if (obj == null) return;
			var so = new SerializedObject(obj);
			var sp = so.FindProperty(variable);
			if (sp != null) {
				if (label != null)
					EditorGUILayout.PropertyField(sp, label, true);
				else
					EditorGUILayout.PropertyField(sp, true);
			} else {
				GUILayout.Label("Property " + variable + " not found", EditorStyles.label);
			}
			so.ApplyModifiedProperties();
		}
	}
}
#endif
