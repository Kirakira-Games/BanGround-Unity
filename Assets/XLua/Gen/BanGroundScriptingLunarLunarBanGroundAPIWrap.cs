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
    public class BanGroundScriptingLunarLunarBanGroundAPIWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(BanGround.Scripting.Lunar.LunarBanGroundAPI);
			Utils.BeginObjectRegister(type, L, translator, 0, 11, 0, 0);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "GetCamera", _m_GetCamera);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "LoadTexture", _m_LoadTexture);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetBackground", _m_SetBackground);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetLaneBackground", _m_SetLaneBackground);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "SetJudgeLineColor", _m_SetJudgeLineColor);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "PrecacheSound", _m_PrecacheSound);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "CreateSprite", _m_CreateSprite);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "AddKeyframeByTime", _m_AddKeyframeByTime);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "AddKeyframeByBeat", _m_AddKeyframeByBeat);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "PrintToConsole", _m_PrintToConsole);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Dispose", _m_Dispose);
			
			
			
			
			
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
					
					BanGround.Scripting.Lunar.LunarBanGroundAPI gen_ret = new BanGround.Scripting.Lunar.LunarBanGroundAPI();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to BanGround.Scripting.Lunar.LunarBanGroundAPI constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetCamera(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                BanGround.Scripting.Lunar.LunarBanGroundAPI gen_to_be_invoked = (BanGround.Scripting.Lunar.LunarBanGroundAPI)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                        BanGround.Scripting.Lunar.ScriptCamera gen_ret = gen_to_be_invoked.GetCamera(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadTexture(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                BanGround.Scripting.Lunar.LunarBanGroundAPI gen_to_be_invoked = (BanGround.Scripting.Lunar.LunarBanGroundAPI)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _tex = LuaAPI.lua_tostring(L, 2);
                    
                        int gen_ret = gen_to_be_invoked.LoadTexture( _tex );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetBackground(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                BanGround.Scripting.Lunar.LunarBanGroundAPI gen_to_be_invoked = (BanGround.Scripting.Lunar.LunarBanGroundAPI)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _texId = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.SetBackground( _texId );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetLaneBackground(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                BanGround.Scripting.Lunar.LunarBanGroundAPI gen_to_be_invoked = (BanGround.Scripting.Lunar.LunarBanGroundAPI)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _texId = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.SetLaneBackground( _texId );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetJudgeLineColor(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                BanGround.Scripting.Lunar.LunarBanGroundAPI gen_to_be_invoked = (BanGround.Scripting.Lunar.LunarBanGroundAPI)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _r = (float)LuaAPI.lua_tonumber(L, 2);
                    float _g = (float)LuaAPI.lua_tonumber(L, 3);
                    float _b = (float)LuaAPI.lua_tonumber(L, 4);
                    
                    gen_to_be_invoked.SetJudgeLineColor( _r, _g, _b );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PrecacheSound(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                BanGround.Scripting.Lunar.LunarBanGroundAPI gen_to_be_invoked = (BanGround.Scripting.Lunar.LunarBanGroundAPI)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _snd = LuaAPI.lua_tostring(L, 2);
                    
                        BanGround.Scripting.Lunar.ScriptSoundEffect gen_ret = gen_to_be_invoked.PrecacheSound( _snd );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_CreateSprite(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                BanGround.Scripting.Lunar.LunarBanGroundAPI gen_to_be_invoked = (BanGround.Scripting.Lunar.LunarBanGroundAPI)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _textureId = LuaAPI.xlua_tointeger(L, 2);
                    
                        BanGround.Scripting.Lunar.ScriptSprite gen_ret = gen_to_be_invoked.CreateSprite( _textureId );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AddKeyframeByTime(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                BanGround.Scripting.Lunar.LunarBanGroundAPI gen_to_be_invoked = (BanGround.Scripting.Lunar.LunarBanGroundAPI)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _time = (float)LuaAPI.lua_tonumber(L, 2);
                    XLua.LuaFunction _callback = (XLua.LuaFunction)translator.GetObject(L, 3, typeof(XLua.LuaFunction));
                    
                    gen_to_be_invoked.AddKeyframeByTime( _time, _callback );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AddKeyframeByBeat(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                BanGround.Scripting.Lunar.LunarBanGroundAPI gen_to_be_invoked = (BanGround.Scripting.Lunar.LunarBanGroundAPI)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    float _beat = (float)LuaAPI.lua_tonumber(L, 2);
                    XLua.LuaFunction _callback = (XLua.LuaFunction)translator.GetObject(L, 3, typeof(XLua.LuaFunction));
                    
                    gen_to_be_invoked.AddKeyframeByBeat( _beat, _callback );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_PrintToConsole(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                BanGround.Scripting.Lunar.LunarBanGroundAPI gen_to_be_invoked = (BanGround.Scripting.Lunar.LunarBanGroundAPI)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _str = LuaAPI.lua_tostring(L, 2);
                    
                    gen_to_be_invoked.PrintToConsole( _str );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Dispose(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                BanGround.Scripting.Lunar.LunarBanGroundAPI gen_to_be_invoked = (BanGround.Scripting.Lunar.LunarBanGroundAPI)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Dispose(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        
        
		
		
		
		
    }
}
