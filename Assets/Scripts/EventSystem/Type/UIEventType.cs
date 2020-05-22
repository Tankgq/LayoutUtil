namespace FarPlane {
	
	public class UIEventType: IEventType {
		
		/// <summary>
		/// 图片切换
		/// </summary>
		public const int SwapImage = 1;
		/// <summary>
		/// 更新 Inspector 信息
		/// </summary>
		public const int UpdateInspectorInfo = 2;
		/// <summary>
		/// 更新窗口的标题
		/// </summary>
		public const int UpdateTitle = 3;
		/// <summary>
		/// 更新左下角的当前选择的 module 的宽度
		/// </summary>
		public const int UpdateModuleTxtWidth = 4;
		/// <summary>
		/// 更新 Hierarchy 面板
		/// </summary>
		public const int UpdateHierarchy = 5;
		/// <summary>
		/// 刷新 Hierarchy 面板中的 ModuleItem
		/// </summary>
		public const int RefreshModuleItem = 6;
	}
}
