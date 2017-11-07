using System;
using System.Runtime.InteropServices;

public class GameRecord : Singleton<GameRecord>
{
    private void Record(params uint[] args)
    {
    }

    public void RecordNewbieGuide(uint id, bool isComplete)
    {
        uint[] args = new uint[3];
        args[1] = id;
        args[2] = !isComplete ? 0 : 1;
        this.Record(args);
    }

    public void RecordSpecialInfo(SpecialInfo info, uint param3 = 0, uint param4 = 0)
    {
        uint[] args = new uint[] { 1, info, param3, param4 };
        this.Record(args);
    }

    public enum SpecialInfo
    {
        chooseAvt = 3,
        chooseProfession = 2,
        createRole = 1,
        dungeon = 6,
        hall = 5,
        startLoadingNewbieDungeon = 4
    }
}

