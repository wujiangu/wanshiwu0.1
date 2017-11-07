using System;
using System.IO;
using System.Runtime.CompilerServices;

public class StreamWriterProxy : IDisposable
{
    private bool bValid;
    private StreamWriter Writer;

    public StreamWriterProxy(string InFilePath, bool bInCreateNew)
    {
        <StreamWriterProxy>c__AnonStorey3D storeyd = new <StreamWriterProxy>c__AnonStorey3D {
            InFilePath = InFilePath,
            bInCreateNew = bInCreateNew
        };
        storeyd.<>f__this = this;
        Singleton<BackgroundWorker>.instance.AddBackgroudOperation(new BackgroundWorker.BackgroudDelegate(storeyd.<>m__3A));
        this.bValid = true;
    }

    public void Close()
    {
        Singleton<BackgroundWorker>.instance.AddBackgroudOperation(() => this.Close_MT());
    }

    protected void Close_MT()
    {
        if (this.Writer != null)
        {
            this.Writer.Close();
        }
    }

    public void Dispose()
    {
        Singleton<BackgroundWorker>.instance.AddBackgroudOperation(() => this.Dispose_MT());
        this.bValid = false;
    }

    protected void Dispose_MT()
    {
        if (this.Writer != null)
        {
            this.Writer.Dispose();
        }
    }

    public void Flush()
    {
        Singleton<BackgroundWorker>.instance.AddBackgroudOperation(() => this.Flush_MT());
    }

    protected void Flush_MT()
    {
        if (this.Writer != null)
        {
            this.Writer.Flush();
        }
    }

    protected void Reset_MT(string InFilePath, bool bInCreateNew)
    {
        try
        {
            FileStream stream = null;
            if (bInCreateNew)
            {
                stream = new FileStream(InFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            }
            else
            {
                stream = new FileStream(InFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            }
            this.Writer = new StreamWriter(stream);
            this.bValid = true;
        }
        catch (Exception)
        {
        }
    }

    public void WriteLine(string InText)
    {
        <WriteLine>c__AnonStorey3E storeye = new <WriteLine>c__AnonStorey3E {
            InText = InText,
            <>f__this = this
        };
        DebugHelper.Assert(this.bValid, "Should not WriteLine When the Proxy is not valid!");
        Singleton<BackgroundWorker>.instance.AddBackgroudOperation(new BackgroundWorker.BackgroudDelegate(storeye.<>m__3E));
    }

    protected void WriteLine_MT(string InText)
    {
        if (this.Writer != null)
        {
            this.Writer.WriteLine(InText);
        }
    }

    public bool isValid
    {
        get
        {
            return this.bValid;
        }
    }

    [CompilerGenerated]
    private sealed class <StreamWriterProxy>c__AnonStorey3D
    {
        internal StreamWriterProxy <>f__this;
        internal bool bInCreateNew;
        internal string InFilePath;

        internal void <>m__3A()
        {
            this.<>f__this.Reset_MT(this.InFilePath, this.bInCreateNew);
        }
    }

    [CompilerGenerated]
    private sealed class <WriteLine>c__AnonStorey3E
    {
        internal StreamWriterProxy <>f__this;
        internal string InText;

        internal void <>m__3E()
        {
            this.<>f__this.WriteLine_MT(this.InText);
        }
    }
}

