﻿//this source code was auto-generated by tolua#, do not modify it
using System;
using LuaInterface;

public class cs_GuiButtonWrap
{
	public static void Register(LuaState L)
	{
		L.BeginClass(typeof(cs.GuiButton), typeof(cs.GuiControl));
		L.RegFunction("Initialize", Initialize);
		L.RegFunction("Clear", Clear);
		L.RegFunction("AddListener", AddListener);
		L.RegFunction("RemoveListener", RemoveListener);
		L.RegFunction("RemoveAllListener", RemoveAllListener);
		L.RegFunction("SetDefaultSprite", SetDefaultSprite);
		L.RegFunction("SetSprite", SetSprite);
		L.RegFunction("SetNativeSize", SetNativeSize);
		L.RegFunction("OnPointerDown", OnPointerDown);
		L.RegFunction("OnPointerUp", OnPointerUp);
		L.RegFunction("AddMouseDownListener", AddMouseDownListener);
		L.RegFunction("RemoveMouseDownListener", RemoveMouseDownListener);
		L.RegFunction("AddMouseUpListener", AddMouseUpListener);
		L.RegFunction("RemoveMouseUpListener", RemoveMouseUpListener);
		L.RegFunction("__eq", op_Equality);
		L.RegFunction("__tostring", ToLua.op_ToString);
		L.RegVar("UGUI_Button", get_UGUI_Button, null);
		L.RegVar("UGUI_Image", get_UGUI_Image, null);
		L.EndClass();
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Initialize(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
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
			cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
			obj.Clear();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddListener(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes<LuaInterface.LuaFunction>(L, 2))
			{
				cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
				LuaFunction arg0 = ToLua.ToLuaFunction(L, 2);
				obj.AddListener(arg0);
				return 0;
			}
			else if (count == 2 && TypeChecker.CheckTypes<UnityEngine.Events.UnityAction>(L, 2))
			{
				cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
				UnityEngine.Events.UnityAction arg0 = (UnityEngine.Events.UnityAction)ToLua.ToObject(L, 2);
				obj.AddListener(arg0);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: cs.GuiButton.AddListener");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemoveListener(IntPtr L)
	{
		try
		{
			int count = LuaDLL.lua_gettop(L);

			if (count == 2 && TypeChecker.CheckTypes<LuaInterface.LuaFunction>(L, 2))
			{
				cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
				LuaFunction arg0 = ToLua.ToLuaFunction(L, 2);
				obj.RemoveListener(arg0);
				return 0;
			}
			else if (count == 2 && TypeChecker.CheckTypes<UnityEngine.Events.UnityAction>(L, 2))
			{
				cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
				UnityEngine.Events.UnityAction arg0 = (UnityEngine.Events.UnityAction)ToLua.ToObject(L, 2);
				obj.RemoveListener(arg0);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: cs.GuiButton.RemoveListener");
			}
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemoveAllListener(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 1);
			cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
			obj.RemoveAllListener();
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
			cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
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
				cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				obj.SetSprite(arg0);
				return 0;
			}
			else if (count == 3)
			{
				cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
				string arg0 = ToLua.CheckString(L, 2);
				bool arg1 = LuaDLL.luaL_checkboolean(L, 3);
				obj.SetSprite(arg0, arg1);
				return 0;
			}
			else
			{
				return LuaDLL.luaL_throw(L, "invalid arguments to method: cs.GuiButton.SetSprite");
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
			cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
			obj.SetNativeSize();
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnPointerDown(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
			UnityEngine.EventSystems.PointerEventData arg0 = (UnityEngine.EventSystems.PointerEventData)ToLua.CheckObject<UnityEngine.EventSystems.PointerEventData>(L, 2);
			obj.OnPointerDown(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnPointerUp(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
			UnityEngine.EventSystems.PointerEventData arg0 = (UnityEngine.EventSystems.PointerEventData)ToLua.CheckObject<UnityEngine.EventSystems.PointerEventData>(L, 2);
			obj.OnPointerUp(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddMouseDownListener(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 2);
			obj.AddMouseDownListener(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemoveMouseDownListener(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 2);
			obj.RemoveMouseDownListener(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddMouseUpListener(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 2);
			obj.AddMouseUpListener(arg0);
			return 0;
		}
		catch (Exception e)
		{
			return LuaDLL.toluaL_exception(L, e);
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int RemoveMouseUpListener(IntPtr L)
	{
		try
		{
			ToLua.CheckArgsCount(L, 2);
			cs.GuiButton obj = (cs.GuiButton)ToLua.CheckObject<cs.GuiButton>(L, 1);
			LuaFunction arg0 = ToLua.CheckLuaFunction(L, 2);
			obj.RemoveMouseUpListener(arg0);
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
	static int get_UGUI_Button(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			cs.GuiButton obj = (cs.GuiButton)o;
			UnityEngine.UI.Button ret = obj.UGUI_Button;
			ToLua.Push(L, ret);
			return 1;
		}
		catch(Exception e)
		{
			return LuaDLL.toluaL_exception(L, e, o, "attempt to index UGUI_Button on a nil value");
		}
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int get_UGUI_Image(IntPtr L)
	{
		object o = null;

		try
		{
			o = ToLua.ToObject(L, 1);
			cs.GuiButton obj = (cs.GuiButton)o;
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

