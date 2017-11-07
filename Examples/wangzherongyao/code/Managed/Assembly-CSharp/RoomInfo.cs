using System;
using System.Collections;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct RoomInfo
{
    private ArrayList urls;
    private long roomID;
    private short memberID;
    private byte[] openID;
}

