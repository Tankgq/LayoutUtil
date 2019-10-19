using System;
using System.Collections.Generic;

public enum AlignType {
	Left,
	VerticalCenter,
	Right,
	Top,
	HorizontalCenter,
	Bottom
}

public class AlignInfo {
	private delegate float GetAction(Rectangle rect);

	private static readonly GetAction GetLeft = rect => rect.Left;
	private static readonly GetAction GetRight = rect => rect.Right;
	private static readonly GetAction GetTop = rect => rect.Top;
	private static readonly GetAction GetBottom = rect => rect.Bottom;
	private static readonly GetAction GetHorizontalCenter = rect => rect.HorizontalCenter;
	private static readonly GetAction GetVerticalCenter = rect => rect.VerticalCenter;

	private static readonly List<AlignType> HorizontalAxis = new List<AlignType> {AlignType.Top, AlignType.HorizontalCenter, AlignType.Bottom};
	private static readonly List<AlignType> VerticalAxis = new List<AlignType> {AlignType.Left, AlignType.VerticalCenter, AlignType.Right};
	
	private static readonly Dictionary<AlignType, GetAction> GetActionDic = new Dictionary<AlignType, GetAction> {
		{AlignType.Left, GetLeft},
		{AlignType.VerticalCenter, GetVerticalCenter},
		{AlignType.Right, GetRight},
		{AlignType.Top, GetTop},
		{AlignType.HorizontalCenter, GetHorizontalCenter},
		{AlignType.Bottom, GetBottom}
	};

	private readonly float _closeValue;
	private readonly float _lineThickness;
	private readonly Rectangle _targetRect;
	private float _curHorizontalCloseValue;
	private float _curVerticalCloseValue;
	private readonly Rectangle _horizontalAlignLine;
	private readonly Rectangle _verticalAlignLine;
	public AlignType HorizontalAlignType;
	public AlignType VerticalAlignType;

	public AlignInfo(Rectangle targetRect, float closeValue, float lineThickness) {
		_targetRect = targetRect;
		_closeValue = closeValue;
		_lineThickness = lineThickness;
		_curHorizontalCloseValue = _closeValue + 1;
		_curVerticalCloseValue = _closeValue + 1;
		HorizontalAlignType = 0;
		VerticalAlignType = 0;
		_horizontalAlignLine = new Rectangle();
		_verticalAlignLine = new Rectangle();
	}

	public Rectangle HorizontalAlignLine => _curHorizontalCloseValue > _closeValue ? null : _horizontalAlignLine;

	public Rectangle VerticalAlignLine => _curVerticalCloseValue > _closeValue ? null : _verticalAlignLine;

	public void Merge(Rectangle rect) {
		MergeHorizontal(rect);
		MergeVertical(rect);
	}

	public void Merge(Rectangle rect, List<AlignType> axis) {
		float minDistance = GlobalData.MaxFloat;
		AlignType selfType, otherType;
		foreach(AlignType self in axis) {
			foreach(AlignType other in axis) {
				float distance = Math.Abs(GetActionDic[self](_targetRect) - GetActionDic[other](rect));
				if(! (distance < minDistance)) continue;
				selfType = self;
				otherType = other;
				minDistance = distance;
			}
		}
		float closeValue = Math.Min(_curHorizontalCloseValue, _closeValue);
		if(minValue > closeValue) return;
		if(minValue < closeValue) {
			HorizontalAlignType = horizontalAlignType;
			_curHorizontalCloseValue = minValue;
			_horizontalAlignAction = selfAction;
			_horizontalAlignLine.Set(_targetRect.Left, selfAction(_targetRect), _targetRect.Width, _lineThickness);
		}

		if(_horizontalAlignAction != selfAction) return;
		_horizontalAlignLine.Y = action(rect);
		_horizontalAlignLine.Left = Math.Min(_horizontalAlignLine.Left, rect.Left);
		_horizontalAlignLine.Right = Math.Max(_horizontalAlignLine.Right, rect.Right);
	}

	public void MergeHorizontal(Rectangle rect) {
		GetAction selfAction = GetTop, action = GetTop;
		AlignType horizontalAlignType = AlignType.Top;
		float minValue = Math.Abs(_targetRect.Top - rect.Top);
		float value = Math.Abs(_targetRect.Top - rect.Bottom);
		if(minValue > value) {
			horizontalAlignType = AlignType.Top;
			selfAction = GetTop;
			action = GetBottom;
			minValue = value;
		}

		value = Math.Abs(_targetRect.Bottom - rect.Top);
		if(minValue > value) {
			horizontalAlignType = AlignType.Bottom;
			selfAction = GetBottom;
			action = GetTop;
			minValue = value;
		}

		value = Math.Abs(_targetRect.Bottom - rect.Bottom);
		if(minValue > value) {
			horizontalAlignType = AlignType.Bottom;
			selfAction = GetBottom;
			action = GetBottom;
			minValue = value;
		}

		float closeValue = Math.Min(_curHorizontalCloseValue, _closeValue);
		if(minValue > closeValue) return;
		if(minValue < closeValue) {
			HorizontalAlignType = horizontalAlignType;
			_curHorizontalCloseValue = minValue;
			_horizontalAlignAction = selfAction;
			_horizontalAlignLine.Set(_targetRect.Left, selfAction(_targetRect), _targetRect.Width, _lineThickness);
		}

		if(_horizontalAlignAction != selfAction) return;
		_horizontalAlignLine.Y = action(rect);
		_horizontalAlignLine.Left = Math.Min(_horizontalAlignLine.Left, rect.Left);
		_horizontalAlignLine.Right = Math.Max(_horizontalAlignLine.Right, rect.Right);
	}

	public void MergeVertical(Rectangle rect) {
		GetAction selfAction = GetLeft, action = GetLeft;
		AlignType verticalAlignType = AlignType.Left;
		float minValue = Math.Abs(_targetRect.Left - rect.Left);
		float value = Math.Abs(_targetRect.Left - rect.Right);
		if(minValue > value) {
			verticalAlignType = AlignType.Left;
			selfAction = GetLeft;
			action = GetRight;
			minValue = value;
		}

		value = Math.Abs(_targetRect.Right - rect.Left);
		if(minValue > value) {
			verticalAlignType = AlignType.Right;
			selfAction = GetRight;
			action = GetLeft;
			minValue = value;
		}

		value = Math.Abs(_targetRect.Right - rect.Right);
		if(minValue > value) {
			verticalAlignType = AlignType.Right;
			selfAction = GetRight;
			action = GetRight;
			minValue = value;
		}

		float closeValue = Math.Min(_curVerticalCloseValue, _closeValue);
		if(minValue > closeValue) return;
		if(minValue < closeValue) {
			VerticalAlignType = verticalAlignType;
			_curVerticalCloseValue = minValue;
			_verticalAlignAction = selfAction;
			_verticalAlignLine.Set(selfAction(_targetRect), _targetRect.Top, _lineThickness, _targetRect.Height);
		}

		if(_verticalAlignAction != selfAction) return;
		_verticalAlignLine.X = action(rect);
		_verticalAlignLine.Top = Math.Min(_verticalAlignLine.Top, rect.Top);
		_verticalAlignLine.Bottom = Math.Max(_verticalAlignLine.Bottom, rect.Bottom);
	}
}
