using UnityEditor;

namespace Ashkatchap.Annotation {
	[CustomEditor(typeof(Annotation))]
	public class AnnotationEditor : Editor {
		SerializedProperty text;

		void OnEnable() {
			text = serializedObject.FindProperty("text");
		}
		
		public override void OnInspectorGUI() {
			EditorGUILayout.HelpBox("Annotations are stripped from builds", MessageType.Info);

			serializedObject.Update();
			text.stringValue = EditorGUILayout.TextArea(text.stringValue);
			serializedObject.ApplyModifiedProperties();
		}
	}
}
