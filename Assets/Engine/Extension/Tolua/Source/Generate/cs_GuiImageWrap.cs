﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class cs_GuiImageWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(cs.GuiImage), typeof(cs.GuiControl));
		L.RegFunction("Initialize", Initialize);
		L.RegFunction("Clear", Clear);
		L.RegFunction("SetDefaultSprite", SetDefaultSprite);
		L.RegFunction("SetSprite", SetSprite);
		L.RegFunction("SetNativeSize", SetNativeSize);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("UGUI_Image", get_UGUI_Image, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Initialize(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			cs.GuiImage obj = (cs.GuiImage)ToLua.CheckObject<cs.GuiImage>(L, 1);
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
			cs.GuiImage obj = (cs.GuiImage)ToLua.CheckObject<cs.GuiImage>(L, 1);
			obj.Clear();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetDefaultSprite(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiImage obj = (cs.GuiImage)ToLua.CheckObject<cs.GuiImage>(L, 1);
			string arg0 = ToLua.CheckString(L, 2);
			obj.SetDefaultSprite(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetSprite(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2)
			{
				cs.GuiImage obj = (cs.GuiImage)ToLua.CheckObject<cs.GuiImage>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				obj.SetSprite(arg0);
				return 0;
			}
			else if (count == 3)
			{
				cs.GuiImage obj = (cs.GuiImage)ToLua.CheckObject<cs.GuiImage>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				obj.SetSprite(arg0, arg1);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: cs.GuiImage.SetSprite");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int SetNativeSize(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			cs.GuiImage obj = (cs.GuiImage)ToLua.CheckObject<cs.GuiImage>(L, 1);
			obj.SetNativeSize();
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

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_UGUI_Image(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			cs.GuiImage obj = (cs.GuiImage)o;
			UnityEngine.UI.Image ret = obj.UGUI_Image;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index UGUI_Image on a nil value");
		}
	}
}

