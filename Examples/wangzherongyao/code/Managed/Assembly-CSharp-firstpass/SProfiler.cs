using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class SProfiler : MonoSingleton<SProfiler>
{
    [CompilerGenerated]
    private static Comparison<Group> <>f__am$cacheE;
    [CompilerGenerated]
    private static Comparison<Group> <>f__am$cacheF;
    private ColumnProp[] column_props;
    private SortType curSortType;
    public static List<Group> groupList = new List<Group>();
    public static Dictionary<string, Group> groups = new Dictionary<string, Group>();
    private bool highToLow;
    private static bool paused = false;
    private static int requestPause = 0;
    private static bool requestReset = false;
    public static Dictionary<string, Sample> samples = new Dictionary<string, Sample>();
    private static bool showGUI = false;
    private bool showPercent;
    private static int stackCount = 0;
    private static Sample[] stacks = new Sample[0x20];
    private static double totalTime = 0.0;

    public SProfiler()
    {
        ColumnProp[] propArray1 = new ColumnProp[6];
        ColumnProp prop = new ColumnProp {
            name = "NAME",
            width = 260
        };
        propArray1[0] = prop;
        ColumnProp prop2 = new ColumnProp {
            name = "TIME",
            width = 120
        };
        propArray1[1] = prop2;
        ColumnProp prop3 = new ColumnProp {
            name = "SELF TIME",
            width = 120
        };
        propArray1[2] = prop3;
        ColumnProp prop4 = new ColumnProp {
            name = "MAX",
            width = 80
        };
        propArray1[3] = prop4;
        ColumnProp prop5 = new ColumnProp {
            name = "MAX SELF",
            width = 80
        };
        propArray1[4] = prop5;
        ColumnProp prop6 = new ColumnProp {
            name = "COUNT",
            width = 80
        };
        propArray1[5] = prop6;
        this.column_props = propArray1;
        this.curSortType = SortType.Time;
        this.highToLow = true;
    }

    public static void Begin(string name)
    {
        if (!string.IsNullOrEmpty(name) && !paused)
        {
            double currentTime = STimer.currentTime;
            string key = getFullName(name);
            Sample sample = null;
            if (!samples.TryGetValue(key, out sample))
            {
                sample = new Sample {
                    name = name,
                    fullName = key
                };
                samples.Add(key, sample);
                Group group = null;
                if (!groups.TryGetValue(name, out group))
                {
                    group = new Group {
                        name = name
                    };
                    groups.Add(name, group);
                    groupList.Add(group);
                }
                group.samples.Add(sample);
            }
            if (stackCount >= stacks.Length)
            {
                Sample[] destinationArray = new Sample[stacks.Length * 2];
                Array.Copy(stacks, destinationArray, stacks.Length);
                stacks = destinationArray;
            }
            stacks[stackCount++] = sample;
            sample.begin();
            if (stackCount > 1)
            {
                currentTime = sample.start - currentTime;
                Sample sample2 = stacks[stackCount - 2];
                sample2.profilerTime1 += currentTime;
                sample2.profilerTimeThisCall1 += currentTime;
                for (int i = 0; i < (stackCount - 1); i++)
                {
                    sample2 = stacks[i];
                    sample2.profilerTime0 += currentTime;
                    sample2.profilerTimeThisCall0 += currentTime;
                }
                totalTime -= currentTime;
            }
        }
    }

    public static void BeginSample(string name)
    {
    }

    public static void Cleanup()
    {
        for (int i = 0; i < stacks.Length; i++)
        {
            stacks[i] = null;
        }
        stackCount = 0;
        samples.Clear();
        groups.Clear();
        groupList.Clear();
    }

    private void DrawGroups()
    {
        string str = string.Empty;
        string str2 = string.Empty;
        string str3 = string.Empty;
        string str4 = string.Empty;
        string str5 = string.Empty;
        string str6 = string.Empty;
        string str7 = string.Empty;
        for (int i = 0; i < groupList.Count; i++)
        {
            Group group = groupList[i];
            group.flush();
            str = str + group.name + "\n";
            str2 = str2 + group.time.ToString("F2");
            if (this.showPercent)
            {
                str2 = (str2 + " (") + group.timePercent.ToString("F2") + "%)";
            }
            str2 = str2 + "\n";
            str3 = str3 + group.selfTime.ToString("F2");
            if (this.showPercent)
            {
                str3 = (str3 + " (") + group.selfTimePercent.ToString("F2") + "%)";
            }
            str3 = str3 + "\n";
            str7 = group.maxTime.ToString("F2");
            if (group.maxTime > 5.0)
            {
                str4 = (str4 + "<color=red>") + str7 + "</color>";
            }
            else
            {
                str4 = str4 + str7;
            }
            str4 = str4 + "\n";
            str7 = group.maxSelfTime.ToString("F2");
            if (group.maxSelfTime > 5.0)
            {
                str5 = (str5 + "<color=red>") + str7 + "</color>";
            }
            else
            {
                str5 = str5 + str7;
            }
            str5 = str5 + "\n";
            str6 = str6 + ((string) group.count) + "\n";
        }
        this.column_props[0].data = str;
        this.column_props[1].data = str2;
        this.column_props[2].data = str3;
        this.column_props[3].data = str4;
        this.column_props[4].data = str5;
        this.column_props[5].data = str6;
        float left = 20f;
        float top = 30f;
        float height = Screen.height - top;
        GUI.color = Color.green;
        for (int j = 0; j < this.column_props.Length; j++)
        {
            ColumnProp prop = this.column_props[j];
            GUI.Label(new Rect(left, top, (float) prop.width, height), prop.data);
            if (GUI.Button(new Rect(left, 0f, (float) prop.width, top), prop.name))
            {
                if (this.curSortType == j)
                {
                    this.highToLow = !this.highToLow;
                }
                else
                {
                    this.curSortType = (SortType) j;
                }
            }
            left += prop.width + 10f;
        }
        float width = 80f;
        if (GUI.Button(new Rect(left, 0f, width, top), "SHOW %"))
        {
            this.showPercent = !this.showPercent;
        }
        left += width + 10f;
        if (GUI.Button(new Rect(left, 0f, width, top), "RESET"))
        {
            Reset();
        }
        left += width + 10f;
        GUI.color = !paused ? GUI.color : Color.red;
        if (GUI.Button(new Rect(left, 0f, width, top), "PAUSE"))
        {
            paused = !paused;
        }
        GUI.color = Color.white;
    }

    public static void End()
    {
        if ((stackCount != 0) && !paused)
        {
            Sample sample = stacks[stackCount - 1];
            stacks[stackCount--] = null;
            double num = sample.end();
            if (stackCount > 0)
            {
                Sample sample2 = stacks[stackCount - 1];
                sample2.stackTime += num;
                sample2.stackTimeThisCall += num;
            }
            else
            {
                totalTime += num;
            }
        }
    }

    public static void EndSample()
    {
    }

    public static string getFullName(string name)
    {
        if (stackCount == 0)
        {
            return name;
        }
        return (stacks[stackCount - 1].fullName + "$$" + name);
    }

    private void LateUpdate()
    {
        if (requestReset)
        {
            requestReset = false;
            Reset();
        }
        if (requestPause != 0)
        {
            paused = requestPause == 1;
        }
    }

    public static void Pause()
    {
        paused = true;
    }

    public void RequestPause(bool pause)
    {
        requestPause = !pause ? -1 : 1;
    }

    public void RequestReset()
    {
        requestReset = true;
    }

    public static void Reset()
    {
        Dictionary<string, Sample>.Enumerator enumerator = samples.GetEnumerator();
        while (enumerator.MoveNext())
        {
            KeyValuePair<string, Sample> current = enumerator.Current;
            current.Value.reset();
        }
        totalTime = 0.0;
        paused = false;
    }

    public void ShowGUI(bool show)
    {
        if (showGUI != show)
        {
            showGUI = show;
            if (showGUI)
            {
                Reset();
            }
        }
    }

    private void SortGroups(SortType sortType, bool HighToLow)
    {
        <SortGroups>c__AnonStorey3C storeyc = new <SortGroups>c__AnonStorey3C();
        for (int i = 0; i < groupList.Count; i++)
        {
            groupList[i].flush();
        }
        storeyc.le = !HighToLow ? -1 : 1;
        storeyc.ge = !HighToLow ? 1 : -1;
        switch (sortType)
        {
            case SortType.Name:
                if (!HighToLow)
                {
                    if (<>f__am$cacheF == null)
                    {
                        <>f__am$cacheF = (x, y) => string.Compare(x.name, y.name);
                    }
                    groupList.Sort(<>f__am$cacheF);
                    break;
                }
                if (<>f__am$cacheE == null)
                {
                    <>f__am$cacheE = (x, y) => -string.Compare(x.name, y.name);
                }
                groupList.Sort(<>f__am$cacheE);
                break;

            case SortType.Time:
                groupList.Sort(new Comparison<Group>(storeyc.<>m__35));
                break;

            case SortType.SelfTime:
                groupList.Sort(new Comparison<Group>(storeyc.<>m__38));
                break;

            case SortType.MaxTime:
                groupList.Sort(new Comparison<Group>(storeyc.<>m__36));
                break;

            case SortType.MaxSelfTime:
                groupList.Sort(new Comparison<Group>(storeyc.<>m__37));
                break;

            case SortType.Count:
                groupList.Sort(new Comparison<Group>(storeyc.<>m__39));
                break;
        }
    }

    public void ToggleVisible()
    {
        this.ShowGUI(!showGUI);
    }

    [CompilerGenerated]
    private sealed class <SortGroups>c__AnonStorey3C
    {
        internal int ge;
        internal int le;

        internal int <>m__35(SProfiler.Group x, SProfiler.Group y)
        {
            if (x.time < y.time)
            {
                return this.le;
            }
            if (x.time == y.time)
            {
                return 0;
            }
            return this.ge;
        }

        internal int <>m__36(SProfiler.Group x, SProfiler.Group y)
        {
            if (x.maxTime < y.maxTime)
            {
                return this.le;
            }
            if (x.maxTime == y.maxTime)
            {
                return 0;
            }
            return this.ge;
        }

        internal int <>m__37(SProfiler.Group x, SProfiler.Group y)
        {
            if (x.maxSelfTime < y.maxSelfTime)
            {
                return this.le;
            }
            if (x.maxSelfTime == y.maxSelfTime)
            {
                return 0;
            }
            return this.ge;
        }

        internal int <>m__38(SProfiler.Group x, SProfiler.Group y)
        {
            if (x.selfTime < y.selfTime)
            {
                return this.le;
            }
            if (x.selfTime == y.selfTime)
            {
                return 0;
            }
            return this.ge;
        }

        internal int <>m__39(SProfiler.Group x, SProfiler.Group y)
        {
            if (x.count < y.count)
            {
                return this.le;
            }
            if (x.count == y.count)
            {
                return 0;
            }
            return this.ge;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ColumnProp
    {
        public string name;
        public int width;
        public string data;
    }

    public class Group
    {
        public int count;
        public double maxSelfTime;
        public double maxTime;
        public string name;
        public List<SProfiler.Sample> samples = new List<SProfiler.Sample>();
        public double selfTime;
        public double selfTimePercent;
        public double time;
        public double timePercent;

        public void flush()
        {
            this.count = 0;
            this.time = 0.0;
            this.selfTime = 0.0;
            this.maxTime = 0.0;
            this.maxSelfTime = 0.0;
            double num = 0.0;
            double num2 = 0.0;
            for (int i = 0; i < this.samples.Count; i++)
            {
                SProfiler.Sample sample = this.samples[i];
                num += sample.profilerTime0;
                num2 += sample.profilerTime1;
                this.time += sample.time;
                this.selfTime += sample.stackTime;
                this.count += sample.count;
                this.maxTime = Math.Max(this.maxTime, sample.maxTime);
                this.maxSelfTime = Math.Max(this.maxSelfTime, sample.maxSelfTime);
            }
            this.selfTime = this.time - this.selfTime;
            this.time -= num;
            this.selfTime -= num2;
            this.timePercent = (this.time * 100.0) / SProfiler.totalTime;
            this.selfTimePercent = (this.selfTime * 100.0) / SProfiler.totalTime;
            this.time *= 1000.0;
            this.selfTime *= 1000.0;
            this.maxTime *= 1000.0;
            this.maxSelfTime *= 1000.0;
        }
    }

    public class Sample
    {
        public int count;
        public string fullName;
        public double maxSelfTime;
        public double maxTime;
        public string name;
        public double profilerTime0;
        public double profilerTime1;
        public double profilerTimeThisCall0;
        public double profilerTimeThisCall1;
        public double stackTime;
        public double stackTimeThisCall;
        public double start;
        public double time;

        public void begin()
        {
            this.stackTimeThisCall = 0.0;
            this.profilerTimeThisCall0 = 0.0;
            this.profilerTimeThisCall1 = 0.0;
            this.start = SProfiler.STimer.currentTime;
        }

        public double end()
        {
            double num = SProfiler.STimer.currentTime - this.start;
            this.time += num;
            this.maxTime = Math.Max(this.maxTime, num - this.profilerTimeThisCall0);
            this.maxSelfTime = Math.Max(this.maxSelfTime, (num - this.stackTimeThisCall) - this.profilerTimeThisCall1);
            this.count++;
            return num;
        }

        public void reset()
        {
            this.time = 0.0;
            this.start = 0.0;
            this.stackTime = 0.0;
            this.stackTimeThisCall = 0.0;
            this.profilerTime0 = 0.0;
            this.profilerTime1 = 0.0;
            this.profilerTimeThisCall0 = 0.0;
            this.profilerTimeThisCall1 = 0.0;
            this.count = 0;
            this.maxTime = 0.0;
            this.maxSelfTime = 0.0;
        }
    }

    public enum SortType
    {
        Name,
        Time,
        SelfTime,
        MaxTime,
        MaxSelfTime,
        Count
    }

    private class STimer
    {
        public static double currentTime
        {
            get
            {
                return (double) Time.realtimeSinceStartup;
            }
        }
    }
}

