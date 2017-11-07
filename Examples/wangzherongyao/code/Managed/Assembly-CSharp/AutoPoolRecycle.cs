using AGE;
using System;
using UnityEngine;

internal class AutoPoolRecycle : MonoBehaviour
{
    private float m_lifeTime = 10f;
    public bool m_needRecordNumber;
    private float m_timer;

    public void OnEnable()
    {
        this.m_timer = 0f;
    }

    private void Update()
    {
        this.m_timer += Time.deltaTime;
        if (this.m_timer > this.m_lifeTime)
        {
            base.gameObject.transform.position = new Vector3(10000f, 10000f, 10000f);
            Singleton<CGameObjectPool>.GetInstance().RecycleGameObject(base.gameObject);
            if (this.m_needRecordNumber)
            {
                ParticleHelper.DecParticleActiveNumber();
            }
        }
    }

    public float lifeTime
    {
        set
        {
            this.m_lifeTime = value;
        }
    }
}

