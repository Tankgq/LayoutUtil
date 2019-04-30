using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
	public class Behavior
	{
		public Action Do;
		public Action Undo;

		public Behavior(Action doBehavior, Action undoBehavior)
		{
			this.Do = doBehavior;
			this.Undo = undoBehavior;
		}
	}
}