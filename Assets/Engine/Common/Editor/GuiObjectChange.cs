using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace cs
{
    public class GuiObjectChange : EditorMonoBehaviour
    {

        public override void Update()
        {
            //Debug.Log ("每一帧回调一次");
        }

        public override void OnPlaymodeStateChanged(PlayModeState playModeState)
        {
            //Debug.Log ("游戏运行模式发生改变， 点击 运行游戏 或者暂停游戏或者 帧运行游戏 按钮时触发: " + playModeState);
        }

        public override void OnGlobalEventHandler(UnityEngine.Event e)
        {
            //Debug.Log("全局事件回调: " + e);
        }

        public override void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            //Debug.Log(string.Format("{0} : {1} - {2}", EditorUtility.InstanceIDToObject(instanceID), instanceID, selectionRect));
        }

        public override void OnHierarchyWindowChanged()
        {
            Selection.selectionChanged = delegate
            {
                if (Selection.activeObject)
                {
                    GuiControl[] arrObjSelect = Selection.GetFiltered<GuiControl>(SelectionMode.ExcludePrefab);
                    _GenValidControlID(arrObjSelect[0]);
                }
                //
                //if (arrObjSelect[0].transform != null)
                //{
                //    Debug.Log("选中" + _GenValidControlID(arrObjSelect[0]));

                //}

            };
        }

        public override void OnModifierKeysChanged()
        {
            //Debug.Log("当触发键盘事件");
        }

        public override void OnProjectWindowChanged()
        {
            //	Debug.Log ("当资源视图发生变化");
        }

        public override void ProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            //根据GUID得到资源的准确路径
            //Debug.Log (string.Format ("{0} : {1} - {2}", AssetDatabase.GUIDToAssetPath (guid), guid, selectionRect));
        }

        private int _GenValidControlID(GuiControl target)
        {

            Transform currTrans = (target as GuiControl).transform;
            GuiObject guiObject = Utility.FindGuiObjectOwner(currTrans);

            int nCurrentMaxID = 0;
            GuiControl[] arrCtrls = guiObject.GetComponentsInChildren<GuiControl>();
            for (int i = 0; i < arrCtrls.Length; ++i)
            {
                if (arrCtrls[i].ID > nCurrentMaxID)
                {
                    nCurrentMaxID = arrCtrls[i].ID;
                }
            }
            Debug.Log("current id-->" + target.ID + "length--->" + arrCtrls.Length);
            //if (target.ID == nCurrentMaxID) target.setID(nCurrentMaxID+1);
            return nCurrentMaxID;
        }
    }
}

