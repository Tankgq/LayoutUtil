using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class SelectManager : MonoBehaviour {
	private readonly Rectangle _selectRect = new Rectangle();
	private GameObject _selectGameObject;
	private Vector3 _startPos = Vector3.zero;
	private RectTransform _selectRt;

	public GameObject containerScrollView;

	private void OnBeginDrag() {
		if(! _selectGameObject) {
			_selectGameObject = Instantiate(GlobalData.SelectPrefab, GlobalData.DisplayObjectContainer.transform);
		}

		_selectGameObject.SetActive(true);
		_selectGameObject.transform.SetAsLastSibling();
		if(! _selectRt) _selectRt = _selectGameObject.GetComponent<RectTransform>();
		_startPos = Input.mousePosition;
		_selectRt.anchoredPosition = Utils.GetAnchoredPositionInContainer(_startPos);
		_selectRt.sizeDelta = Vector2.zero;
		_selectRt.localScale = Vector2.one;
	}

	private void OnDrag() {
		if(! _selectGameObject.activeSelf) return;
		Vector2 size = Input.mousePosition - _startPos;
		Vector3 scale = Vector3.one;
		if(size.x < 0.0) {
			size.x = -size.x;
			scale.x = -1;
		}

		if(size.y > 0.0)
			scale.y = -1;
		else
			size.y = -size.y;
		RectTransform rt = GlobalData.ContainerRect;
		size /= rt.localScale.x;
		_selectRt.sizeDelta = size;
		_selectRt.localScale = scale;
	}

	private void OnEndDrag() {
		if(! string.IsNullOrWhiteSpace(GlobalData.CurrentModule) && _selectGameObject && _selectGameObject.activeSelf) {
			Vector2 leftTopPos = Element.ConvertTo(_selectRt.anchoredPosition);
			Vector2 scale = _selectRt.localScale;
			Vector2 size = _selectRt.sizeDelta;
			if(scale.x < 0) leftTopPos.x -= size.x;
			if(scale.y < 0) leftTopPos.y -= size.y;
			_selectRect.Set(leftTopPos.x, leftTopPos.y, size.x, size.y);
			ContainerManager.SelectDisplayObjectsInDisplayObject(_selectRect);
		}

		if(_selectGameObject) _selectGameObject.SetActive(false);
	}

	private void Update() {
		if(! Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)) {
			OnEndDrag();
			return;
		}

		if(Input.GetMouseButtonDown(0) && Utils.IsPointOverGameObject(containerScrollView)) {
			if(ContainerManager.CheckPointOnAnyDisplayObject()) return;
			OnBeginDrag();
			return;
		}

		if(_selectGameObject && _selectGameObject.activeSelf)
			OnDrag();
		else
			OnEndDrag();
	}
}
