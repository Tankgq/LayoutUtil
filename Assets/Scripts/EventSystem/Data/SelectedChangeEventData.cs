using System.Collections.Generic;

namespace FarPlane {
	public class SelectedChangeData : IEventData {
		public readonly string ModuleName;
		public readonly List<string> AddElements;
		public readonly List<string> RemoveElements;

		public SelectedChangeData(string moduleName, List<string> addElements = null, List<string> removeElements = null) {
			ModuleName = moduleName;
			AddElements = addElements;
			RemoveElements = removeElements;
		}
	}
}
