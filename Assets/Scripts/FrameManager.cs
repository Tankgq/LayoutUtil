using UnityEngine;
using UnityEngine.UI;

public class FrameManager : MonoBehaviour
{
    public Image FrameTop;
    public Image FrameBottom;
    public Image FrameLeft;
    public Image FrameRight;

    private bool _isSelect;

    public FrameManager(bool isSelect = false)
    {
        _isSelect = isSelect;
    }

    public bool IsSelect
    {
        get => _isSelect;
        set
        {
            if (_isSelect == value) return;
            _isSelect = value;
            Color color = _isSelect ? Color.red : Color.blue;
            if (FrameTop) FrameTop.color = color;
            if (FrameBottom) FrameBottom.color = color;
            if (FrameLeft) FrameLeft.color = color;
            if (FrameRight) FrameRight.color = color;
        }
    }
}
