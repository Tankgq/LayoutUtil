using System.Collections.Generic;

public static class HistoryManager {
	private static readonly List<Behavior> Behaviors = new List<Behavior>();
	private static int _currentIndex = 0;

	public static void Do(Behavior behavior) {
		if(behavior == null) return;
		Add(behavior);
		behavior.Do(false);
	}

	private static void Add(Behavior behavior) {
		if(behavior == null) return;
		if(Behaviors.Count > _currentIndex) return;
		if(Behaviors.Count == _currentIndex) {
			Behaviors.Add(behavior);
			++ _currentIndex;
			return;
		}
		Behaviors[_currentIndex] = behavior;
	}

	public static void Do() {
		while(true) {
			if(_currentIndex >= Behaviors.Count) return;
			Behavior behavior = Behaviors[_currentIndex];
			behavior.Do(behavior.DoCount > 0);
			++ behavior.DoCount;
			++ _currentIndex;
			if(behavior.IsCombineWithNextBehavior) continue;
			break;
		}
	}

	public static void Undo() {
		while(true) {
			if(_currentIndex < 1) return;
			Behavior behavior = Behaviors[_currentIndex - 1];
			behavior.Undo(behavior.UndoCount > 0);
			++ behavior.UndoCount;
			-- _currentIndex;
			if(_currentIndex < 1) return;
			behavior = Behaviors[_currentIndex - 1];
			if(behavior.IsCombineWithNextBehavior) continue;
			break;
		}
	}
}
