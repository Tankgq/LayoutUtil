namespace FarPlane {

	/// <summary>
	/// 最简单的事件的数据, 里面不包含数据
	/// </summary>
	public class TriggerEventData : IEventData {
		public static readonly TriggerEventData Default = new TriggerEventData();
	}
}
