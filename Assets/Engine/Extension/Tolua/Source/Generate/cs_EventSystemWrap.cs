﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class cs_EventSystemWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(cs.EventSystem), typeof(cs.Singleton<cs.EventSystem>));
		L.RegFunction("GetBuildInEventCount", GetBuildInEventCount);
		L.RegFunction("DefineEvent", DefineEvent);
		L.RegFunction("RegisterEventHandler", RegisterEventHandler);
		L.RegFunction("UnRegisterEventHandler", UnRegisterEventHandler);
		L.RegFunction("SendEvent", SendEvent);
		L.RegFunction("Tick", Tick);
		L.RegFunction("New", _Createcs_EventSystem);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _Createcs_EventSystem(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 0)
			{
				cs.EventSystem obj = new cs.EventSystem();
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: cs.EventSystem.New");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetBuildInEventCount(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
			int o = obj.GetBuildInEventCount();
			LuaDLL.lua_pushinteger(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int DefineEvent(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
			int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
			string arg1 = ToLua.CheckString(L, 3);
			obj.DefineEvent(arg0, arg1);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RegisterEventHandler(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 3 && TypeChecker.CheckTypes<LuaInterface.LuaFunction>(L, 3))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
				LuaFunction arg1 = ToLua.ToLuaFunction(L, 3);
				obj.RegisterEventHandler(arg0, arg1);
				return 0;
			}
			else if (count == 3 && TypeChecker.CheckTypes<UnityEngine.Events.UnityAction<cs.Event>>(L, 3))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
				UnityEngine.Events.UnityAction<cs.Event> arg1 = (UnityEngine.Events.UnityAction<cs.Event>)ToLua.ToObject(L, 3);
				obj.RegisterEventHandler(arg0, arg1);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: cs.EventSystem.RegisterEventHandler");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int UnRegisterEventHandler(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 3 && TypeChecker.CheckTypes<LuaInterface.LuaFunction>(L, 3))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
				LuaFunction arg1 = ToLua.ToLuaFunction(L, 3);
				obj.UnRegisterEventHandler(arg0, arg1);
				return 0;
			}
			else if (count == 3 && TypeChecker.CheckTypes<UnityEngine.Events.UnityAction<cs.Event>>(L, 3))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				int arg0 = (int)LuaDLL.luaL_checknumber(L, 2);
				UnityEngine.Events.UnityAction<cs.Event> arg1 = (UnityEngine.Events.UnityAction<cs.Event>)ToLua.ToObject(L, 3);
				obj.UnRegisterEventHandler(arg0, arg1);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: cs.EventSystem.UnRegisterEventHandler");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SendEvent(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes<int>(L, 2))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				int arg0 = (int)LuaDLL.lua_tonumber(L, 2);
				obj.SendEvent(arg0);
				return 0;
			}
			else if (count == 2 && TypeChecker.CheckTypes<string>(L, 2))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				string arg0 = ToLua.ToString(L, 2);
				obj.SendEvent(arg0);
				return 0;
			}
			else if (count == 3 && TypeChecker.CheckTypes<string, object>(L, 2))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				string arg0 = ToLua.ToString(L, 2);
				object arg1 = ToLua.ToVarObject(L, 3);
				obj.SendEvent(arg0, arg1);
				return 0;
			}
			else if (count == 3 && TypeChecker.CheckTypes<int, object>(L, 2))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				int arg0 = (int)LuaDLL.lua_tonumber(L, 2);
				object arg1 = ToLua.ToVarObject(L, 3);
				obj.SendEvent(arg0, arg1);
				return 0;
			}
			else if (count == 4 && TypeChecker.CheckTypes<int, object, object>(L, 2))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				int arg0 = (int)LuaDLL.lua_tonumber(L, 2);
				object arg1 = ToLua.ToVarObject(L, 3);
				object arg2 = ToLua.ToVarObject(L, 4);
				obj.SendEvent(arg0, arg1, arg2);
				return 0;
			}
			else if (count == 4 && TypeChecker.CheckTypes<string, object, object>(L, 2))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				string arg0 = ToLua.ToString(L, 2);
				object arg1 = ToLua.ToVarObject(L, 3);
				object arg2 = ToLua.ToVarObject(L, 4);
				obj.SendEvent(arg0, arg1, arg2);
				return 0;
			}
			else if (count == 5 && TypeChecker.CheckTypes<string, object, object, object>(L, 2))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				string arg0 = ToLua.ToString(L, 2);
				object arg1 = ToLua.ToVarObject(L, 3);
				object arg2 = ToLua.ToVarObject(L, 4);
				object arg3 = ToLua.ToVarObject(L, 5);
				obj.SendEvent(arg0, arg1, arg2, arg3);
				return 0;
			}
			else if (count == 5 && TypeChecker.CheckTypes<int, object, object, object>(L, 2))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				int arg0 = (int)LuaDLL.lua_tonumber(L, 2);
				object arg1 = ToLua.ToVarObject(L, 3);
				object arg2 = ToLua.ToVarObject(L, 4);
				object arg3 = ToLua.ToVarObject(L, 5);
				obj.SendEvent(arg0, arg1, arg2, arg3);
				return 0;
			}
			else if (count == 6 && TypeChecker.CheckTypes<int, object, object, object, object>(L, 2))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				int arg0 = (int)LuaDLL.lua_tonumber(L, 2);
				object arg1 = ToLua.ToVarObject(L, 3);
				object arg2 = ToLua.ToVarObject(L, 4);
				object arg3 = ToLua.ToVarObject(L, 5);
				object arg4 = ToLua.ToVarObject(L, 6);
				obj.SendEvent(arg0, arg1, arg2, arg3, arg4);
				return 0;
			}
			else if (count == 6 && TypeChecker.CheckTypes<string, object, object, object, object>(L, 2))
			{
				cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
				string arg0 = ToLua.ToString(L, 2);
				object arg1 = ToLua.ToVarObject(L, 3);
				object arg2 = ToLua.ToVarObject(L, 4);
				object arg3 = ToLua.ToVarObject(L, 5);
				object arg4 = ToLua.ToVarObject(L, 6);
				obj.SendEvent(arg0, arg1, arg2, arg3, arg4);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: cs.EventSystem.SendEvent");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Tick(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.EventSystem obj = (cs.EventSystem)ToLua.CheckObject<cs.EventSystem>(L, 1);
			float arg0 = (float)LuaDLL.luaL_checknumber(L, 2);
			obj.Tick(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}
}

