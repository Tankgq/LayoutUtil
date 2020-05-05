using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace FarPlane {
	
	public class SelectedChangeData : IEventData {
		
		public string ModuleName;
		public List<string> AddElements;
		public List<string> RemoveElements;

		public SelectedChangeData(string moduleName, List<string> addElements = null, List<string> removeElements = null) {
			ModuleName = moduleName;
			AddElements = addElements;
			RemoveElements = removeElements;
		}
	}
}
