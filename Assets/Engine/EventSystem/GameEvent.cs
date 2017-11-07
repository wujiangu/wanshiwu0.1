using System.Runtime.InteropServices;
using UnityEngine.Events;

namespace cs
{
    public class Event
    {
        public EventDefine eventDefine;
        public object varParam1;
        public object varParam2;
        public object varParam3;
        public object varParam4;

        public void Initialize()
        {
            eventDefine = null;
            varParam1 = null;
            varParam2 = null;
            varParam3 = null;
            varParam4 = null;
        }

        public void Clear()
        {
            eventDefine = null;
            varParam1 = null;
            varParam2 = null;
            varParam3 = null;
            varParam4 = null;
        }
    }
}
