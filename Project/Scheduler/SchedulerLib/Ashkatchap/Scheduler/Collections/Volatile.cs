using System.Runtime.InteropServices;
using System.Threading;

namespace Ashkatchap.Scheduler.Collections {
	internal static class Volatile {
		private const int CacheLineSize = 64;
		private const int IntSize = 4;

		[StructLayout(LayoutKind.Explicit, Size = CacheLineSize * 2 - IntSize)]
		public struct PaddedVolatileInt {
			[FieldOffset(CacheLineSize - IntSize)]
			public int value;

			public PaddedVolatileInt(int value) {
				this.value = value;
			}

			public override string ToString() {
				return value.ToString();
			}
		}
	}
}
