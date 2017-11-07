using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public static class AkBankManager
{
    private static DictionaryView<string, AkBankHandle> m_BankHandles = new DictionaryView<string, AkBankHandle>();
    private static Mutex m_Mutex = new Mutex();

    public static void GlobalBankCallback(uint in_bankID, IntPtr in_pInMemoryBankPtr, AKRESULT in_eLoadResult, uint in_memPoolId, object in_Cookie)
    {
        m_Mutex.WaitOne();
        AkBankHandle handle = (AkBankHandle) in_Cookie;
        AkCallbackManager.BankCallback bankCallback = handle.bankCallback;
        if (in_eLoadResult != AKRESULT.AK_Success)
        {
            Debug.LogWarning("Wwise: Bank " + handle.bankName + " failed to load (" + in_eLoadResult.ToString() + ")");
            m_BankHandles.Remove(handle.bankName);
        }
        m_Mutex.ReleaseMutex();
        if (bankCallback != null)
        {
            bankCallback(in_bankID, in_pInMemoryBankPtr, in_eLoadResult, in_memPoolId, null);
        }
    }

    public static void LoadBank(string name)
    {
        m_Mutex.WaitOne();
        AkBankHandle handle = null;
        if (!m_BankHandles.TryGetValue(name, out handle))
        {
            handle = new AkBankHandle(name);
            m_BankHandles.Add(name, handle);
            m_Mutex.ReleaseMutex();
            handle.LoadBank();
        }
        else
        {
            m_Mutex.ReleaseMutex();
        }
    }

    public static void LoadBank(string name, byte[] data)
    {
        m_Mutex.WaitOne();
        AkBankHandle handle = null;
        if (!m_BankHandles.TryGetValue(name, out handle))
        {
            handle = new AkBankHandle(name);
            m_BankHandles.Add(name, handle);
            m_Mutex.ReleaseMutex();
            handle.LoadBank(data);
        }
        else
        {
            m_Mutex.ReleaseMutex();
        }
    }

    public static void LoadBankAsync(string name, AkCallbackManager.BankCallback callback = null)
    {
        m_Mutex.WaitOne();
        AkBankHandle handle = null;
        if (!m_BankHandles.TryGetValue(name, out handle))
        {
            handle = new AkBankHandle(name);
            m_BankHandles.Add(name, handle);
            m_Mutex.ReleaseMutex();
            handle.LoadBankAsync(callback);
        }
        else
        {
            m_Mutex.ReleaseMutex();
        }
    }

    public static void UnloadBank(string name)
    {
        m_Mutex.WaitOne();
        AkBankHandle handle = null;
        if (m_BankHandles.TryGetValue(name, out handle))
        {
            handle.DecRef();
            if (handle.RefCount == 0)
            {
                m_BankHandles.Remove(name);
            }
        }
        m_Mutex.ReleaseMutex();
    }
}

