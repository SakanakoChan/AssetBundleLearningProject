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
    public class AssetBundleUpdateManagerWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(AssetBundleUpdateManager);
			Utils.BeginObjectRegister(type, L, translator, 0, 3, 1, 1);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "UpdateGame", _m_UpdateGame);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "DownloadNecessaryAssetBundlesFromFTPServer", _m_DownloadNecessaryAssetBundlesFromFTPServer);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "DownloadAssetBundleComparisonFileFromFTPServer", _m_DownloadAssetBundleComparisonFileFromFTPServer);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "PressAnyKeyToStartGameScreen", _g_get_PressAnyKeyToStartGameScreen);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "PressAnyKeyToStartGameScreen", _s_set_PressAnyKeyToStartGameScreen);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 1, 1);
			
			
            
			Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "instance", _g_get_instance);
            
			Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "instance", _s_set_instance);
            
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					var gen_ret = new AssetBundleUpdateManager();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to AssetBundleUpdateManager constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_UpdateGame(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                AssetBundleUpdateManager gen_to_be_invoked = (AssetBundleUpdateManager)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.UpdateGame(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DownloadNecessaryAssetBundlesFromFTPServer(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                AssetBundleUpdateManager gen_to_be_invoked = (AssetBundleUpdateManager)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.Action<bool> _OnDownloadProcessEnd = translator.GetDelegate<System.Action<bool>>(L, 2);
                    
                    gen_to_be_invoked.DownloadNecessaryAssetBundlesFromFTPServer( _OnDownloadProcessEnd );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DownloadAssetBundleComparisonFileFromFTPServer(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                AssetBundleUpdateManager gen_to_be_invoked = (AssetBundleUpdateManager)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    System.Action<bool> _OnDownloadProcessEnd = translator.GetDelegate<System.Action<bool>>(L, 2);
                    
                    gen_to_be_invoked.DownloadAssetBundleComparisonFileFromFTPServer( _OnDownloadProcessEnd );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_instance(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, AssetBundleUpdateManager.instance);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_PressAnyKeyToStartGameScreen(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                AssetBundleUpdateManager gen_to_be_invoked = (AssetBundleUpdateManager)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.PressAnyKeyToStartGameScreen);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_instance(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    AssetBundleUpdateManager.instance = (AssetBundleUpdateManager)translator.GetObject(L, 1, typeof(AssetBundleUpdateManager));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_PressAnyKeyToStartGameScreen(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                AssetBundleUpdateManager gen_to_be_invoked = (AssetBundleUpdateManager)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.PressAnyKeyToStartGameScreen = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
