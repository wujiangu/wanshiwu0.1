namespace Assets.Scripts.GameSystem
{
    using Assets.Scripts.Framework;
    using Assets.Scripts.UI;
    using ResData;
    using System;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class CBattleFloatDigitManager
    {
        private const string FLOAT_TEXT_PREFAB = "Text/FloatText/FloatText.prefab";
        private Queue<GameObject> m_floatTexts = new Queue<GameObject>();
        private static string[][] s_battleFloatDigitAnimatorStates;
        private static string[][] s_battleFloatDigitAnimatorStates_OLD;
        private static string[] s_otherFloatTextAnimatorStates;
        private static string[] s_otherFloatTextKeys;
        private static string s_restrictTextAnimatorState;
        private static string[] s_restrictTextKeys;

        static CBattleFloatDigitManager()
        {
            string[][] textArrayArray1 = new string[8][];
            textArrayArray1[0] = new string[] { string.Empty };
            textArrayArray1[1] = new string[] { "AttackNormal_Anim_0", "AttackNormal_Anim_1" };
            textArrayArray1[2] = new string[] { "AttackCrit_Anim" };
            textArrayArray1[3] = new string[] { "SufferDamage_Anim" };
            textArrayArray1[4] = new string[] { "SufferMagicDamage_Anim" };
            textArrayArray1[5] = new string[] { "ReviveHp_Anim" };
            textArrayArray1[6] = new string[] { "ReceiveSpirit_Anim" };
            textArrayArray1[7] = new string[] { "ReceiveSpirit_Anim" };
            s_battleFloatDigitAnimatorStates_OLD = textArrayArray1;
            string[][] textArrayArray2 = new string[13][];
            textArrayArray2[0] = new string[] { string.Empty };
            textArrayArray2[1] = new string[] { "Physics_Right", "Physics_Left" };
            textArrayArray2[2] = new string[] { "Physics_RightCrit", "Physics_LeftCrit" };
            textArrayArray2[3] = new string[] { "Magic_Right", "Magic_Left" };
            textArrayArray2[4] = new string[] { "Magic_RightCrit", "Magic_LeftCrit" };
            textArrayArray2[5] = new string[] { "ZhenShi_Right", "ZhenShi_Left" };
            textArrayArray2[6] = new string[] { "ZhenShi_RightCrit", "ZhenShi_LeftCrit" };
            textArrayArray2[7] = new string[] { "SufferPhysicalDamage" };
            textArrayArray2[8] = new string[] { "SufferMagicDamage" };
            textArrayArray2[9] = new string[] { "SufferRealDamage" };
            textArrayArray2[10] = new string[] { "ReviveHp" };
            textArrayArray2[11] = new string[] { "Exp" };
            textArrayArray2[12] = new string[] { "Gold" };
            s_battleFloatDigitAnimatorStates = textArrayArray2;
            s_restrictTextKeys = new string[] { 
                "Restrict_None", "Restrict_Dizzy", "Restrict_SlowDown", "Restrict_Taunt", "Restrict_Fear", "Restrict_Frozen", "Restrict_Floating", "Restrict_Slient", "Restrict_Stone", "SkillBuff_Custom_Type_1", "SkillBuff_Custom_Type_2", "SkillBuff_Custom_Type_3", "SkillBuff_Custom_Type_4", "SkillBuff_Custom_Type_5", "SkillBuff_Custom_Type_6", "SkillBuff_Custom_Type_7", 
                "SkillBuff_Custom_Type_8", "SkillBuff_Custom_Type_9", "SkillBuff_Custom_Type_10", "SkillBuff_Custom_Type_11", "SkillBuff_Custom_Type_12", "SkillBuff_Custom_Type_13", "SkillBuff_Custom_Type_14", "SkillBuff_Custom_Type_15", "SkillBuff_Custom_Type_16", "SkillBuff_Custom_Type_17", "SkillBuff_Custom_Type_18", "SkillBuff_Custom_Type_19", "SkillBuff_Custom_Type_20", "SkillBuff_Custom_Type_21", "SkillBuff_Custom_Type_22", "SkillBuff_Custom_Type_23", 
                "SkillBuff_Custom_Type_24", "SkillBuff_Custom_Type_25", "SkillBuff_Custom_Type_26", "SkillBuff_Custom_Type_27", "SkillBuff_Custom_Type_28", "SkillBuff_Custom_Type_29", "SkillBuff_Custom_Type_30", "SkillBuff_Custom_Type_31", "SkillBuff_Custom_Type_32", "SkillBuff_Custom_Type_33", "SkillBuff_Custom_Type_34", "SkillBuff_Custom_Type_35", "SkillBuff_Custom_Type_36", "SkillBuff_Custom_Type_37", "SkillBuff_Custom_Type_38", "SkillBuff_Custom_Type_39", 
                "SkillBuff_Custom_Type_40"
             };
            s_restrictTextAnimatorState = "RestrictText_Anim";
            s_otherFloatTextKeys = new string[] { "Accept_Task", "Complete_Task", "Level_Up", "Talent_Open", "Talent_Learn", "DragonBuff_Get1", "DragonBuff_Get2", "DragonBuff_Get3", "Battle_Absorb", "Battle_ShieldDisappear", "Battle_Immunity", "Battle_InCooldown", "Battle_NoTarget", "Battle_EnergyShortage", "Battle_Blindess" };
            s_otherFloatTextAnimatorStates = new string[] { "Other_Anim", "Other_Anim", "Other_Anim", "Other_Anim", "Other_Anim", "Other_Anim", "Other_Anim", "Other_Anim", "Other_Anim", "Other_Anim", "Other_Anim", "Other_Anim", "Other_Anim", "Other_Anim", "Other_Anim" };
        }

        public void ClearAllBattleFloatText()
        {
            while (this.m_floatTexts.Count > 0)
            {
                Singleton<CGameObjectPool>.GetInstance().RecycleGameObject(this.m_floatTexts.Dequeue());
            }
        }

        public void ClearBattleFloatText(CUIAnimatorScript animatorScript)
        {
        }

        public void CreateBattleFloatDigit(int digitValue, DIGIT_TYPE digitType, ref Vector3 worldPosition)
        {
            if (((((GameSettings.RenderQuality != SGameRenderQuality.Low) || (digitType == DIGIT_TYPE.MagicAttackCrit)) || ((digitType == DIGIT_TYPE.PhysicalAttackCrit) || (digitType == DIGIT_TYPE.RealAttackCrit))) || (digitType == DIGIT_TYPE.ReceiveGoldCoinInBattle)) && ((digitType != DIGIT_TYPE.ReviveHp) || (digitValue >= 20)))
            {
                string[] strArray = s_battleFloatDigitAnimatorStates[(int) digitType];
                if (strArray.Length > 0)
                {
                    string content = (((((digitType != DIGIT_TYPE.ReviveHp) && (digitType != DIGIT_TYPE.ReceiveSpirit)) && (digitType != DIGIT_TYPE.ReceiveGoldCoinInBattle)) || (digitValue <= 0)) ? string.Empty : "+") + Mathf.Abs(digitValue).ToString();
                    if (digitType == DIGIT_TYPE.ReceiveSpirit)
                    {
                        content = content + "xp";
                    }
                    else
                    {
                        if (digitType == DIGIT_TYPE.ReceiveGoldCoinInBattle)
                        {
                            content = content + "g";
                        }
                        this.CreateBattleFloatText(content, ref worldPosition, strArray[UnityEngine.Random.Range(0, strArray.Length)]);
                    }
                }
            }
        }

        public void CreateBattleFloatDigit(int digitValue, DIGIT_TYPE digitType, ref Vector3 worldPosition, int animatIndex)
        {
            if ((((GameSettings.RenderQuality != SGameRenderQuality.Low) || (digitType == DIGIT_TYPE.MagicAttackCrit)) || ((digitType == DIGIT_TYPE.PhysicalAttackCrit) || (digitType == DIGIT_TYPE.RealAttackCrit))) || (digitType == DIGIT_TYPE.ReceiveGoldCoinInBattle))
            {
                string[] strArray = s_battleFloatDigitAnimatorStates[(int) digitType];
                if (((strArray.Length > 0) && (animatIndex >= 0)) && (animatIndex < strArray.Length))
                {
                    string content = (((digitType != DIGIT_TYPE.ReceiveSpirit) || (digitValue <= 0)) ? string.Empty : "+") + Mathf.Abs(digitValue).ToString();
                    this.CreateBattleFloatText(content, ref worldPosition, strArray[animatIndex]);
                }
            }
        }

        private void CreateBattleFloatText(string content, ref Vector3 worldPosition, string animatorState)
        {
            if (!string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(animatorState))
            {
                int num;
                GameObject gameObject = Singleton<CGameObjectPool>.GetInstance().GetGameObject("Text/FloatText/FloatText.prefab", enResourceType.BattleScene);
                gameObject.transform.parent = Camera.main.transform;
                gameObject.transform.localRotation = Quaternion.identity;
                TextMeshPro component = gameObject.transform.GetChild(0).GetComponent<TextMeshPro>();
                Animator animator = gameObject.GetComponent<Animator>();
                Vector3 position = Camera.main.WorldToScreenPoint(worldPosition);
                position.Set(position.x, position.y, 30f);
                gameObject.transform.position = Camera.main.ScreenToWorldPoint(position);
                if (animatorState.IndexOf("Crit") != -1)
                {
                    GameObject obj3 = gameObject.transform.Find("Text/icon").gameObject;
                    Vector3 localPosition = obj3.transform.localPosition;
                    if (animatorState.IndexOf("Left") != -1)
                    {
                        localPosition.x = -0.3f * (content.Length + 1);
                    }
                    else
                    {
                        localPosition.x = -0.3f * (content.Length + 1);
                    }
                    obj3.transform.localPosition = localPosition;
                }
                if (int.TryParse(content, out num))
                {
                    Vector3 localScale = gameObject.transform.localScale;
                    if (num > 0x5dc)
                    {
                        localScale.x = 1.2f;
                        localScale.y = 1.2f;
                    }
                    else if (num > 600)
                    {
                        localScale.x = 1.1f;
                        localScale.y = 1.1f;
                    }
                    else if (num > 300)
                    {
                        localScale.x = 1f;
                        localScale.y = 1f;
                    }
                    else if (num > 100)
                    {
                        localScale.x = 0.8f;
                        localScale.y = 0.8f;
                    }
                    else
                    {
                        localScale.x = 0.7f;
                        localScale.y = 0.7f;
                    }
                    gameObject.transform.localScale = localScale;
                }
                animator.Play(animatorState);
                component.text = content;
                this.m_floatTexts.Enqueue(gameObject);
                Singleton<CTimerManager>.GetInstance().AddTimer(0x7d0, 1, new CTimer.OnTimeUpHandler(this.OnRecycle));
            }
        }

        public void CreateOtherFloatText(enOtherFloatTextContent otherFloatTextContent, ref Vector3 worldPosition, params string[] args)
        {
            if (GameSettings.RenderQuality != SGameRenderQuality.Low)
            {
                string text = Singleton<CTextManager>.GetInstance().GetText(s_otherFloatTextKeys[(int) otherFloatTextContent], args);
                this.CreateBattleFloatText(text, ref worldPosition, s_otherFloatTextAnimatorStates[(int) otherFloatTextContent]);
            }
        }

        public void CreateRestrictFloatText(RESTRICT_TYPE restrictType, ref Vector3 worldPosition)
        {
            string text = Singleton<CTextManager>.GetInstance().GetText(s_restrictTextKeys[(int) restrictType]);
            this.CreateBattleFloatText(text, ref worldPosition, s_restrictTextAnimatorState);
        }

        public void LateUpdate()
        {
        }

        private void OnRecycle(int timerSequence)
        {
            if (this.m_floatTexts.Count != 0)
            {
                GameObject pooledGameObject = this.m_floatTexts.Dequeue();
                Singleton<CGameObjectPool>.GetInstance().RecycleGameObject(pooledGameObject);
            }
        }

        public static void Preload(ref ActorPreloadTab preloadTab)
        {
            preloadTab.AddParticle("Text/FloatText/FloatText.prefab");
        }
    }
}

