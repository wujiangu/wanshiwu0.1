﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class cs_GuiLabelWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(cs.GuiLabel), typeof(cs.GuiControl));
		L.RegFunction("Initialize", Initialize);
		L.RegFunction("Clear", Clear);
		L.RegFunction("SetLabel", SetLabel);
		L.RegFunction("SetHyperlinkCallback", SetHyperlinkCallback);
		L.RegFunction("SetHyperlinkCallbackScript", SetHyperlinkCallbackScript);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Initialize(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			cs.GuiLabel obj = (cs.GuiLabel)ToLua.CheckObject<cs.GuiLabel>(L, 1);
			bool o = obj.Initialize();
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Clear(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			cs.GuiLabel obj = (cs.GuiLabel)ToLua.CheckObject<cs.GuiLabel>(L, 1);
			obj.Clear();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetLabel(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiLabel obj = (cs.GuiLabel)ToLua.CheckObject<cs.GuiLabel>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.SetLabel(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetHyperlinkCallback(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiLabel obj = (cs.GuiLabel)ToLua.CheckObject<cs.GuiLabel>(L, 1);
			UnityEngine.Events.UnityAction<string> arg0 = (UnityEngine.Events.UnityAction<string>)ToLua.CheckDelegate<UnityEngine.Events.UnityAction<string>>(L, 2);
			obj.SetHyperlinkCallback(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetHyperlinkCallbackScript(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiLabel obj = (cs.GuiLabel)ToLua.CheckObject<cs.GuiLabel>(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 2);
			obj.SetHyperlinkCallbackScript(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int op_Equality(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			UnityEngine.Object arg0 = (UnityEngine.Object)ToLua.ToObject(L, 1);
			UnityEngine.Object arg1 = (UnityEngine.Object)ToLua.ToObject(L, 2);
			bool o = arg0 == arg1;
			LuaDLL.lua_pushboolean(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

