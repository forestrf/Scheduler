using UnityEngine;
using System;
using System.Diagnostics;
using UnityEngine.SceneManagement;

namespace Ashkatchap.Shared {
	public static class UndoWrapper {
		public static Component AddComponent(GameObject gameObject, Type type) {
#if UNITY_EDITOR
			if (Application.isPlaying) return gameObject.AddComponent(type);
			else return UnityEditor.Undo.AddComponent(gameObject, type);
#else
			return gameObject.AddComponent(type);
#endif
		}

		public static T AddComponent<T>(GameObject gameObject) where T : Component {
#if UNITY_EDITOR
			if (Application.isPlaying) return gameObject.AddComponent<T>();
			else return UnityEditor.Undo.AddComponent<T>(gameObject);
#else
			return gameObject.AddComponent<T>();
#endif
		}
		
		[Conditional("UNITY_EDITOR")]
		public static void ClearUndo(UnityEngine.Object identifier) {
#if UNITY_EDITOR
			UnityEditor.Undo.ClearUndo(identifier);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void CollapseUndoOperations(int groupIndex) {
#if UNITY_EDITOR
			UnityEditor.Undo.CollapseUndoOperations(groupIndex);
#endif
		}

		/// <summary>
		/// Call Destry if Playing, otherwise call DestroyInmediate
		/// </summary>
		[Conditional("UNITY_EDITOR")]
		public static void DestroyObject(UnityEngine.Object objectToUndo) {
#if UNITY_EDITOR
			if (Application.isPlaying) UnityEngine.Object.Destroy(objectToUndo);
			else UnityEditor.Undo.DestroyObjectImmediate(objectToUndo);
#else
			UnityEngine.Object.Destroy(objectToUndo);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void FlushUndoRecordObjects() {
#if UNITY_EDITOR
			UnityEditor.Undo.FlushUndoRecordObjects();
#endif
		}

		public static int GetCurrentGroup() {
#if UNITY_EDITOR
			if (Application.isPlaying) return -1;
			else return UnityEditor.Undo.GetCurrentGroup();
#else
			return -1;
#endif
		}

		public static string GetCurrentGroupName() {
#if UNITY_EDITOR
			if (Application.isPlaying) return "";
			else return UnityEditor.Undo.GetCurrentGroupName();
#else
			return "";
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void IncrementCurrentGroup() {
#if UNITY_EDITOR
			UnityEditor.Undo.IncrementCurrentGroup();
#endif
		}

		public static void MoveGameObjectToScene(GameObject go, Scene scene, string name) {
#if UNITY_EDITOR
			if (Application.isPlaying) SceneManager.MoveGameObjectToScene(go, scene);
			else UnityEditor.Undo.MoveGameObjectToScene(go, scene, name);
#else
			SceneManager.MoveGameObjectToScene(go, scene);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void PerformRedo() {
#if UNITY_EDITOR
			UnityEditor.Undo.PerformRedo();
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void PerformUndo() {
#if UNITY_EDITOR
			UnityEditor.Undo.PerformUndo();
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void RecordObject(UnityEngine.Object objectToUndo, string name) {
#if UNITY_EDITOR
			UnityEditor.Undo.RecordObject(objectToUndo, name);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void RecordObjects(UnityEngine.Object[] objectsToUndo, string name) {
#if UNITY_EDITOR
			UnityEditor.Undo.RecordObjects(objectsToUndo, name);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void RegisterCompleteObjectUndo(UnityEngine.Object objectToUndo, string name) {
#if UNITY_EDITOR
			UnityEditor.Undo.RegisterCompleteObjectUndo(objectToUndo, name);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void RegisterCompleteObjectUndo(UnityEngine.Object[] objectsToUndo, string name) {
#if UNITY_EDITOR
			UnityEditor.Undo.RegisterCompleteObjectUndo(objectsToUndo, name);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void RegisterCreatedObjectUndo(UnityEngine.Object objectToUndo, string name) {
#if UNITY_EDITOR
			UnityEditor.Undo.RegisterCreatedObjectUndo(objectToUndo, name);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void RegisterFullObjectHierarchyUndo(UnityEngine.Object objectToUndo, string name) {
#if UNITY_EDITOR
			UnityEditor.Undo.RegisterFullObjectHierarchyUndo(objectToUndo, name);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void RevertAllDownToGroup(int group) {
#if UNITY_EDITOR
			UnityEditor.Undo.RevertAllDownToGroup(group);
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void RevertAllInCurrentGroup() {
#if UNITY_EDITOR
			UnityEditor.Undo.RevertAllInCurrentGroup();
#endif
		}

		[Conditional("UNITY_EDITOR")]
		public static void SetCurrentGroupName(string name) {
#if UNITY_EDITOR
			UnityEditor.Undo.SetCurrentGroupName(name);
#endif
		}
		
		public static void SetTransformParent(Transform transform, Transform newParent, string name) {
#if UNITY_EDITOR
			if (Application.isPlaying) transform.parent = newParent;
			else UnityEditor.Undo.SetTransformParent(transform, newParent, name);
#else
			transform.parent = newParent;
#endif
		}
	}
}
