using System;

namespace Assets.Scripts
{
	public class AlignInfo
	{
		public delegate float GetAction(Rectangle rect);
		public delegate void SetAction(Rectangle rect, float value);
		public static readonly GetAction GetLeft = (Rectangle rect) => rect.Left;
		public static readonly GetAction GetRight = (Rectangle rect) => rect.Right;
		public static readonly GetAction GetTop = (Rectangle rect) => rect.Top;
		public static readonly GetAction GetBottom = (Rectangle rect) => rect.Bottom;

		public const int ALIGN_LEFT = 1;
		public const int ALIGN_RIGHT = 2;
		public const int ALIGN_TOP = 3;
		public const int ALIGN_BOTTOM = 4;

		private float _closeValue;
		private float _lineThickness;
		private Rectangle _targetRect;
		private float _curHorizontalCloseValue;
		private float _curVerticalCloseValue;
		public GetAction HorizontalAlignAction;
		public GetAction VerticalAlignAction;
		private Rectangle _horizontalAlignLine;
		private Rectangle _verticalAlignLine;
		public int HorizontalAlignType;
		public int VerticalAlignType;

		public AlignInfo(Rectangle targetRect, float closeValue, float lineThickness)
		{
			_targetRect = targetRect;
			_closeValue = closeValue;
			_lineThickness = lineThickness;
			_curHorizontalCloseValue = _closeValue + 1;
			_curVerticalCloseValue = _closeValue + 1;
			HorizontalAlignAction = null;
			VerticalAlignAction = null;
			HorizontalAlignType = 0;
			VerticalAlignType = 0;
			_horizontalAlignLine = new Rectangle();
			_verticalAlignLine = new Rectangle();
		}

		public Rectangle HorizontalAlignLine
		{
			get
			{
				if (_curHorizontalCloseValue > _closeValue)
					return null;
				return _horizontalAlignLine;
			}
		}

		public Rectangle VerticalAlignLine
		{
			get
			{
				if (_curVerticalCloseValue > _closeValue)
					return null;
				return _verticalAlignLine;
			}
		}

		public void Merge(Rectangle rect)
		{
			MergeHorizontal(rect);
			MergeVertical(rect);
		}

		public void MergeHorizontal(Rectangle rect)
		{
			GetAction selfAction = GetTop, action = GetTop;
			int horizontalAlignType = ALIGN_TOP;
			float minValue = Math.Abs(_targetRect.Top - rect.Top);
			float value = Math.Abs(_targetRect.Top - rect.Bottom);
			if (minValue > value)
			{
				horizontalAlignType = ALIGN_TOP;
				selfAction = GetTop;
				action = GetBottom;
				minValue = value;
			}
			value = Math.Abs(_targetRect.Bottom - rect.Top);
			if (minValue > value)
			{
				horizontalAlignType = ALIGN_BOTTOM;
				selfAction = GetBottom;
				action = GetTop;
				minValue = value;
			}
			value = Math.Abs(_targetRect.Bottom - rect.Bottom);
			if (minValue > value)
			{
				horizontalAlignType = ALIGN_BOTTOM;
				selfAction = GetBottom;
				action = GetBottom;
				minValue = value;
			}
			float closeValue = Math.Min(_curHorizontalCloseValue, _closeValue);
			if (minValue > closeValue) return;
			if (minValue < closeValue)
			{
				HorizontalAlignType = horizontalAlignType;
				_curHorizontalCloseValue = minValue;
				HorizontalAlignAction = selfAction;
				_horizontalAlignLine.Set(_targetRect.Left, selfAction(_targetRect), _targetRect.Width, _lineThickness);
			}
			if (HorizontalAlignAction != selfAction) return;
			_horizontalAlignLine.Y = action(rect);
			_horizontalAlignLine.Left = Math.Min(_horizontalAlignLine.Left, rect.Left);
			_horizontalAlignLine.Right = Math.Max(_horizontalAlignLine.Right, rect.Right);
		}

		public void MergeVertical(Rectangle rect)
		{
			GetAction selfAction = GetLeft, action = GetLeft;
			int verticalAlignType = ALIGN_LEFT;
			float minValue = Math.Abs(_targetRect.Left - rect.Left);
			float value = Math.Abs(_targetRect.Left - rect.Right);
			if (minValue > value)
			{
				verticalAlignType = ALIGN_LEFT;
				selfAction = GetLeft;
				action = GetRight;
				minValue = value;
			}
			value = Math.Abs(_targetRect.Right - rect.Left);
			if (minValue > value)
			{
				verticalAlignType = ALIGN_RIGHT;
				selfAction = GetRight;
				action = GetLeft;
				minValue = value;
			}
			value = Math.Abs(_targetRect.Right - rect.Right);
			if (minValue > value)
			{
				verticalAlignType = ALIGN_RIGHT;
				selfAction = GetRight;
				action = GetRight;
				minValue = value;
			}
			float closeValue = Math.Min(_curVerticalCloseValue, _closeValue);
			if (minValue > closeValue) return;
			if (minValue < closeValue)
			{
				VerticalAlignType = verticalAlignType;
				_curVerticalCloseValue = minValue;
				VerticalAlignAction = selfAction;
				_verticalAlignLine.Set(selfAction(_targetRect), _targetRect.Top, _lineThickness, _targetRect.Height);
			}
			if (VerticalAlignAction != selfAction) return;
			_verticalAlignLine.X = action(rect);
			_verticalAlignLine.Top = Math.Min(_verticalAlignLine.Top, rect.Top);
			_verticalAlignLine.Bottom = Math.Max(_verticalAlignLine.Bottom, rect.Bottom);
		}
	}
}