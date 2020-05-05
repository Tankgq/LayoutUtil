namespace FarPlane {
	
	public class SwapImageEventData: IEventData {
		
		public string ModuleName;
		public string ElementName;
		public bool IsSwap;
		
		public SwapImageEventData(string moduleName, string elementName, bool isSwap) {
			ModuleName = moduleName;
			ElementName = elementName;
			IsSwap = isSwap;
		}
	}
}
