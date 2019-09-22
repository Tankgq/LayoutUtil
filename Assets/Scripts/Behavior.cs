using System;

public class Behavior {
	// 参数为 isRedo
	public readonly Action<bool> Do;
	public int DoCount;

	// 参数为 isReUndo
	public readonly Action<bool> Undo;
	public int UndoCount;

	// 是否要和
	public readonly bool IsCombineWithNextBehavior;

	public Behavior(Action<bool> doBehavior, Action<bool> undoBehavior, bool combineWithNextBehavior = false) {
		Do = doBehavior;
		Undo = undoBehavior;
		DoCount = UndoCount = 0;
		IsCombineWithNextBehavior = combineWithNextBehavior;
	}
}
