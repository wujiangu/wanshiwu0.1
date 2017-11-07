namespace AGE
{
    using System;
    using System.Collections.Generic;

    public class ActionSet
    {
        public Dictionary<Action, bool> actionSet;

        public ActionSet()
        {
            this.actionSet = new Dictionary<Action, bool>();
            this.actionSet = new Dictionary<Action, bool>();
        }

        public ActionSet(Dictionary<Action, bool> _actionSet)
        {
            this.actionSet = new Dictionary<Action, bool>();
            this.actionSet = new Dictionary<Action, bool>();
            foreach (KeyValuePair<Action, bool> pair in _actionSet)
            {
                this.actionSet.Add(pair.Key, pair.Value);
            }
        }

        public static ActionSet AndSet(ActionSet src1, ActionSet src2)
        {
            ActionSet set = new ActionSet();
            foreach (Action action in src1.actionSet.Keys)
            {
                if (src2.actionSet.ContainsKey(action))
                {
                    set.actionSet.Add(action, true);
                }
            }
            return set;
        }

        public static ActionSet InvertSet(ActionSet all, ActionSet exclusion)
        {
            ActionSet set = new ActionSet();
            foreach (Action action in all.actionSet.Keys)
            {
                if (!exclusion.actionSet.ContainsKey(action))
                {
                    set.actionSet.Add(action, true);
                }
            }
            return set;
        }

        public static ActionSet OrSet(ActionSet src1, ActionSet src2)
        {
            ActionSet set = new ActionSet();
            foreach (Action action in src1.actionSet.Keys)
            {
                set.actionSet.Add(action, true);
            }
            foreach (Action action2 in src2.actionSet.Keys)
            {
                if (!set.actionSet.ContainsKey(action2))
                {
                    set.actionSet.Add(action2, true);
                }
            }
            return set;
        }
    }
}

