namespace Assets.Scripts.GameLogic
{
    using System;
    using UnityEngine;

    public sealed class JoystickMode : GameInputMode
    {
        public int[] m_buttonStates;
        public Vector2 m_dpad;
        public int m_dpadState;
        public Vector2 m_leftAxis;
        public int m_leftAxisState;
        public float m_leftTrigger;
        public int m_leftTriggerState;
        public Vector2 m_rightAxis;
        public int m_rightAxisState;
        public float m_rightTrigger;
        public int m_rightTriggerState;
        private SkillSlotType m_selectedSkillSlot;
        public static string[] s_joystickButtons = new string[] { "JoystickButtonX", "JoystickButtonY", "JoystickButtonA", "JoystickButtonB", "JoystickButtonL", "JoystickButtonR", "JoystickButtonSelect", "JoystickButtonStart", "JoystickButtonL3", "JoystickButtonR3" };
        public static string s_joystickDPadHorizontal = "JoystickDPadHorizontal";
        public static string s_joystickDPadVertical = "JoystickDPadVertical";
        public static string s_joystickLeftAxisHorizontal = "JoystickLeftAxisHorizontal";
        public static string s_joystickLeftAxisVertical = "JoystickLeftAxisVertical";
        public static string s_joystickLeftTrigger = "JoystickLeftTrigger";
        public static string s_joystickRightAxisHorizontal = "JoystickRightAxisHorizontal";
        public static string s_joystickRightAxisVertical = "JoystickRightAxisVertical";
        public static string s_joystickRightTrigger = "JoystickRightTrigger";

        public JoystickMode(GameInput InSys) : base(InSys)
        {
            this.m_buttonStates = new int[s_joystickButtons.Length];
            this.m_selectedSkillSlot = SkillSlotType.SLOT_SKILL_VALID;
        }

        public override void OnStateEnter()
        {
        }

        public override void OnStateLeave()
        {
        }

        public override void Update()
        {
        }

        public enum enJoystickButtonState
        {
            None,
            Down,
            Hold,
            Up
        }

        public enum enJoystickInput
        {
            LeftAxis,
            RightAxis,
            LeftTrigger,
            RightTrigger,
            DPad,
            ButtonX,
            ButtonY,
            ButtonA,
            ButtonB,
            ButtonL1,
            ButtonR1,
            ButtonSelect,
            ButtonStart,
            ButtonL3,
            ButtonR3
        }
    }
}

