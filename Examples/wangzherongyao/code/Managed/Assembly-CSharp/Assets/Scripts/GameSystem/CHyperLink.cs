namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.UI;
    using CSProtocol;
    using System;
    using UnityEngine;

    public class CHyperLink
    {
        public const char CommandDelimiter = '|';
        public const char EndChar = ']';
        public const char ParamDelimiter = ',';
        public const char StartChar = '[';

        public static bool Bind(GameObject target, string hyperlink)
        {
            int num;
            if (string.IsNullOrEmpty(hyperlink))
            {
                return false;
            }
            char[] separator = new char[] { '|' };
            string[] strArray = hyperlink.Split(separator);
            if (strArray.Length != 2)
            {
                return false;
            }
            if (!int.TryParse(strArray[0], out num))
            {
                return false;
            }
            char[] chArray2 = new char[] { ',' };
            string[] strArray2 = strArray[1].Split(chArray2);
            CUIEventScript component = target.GetComponent<CUIEventScript>();
            stUIEventParams eventParams = new stUIEventParams();
            COM_HYPERLINK_TYPE com_hyperlink_type = (COM_HYPERLINK_TYPE) num;
            if (com_hyperlink_type != COM_HYPERLINK_TYPE.COM_HYPERLINK_TYPE_GUILD_INVITE)
            {
                if (com_hyperlink_type != COM_HYPERLINK_TYPE.COM_HYPERLINK_TYPE_PREGUILD_INVITE)
                {
                    return false;
                }
            }
            else
            {
                if (strArray2.Length != 4)
                {
                    return false;
                }
                ulong num3 = ulong.Parse(strArray2[0]);
                int num4 = int.Parse(strArray2[1]);
                ulong num5 = ulong.Parse(strArray2[2]);
                int num6 = int.Parse(strArray2[3]);
                eventParams.commonUInt64Param1 = num3;
                eventParams.tag = num4;
                eventParams.commonUInt64Param2 = num5;
                eventParams.tag2 = num6;
                component.SetUIEvent(enUIEventType.Click, enUIEventID.Guild_Hyperlink_Search_Guild, eventParams);
                return true;
            }
            if (strArray2.Length != 1)
            {
                return false;
            }
            ulong num8 = ulong.Parse(strArray2[0]);
            eventParams.commonUInt64Param1 = num8;
            component.SetUIEvent(enUIEventType.Click, enUIEventID.Guild_Hyperlink_Search_PrepareGuild, eventParams);
            return true;
        }
    }
}

