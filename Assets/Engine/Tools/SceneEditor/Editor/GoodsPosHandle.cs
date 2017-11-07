using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GoodsMove)),CanEditMultipleObjects]
public class GoodsPosHandle : Editor {

	protected virtual void OnSceneGUI()
    {
        GoodsMove gm = (GoodsMove)target;
        EventType type = Event.current.type;
        gm.OnCheck(type);
    }
}
