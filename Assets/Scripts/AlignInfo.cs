using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public enum AlignType {
	Null,
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

	private static readonly List<AlignType> HorizontalAxis = new List<AlignType> {AlignType.Top, AlignType.Bottom, AlignType.HorizontalCenter};
	private static readonly List<AlignType> VerticalAxis = new List<AlignType> {AlignType.Left, AlignType.Right, AlignType.VerticalCenter};

	private static readonly Dictionary<AlignType, GetAction> ActionDic = new Dictionary<AlignType, GetAction> {
		{AlignType.Left, GetLeft},
		{AlignType.VerticalCenter, GetVerticalCenter},
		{AlignType.Right, GetRight},
		{AlignType.Top, GetTop},
		{AlignType.HorizontalCenter, GetHorizontalCenter},
		{AlignType.Bottom, GetBottom}
	};

	private float _closeValue;
	private float _lineThickness;
	private Rectangle _targetRect;
	private float _curHorizontalCloseValue;
	private float _curVerticalCloseValue;
	private readonly Rectangle _horizontalAlignLine;
	private readonly Rectangle _verticalAlignLine;
	public AlignType HorizontalAlignType;
	public AlignType VerticalAlignType;
	public AlignType OtherHorizontalAlignType;
	public AlignType OtherVerticalAlignType;

	public AlignInfo(Rectangle targetRect, float closeValue, float lineThickness) {
		_targetRect = targetRect;
		_closeValue = closeValue;
		_lineThickness = lineThickness;
		_curHorizontalCloseValue = _closeValue + 1;
		_curVerticalCloseValue = _closeValue + 1;
		HorizontalAlignType = AlignType.Null;
		VerticalAlignType = AlignType.Null;
		_horizontalAlignLine = new Rectangle();
		_verticalAlignLine = new Rectangle();
	}

	public void UpdateInfo(Rectangle targetRect, float closeValue, float lineThickness) {
		UpdateTargetRect(targetRect);
		_closeValue = closeValue;
		_lineThickness = lineThickness;
		_curHorizontalCloseValue = _closeValue + 1;
		_curVerticalCloseValue = _closeValue + 1;
		HorizontalAlignType = AlignType.Null;
		VerticalAlignType = AlignType.Null;
		_horizontalAlignLine.Set();
		_verticalAlignLine.Set();
	}

	public void UpdateTargetRect(Rectangle targetRect) {
		if(_targetRect == null) _targetRect = new Rectangle();
		_targetRect.Set(targetRect.X, targetRect.Y, targetRect.Width, targetRect.Height);
	}

	public Rectangle HorizontalAlignLine => _curHorizontalCloseValue > _closeValue ? null : _horizontalAlignLine;

	public Rectangle VerticalAlignLine => _curVerticalCloseValue > _closeValue ? null : _verticalAlignLine;

	public void Merge(Rectangle rect) {
		Merge(rect, HorizontalAxis, MergeHorizontal);
		Merge(rect, VerticalAxis, MergeVertical);
	}

	private void Merge(Rectangle rect, IReadOnlyCollection<AlignType> axis, Action<Rectangle, AlignType, AlignType, float> merge) {
		float minDistance = GlobalData.MaxFloat;
		AlignType selfType = AlignType.Null, otherType = AlignType.Null;
		foreach(AlignType self in axis) {
			foreach(AlignType other in axis) {
				float distance = Math.Abs(ActionDic[self](_targetRect) - ActionDic[other](rect));
				// 优先考虑上下左右
				if(Utils.IsEqual(distance, minDistance) || distance > minDistance) continue;
				// 优先考虑中间
				// if(distance > minDistance) continue;
				selfType = self;
				otherType = other;
				minDistance = distance;
			}
		}
		if(selfType == AlignType.Null) return;
		merge?.Invoke(rect, selfType, otherType, minDistance);
	}

	private void MergeHorizontal(Rectangle rect, AlignType selfType, AlignType otherType, float minDistance) {
		float closeValue = Math.Min(_curHorizontalCloseValue, _closeValue);
		if(minDistance > closeValue) return;
		HorizontalAlignType = selfType;
		if(OtherHorizontalAlignType != AlignType.HorizontalCenter) OtherHorizontalAlignType = otherType;
		_curHorizontalCloseValue = minDistance;
		_horizontalAlignLine.Set(_targetRect.Left, ActionDic[otherType](rect), _targetRect.Width, _lineThickness);
		_horizontalAlignLine.Right = Math.Max(_horizontalAlignLine.Right, rect.Right);
		_horizontalAlignLine.Left = Math.Min(_horizontalAlignLine.Left, rect.Left);
	}

	private void MergeVertical(Rectangle rect, AlignType selfType, AlignType otherType, float minDistance) {
		float closeValue = Math.Min(_curVerticalCloseValue, _closeValue);
		if(minDistance > closeValue) return;
		VerticalAlignType = selfType;
		if(OtherVerticalAlignType != AlignType.VerticalCenter) OtherVerticalAlignType = otherType;
		_curVerticalCloseValue = minDistance;
		_verticalAlignLine.Set(ActionDic[otherType](rect), _targetRect.Top, _lineThickness, _targetRect.Height);
		_verticalAlignLine.Bottom = Math.Max(_verticalAlignLine.Bottom, rect.Bottom);
		_verticalAlignLine.Top = Math.Min(_verticalAlignLine.Top, rect.Top);
	}
}
