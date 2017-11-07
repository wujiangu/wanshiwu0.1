namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using CSProtocol;
    using ResData;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class CTaskModel
    {
        public string Daily_Quest_Career = string.Empty;
        public string Daily_Quest_NeedGrowing = string.Empty;
        public string Daily_Quest_NeedHero = string.Empty;
        public string Daily_Quest_NeedMoney = string.Empty;
        public string Daily_Quest_NeedSeal = string.Empty;
        public HuoyueData huoyue_data = new HuoyueData();
        public uint share_task_id;
        public CombineData task_Data = new CombineData();

        public void AddTask(CTask task)
        {
            if (task != null)
            {
                this.task_Data.Add(task.m_baseId, task);
            }
        }

        public bool AnyTaskOfState(COM_TASK_STATE state, RES_TASK_TYPE taskType, out CTask outTask)
        {
            outTask = null;
            ListView<CTask> listView = this.task_Data.GetListView(taskType);
            if (listView != null)
            {
                for (int i = 0; i < listView.Count; i++)
                {
                    CTask task = listView[i];
                    if ((task != null) && (task.m_taskState == ((byte) state)))
                    {
                        outTask = task;
                        return true;
                    }
                }
            }
            return false;
        }

        public void Clear()
        {
            this.task_Data.Clear();
            this.huoyue_data.Clear();
        }

        public CTask GetMaxIndex_TaskID_InState(RES_TASK_TYPE type, CTask.State state)
        {
            return this.task_Data.GetMaxIndex_TaskID_InState(type, state);
        }

        public CTask GetTask(uint TaskId)
        {
            return this.task_Data.GetTask(TaskId);
        }

        public ListView<CTask> GetTasks(RES_TASK_TYPE type)
        {
            return this.task_Data.GetListView(type);
        }

        public ListView<CTask> GetTasks(RES_TASK_TYPE type, CTask.State state, bool bSort = false)
        {
            return null;
        }

        public int GetTasks_Count(RES_TASK_TYPE type, CTask.State state)
        {
            ListView<CTask> listView = this.task_Data.GetListView(type);
            DebugHelper.Assert(listView != null);
            if (listView == null)
            {
                return 0;
            }
            int num = 0;
            for (int i = 0; i < listView.Count; i++)
            {
                CTask task = listView[i];
                if ((task != null) && (task.m_taskState == ((byte) state)))
                {
                    num++;
                }
            }
            return num;
        }

        public int GetTotalTaskOfState(RES_TASK_TYPE type, COM_TASK_STATE state)
        {
            ListView<CTask> listView = this.task_Data.GetListView(type);
            if (listView == null)
            {
                return 0;
            }
            int num = 0;
            for (int i = 0; i < listView.Count; i++)
            {
                if (((COM_TASK_STATE) listView[i].m_taskState) == state)
                {
                    num++;
                }
            }
            return num;
        }

        public bool IsAnyTaskInState(RES_TASK_TYPE type, CTask.State state)
        {
            ListView<CTask> listView = this.task_Data.GetListView(type);
            DebugHelper.Assert(listView != null);
            if (listView != null)
            {
                for (int i = 0; i < listView.Count; i++)
                {
                    CTask task = listView[i];
                    if ((task != null) && (task.m_taskState == ((byte) state)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsTaskAllCompelete(RES_TASK_TYPE type)
        {
            ListView<CTask> listView = this.task_Data.GetListView(type);
            DebugHelper.Assert(listView != null);
            if (listView == null)
            {
                return false;
            }
            for (int i = 0; i < listView.Count; i++)
            {
                CTask task = listView[i];
                if ((task != null) && (task.m_taskState != 6))
                {
                    return false;
                }
            }
            return true;
        }

        public void Load_Share_task()
        {
            Dictionary<long, object>.Enumerator enumerator = GameDataMgr.taskDatabin.GetEnumerator();
            while (enumerator.MoveNext())
            {
                KeyValuePair<long, object> current = enumerator.Current;
                ResTask task = (ResTask) current.Value;
                if (task.astPrerequisiteArray[0].dwPrerequisiteType == 0x13)
                {
                    this.share_task_id = task.dwTaskID;
                    break;
                }
            }
        }

        public void Load_Task_Tab_String()
        {
            this.Daily_Quest_Career = UT.GetText("Daily_Quest_Career");
            this.Daily_Quest_NeedGrowing = UT.GetText("Daily_Quest_NeedGrowing");
            this.Daily_Quest_NeedMoney = UT.GetText("Daily_Quest_NeedMoney");
            this.Daily_Quest_NeedSeal = UT.GetText("Daily_Quest_NeedSeal");
            this.Daily_Quest_NeedHero = UT.GetText("Daily_Quest_NeedHero");
        }

        public void Remove(CTask task)
        {
            if (task != null)
            {
                this.task_Data.Remove(task.m_baseId);
            }
        }

        public void Remove(uint id)
        {
            this.task_Data.Remove(id);
        }

        public void UpdateTaskState()
        {
            if (this.task_Data != null)
            {
                DictionaryView<uint, CTask>.Enumerator enumerator = this.task_Data.task_map.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    KeyValuePair<uint, CTask> current = enumerator.Current;
                    CTask task = current.Value;
                    if (task != null)
                    {
                        task.UpdateState();
                    }
                }
            }
        }
    }
}

