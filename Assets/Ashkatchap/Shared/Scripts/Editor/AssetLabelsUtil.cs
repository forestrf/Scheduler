using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ashkatchap.Shared {
	public static class AssetLabelsUtil {
		public static void AddAssetLabel(GameObject obj, string label) {
			var lst = new HashSet<string>(AssetDatabase.GetLabels(obj));
			if (!lst.Contains(label)) lst.Add(label);
			string[] finalLabels = new string[lst.Count];
			lst.CopyTo(finalLabels);
			AssetDatabase.SetLabels(obj, finalLabels);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(obj));
		}

		public static void RemoveAssetLabel(GameObject obj, string label) {
			var lst = new HashSet<string>(AssetDatabase.GetLabels(obj));
			if (lst.Contains(label)) lst.Remove(label);
			string[] finalLabels = new string[lst.Count];
			lst.CopyTo(finalLabels);
			AssetDatabase.SetLabels(obj, finalLabels);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(obj));
		}
	}
}
