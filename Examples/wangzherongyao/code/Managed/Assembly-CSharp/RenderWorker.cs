using System;

public class RenderWorker : WorkerThreadBase<RenderWorker>
{
    protected override void _PrepareWorkerData()
    {
        FogOfWar.PrepareData();
    }

    protected override void _Run()
    {
        FogOfWar.Run();
    }

    protected override void BeforeStart()
    {
        base.BeforeStart();
        FogOfWar.EnableShaderFogFunction();
    }

    public void BeginLevel()
    {
        base.GetLock();
        try
        {
            FogOfWar.BeginLevel();
        }
        catch (Exception)
        {
        }
        finally
        {
            base.ReleaseLock();
        }
    }
}

