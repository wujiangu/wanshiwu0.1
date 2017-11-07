﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class cs_GuiSceneWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(cs.GuiScene), typeof(System.Object));
		L.RegFunction("Init", Init);
		L.RegFunction("Reset", Reset);
		L.RegFunction("Hide", Hide);
		L.RegFunction("Clear", Clear);
		L.RegFunction("LoadGuiObject", LoadGuiObject);
		L.RegFunction("UnloadGuiObject", UnloadGuiObject);
		L.RegFunction("FindGuiObject", FindGuiObject);
		L.RegFunction("GetMaxOrderInLayer", GetMaxOrderInLayer);
		L.RegFunction("ResetOrderInLayer", ResetOrderInLayer);
		L.RegFunction("SetInitCallback", SetInitCallback);
		L.RegFunction("New", _Createcs_GuiScene);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("Name", get_Name, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _Createcs_GuiScene(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 1)
			{
				string arg0 = ToLua.CheckString(L, 1);
				cs.GuiScene obj = new cs.GuiScene(arg0);
				ToLua.PushObject(L, obj);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to ctor method: cs.GuiScene.New");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Init(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			cs.GuiScene obj = (cs.GuiScene)ToLua.CheckObject<cs.GuiScene>(L, 1);
			obj.Init();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Reset(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			cs.GuiScene obj = (cs.GuiScene)ToLua.CheckObject<cs.GuiScene>(L, 1);
			obj.Reset();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Hide(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			cs.GuiScene obj = (cs.GuiScene)ToLua.CheckObject<cs.GuiScene>(L, 1);
			obj.Hide();
			return 0;
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
			cs.GuiScene obj = (cs.GuiScene)ToLua.CheckObject<cs.GuiScene>(L, 1);
			obj.Clear();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int LoadGuiObject(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				cs.GuiScene obj = (cs.GuiScene)ToLua.CheckObject<cs.GuiScene>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				cs.GuiObject o = obj.LoadGuiObject(arg0);
				ToLua.Push(L, o);
				return 1;
			}
			else if (count == 3)
			{
				cs.GuiScene obj = (cs.GuiScene)ToLua.CheckObject<cs.GuiScene>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				string arg1 = ToLua.CheckString(L, 3);
				cs.GuiObject o = obj.LoadGuiObject(arg0, arg1);
				ToLua.Push(L, o);
				return 1;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: cs.GuiScene.LoadGuiObject");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int UnloadGuiObject(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiScene obj = (cs.GuiScene)ToLua.CheckObject<cs.GuiScene>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.UnloadGuiObject(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int FindGuiObject(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiScene obj = (cs.GuiScene)ToLua.CheckObject<cs.GuiScene>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			cs.GuiObject o = obj.FindGuiObject(arg0);
			ToLua.Push(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetMaxOrderInLayer(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiScene obj = (cs.GuiScene)ToLua.CheckObject<cs.GuiScene>(L, 1);
			cs.EGuiLayer arg0 = (cs.EGuiLayer)ToLua.CheckObject(L, 2, typeof(cs.EGuiLayer));
			int o = obj.GetMaxOrderInLayer(arg0);
			LuaDLL.lua_pushinteger(L, o);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ResetOrderInLayer(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 3);
			cs.GuiScene obj = (cs.GuiScene)ToLua.CheckObject<cs.GuiScene>(L, 1);
			cs.EGuiLayer arg0 = (cs.EGuiLayer)ToLua.CheckObject(L, 2, typeof(cs.EGuiLayer));
			int arg1 = (int)LuaDLL.luaL_checknumber(L, 3);
			obj.ResetOrderInLayer(arg0, ref arg1);
			LuaDLL.lua_pushinteger(L, arg1);
			return 1;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetInitCallback(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiScene obj = (cs.GuiScene)ToLua.CheckObject<cs.GuiScene>(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 2);
			obj.SetInitCallback(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_Name(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			cs.GuiScene obj = (cs.GuiScene)o;
			string ret = obj.Name;
			LuaDLL.lua_pushstring(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index Name on a nil value");
		}
	}
}
