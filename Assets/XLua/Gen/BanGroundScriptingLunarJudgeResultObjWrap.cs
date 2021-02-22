#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class BanGroundScriptingLunarJudgeResultObjWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(BanGround.Scripting.Lunar.JudgeResultObj);
			Utils.BeginObjectRegister(type, L, translator, 0, 1, 7, 7);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "ToString", _m_ToString);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "Lane", _g_get_Lane);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Type", _g_get_Type);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Time", _g_get_Time);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Beat", _g_get_Beat);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "JudgeResult", _g_get_JudgeResult);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "JudgeTime", _g_get_JudgeTime);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "JudgeOffset", _g_get_JudgeOffset);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "Lane", _s_set_Lane);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Type", _s_set_Type);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Time", _s_set_Time);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Beat", _s_set_Beat);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "JudgeResult", _s_set_JudgeResult);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "JudgeTime", _s_set_JudgeTime);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "JudgeOffset", _s_set_JudgeOffset);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					BanGround.Scripting.Lunar.JudgeResultObj gen_ret = new BanGround.Scripting.Lunar.JudgeResultObj();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to BanGround.Scripting.Lunar.JudgeResultObj constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ToString(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        string gen_ret = gen_to_be_invoked.ToString(  );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Lane(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.Lane);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Type(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.Type);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Time(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.Time);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Beat(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushnumber(L, gen_to_be_invoked.Beat);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_JudgeResult(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.JudgeResult);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_JudgeTime(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.JudgeTime);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_JudgeOffset(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.JudgeOffset);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Lane(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Lane = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Type(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Type = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Time(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Time = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Beat(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Beat = (float)LuaAPI.lua_tonumber(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_JudgeResult(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.JudgeResult = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_JudgeTime(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.JudgeTime = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_JudgeOffset(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                BanGround.Scripting.Lunar.JudgeResultObj gen_to_be_invoked = (BanGround.Scripting.Lunar.JudgeResultObj)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.JudgeOffset = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
