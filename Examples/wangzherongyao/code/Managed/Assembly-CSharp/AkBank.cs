using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Wwise/AkBank")]
public class AkBank : AkUnityEventHandler
{
    public string bankName = string.Empty;
    public bool loadAsynchronous;
    public List<int> unloadTriggerList = new List<int> { -358577003 };

    protected override void Awake()
    {
        base.Awake();
        base.RegisterTriggers(this.unloadTriggerList, new AkTriggerBase.Trigger(this.UnloadBank));
        if (this.unloadTriggerList.Contains(0x449d8dae))
        {
            this.UnloadBank(null);
        }
    }

    public override void HandleEvent(GameObject in_gameObject)
    {
        if (!this.loadAsynchronous)
        {
            AkBankManager.LoadBank(this.bankName);
        }
        else
        {
            AkBankManager.LoadBankAsync(this.bankName, null);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        base.UnregisterTriggers(this.unloadTriggerList, new AkTriggerBase.Trigger(this.UnloadBank));
        if (this.unloadTriggerList.Contains(-358577003))
        {
            this.UnloadBank(null);
        }
    }

    protected override void Start()
    {
        base.Start();
        if (this.unloadTriggerList.Contains(0x4c66e1f7))
        {
            this.UnloadBank(null);
        }
    }

    public void UnloadBank(GameObject in_gameObject)
    {
        AkBankManager.UnloadBank(this.bankName);
    }
}

