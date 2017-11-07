namespace Assets.Scripts.GameSystem
{
    using System;

    public class TeamInfo
    {
        public ListView<TeamMember> MemInfoList = new ListView<TeamMember>();
        public PlayerUniqueID stSelfInfo = new PlayerUniqueID();
        public TeamAttrib stTeamInfo = new TeamAttrib();
        public PlayerUniqueID stTeamMaster = new PlayerUniqueID();
    }
}

