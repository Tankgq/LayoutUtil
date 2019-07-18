using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
	public class HistoryManager
	{
		public static readonly List<Behavior> _behaviors = new List<Behavior>();
		public static int _current = 0;

		public static void Do(Behavior behavior)
		{
			if (behavior == null) return;
			Add(behavior);
			behavior.Do();
		}

		public static void Add(Behavior behavior)
		{
			if (behavior == null) return;
			if (_behaviors.Count == _current) _behaviors.Add(behavior);
			else _behaviors[_current] = behavior;
			++_current;
		}

		public static void Do()
		{
			if (_current >= _behaviors.Count) return;
			Behavior behavior = _behaviors[_current];
			++_current;
			behavior.Do();
		}

		public static void Undo()
		{
			if (_current > _behaviors.Count) return;
			if (_current <= 0) return;
			Behavior behavior = _behaviors[_current - 1];
			--_current;
			behavior.Undo();
		}
	}
}
