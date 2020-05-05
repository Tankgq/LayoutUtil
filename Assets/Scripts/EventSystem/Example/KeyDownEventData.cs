namespace FarPlane {
	
	public class KeyDownEventData : IEventData {
		public long KeyDownTime { get; }

		public KeyDownEventData(long keyDownTime) {
			KeyDownTime = keyDownTime;
		}
	}
}
