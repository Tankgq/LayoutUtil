using System;

public class Behavior {
	
	public readonly Action Do;
	public readonly Action Undo;
	public readonly bool IsModify;

	public Behavior(Action doBehavior, Action undoBehavior, bool isModify = true) {
		Do = doBehavior;
		Undo = undoBehavior;
		IsModify = isModify;
	}
}