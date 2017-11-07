namespace Assets.Scripts.GameSystem
{
    using System;

    public class TabElement
    {
        public uint cfgId;
        public string content;

        public TabElement(uint cfgid, string content)
        {
            this.cfgId = cfgid;
            this.content = content;
        }
    }
}

