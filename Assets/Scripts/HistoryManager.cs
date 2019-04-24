using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
	public class HistoryManager
	{
		public static readonly List<int> _options = new List<int>();

		public const int TYPE_ADD_MODULE = 1;
		public const int SELECT_MODULE = 2;
		public const int ADD_DISPLAY_OBJECT = 3;
		public const int SELECT_DISPLAY_OBJECT = 4;
		public const int REMOVE_DISPLAY_OBJECT = 5;
	}
}
