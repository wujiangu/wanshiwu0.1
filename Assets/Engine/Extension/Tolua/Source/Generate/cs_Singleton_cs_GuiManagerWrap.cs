﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class cs_Singleton_cs_GuiManagerWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(cs.Singleton<cs.GuiManager>), typeof(System.Object), "Singleton_cs_GuiManager");
		L.RegFunction("Get", Get);
		L.RegFunction("Destroy", Destroy);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Get(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			cs.GuiManager o = cs.Singleton<cs.GuiManager>.Get();
			ToLua.PushObject(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Destroy(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 0);
			cs.Singleton<cs.GuiManager>.Destroy();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}
