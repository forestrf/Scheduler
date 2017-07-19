using System;
using UnityEngine;

namespace Ashkatchap.Shared {
	public static class FastMath {
		/// <summary>
		/// Replacement for Vector3.Distance.
		/// </summary>
		public static float Distance(ref Vector3 a, ref Vector3 b) {
			return (float) Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z));
		}
		/// <summary>
		/// Replacement for Vector3.Distance. Faster with ref
		/// </summary>
		public static float Distance(Vector3 a, Vector3 b) {
			return (float) Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z));
		}

		/// <summary>
		/// Replacement for Vector2.Distance.
		/// </summary>
		public static float Distance(ref Vector2 a, ref Vector2 b) {
			return (float) Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
		}
		/// <summary>
		/// Replacement for Vector2.Distance. Faster with ref
		/// </summary>
		public static float Distance(Vector2 a, Vector2 b) {
			return (float) Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
		}

		/// <summary>
		/// Replacement for Vector3.Distance but without sqrt.
		/// </summary>
		public static float SqrDistance(ref Vector3 a, ref Vector3 b) {
			return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
		}
		/// <summary>
		/// Replacement for Vector3.Distance but without sqrt. Faster with ref
		/// </summary>
		public static float SqrDistance(Vector3 a, Vector3 b) {
			return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y) + (a.z - b.z) * (a.z - b.z);
		}
		
		public static float Pow(float num, uint exp) {
			float result = 1;
			while (exp > 0) {
				if (exp % 2 == 1) result *= num;
				exp >>= 1;
				num *= num;
			}

			return result;
		}

		public static Vector3 Multiply(ref Quaternion rotation, ref Vector3 point) {
			float num = rotation.x * 2f;
			float num2 = rotation.y * 2f;
			float num3 = rotation.z * 2f;
			float num4 = rotation.x * num;
			float num5 = rotation.y * num2;
			float num6 = rotation.z * num3;
			float num7 = rotation.x * num2;
			float num8 = rotation.x * num3;
			float num9 = rotation.y * num3;
			float num10 = rotation.w * num;
			float num11 = rotation.w * num2;
			float num12 = rotation.w * num3;
			Vector3 result;
			result.x = (1f - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z;
			result.y = (num7 + num12) * point.x + (1f - (num4 + num6)) * point.y + (num9 - num10) * point.z;
			result.z = (num8 - num11) * point.x + (num9 + num10) * point.y + (1f - (num4 + num5)) * point.z;
			return result;
		}

		public static Vector3 MultiplyPoint3x4(ref Matrix4x4 m, ref Vector3 v) {
			Vector3 result;
			result.x = m.m00 * v.x + m.m01 * v.y + m.m02 * v.z + m.m03;
			result.y = m.m10 * v.x + m.m11 * v.y + m.m12 * v.z + m.m13;
			result.z = m.m20 * v.x + m.m21 * v.y + m.m22 * v.z + m.m23;
			return result;
		}

		public static float Abs(float value) {
			return value >= 0 ? value : -value;
		}
	}
}
