using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Text.RegularExpressions;

namespace cs
{
    [CustomEditor(typeof(GuiObject))]
    public class GuiObjectInspector : Editor
    {
        SerializedProperty m_guiEffectPlayer = null;

        SerializedProperty m_propFadeInEffct;
        SerializedProperty m_propFadeOutEffct;
        ReorderableList m_listCommonEffects;
        const string m_strNameFormat = "DefaultEffect_{0}";
        Regex m_nameRegex = new Regex(@"^DefaultEffect_(\d+)$");

        //private ReorderableList m_listCtrls;

        private void OnEnable()
        {
            m_guiEffectPlayer = serializedObject.FindProperty("m_guiEffectPlayer");
            m_propFadeInEffct = m_guiEffectPlayer.FindPropertyRelative("fadeInEffect");
            m_propFadeOutEffct = m_guiEffectPlayer.FindPropertyRelative("fadeOutEffect");
            m_listCommonEffects = new ReorderableList(serializedObject, 
                m_guiEffectPlayer.FindPropertyRelative("commonEffects"), true, true, true, true);
            m_listCommonEffects.drawElementCallback =
                (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = m_listCommonEffects.serializedProperty.GetArrayElementAtIndex(index);

                    bool isExpanded = element.isExpanded;
                    rect.height = EditorGUI.GetPropertyHeight(element, GUIContent.none, isExpanded);

                    if (element.hasVisibleChildren)
                    {
                        rect.xMin += 10;
                    }

                    // Get Unity to handle drawing each element
                    GUIContent propHeader = new GUIContent(element.displayName);
                    EditorGUI.PropertyField(rect, element, propHeader, isExpanded);
                };

            m_listCommonEffects.drawHeaderCallback = (Rect a_rect) =>
            {
                EditorGUI.LabelField(a_rect, "其他效果列表");
            };

            m_listCommonEffects.onAddCallback = (ReorderableList a_list) =>
            {
                var index = a_list.serializedProperty.arraySize;
                a_list.serializedProperty.arraySize++;
                a_list.index = index;
                var newElement = a_list.serializedProperty.GetArrayElementAtIndex(index);

                int nMaxIndex = 0;
                for (int i = 0; i < index; ++i)
                {
                    var element = a_list.serializedProperty.GetArrayElementAtIndex(i);
                    Match match = m_nameRegex.Match(element.FindPropertyRelative("strName").stringValue);
                    if (match.Success && match.Groups.Count > 0)
                    {
                        int nTemp = int.Parse(match.Groups[1].Value);
                        if (nTemp > nMaxIndex)
                        {
                            nMaxIndex = nTemp;
                        }
                    }
                }

                newElement.FindPropertyRelative("strName").stringValue = string.Format(m_strNameFormat, nMaxIndex + 1);
                newElement.FindPropertyRelative("timelineAsset").objectReferenceValue = null;
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PrefixLabel(new GUIContent("效果播放器"));
            EditorGUILayout.PropertyField(m_propFadeInEffct, new GUIContent("淡入效果"));
            EditorGUILayout.PropertyField(m_propFadeOutEffct, new GUIContent("淡出效果"));
            m_listCommonEffects.DoLayoutList();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
