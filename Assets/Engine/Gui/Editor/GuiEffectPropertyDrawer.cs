using UnityEngine;
using UnityEditor;

namespace cs
{
    [CustomPropertyDrawer(typeof(GuiEffect))]
    public class GuiEffectPropertyDrawer : PropertyDrawer
    {

        public override void OnGUI(Rect position, SerializedProperty a_property, GUIContent a_label)
        {
            EditorGUI.BeginProperty(position, a_label, a_property);

            SerializedProperty nameProp = a_property.FindPropertyRelative("strName");
            SerializedProperty assetProp = a_property.FindPropertyRelative("timelineAsset");

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (a_property.name == "fadeInEffect")
            {
                nameProp.stringValue = GuiEffectPlayer.s_strFadeIn;

                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), a_label);
                EditorGUI.PropertyField(position, assetProp, GUIContent.none);
            }
            else if (a_property.name == "fadeOutEffect")
            {
                nameProp.stringValue = GuiEffectPlayer.s_strFadeOut;

                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), a_label);
                EditorGUI.PropertyField(position, assetProp, GUIContent.none);
            }
            else
            {
                //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), a_label);

                //if (string.IsNullOrEmpty(nameProp.stringValue))
                //{
                //    nameProp.stringValue = a_label.text;
                //}

                float fNameWidth = position.width * 0.4f;
                float fOffset = fNameWidth + 5;
                var nameRect = new Rect(position.x, position.y, fNameWidth, position.height);
                var assetRect = new Rect(position.x + fOffset, position.y, position.width - fOffset, position.height);

                EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
                EditorGUI.PropertyField(assetRect, assetProp, GUIContent.none);
            }


            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}

