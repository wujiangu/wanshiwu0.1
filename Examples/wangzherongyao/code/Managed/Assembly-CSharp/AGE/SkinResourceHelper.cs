namespace AGE
{
    using Assets.Scripts.GameLogic;
    using System;
    using System.Text;

    public class SkinResourceHelper
    {
        public static string GetResourceName(Action _action, string _resName)
        {
            SkillUseContext refParamObject = _action.refParams.GetRefParamObject<SkillUseContext>("SkillContext");
            if (((refParamObject != null) && (refParamObject.Originator != 0)) && ((refParamObject.Originator.handle.TheActorMeta.ActorType == ActorTypeDef.Actor_Type_Hero) && (refParamObject.Originator.handle.ActorControl != null)))
            {
                uint num;
                HeroWrapper actorControl = (HeroWrapper) refParamObject.Originator.handle.ActorControl;
                if ((actorControl != null) && actorControl.GetSkinCfgID(out num))
                {
                    int num2 = _resName.LastIndexOf('/');
                    StringBuilder builder = new StringBuilder(_resName);
                    if (num2 >= 0)
                    {
                        StringBuilder builder2 = new StringBuilder();
                        builder2.AppendFormat("{0}{1}", num, "/");
                        builder.Insert(num2 + 1, builder2);
                        return builder.ToString();
                    }
                }
            }
            return _resName;
        }
    }
}

