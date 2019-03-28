using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardEventManager : MonoBehaviour
{
    public static bool IsShiftDown()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    public static bool IsControlDown() {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    public static bool IsEnterDown()
    {
        return Input.GetKey(KeyCode.KeypadEnter);
    }
}
