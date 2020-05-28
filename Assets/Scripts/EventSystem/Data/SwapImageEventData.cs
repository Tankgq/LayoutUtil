namespace FarPlane {
	public class SwapImageEventData : IEventData {
		public readonly string ModuleName;
		public readonly string ElementName;
		public readonly bool IsSwap;

		public SwapImageEventData(string moduleName, string elementName, bool isSwap) {
			ModuleName = moduleName;
			ElementName = elementName;
			IsSwap = isSwap;
		}
	}
}
