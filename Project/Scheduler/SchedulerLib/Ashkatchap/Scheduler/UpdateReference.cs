namespace Ashkatchap.Scheduler {
	public struct UpdateReference {
		internal readonly long id;
		internal readonly byte order;

		public UpdateReference(long id, byte order) {
			this.id = id;
			this.order = order;
		}
	}
}
