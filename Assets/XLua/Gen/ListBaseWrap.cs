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
    public class ListBaseWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(ListBase);
			Utils.BeginObjectRegister(type, L, translator, 0, 20, 2, 2);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Awake", _m_Awake);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "back", _m_back);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "show", _m_show);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "onBack", _m_onBack);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "getItem", _m_getItem);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "shrinkContent", _m_shrinkContent);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "updateItems", _m_updateItems);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Reposition", _m_Reposition);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "getChild", _m_getChild);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "setText", _m_setText);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "setIcon", _m_setIcon);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "setBtnEvent", _m_setBtnEvent);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "setActive", _m_setActive);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "setInput", _m_setInput);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "getInput", _m_getInput);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "enter", _m_enter);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "setToggle", _m_setToggle);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "setToggleEvent", _m_setToggleEvent);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "setSliderEvent", _m_setSliderEvent);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "UpdateEvents", _e_UpdateEvents);
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "mShow", _g_get_mShow);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "listPath", _g_get_listPath);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "mShow", _s_set_mShow);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "listPath", _s_set_listPath);
            
			
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
					
					ListBase gen_ret = new ListBase();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to ListBase constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Awake(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Awake(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_back(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 2)) 
                {
                    bool _update = LuaAPI.lua_toboolean(L, 2);
                    
                    gen_to_be_invoked.back( _update );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1) 
                {
                    
                    gen_to_be_invoked.back(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to ListBase.back!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_show(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& translator.Assignable<System.Action>(L, 2)) 
                {
                    System.Action _cb = translator.GetDelegate<System.Action>(L, 2);
                    
                    gen_to_be_invoked.show( _cb );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1) 
                {
                    
                    gen_to_be_invoked.show(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to ListBase.show!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_onBack(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.onBack(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_getItem(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _id = LuaAPI.xlua_tointeger(L, 2);
                    
                        UnityEngine.Transform gen_ret = gen_to_be_invoked.getItem( _id );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_shrinkContent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _num = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.shrinkContent( _num );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_updateItems(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    int _count = LuaAPI.xlua_tointeger(L, 2);
                    
                    gen_to_be_invoked.updateItems( _count );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Reposition(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)) 
                {
                    string _grid = LuaAPI.lua_tostring(L, 2);
                    
                    gen_to_be_invoked.Reposition( _grid );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1) 
                {
                    
                    gen_to_be_invoked.Reposition(  );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& translator.Assignable<UnityEngine.Transform>(L, 2)) 
                {
                    UnityEngine.Transform _ob = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    
                    gen_to_be_invoked.Reposition( _ob );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to ListBase.Reposition!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_getChild(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Transform _item = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    string _child = LuaAPI.lua_tostring(L, 3);
                    
                        UnityEngine.Transform gen_ret = gen_to_be_invoked.getChild( _item, _child );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_setText(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Transform _item = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    string _child = LuaAPI.lua_tostring(L, 3);
                    string _text = LuaAPI.lua_tostring(L, 4);
                    
                    gen_to_be_invoked.setText( _item, _child, _text );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_setIcon(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.Transform>(L, 2)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)) 
                {
                    UnityEngine.Transform _item = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    string _child = LuaAPI.lua_tostring(L, 3);
                    int _uid = LuaAPI.xlua_tointeger(L, 4);
                    
                    gen_to_be_invoked.setIcon( _item, _child, _uid );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 4&& translator.Assignable<UnityEngine.Transform>(L, 2)&& (LuaAPI.lua_isnil(L, 3) || LuaAPI.lua_type(L, 3) == LuaTypes.LUA_TSTRING)&& (LuaAPI.lua_isnil(L, 4) || LuaAPI.lua_type(L, 4) == LuaTypes.LUA_TSTRING)) 
                {
                    UnityEngine.Transform _item = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    string _child = LuaAPI.lua_tostring(L, 3);
                    string _url = LuaAPI.lua_tostring(L, 4);
                    
                    gen_to_be_invoked.setIcon( _item, _child, _url );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to ListBase.setIcon!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_setBtnEvent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Transform _item = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    string _child = LuaAPI.lua_tostring(L, 3);
                    System.Action _cb = translator.GetDelegate<System.Action>(L, 4);
                    
                    gen_to_be_invoked.setBtnEvent( _item, _child, _cb );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_setActive(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Transform _item = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    string _child = LuaAPI.lua_tostring(L, 3);
                    bool _enable = LuaAPI.lua_toboolean(L, 4);
                    
                    gen_to_be_invoked.setActive( _item, _child, _enable );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_setInput(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Transform _item = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    string _child = LuaAPI.lua_tostring(L, 3);
                    string _text = LuaAPI.lua_tostring(L, 4);
                    
                    gen_to_be_invoked.setInput( _item, _child, _text );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_getInput(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Transform _item = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    string _child = LuaAPI.lua_tostring(L, 3);
                    
                        string gen_ret = gen_to_be_invoked.getInput( _item, _child );
                        LuaAPI.lua_pushstring(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_enter(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.enter(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_setToggle(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Transform _item = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    string _child = LuaAPI.lua_tostring(L, 3);
                    bool _value = LuaAPI.lua_toboolean(L, 4);
                    
                    gen_to_be_invoked.setToggle( _item, _child, _value );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_setToggleEvent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Transform _item = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    string _child = LuaAPI.lua_tostring(L, 3);
                    System.Action<bool> _cb = translator.GetDelegate<System.Action<bool>>(L, 4);
                    
                    gen_to_be_invoked.setToggleEvent( _item, _child, _cb );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_setSliderEvent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    UnityEngine.Transform _item = (UnityEngine.Transform)translator.GetObject(L, 2, typeof(UnityEngine.Transform));
                    string _child = LuaAPI.lua_tostring(L, 3);
                    System.Action<float> _cb = translator.GetDelegate<System.Action<float>>(L, 4);
                    
                    gen_to_be_invoked.setSliderEvent( _item, _child, _cb );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_mShow(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushboolean(L, gen_to_be_invoked.mShow);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_listPath(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.listPath);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_mShow(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.mShow = LuaAPI.lua_toboolean(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_listPath(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.listPath = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _e_UpdateEvents(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    int gen_param_count = LuaAPI.lua_gettop(L);
			ListBase gen_to_be_invoked = (ListBase)translator.FastGetCSObj(L, 1);
                System.Action gen_delegate = translator.GetDelegate<System.Action>(L, 3);
                if (gen_delegate == null) {
                    return LuaAPI.luaL_error(L, "#3 need System.Action!");
                }
				
				if (gen_param_count == 3)
				{
					
					if (LuaAPI.xlua_is_eq_str(L, 2, "+")) {
						gen_to_be_invoked.UpdateEvents += gen_delegate;
						return 0;
					} 
					
					
					if (LuaAPI.xlua_is_eq_str(L, 2, "-")) {
						gen_to_be_invoked.UpdateEvents -= gen_delegate;
						return 0;
					} 
					
				}
			} catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
			LuaAPI.luaL_error(L, "invalid arguments to ListBase.UpdateEvents!");
            return 0;
        }
        
		
		
    }
}
