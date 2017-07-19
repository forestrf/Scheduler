using System;
using System.Reflection;
using UnityEngine;

namespace Ashkatchap.Shared {
	public static class Helpers {
		public static bool IsInLayerMask(this GameObject obj, LayerMask mask) {
			return ((mask.value & (1 << obj.layer)) > 0);
		}

		// http://answers.unity3d.com/questions/206665/typegettypestring-does-not-work-in-unity.html
		public static Type GetType(string TypeName) {

			// Try Type.GetType() first. This will work with types defined
			// by the Mono runtime, in the same assembly as the caller, etc.
			var type = Type.GetType(TypeName);

			// If it worked, then we're done here
			if (type != null)
				return type;

			// If the TypeName is a full name, then we can try loading the defining assembly directly
			if (TypeName.Contains(".")) {

				// Get the name of the assembly (Assumption is that we are using 
				// fully-qualified type names)
				var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));

				// Attempt to load the indicated Assembly
				var assembly = Assembly.Load(assemblyName);
				if (assembly == null)
					return null;

				// Ask that assembly to return the proper Type
				type = assembly.GetType(TypeName);
				if (type != null)
					return type;

			}

			// If we still haven't found the proper type, we can enumerate all of the 
			// loaded assemblies and see if any of them define the type
			var currentAssembly = Assembly.GetExecutingAssembly();
			var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
			foreach (var assemblyName in referencedAssemblies) {

				// Load the referenced assembly
				var assembly = Assembly.Load(assemblyName);
				if (assembly != null) {
					// See if that assembly defines the named type
					type = assembly.GetType(TypeName);
					if (type != null)
						return type;
				}
			}

			// The type just couldn't be found...
			return null;

		}
	}
}
