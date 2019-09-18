using System.Collections.Generic;

public static class HistoryManager {
	private static readonly List<Behavior> Behaviors = new List<Behavior>();
	private static int _current;

	public static void Do(Behavior behavior) {
		if(behavior == null) return;
		Add(behavior);
		behavior.Do();
	}

	private static void Add(Behavior behavior) {
		if(behavior == null) return;
		if(Behaviors.Count == _current)
			Behaviors.Add(behavior);
		else
			Behaviors[_current] = behavior;
		++ _current;
	}

	public static void Do() {
		while(_current < Behaviors.Count) {
			Behavior behavior = Behaviors[_current];
			behavior.Do();
			++ _current;
			if(behavior.IsModify) break;
		}
	}

	public static void Undo() {
		while(_current > 0) {
			Behavior behavior = Behaviors[_current - 1];
			behavior.Undo();
			-- _current;
			if(behavior.IsModify) break;
		}
	}
}
