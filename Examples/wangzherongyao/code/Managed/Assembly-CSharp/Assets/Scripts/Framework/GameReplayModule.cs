namespace Assets.Scripts.Framework
{
    using Assets.Scripts.Common;
    using Assets.Scripts.GameLogic;
    using CSProtocol;
    using Pathfinding.RVO;
    using Pathfinding.RVO.Sampled;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using tsf4g_tdr_csharp;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class GameReplayModule : Singleton<GameReplayModule>, IGameModule
    {
        [CompilerGenerated]
        private static Comparison<string> <>f__am$cache26;
        private SLogObj actorLogObj;
        public bool bBattleStarted;
        public bool bGameLoadComplete;
        private bool bProfileReplay;
        public bool bRecord = true;
        private bool bRequestFightOver;
        private bool bRequestGameEnd;
        private bool bStartRecord = true;
        private GUIStyle buttonStyle;
        private CSPkg cacheSettleMsg;
        private bool gui_enableUseGmPanel;
        protected EventSystem gui_eventSystem;
        private int gui_playerIndex;
        private uint gui_playSpeed = 1;
        private static uint[] gui_playSpeedList = new uint[] { 1, 2, 4, 8, 0x10 };
        public bool isReplay;
        private GUIStyle labelStyle;
        private SLogObj logObj;
        public int playerIdx;
        private BinaryReader reader;
        public string replayFileDir;
        public string[] replayFiles;
        private uint replaySpeed = 1;
        private SLogObj rvoLogObj;
        private bool showReplayWnd;
        private FileStream stream;
        private byte[] tempBuf = new byte[0xfa000];
        private BinaryWriter writer;

        public void BattleStart()
        {
            if (this.bRecord || this.isReplay)
            {
                this.bBattleStarted = true;
            }
            if (this.isReplay)
            {
                Singleton<FrameSynchr>.GetInstance().FrameSpeed = this.replaySpeed;
                Time.timeScale = this.replaySpeed;
            }
        }

        private bool BeginRecordMsg()
        {
            try
            {
                if ((this.writer == null) && (this.streamPath != null))
                {
                    this.stream = new FileStream(this.streamPath, FileMode.Append, FileAccess.Write);
                    this.writer = new BinaryWriter(this.stream);
                }
            }
            catch (Exception)
            {
                this.stream = null;
                this.writer = null;
            }
            return (this.writer != null);
        }

        public void BeginReplay(string path, uint speed = 1, int player = 0, bool generateReplayLog = true)
        {
            if (!File.Exists(path))
            {
                this.isReplay = false;
            }
            else
            {
                this.isReplay = true;
                this.replaySpeed = (uint) Mathf.Clamp((int) speed, 1, 0x10);
                this.playerIdx = Mathf.Clamp(player, 0, 6);
                try
                {
                    this.stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    this.reader = new BinaryReader(this.stream);
                    if (generateReplayLog)
                    {
                        this.InitReplayLog(path);
                    }
                }
                catch (Exception)
                {
                    this.stream = null;
                    this.reader = null;
                    this.isReplay = false;
                }
            }
        }

        public void CloseReplayWndInGame()
        {
            if (MonoSingleton<ConsoleWindow>.HasInstance())
            {
                MonoSingleton<ConsoleWindow>.instance.bEnableUseGmPanel = this.gui_enableUseGmPanel;
            }
            this.showReplayWnd = false;
            this.EnableEventSystem(true);
        }

        public void CloseStream()
        {
            if (this.writer != null)
            {
                this.writer.Close();
                this.writer = null;
            }
            if (this.reader != null)
            {
                this.reader.Close();
                this.reader = null;
            }
            if (this.stream != null)
            {
                this.stream.Close();
                this.stream = null;
            }
            this.streamPath = null;
        }

        private bool DrawButton(string text)
        {
            GUIContent content = new GUIContent(text, string.Empty);
            Vector2 vector = this.buttonStyle.CalcSize(content);
            GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.Width(vector.x + 20f) };
            return GUILayout.Button(content, this.buttonStyle, options);
        }

        public void DrawGUI()
        {
            this.DrawStat();
            this.DrawReplayWnd();
        }

        private void DrawLabel(string text)
        {
            GUIContent content = new GUIContent(text);
            Vector2 vector = this.labelStyle.CalcSize(content);
            GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.Width(vector.x + 15f) };
            GUILayout.Label(content, this.labelStyle, options);
        }

        private void DrawReplayWnd()
        {
            if (this.showReplayWnd)
            {
                GUILayout.Window(0xbaddad, new Rect(0f, 0f, (float) Screen.width, (float) Screen.height), new GUI.WindowFunction(this.OnDrawReplayWnd), "ReplayWindow", new GUILayoutOption[0]);
            }
        }

        private void DrawStat()
        {
            if (this.isReplay)
            {
                int num = (Screen.width / 2) - 200;
                int num2 = Screen.height - 80;
                object[] args = new object[] { !this.isReplay ? "red" : "lime", !this.isReplay ? "Recording" : "Replay", (!this.isReplay || (this.replaySpeed <= 1)) ? string.Empty : string.Format(" {0}x ", this.replaySpeed), Singleton<FrameSynchr>.GetInstance().CurFrameNum };
                string text = string.Format("<color={0}><size=16>{1}{2}...    frame={3}</size></color>", args);
                if (Singleton<BattleLogic>.HasInstance())
                {
                    float num3 = 0f;
                    if (Singleton<BattleLogic>.instance.m_fpsCount > 0f)
                    {
                        num3 = Singleton<BattleLogic>.instance.m_fAveFPS / ((float) Singleton<BattleLogic>.instance.m_fpsCount);
                    }
                    text = text + string.Format("\n<size=32>{0:F2}</size>", num3);
                }
                GUI.Label(new Rect((float) num, (float) num2, 200f, 80f), text);
            }
        }

        private void EnableEventSystem(bool enable)
        {
            if (this.gui_eventSystem == null)
            {
                this.gui_eventSystem = EventSystem.current;
            }
            if (this.gui_eventSystem != null)
            {
                this.gui_eventSystem.enabled = enable;
            }
        }

        private void EndRecordMsg()
        {
            if (this.writer != null)
            {
                this.writer.Close();
                this.stream.Close();
            }
            this.writer = null;
            this.stream = null;
        }

        private void FinishRecord()
        {
            string streamPath = this.streamPath;
            bool flag = !this.bBattleStarted;
            this.Reset();
            this.ResetLog();
            if (flag && File.Exists(streamPath))
            {
                File.Delete(streamPath);
            }
        }

        public void FlushLogFiles()
        {
            SLog.Flush();
            if (this.logObj != null)
            {
                this.logObj.Flush();
            }
            if (this.actorLogObj != null)
            {
                this.actorLogObj.Flush();
            }
            if (this.rvoLogObj != null)
            {
                this.rvoLogObj.Flush();
            }
        }

        public string[] GetReplayFileList(out string dirPath)
        {
            dirPath = null;
            DirectoryInfo info = new DirectoryInfo(GetReplayFolder());
            if (!info.Exists)
            {
                return null;
            }
            string[] array = Directory.GetFiles(info.FullName, "*.abc", 0);
            if (array.Length == 0)
            {
                return null;
            }
            if (<>f__am$cache26 == null)
            {
                <>f__am$cache26 = delegate (string a, string b) {
                    FileInfo info = new FileInfo(a);
                    FileInfo info2 = new FileInfo(b);
                    if (info.get_LastWriteTime() > info2.get_LastWriteTime())
                    {
                        return -1;
                    }
                    if (info.get_LastWriteTime() < info2.get_LastWriteTime())
                    {
                        return 1;
                    }
                    return (info.get_CreationTime() <= info2.get_CreationTime()) ? ((info.get_CreationTime() >= info2.get_CreationTime()) ? 0 : 1) : -1;
                };
            }
            Array.Sort<string>(array, <>f__am$cache26);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array[i].Substring(info.FullName.Length);
            }
            dirPath = info.FullName;
            return array;
        }

        public static string GetReplayFolder()
        {
            return DebugHelper.logRootPath;
        }

        private void InitRecord()
        {
            string replayFolder = GetReplayFolder();
            string str2 = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            this.startupTag = str2;
            DebugHelper.timeTag = this.startupTag;
            this.streamPath = string.Format("{0}/{1}.abc", replayFolder, str2);
            this.replayFilePath = this.streamPath;
            try
            {
                this.stream = new FileStream(this.streamPath, FileMode.Create, FileAccess.Write);
                this.writer = new BinaryWriter(this.stream);
            }
            catch (Exception)
            {
                this.stream = null;
                this.writer = null;
            }
        }

        private void InitReplayLog(string path)
        {
            int length = path.LastIndexOf('.');
            if (length != -1)
            {
                string str = path.Substring(0, length);
                if (!Directory.Exists(str))
                {
                    Directory.CreateDirectory(str);
                }
                string str2 = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                SLog.TargetPath = string.Format("{0}/{1}_sgame_debug.txt", str, str2);
                DebugHelper.OpenLoger(SLogCategory.Normal, string.Format("{0}/{1}_mgame_log.txt", str, str2));
                DebugHelper.OpenLoger(SLogCategory.Skill, string.Format("{0}/{1}_skill_log.txt", str, str2));
                DebugHelper.OpenLoger(SLogCategory.Misc, string.Format("{0}/{1}_misc_log.txt", str, str2));
                this.actorLogObj = new SLogObj();
                this.actorLogObj.TargetPath = string.Format("{0}/{1}_actor_state.txt", str, str2);
                this.rvoLogObj = new SLogObj();
                this.rvoLogObj.TargetPath = string.Format("{0}/{1}_rvo.txt", str, str2);
            }
        }

        private bool LoadMsgOrCmd(long frameNum, out CSPkg msg, out IFrameCommand cmd)
        {
            msg = null;
            cmd = null;
            if (this.stream == null)
            {
                return false;
            }
            long position = this.stream.Position;
            if (position == this.stream.Length)
            {
                return false;
            }
            long num2 = this.reader.ReadInt64();
            if (((frameNum < 0L) && (num2 > frameNum)) || ((frameNum >= 0L) && (Singleton<FrameSynchr>.instance.svrLogicFrameNum > (frameNum + 40L))))
            {
                this.stream.Position = position;
                return false;
            }
            RecordType type = (RecordType) this.reader.ReadInt32();
            int count = this.reader.ReadInt32();
            long num4 = this.stream.Position;
            if ((num4 + count) > this.stream.Length)
            {
                return false;
            }
            if (type != RecordType.Msg)
            {
                return false;
            }
            if (this.reader.Read(this.tempBuf, 0, count) != count)
            {
                this.stream.Position = num4 + count;
            }
            else
            {
                int usedSize = 0;
                msg = new CSPkg();
                if (msg.unpack(ref this.tempBuf, count, ref usedSize, 0) != TdrError.ErrorType.TDR_NO_ERROR)
                {
                    msg = null;
                }
            }
            if (this.stream.Position == this.stream.Length)
            {
                this.CloseStream();
            }
            return true;
        }

        private void ObjToStr(StringBuilder sb, object obj, object prtObj = null)
        {
            if (obj == null)
            {
                sb.Append("null");
            }
            else
            {
                System.Type type = obj.GetType();
                if (type.IsArray)
                {
                    sb.Append("[");
                    Array array = obj as Array;
                    int length = array.Length;
                    if (prtObj != null)
                    {
                        FieldInfo field = prtObj.GetType().GetField("wLen");
                        if (field != null)
                        {
                            length = (ushort) field.GetValue(prtObj);
                        }
                    }
                    for (int i = 0; i < length; i++)
                    {
                        object obj2 = array.GetValue(i);
                        if (obj2 != null)
                        {
                            if (i > 0)
                            {
                                sb.Append(", ");
                            }
                            this.ObjToStr(sb, obj2, obj);
                        }
                    }
                    sb.Append("]");
                }
                else if (type.IsClass)
                {
                    sb.Append("{");
                    foreach (FieldInfo info2 in type.GetFields())
                    {
                        if (!info2.IsStatic && (info2.Name != "bReserve"))
                        {
                            sb.Append(info2.Name);
                            sb.Append(": ");
                            this.ObjToStr(sb, info2.GetValue(obj), obj);
                            sb.Append("; ");
                        }
                    }
                    sb.Append("}");
                }
                else
                {
                    sb.Append(obj.ToString());
                }
            }
        }

        private void OnDrawReplayWnd(int id)
        {
            GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.Width((float) Screen.width), GUILayout.Height((float) Screen.height) };
            GUILayout.BeginScrollView(Vector2.zero, false, false, options);
            GUILayout.BeginVertical(new GUILayoutOption[0]);
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            this.setButtonFontSize(60);
            if (this.DrawButton("关闭"))
            {
                this.CloseReplayWndInGame();
            }
            if (this.DrawButton("刷新"))
            {
                this.replayFiles = this.GetReplayFileList(out this.replayFileDir);
            }
            GUILayout.EndHorizontal();
            if (((this.replayFiles == null) || string.IsNullOrEmpty(this.replayFileDir)) || (this.replayFiles.Length == 0))
            {
                GUI.color = Color.green;
                if (this.DrawButton("找不到录像文件!!"))
                {
                    this.CloseReplayWndInGame();
                }
                GUI.color = Color.white;
            }
            else
            {
                GUILayout.BeginHorizontal(new GUILayoutOption[0]);
                this.DrawLabel("玩家ID");
                for (int i = 0; i < 6; i++)
                {
                    GUI.color = (i != this.gui_playerIndex) ? Color.white : Color.red;
                    if (this.DrawButton(i.ToString()))
                    {
                        this.gui_playerIndex = i;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal(new GUILayoutOption[0]);
                this.DrawLabel("速度");
                for (int j = 0; j < gui_playSpeedList.Length; j++)
                {
                    uint num3 = gui_playSpeedList[j];
                    GUI.color = (num3 != this.gui_playSpeed) ? Color.white : Color.red;
                    if (this.DrawButton(string.Format("{0}x", num3)))
                    {
                        this.gui_playSpeed = num3;
                    }
                }
                GUILayout.EndHorizontal();
                GUI.color = Color.white;
                this.setButtonFontSize(40);
                for (int k = 0; k < this.replayFiles.Length; k++)
                {
                    if (this.DrawButton(this.replayFiles[k]))
                    {
                        this.showReplayWnd = false;
                        bool generateReplayLog = false;
                        this.BeginReplay(this.replayFileDir + this.replayFiles[k], this.gui_playSpeed, this.gui_playerIndex, generateReplayLog);
                        this.CloseReplayWndInGame();
                        break;
                    }
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        public void OnGameFightOver()
        {
            if (this.isReplay)
            {
                this.bRequestFightOver = true;
            }
            DebugHelper.CloseLoger(SLogCategory.Skill);
            if (this.actorLogObj != null)
            {
                this.actorLogObj.Close();
                this.actorLogObj = null;
            }
            if (this.rvoLogObj != null)
            {
                this.rvoLogObj.Close();
                this.rvoLogObj = null;
            }
        }

        public void OnGameLoadComplete()
        {
            if (this.isReplay)
            {
                this.bGameLoadComplete = true;
            }
        }

        public void OnMultiGameEnd()
        {
            this.bRequestGameEnd = true;
        }

        public void RecordMsg(CSPkg msg)
        {
            uint dwMsgID = msg.stPkgHead.dwMsgID;
            if ((((dwMsgID < 0x44c) || (dwMsgID >= 0x7d0)) && (dwMsgID != 0x3f2)) && (!this.isReplay && this.bRecord))
            {
                long num2 = !this.bBattleStarted ? -2L : ((long) Singleton<FrameSynchr>.GetInstance().svrLogicFrameNum);
                bool flag = false;
                if (!this.bStartRecord)
                {
                    if ((msg.stPkgHead.dwMsgID != 0x433) && (msg.stPkgHead.dwMsgID != 0x444))
                    {
                        return;
                    }
                    this.bStartRecord = true;
                }
                else if ((msg.stPkgHead.dwMsgID == 0x400) || (msg.stPkgHead.dwMsgID == 0x43a))
                {
                    flag = true;
                }
                if (msg.stPkgHead.dwMsgID == 0x435)
                {
                    num2 = -1L;
                }
                if (this.streamPath == null)
                {
                    this.InitRecord();
                }
                if (this.logObj != null)
                {
                    try
                    {
                        ProtocolObject obj2 = msg.stPkgData.select((long) msg.stPkgHead.dwMsgID);
                        StringBuilder sb = new StringBuilder();
                        object[] args = new object[] { num2, msg.stPkgHead.dwMsgID, msg.stPkgHead.dwSvrPkgSeq, (obj2 == null) ? string.Empty : obj2.GetType().ToString() };
                        sb.AppendFormat("EndFrameNum: {0}; Msg: {1}; Seq: {2}; Name: {3};  ", args);
                        if (obj2 != null)
                        {
                            if (msg.stPkgHead.dwMsgID == 0x40a)
                            {
                                SCPKG_FRAPBOOT_SINGLE scpkg_frapboot_single = obj2 as SCPKG_FRAPBOOT_SINGLE;
                                CSDT_FRAPBOOT_INFO csdt_frapboot_info = CSDT_FRAPBOOT_INFO.New();
                                int usedSize = 0;
                                if ((csdt_frapboot_info.unpack(ref scpkg_frapboot_single.szInfoBuff, scpkg_frapboot_single.wLen, ref usedSize, 0) == TdrError.ErrorType.TDR_NO_ERROR) && (usedSize > 0))
                                {
                                    this.ObjToStr(sb, csdt_frapboot_info, null);
                                }
                                csdt_frapboot_info.Release();
                            }
                            else
                            {
                                this.ObjToStr(sb, obj2, null);
                            }
                        }
                        this.logObj.Log(sb.ToString());
                    }
                    catch (Exception)
                    {
                    }
                }
                if (this.BeginRecordMsg())
                {
                    int num4 = 0;
                    if (msg.pack(ref this.tempBuf, this.tempBuf.Length, ref num4, 0) == TdrError.ErrorType.TDR_NO_ERROR)
                    {
                        this.writer.Write(num2);
                        this.writer.Write(1);
                        this.writer.Write(num4);
                        this.writer.Write(this.tempBuf, 0, num4);
                    }
                    else
                    {
                        Debug.LogError(string.Format("Recorde Msg failed : {0} !!!", msg.stPkgHead.dwMsgID));
                        flag = true;
                    }
                    this.EndRecordMsg();
                }
                if (flag)
                {
                    this.FinishRecord();
                }
            }
        }

        public void RecordMsgBeginDiscardingBroadcast()
        {
            if (this.logObj != null)
            {
                try
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("---BeginDiscardingBroadcast", new object[0]);
                    this.logObj.Log(builder.ToString());
                }
                catch (Exception)
                {
                }
            }
        }

        public void RecordMsgBeginParseLaterPkg()
        {
            if (this.logObj != null)
            {
                try
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("---BeginParseLaterPkg", new object[0]);
                    this.logObj.Log(builder.ToString());
                }
                catch (Exception)
                {
                }
            }
        }

        public void RecordMsgBeginParseRelaySync()
        {
            if (this.logObj != null)
            {
                try
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("---BeginParseRelaySync", new object[0]);
                    this.logObj.Log(builder.ToString());
                }
                catch (Exception)
                {
                }
            }
        }

        public void RecordMsgOnRelaySyncOver()
        {
            if (this.logObj != null)
            {
                try
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendFormat("---OnRelaySyncOver", new object[0]);
                    this.logObj.Log(builder.ToString());
                }
                catch (Exception)
                {
                }
            }
        }

        public void ReportActorStates(List<PoolObjHandle<ActorRoot>> GameActors)
        {
            if (((this.actorLogObj != null) && (GameActors != null)) && (this.isReplay || ((this.bRecord && this.bStartRecord) && this.bBattleStarted)))
            {
                this.actorLogObj.Log(string.Format("FrameNum: {0}", Singleton<FrameSynchr>.GetInstance().CurFrameNum));
                List<PoolObjHandle<ActorRoot>>.Enumerator enumerator = GameActors.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    PoolObjHandle<ActorRoot> current = enumerator.Current;
                    current.handle.ActorControl.ActorStateLog(this.actorLogObj);
                }
                DictionaryView<uint, CampInfo>.Enumerator enumerator2 = Singleton<BattleLogic>.GetInstance().battleStat.GetCampStat().GetEnumerator();
                while (enumerator2.MoveNext())
                {
                    KeyValuePair<uint, CampInfo> pair = enumerator2.Current;
                    CampInfo info = pair.Value;
                    KeyValuePair<uint, CampInfo> pair2 = enumerator2.Current;
                    this.actorLogObj.Log(string.Format("Camp:{0} allHurtHp:{1} numDeadSoldier:{2}", (int) pair2.Key, info.allHurtHp, info.numDeadSoldier));
                }
                this.actorLogObj.Log("\n");
            }
        }

        public void ReportRVOAgents()
        {
            List<Agent> agents = RVOSimulator.Instance.GetSimulator().agents;
            if ((this.rvoLogObj != null) && (this.isReplay || ((this.bRecord && this.bStartRecord) && this.bBattleStarted)))
            {
                this.rvoLogObj.Log(string.Format("FrameNum: {0}", Singleton<FrameSynchr>.GetInstance().CurFrameNum));
                for (int i = 0; i < agents.Count; i++)
                {
                    Agent agent = agents[i];
                    GameObject owner = agent.owner as GameObject;
                    StringBuilder builder = new StringBuilder();
                    builder.Append((owner == null) ? "<null>" : owner.name);
                    builder.Append("   ");
                    builder.Append(" smoothPos:");
                    builder.Append(agent.InterpolatedPosition.ToString());
                    builder.Append(" prevSmoothPos:");
                    builder.Append(agent.prevSmoothPos.ToString());
                    builder.Append(" position:");
                    builder.Append(agent.position.ToString());
                    builder.Append(" desiredVelocity:");
                    builder.Append(agent.desiredVelocity.ToString());
                    builder.Append(" Velocity:");
                    builder.Append(agent.Velocity.ToString());
                    builder.Append(" velocity:");
                    builder.Append(agent.velocity.ToString());
                    builder.Append(" newVelocity:");
                    builder.Append(agent.newVelocity.ToString());
                    if (agent.neighbours.Count > 0)
                    {
                        builder.Append(" neighbours:");
                        for (int j = 0; j < agent.neighbours.Count; j++)
                        {
                            Agent agent2 = agent.neighbours[j];
                            builder.Append((agent2.owner as GameObject).name);
                            builder.Append("=");
                            builder.Append(agent.neighbourDists[j]);
                            builder.Append(";");
                        }
                    }
                    this.rvoLogObj.Log(builder.ToString());
                }
                this.rvoLogObj.Log("\n\n");
            }
        }

        public void Reset()
        {
            if (this.isReplay)
            {
                this.replaySpeed = 1;
                this.playerIdx = 0;
                Singleton<FrameSynchr>.GetInstance().FrameSpeed = this.replaySpeed;
                Time.timeScale = this.replaySpeed;
                Singleton<FrameWindow>.GetInstance().Reset();
            }
            this.bStartRecord = false;
            this.isReplay = false;
            this.bBattleStarted = false;
            this.bGameLoadComplete = false;
            this.bRequestFightOver = false;
            this.bRequestGameEnd = false;
            this.cacheSettleMsg = null;
            this.CloseStream();
        }

        public void ResetLog()
        {
            SLog.TargetPath = null;
            DebugHelper.CloseLoger(SLogCategory.Normal);
            DebugHelper.CloseLoger(SLogCategory.Skill);
            DebugHelper.CloseLoger(SLogCategory.Misc);
            if (this.logObj != null)
            {
                this.logObj.Close();
                this.logObj = null;
            }
            if (this.actorLogObj != null)
            {
                this.actorLogObj.Close();
                this.actorLogObj = null;
            }
            if (this.rvoLogObj != null)
            {
                this.rvoLogObj.Close();
                this.rvoLogObj = null;
            }
        }

        private void setButtonFontSize(int fontSize)
        {
            float height = Screen.height;
            float width = Screen.width;
            fontSize = (int) (fontSize * Mathf.Min((float) (height / 800f), (float) (width / 1280f)));
            this.buttonStyle.fontSize = fontSize;
            this.labelStyle.fontSize = fontSize;
        }

        public bool ShowReplayWndInGame()
        {
            if (Singleton<GameStateCtrl>.GetInstance().isBattleState)
            {
                return false;
            }
            if (MonoSingleton<ConsoleWindow>.HasInstance())
            {
                this.gui_enableUseGmPanel = MonoSingleton<ConsoleWindow>.instance.bEnableUseGmPanel;
                MonoSingleton<ConsoleWindow>.instance.bEnableUseGmPanel = false;
            }
            if (this.buttonStyle == null)
            {
                this.buttonStyle = new GUIStyle(GUI.skin.button);
            }
            if (this.labelStyle == null)
            {
                this.labelStyle = new GUIStyle(GUI.skin.label);
            }
            this.setButtonFontSize(60);
            this.replayFiles = this.GetReplayFileList(out this.replayFileDir);
            this.EnableEventSystem(false);
            this.showReplayWnd = true;
            return true;
        }

        public void TestReplayFile(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    CSPkg pkg;
                    IFrameCommand command;
                    this.stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                    this.reader = new BinaryReader(this.stream);
                    while (this.LoadMsgOrCmd(0x7fffffffL, out pkg, out command))
                    {
                        if (pkg != null)
                        {
                            Debug.LogError(string.Format("Replay msg={0}", pkg.stPkgHead.dwMsgID));
                        }
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    this.stream = null;
                    this.reader = null;
                }
            }
        }

        public void UpdateFrame()
        {
            if (this.bRequestGameEnd)
            {
                this.Reset();
                this.ResetLog();
            }
            else if (this.isReplay)
            {
                CSPkg pkg;
                IFrameCommand command;
                NetworkModule instance = Singleton<NetworkModule>.GetInstance();
                FrameSynchr synchr = Singleton<FrameSynchr>.GetInstance();
                if ((this.bRequestFightOver || (this.stream == null)) && (this.cacheSettleMsg != null))
                {
                    NetMsgDelegate msgHandler = instance.GetMsgHandler(this.cacheSettleMsg.stPkgHead.dwMsgID);
                    if (msgHandler != null)
                    {
                        try
                        {
                            msgHandler(this.cacheSettleMsg);
                            this.cacheSettleMsg = null;
                        }
                        catch (Exception exception)
                        {
                            Debug.LogException(exception);
                        }
                    }
                }
                long curFrameNum = synchr.CurFrameNum;
                if (!this.bBattleStarted)
                {
                    curFrameNum = -2L;
                    if (this.bGameLoadComplete)
                    {
                        curFrameNum = -1L;
                    }
                }
                while (this.LoadMsgOrCmd(curFrameNum, out pkg, out command))
                {
                    if (pkg != null)
                    {
                        if ((pkg.stPkgHead.dwMsgID == 0x43a) && !this.bRequestFightOver)
                        {
                            this.cacheSettleMsg = pkg;
                            break;
                        }
                        NetMsgDelegate delegate3 = instance.GetMsgHandler(pkg.stPkgHead.dwMsgID);
                        if (delegate3 != null)
                        {
                            try
                            {
                                delegate3(pkg);
                            }
                            catch (Exception exception2)
                            {
                                Debug.LogException(exception2);
                            }
                        }
                    }
                    else if (command != null)
                    {
                        synchr.PushFrameCommand(command);
                    }
                }
            }
        }

        public string actorStateLogPath { get; protected set; }

        public string debugFilePath { get; protected set; }

        public string miscLogPath { get; protected set; }

        public string msgLogPath { get; protected set; }

        public string normalLogPath { get; protected set; }

        public string replayFilePath { get; protected set; }

        public string rvoLogPath { get; protected set; }

        public string skillLogPath { get; protected set; }

        public string startupTag { get; protected set; }

        public string streamPath { get; private set; }

        private enum RecordType
        {
            Cmd = 2,
            Msg = 1
        }
    }
}

