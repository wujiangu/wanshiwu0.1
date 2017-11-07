using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

namespace cs
{
    /// <summary>
    /// UI粒子控件
    /// UI粒子的sortingorder默认比其他UI元素大1，也就是所有的UI粒子都是显示在其他UI控件上面
    /// 如果有特殊需要，则自行在编辑器里组织canvas
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class GuiParticleSystem : GuiControl
    {
        ParticleSystem m_particleSystem = null;
        List<ParticleSystemRenderer> m_renders = new List<ParticleSystemRenderer>();

        public GuiParticleSystem()
        {
            SortingOrderOffset = 1;
        }

        public override bool Initialize()
        {
            if (base.Initialize())
            {
                m_particleSystem = gameObject.GetComponent<ParticleSystem>();
                Assert.IsNotNull(m_particleSystem);
                InitRenderers();
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Clear()
        {
            m_particleSystem = null;
            m_renders.Clear();
            base.Clear();
        }

        public void InitRenderers()
        {
            m_renders.Clear();
            ParticleSystemRenderer[] allChildRenders = gameObject.GetComponentsInChildren<ParticleSystemRenderer>();
            for (int i = 0; i < allChildRenders.Length; ++i)
            {
                ParticleSystemRenderer temp = allChildRenders[i];
                if (Utility.FindGuiCtrlOwner(temp.transform) == this)
                {
                    m_renders.Add(allChildRenders[i]);
                }
            }
        }

        public override void SetSortingOrder(int a_nOrder)
        {
            if (m_renders != null)
            {
                int nOrder = a_nOrder + SortingOrderOffset;
                for (int i = 0; i < m_renders.Count; ++i)
                {
                    m_renders[i].sortingOrder = nOrder;
                }
            }
        }

#if UNITY_EDITOR

        private void Start()
        {
            Initialize();
        }
#endif
    }
}

