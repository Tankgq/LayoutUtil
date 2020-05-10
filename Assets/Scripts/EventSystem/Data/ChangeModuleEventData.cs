namespace FarPlane {
	public class ChangeModuleEventData : IEventData {

		public string Module;

		public ChangeModuleEventData(string module) {
			Module = module;
		}
	}
}
