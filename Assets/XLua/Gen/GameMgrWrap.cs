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
    public class GameMgrWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(GameMgr);
			Utils.BeginObjectRegister(type, L, translator, 0, 11, 2, 2);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Init", _m_Init);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Reset", _m_Reset);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "QueueEvent", _m_QueueEvent);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "DispatchEvent", _m_DispatchEvent);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "AddHandler", _m_AddHandler);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "RemoveHandler", _m_RemoveHandler);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "onLogin", _m_onLogin);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "onResume", _m_onResume);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "createRoom", _m_createRoom);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "enterRoom", _m_enterRoom);
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "get_coins", _m_get_coins);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "userMgr", _g_get_userMgr);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "club_channel", _g_get_club_channel);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "userMgr", _s_set_userMgr);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "club_channel", _s_set_club_channel);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 13, 2, 2);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "GetInstance", _m_GetInstance_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "getUserMgr", _m_getUserMgr_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "myself", _m_myself_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "share_club", _m_share_club_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "list_club_rooms", _m_list_club_rooms_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "start_room", _m_start_room_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "stop_room", _m_stop_room_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "destroy_club_room", _m_destroy_club_room_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "kick", _m_kick_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "join_club_channel", _m_join_club_channel_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "leave_club_channel", _m_leave_club_channel_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "get_club_message_cnt", _m_get_club_message_cnt_xlua_st_);
            
			
            
			Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "sclub_message_notify", _g_get_sclub_message_notify);
            Utils.RegisterFunc(L, Utils.CLS_GETTER_IDX, "slist_club_rooms", _g_get_slist_club_rooms);
            
			Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "sclub_message_notify", _s_set_sclub_message_notify);
            Utils.RegisterFunc(L, Utils.CLS_SETTER_IDX, "slist_club_rooms", _s_set_slist_club_rooms);
            
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 1)
				{
					
					GameMgr gen_ret = new GameMgr();
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to GameMgr constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetInstance_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    
                        GameMgr gen_ret = GameMgr.GetInstance(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_getUserMgr_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    
                        UserMgr gen_ret = GameMgr.getUserMgr(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_myself_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    int _uid = LuaAPI.xlua_tointeger(L, 1);
                    
                        bool gen_ret = GameMgr.myself( _uid );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Init(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Init(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Reset(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Reset(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_QueueEvent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.QueueEvent(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_DispatchEvent(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<object>(L, 3)) 
                {
                    string _msg = LuaAPI.lua_tostring(L, 2);
                    object _data = translator.GetObject(L, 3, typeof(object));
                    
                    gen_to_be_invoked.DispatchEvent( _msg, _data );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)) 
                {
                    string _msg = LuaAPI.lua_tostring(L, 2);
                    
                    gen_to_be_invoked.DispatchEvent( _msg );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to GameMgr.DispatchEvent!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_AddHandler(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _msg = LuaAPI.lua_tostring(L, 2);
                    GameMgr.MsgHandler _handler = translator.GetDelegate<GameMgr.MsgHandler>(L, 3);
                    
                    gen_to_be_invoked.AddHandler( _msg, _handler );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RemoveHandler(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    string _msg = LuaAPI.lua_tostring(L, 2);
                    GameMgr.MsgHandler _handler = translator.GetDelegate<GameMgr.MsgHandler>(L, 3);
                    
                    gen_to_be_invoked.RemoveHandler( _msg, _handler );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_onLogin(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    SimpleJson.JsonObject _data = (SimpleJson.JsonObject)translator.GetObject(L, 2, typeof(SimpleJson.JsonObject));
                    
                    gen_to_be_invoked.onLogin( _data );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_onResume(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    SimpleJson.JsonObject _data = (SimpleJson.JsonObject)translator.GetObject(L, 2, typeof(SimpleJson.JsonObject));
                    
                    gen_to_be_invoked.onResume( _data );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_createRoom(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    SimpleJson.JsonObject _conf = (SimpleJson.JsonObject)translator.GetObject(L, 2, typeof(SimpleJson.JsonObject));
                    System.Action<SimpleJson.JsonObject> _cb = translator.GetDelegate<System.Action<SimpleJson.JsonObject>>(L, 3);
                    
                    gen_to_be_invoked.createRoom( _conf, _cb );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_enterRoom(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 3&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)&& translator.Assignable<System.Action<int>>(L, 3)) 
                {
                    string _roomid = LuaAPI.lua_tostring(L, 2);
                    System.Action<int> _cb = translator.GetDelegate<System.Action<int>>(L, 3);
                    
                    gen_to_be_invoked.enterRoom( _roomid, _cb );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 2&& (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING)) 
                {
                    string _roomid = LuaAPI.lua_tostring(L, 2);
                    
                    gen_to_be_invoked.enterRoom( _roomid );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to GameMgr.enterRoom!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_get_coins(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1) 
                {
                    
                        int gen_ret = gen_to_be_invoked.get_coins(  );
                        LuaAPI.xlua_pushinteger(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 2&& translator.Assignable<System.Action>(L, 2)) 
                {
                    System.Action _cb = translator.GetDelegate<System.Action>(L, 2);
                    
                    gen_to_be_invoked.get_coins( _cb );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 1) 
                {
                    
                    gen_to_be_invoked.get_coins(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to GameMgr.get_coins!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_share_club_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    int _club_id = LuaAPI.xlua_tointeger(L, 1);
                    bool _tl = LuaAPI.lua_toboolean(L, 2);
                    
                    GameMgr.share_club( _club_id, _tl );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_list_club_rooms_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    int _club_id = LuaAPI.xlua_tointeger(L, 1);
                    System.Action<bool> _cb = translator.GetDelegate<System.Action<bool>>(L, 2);
                    
                    GameMgr.list_club_rooms( _club_id, _cb );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_start_room_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _room_tag = LuaAPI.lua_tostring(L, 1);
                    System.Action<bool> _cb = translator.GetDelegate<System.Action<bool>>(L, 2);
                    
                    GameMgr.start_room( _room_tag, _cb );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_stop_room_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    string _room_tag = LuaAPI.lua_tostring(L, 1);
                    System.Action<bool> _cb = translator.GetDelegate<System.Action<bool>>(L, 2);
                    
                    GameMgr.stop_room( _room_tag, _cb );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_destroy_club_room_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    int _id = LuaAPI.xlua_tointeger(L, 1);
                    string _room_tag = LuaAPI.lua_tostring(L, 2);
                    int _club_id = LuaAPI.xlua_tointeger(L, 3);
                    System.Action<bool> _cb = translator.GetDelegate<System.Action<bool>>(L, 4);
                    
                    GameMgr.destroy_club_room( _id, _room_tag, _club_id, _cb );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_kick_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    int _uid = LuaAPI.xlua_tointeger(L, 1);
                    int _roomid = LuaAPI.xlua_tointeger(L, 2);
                    string _room_tag = LuaAPI.lua_tostring(L, 3);
                    System.Action<bool> _cb = translator.GetDelegate<System.Action<bool>>(L, 4);
                    
                    GameMgr.kick( _uid, _roomid, _room_tag, _cb );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_join_club_channel_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    int _club_id = LuaAPI.xlua_tointeger(L, 1);
                    
                    GameMgr.join_club_channel( _club_id );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_leave_club_channel_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    int _club_id = LuaAPI.xlua_tointeger(L, 1);
                    
                    GameMgr.leave_club_channel( _club_id );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_get_club_message_cnt_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    int _club_id = LuaAPI.xlua_tointeger(L, 1);
                    System.Action<int> _cb = translator.GetDelegate<System.Action<int>>(L, 2);
                    
                    GameMgr.get_club_message_cnt( _club_id, _cb );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_userMgr(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.userMgr);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_club_channel(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
                LuaAPI.xlua_pushinteger(L, gen_to_be_invoked.club_channel);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_sclub_message_notify(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, GameMgr.sclub_message_notify);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_slist_club_rooms(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    translator.Push(L, GameMgr.slist_club_rooms);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_userMgr(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.userMgr = (UserMgr)translator.GetObject(L, 2, typeof(UserMgr));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_club_channel(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                GameMgr gen_to_be_invoked = (GameMgr)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.club_channel = LuaAPI.xlua_tointeger(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_sclub_message_notify(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    GameMgr.sclub_message_notify = (ClubMessageNotify)translator.GetObject(L, 1, typeof(ClubMessageNotify));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_slist_club_rooms(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			    GameMgr.slist_club_rooms = (ListClubRoom)translator.GetObject(L, 1, typeof(ListClubRoom));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
