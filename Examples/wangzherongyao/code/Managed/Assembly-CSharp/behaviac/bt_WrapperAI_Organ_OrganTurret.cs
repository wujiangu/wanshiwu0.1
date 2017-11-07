﻿namespace behaviac
{
    using System;

    public static class bt_WrapperAI_Organ_OrganTurret
    {
        public static bool build_behavior_tree(BehaviorTree bt)
        {
            bt.SetClassNameString("BehaviorTree");
            bt.SetId(-1);
            bt.SetName("WrapperAI/Organ/OrganTurret");
            bt.AddPar("Assets.Scripts.GameLogic.SkillSlotType", "p_curSlotType", "SLOT_SKILL_0", string.Empty);
            bt.AddPar("uint", "p_targetID", "0", string.Empty);
            bt.AddPar("int", "p_srchRange", "0", string.Empty);
            bt.AddPar("UnityEngine.Vector3", "p_AttackMoveDest", "{kEpsilon=0;x=0;y=0;z=0;}", string.Empty);
            bt.AddPar("bool", "p_IsAttackMove_Attack", "false", string.Empty);
            bt.AddPar("bool", "p_AttackIsFinished", "true", string.Empty);
            bt.AddPar("uint", "p_CmdID", "0", string.Empty);
            bt.AddPar("UnityEngine.Vector3", "p_attackPathCurTargetPos", "{kEpsilon=0;x=0;y=0;z=0;}", string.Empty);
            DecoratorLoop_bt_WrapperAI_Organ_OrganTurret_node14 pChild = new DecoratorLoop_bt_WrapperAI_Organ_OrganTurret_node14();
            pChild.SetClassNameString("DecoratorLoop");
            pChild.SetId(14);
            bt.AddChild(pChild);
            SelectorLoop loop = new SelectorLoop();
            loop.SetClassNameString("SelectorLoop");
            loop.SetId(1);
            pChild.AddChild(loop);
            WithPrecondition precondition = new WithPrecondition();
            precondition.SetClassNameString("WithPrecondition");
            precondition.SetId(11);
            loop.AddChild(precondition);
            Or or = new Or();
            or.SetClassNameString("Or");
            or.SetId(0);
            precondition.AddChild(or);
            Condition_bt_WrapperAI_Organ_OrganTurret_node12 _node2 = new Condition_bt_WrapperAI_Organ_OrganTurret_node12();
            _node2.SetClassNameString("Condition");
            _node2.SetId(12);
            or.AddChild(_node2);
            or.SetHasEvents(or.HasEvents() | _node2.HasEvents());
            Condition_bt_WrapperAI_Organ_OrganTurret_node26 _node3 = new Condition_bt_WrapperAI_Organ_OrganTurret_node26();
            _node3.SetClassNameString("Condition");
            _node3.SetId(0x1a);
            or.AddChild(_node3);
            or.SetHasEvents(or.HasEvents() | _node3.HasEvents());
            Condition_bt_WrapperAI_Organ_OrganTurret_node27 _node4 = new Condition_bt_WrapperAI_Organ_OrganTurret_node27();
            _node4.SetClassNameString("Condition");
            _node4.SetId(0x1b);
            or.AddChild(_node4);
            or.SetHasEvents(or.HasEvents() | _node4.HasEvents());
            precondition.SetHasEvents(precondition.HasEvents() | or.HasEvents());
            Sequence sequence = new Sequence();
            sequence.SetClassNameString("Sequence");
            sequence.SetId(0x56);
            precondition.AddChild(sequence);
            Action_bt_WrapperAI_Organ_OrganTurret_node140 _node5 = new Action_bt_WrapperAI_Organ_OrganTurret_node140();
            _node5.SetClassNameString("Action");
            _node5.SetId(140);
            sequence.AddChild(_node5);
            sequence.SetHasEvents(sequence.HasEvents() | _node5.HasEvents());
            Action_bt_WrapperAI_Organ_OrganTurret_node88 _node6 = new Action_bt_WrapperAI_Organ_OrganTurret_node88();
            _node6.SetClassNameString("Action");
            _node6.SetId(0x58);
            sequence.AddChild(_node6);
            sequence.SetHasEvents(sequence.HasEvents() | _node6.HasEvents());
            Action_bt_WrapperAI_Organ_OrganTurret_node91 _node7 = new Action_bt_WrapperAI_Organ_OrganTurret_node91();
            _node7.SetClassNameString("Action");
            _node7.SetId(0x5b);
            sequence.AddChild(_node7);
            sequence.SetHasEvents(sequence.HasEvents() | _node7.HasEvents());
            DecoratorLoopUntil_bt_WrapperAI_Organ_OrganTurret_node92 _node8 = new DecoratorLoopUntil_bt_WrapperAI_Organ_OrganTurret_node92();
            _node8.SetClassNameString("DecoratorLoopUntil");
            _node8.SetId(0x5c);
            sequence.AddChild(_node8);
            Condition_bt_WrapperAI_Organ_OrganTurret_node93 _node9 = new Condition_bt_WrapperAI_Organ_OrganTurret_node93();
            _node9.SetClassNameString("Condition");
            _node9.SetId(0x5d);
            _node8.AddChild(_node9);
            _node8.SetHasEvents(_node8.HasEvents() | _node9.HasEvents());
            sequence.SetHasEvents(sequence.HasEvents() | _node8.HasEvents());
            precondition.SetHasEvents(precondition.HasEvents() | sequence.HasEvents());
            loop.SetHasEvents(loop.HasEvents() | precondition.HasEvents());
            WithPrecondition precondition2 = new WithPrecondition();
            precondition2.SetClassNameString("WithPrecondition");
            precondition2.SetId(5);
            loop.AddChild(precondition2);
            Condition_bt_WrapperAI_Organ_OrganTurret_node9 _node10 = new Condition_bt_WrapperAI_Organ_OrganTurret_node9();
            _node10.SetClassNameString("Condition");
            _node10.SetId(9);
            precondition2.AddChild(_node10);
            precondition2.SetHasEvents(precondition2.HasEvents() | _node10.HasEvents());
            Sequence sequence2 = new Sequence();
            sequence2.SetClassNameString("Sequence");
            sequence2.SetId(0x3b);
            precondition2.AddChild(sequence2);
            Selector selector = new Selector();
            selector.SetClassNameString("Selector");
            selector.SetId(0x1e7);
            sequence2.AddChild(selector);
            Action_bt_WrapperAI_Organ_OrganTurret_node488 _node11 = new Action_bt_WrapperAI_Organ_OrganTurret_node488();
            _node11.SetClassNameString("Action");
            _node11.SetId(0x1e8);
            selector.AddChild(_node11);
            selector.SetHasEvents(selector.HasEvents() | _node11.HasEvents());
            Action_bt_WrapperAI_Organ_OrganTurret_node10 _node12 = new Action_bt_WrapperAI_Organ_OrganTurret_node10();
            _node12.SetClassNameString("Action");
            _node12.SetId(10);
            selector.AddChild(_node12);
            selector.SetHasEvents(selector.HasEvents() | _node12.HasEvents());
            sequence2.SetHasEvents(sequence2.HasEvents() | selector.HasEvents());
            DecoratorLoop_bt_WrapperAI_Organ_OrganTurret_node65 _node13 = new DecoratorLoop_bt_WrapperAI_Organ_OrganTurret_node65();
            _node13.SetClassNameString("DecoratorLoop");
            _node13.SetId(0x41);
            sequence2.AddChild(_node13);
            Noop noop = new Noop();
            noop.SetClassNameString("Noop");
            noop.SetId(0x42);
            _node13.AddChild(noop);
            _node13.SetHasEvents(_node13.HasEvents() | noop.HasEvents());
            sequence2.SetHasEvents(sequence2.HasEvents() | _node13.HasEvents());
            precondition2.SetHasEvents(precondition2.HasEvents() | sequence2.HasEvents());
            loop.SetHasEvents(loop.HasEvents() | precondition2.HasEvents());
            pChild.SetHasEvents(pChild.HasEvents() | loop.HasEvents());
            bt.SetHasEvents(bt.HasEvents() | pChild.HasEvents());
            return true;
        }
    }
}

