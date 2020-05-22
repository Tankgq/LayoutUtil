namespace FarPlane {
	
	public class DataEventType : IEventType {

		/// <summary>
		/// 当前选择的 DisplayObject 变化了
		/// </summary>
		public const int SelectedChange = 1;
		/// <summary>
		/// 当前打开的 Module 改变
		/// </summary>
		public const int ChangeModule = 2;
		/// <summary>
		/// 当前的 ModuleName 被修改
		/// </summary>
		public const int CurrentModuleNameChanged = 3;
	}
}
