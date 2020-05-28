namespace FarPlane {
	public class ChangeModuleEventData : IEventData {
		public readonly string Module;

		public ChangeModuleEventData(string module) {
			Module = module;
		}
	}
}
