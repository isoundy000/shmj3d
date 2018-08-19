using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Pomelo.DotNetClient;
using SimpleJson;
using System.IO;
using XLua;

[Serializable]
public class UserMgr {
	public string account;
	public int userid;
	public string username;
	public int lv;
	public int coins;
	public int gems;
	public string roomid;
	public int sex;
	public string ip;
	public string sign;
};

[Serializable]
public class HuPushInfo {
	public int seatindex;
	public bool iszimo;
	public int hupai;
	public List<int> holds;
	public int target;
	public string action;
	public List<string> types;
	public bool bg;
}

[Serializable]
public class ClubMessageNotify {
	public int club_id;
	public int cnt;
}

[Serializable]
public class ChatInfo {
	public int sender;
	public string content;
}

[Serializable]
public class QuickChatInfo {
	public int sender;
	public int content;
}

[Serializable]
public class EmojiPush {
	public int sender;
	public int content;
}

[Serializable]
public class DEmojiPush {
	public int sender;
	public int target;
	public int id;
}

[Serializable]
public class ChupaiPush {
	public int turn;
	public List<int> limit;
	public bool bg;
}

[Serializable]
public class GetCoins {
	public int errcode;
	public string errmsg;
	public int gems;
	public int golds;
	public int lottery;
}

[Serializable]
public class MessageCount {
	public int cnt;
}

[Serializable]
public class GetClubMessageCnt {
	public int errcode;
	public string errmsg;
	public MessageCount data;
}

[Serializable]
public class ClubRoomPlayer {
	public int id;
	public string name;
	public string icon;
	public int seatindex;
	public bool ready;
}

[Serializable]
public class ClubRoomBaseInfo {
	public string type;
	public int huafen;
	public bool maima;
	public int maxGames;
	public int maxFan;
	public bool qidui;

	public bool jyw;
	public bool j7w;
	public bool ryj;

	public int numOfSeats;
	public bool limit_ip;
	public bool limit_gps;

	public string getDesc() {
		List<string> tips = new List<string> ();
		if (type == "shmj") {
			tips.Add ("上海敲麻");
			tips.Add (huafen + "/" + huafen);
			tips.Add (maima ? "带苍蝇" : "不带苍蝇");
		} else if (type == "gzmj") {
			tips.Add ("酒都麻将");
			if (jyw) tips.Add ("金银乌");
			if (j7w) tips.Add ("见7挖");
			if (ryj) tips.Add ("软硬鸡");
		}

		tips.Add (maxGames + "局");

		return string.Join (" ", tips.ToArray ());
	}
}

[Serializable]
public class ClubRoomInfo {
	public int id;
	public string room_tag;
	public string status;
	public ClubRoomBaseInfo base_info;
	public int num_of_turns;
	public int club_id;
	public List<ClubRoomPlayer> players;
}

[Serializable]
public class ListClubRoom {
	public int errcode;
	public string errmsg;
	public List<ClubRoomInfo> data;
}

[Serializable]
public class GoodsInfo {
	public int id;
	public int price;
	public int quantity;
	public string product;
}

[Serializable]
public class ListGoodsFromShop {
	public int errcode;
	public string errmsg;
	public List<GoodsInfo> data;
}

[Serializable]
public class RoomCost {
	public string game;
	public int round;
	public int players_num;
	public int gem;
}

[Serializable]
public class GetRoomCosts {
	public int errcode;
	public int errmsg;
	public List<RoomCost> data;
}

[Serializable]
public class GameChickenPush {
	public int si;
	public int key;
	public List<int> chickens;
}

[Serializable]
public class LocationWarning {
	public string text;
}

public class GameMgr {
	static GameMgr mInstance = null;

	bool inited = false;

	public delegate void MsgHandler(object data);

	Dictionary<string, MsgHandler> mHandlerMap = new Dictionary<string, MsgHandler>();

	public UserMgr userMgr = new UserMgr();

	public int club_channel = 0;

	public static ClubMessageNotify sclub_message_notify = null;

	public static GameMgr GetInstance () {
		if (mInstance == null)
			mInstance = new GameMgr ();

		return mInstance;
	}

	public static UserMgr getUserMgr() {
		return mInstance.userMgr;
	}

	public static bool myself(int uid) {
		return mInstance.userMgr.userid == uid;
	}

	public GameMgr () {

	}

	public void Init() {
		if (inited)
			return;

		inited = true;
	}

	void InitHandler () {
		NetMgr net = NetMgr.GetInstance ();
		RoomMgr rm = RoomMgr.GetInstance ();
		PomeloClient pc = net.pc;

		pc.on ("login_result", data => {
			Debug.Log("login_result");
	
			object ob;
			if (!data.TryGetValue("errcode", out ob))
				return;

			int ret = Convert.ToInt32(ob);
			if (ret != 0) {
				Debug.Log("login_result ret=" + ret);
				return;
			}

			JsonObject room = (JsonObject)data["data"];

			rm.updateRoom(room);

			DispatchEvent("game_reset");
		});

		pc.on ("login_finished", data => {
			Debug.Log("login_finished");

			string table = "04.table3d";
			string name = SceneManager.GetActiveScene().name;

			if (name != table) {
				mHandlerMap.Clear();
				LoadingScene.LoadNewScene(table);
			} else {
				if (rm.isPlaying())
					net.send("ready");
			}
		});

		pc.on ("exit_result", data=>{
			string reason = (string)data["reason"];

			if (reason == "kick") {
				GameAlert.GetInstance().show("您已被管理员请出房间", ()=>{
					GameManager.GetInstance().exit();
				});
			} else if (reason == "request") {
				GameManager.GetInstance().exit();
			}
		});

		pc.on ("exit_notify_push", data => {
			int uid = Convert.ToInt32(data["value"]);
			int seatindex = rm.userExit(uid);

			DispatchEvent("user_state_changed", seatindex);

			AudioManager.GetInstance().PlayEffectAudio("playerOut");
		});

		pc.on ("dispress_push", data => {
			GameManager.GetInstance().exit();
		});

		pc.on ("new_user_comes_push", data => {
			int seatindex = rm.newUserCome(data);

			DispatchEvent("user_state_changed", seatindex);

			AudioManager.GetInstance().PlayEffectAudio("playerIn");
		});

		pc.on ("user_state_push", data => {
			int seatindex = rm.updateUser(data);

			DispatchEvent("user_state_changed", seatindex);
		});

		pc.on ("game_wait_maima_push", data => {
			rm.updateMaima(data);

			DispatchEvent("game_wait_maima");
		});

		pc.on ("game_maima_push", data => {
			rm.updateMaima(data);

			DispatchEvent("game_maima");
		});

		pc.on ("game_wait_dingque_push", data => {
			rm.updateDingque(data);

			DispatchEvent("game_wait_dingque");
		});

		pc.on ("game_dingque_push", data => {
			rm.updateDingque(data);

			DispatchEvent("game_dingque");
		});

		pc.on ("user_ready_push", data => {
			int seatindex = rm.updateUser(data);

			DispatchEvent("user_state_changed", seatindex);
		});

		pc.on ("game_dice_push", data => {
			rm.updateState(data);

			DispatchEvent("game_dice");
		});

		pc.on ("game_holds_push", data => {
			Debug.Log("get game_holds_push");

			int si = rm.updateSeat(data);
			DispatchEvent("game_holds", si);
		});

		pc.on ("game_holds_len_push", data => {
			Debug.Log("get game_holds_len_push");

			int si = rm.updateSeat(data);

			DispatchEvent("game_holds_len", si);
		});

		pc.on ("game_state_push", data => {
			rm.updateState(data);

			DispatchEvent("game_state");
		});

		pc.on ("game_begin_push", data => {
			Debug.Log("get game_begin_push");

			rm.newRound();
			rm.updateState(data);

			foreach (PlayerInfo p in rm.players) {
				p.ready = false;
				DispatchEvent("user_state_changed", p.seatindex);
			}

			DispatchEvent("game_begin");
		});

		pc.on ("game_playing_push", data => {
			Debug.Log("get game_playing_push");
			rm.updateState(data);

			DispatchEvent("game_playing");
		});

		pc.on ("game_sync_push", data => {
			Debug.Log("get game_sync_push");

			if (!data.ContainsKey("pseudo")) {
				rm.updateState(data);
				rm.updateSeats((JsonArray)data["seats"]);
			}

			DispatchEvent("user_hf_updated");
			DispatchEvent("game_sync");
		});

		pc.on ("hangang_notify_push", data => {
			int seatindex = Convert.ToInt32(data["seatindex"]);
			DispatchEvent("hangang_notify", seatindex);
		});
	
		pc.on ("game_action_push", data => {
			Debug.Log("get game_action_push: " + data.ToString());
			rm.updateAction(data);

			DispatchEvent("game_action");
		});

		pc.on ("game_chupai_push", data => {
			Debug.Log("get game_chupai_push");
			rm.updateState(data);

			ChupaiPush cp = JsonUtility.FromJson<ChupaiPush> (data.ToString());
			rm.updateLimit(cp.turn, cp.limit);

			if (cp.bg == true) return;

			DispatchEvent("game_turn_change");
		});

		pc.on ("game_num_push", data => {
			rm.updateRoomInfo(data);

			DispatchEvent("game_num");
		});

		pc.on ("game_chicken_push", data => {
			GameChickenPush gcp = JsonUtility.FromJson<GameChickenPush> (data.ToString());

			DispatchEvent("game_chicken", gcp);
		});

		pc.on ("game_over_push", data => {
			rm.updateOverInfo(data);

			DispatchEvent("game_over");
		});

		pc.on ("mj_count_push", data => {
			rm.updateState(data);

			if (data.ContainsKey("bg")) return;

			DispatchEvent("mj_count");
		});

		pc.on ("hu_push", data => {
			HuPushInfo info = rm.updateHu(data);

			if (info.bg) return;

			DispatchEvent("hupai", info);
		});

		pc.on ("game_chupai_notify_push", data => {
			Debug.Log("get game_chupai_notify_push");
			ActionInfo info = rm.doChupai(data);

			if (info.bg == true) return;

			DispatchEvent("game_chupai_notify", info);
		});

		pc.on ("game_hf_push", data => {
			rm.updateFlowers(data);

			Debug.Log("get game_hf_push");

			DispatchEvent("user_hf_updated");
		});

		pc.on ("game_af_push", data => {
			ActionInfo info = rm.doAddFlower(data);

			if (info.bg == true) return;

			DispatchEvent("user_hf_updated", info);
		});

		pc.on ("game_mopai_push", data => {
			Debug.Log("get game_mopai_push");
			ActionInfo info = rm.doMopai(data);

			if (info.bg == true) return;

			DispatchEvent("game_mopai", info);
		});

		pc.on ("guo_notify_push", data => {
			Debug.Log("get guo_notify_push");
			ActionInfo info = rm.doGuo(data);

			if (info.bg == true) return;

			DispatchEvent("guo_notify", info);
		});

		pc.on ("guo_result", data => {
			Debug.Log("get guo_result");
			DispatchEvent("guo_result");
		});

		pc.on ("peng_notify_push", data => {
			ActionInfo info = rm.doPeng(data);

			if (info.bg == true) return;

			DispatchEvent("peng_notify", info);
		});

		pc.on ("chi_notify_push", data => {
			ActionInfo info = rm.doChi(data);

			if (info.bg == true) return;

			DispatchEvent("chi_notify", info);
		});

		pc.on ("gang_notify_push", data => {
			GangInfo info = rm.doGang(data);

			if (info.bg == true) return;

			DispatchEvent("gang_notify", info);
		});

		pc.on ("ting_notify_push", data => {
			TingInfo info = rm.doTing(data);

			if (info.bg == true) return;

			DispatchEvent("ting_notify", info.seatindex);
		});

		pc.on ("chat_push", data => {
			ChatInfo info = JsonUtility.FromJson<ChatInfo>(data.ToString());

			DispatchEvent("chat", info);
		});

		pc.on ("quick_chat_push", data => {
			QuickChatInfo info = JsonUtility.FromJson<QuickChatInfo>(data.ToString());

			DispatchEvent("quick_chat_push", info);
		});

		pc.on ("emoji_push", data => {
			EmojiPush info = JsonUtility.FromJson<EmojiPush>(data.ToString());
			DispatchEvent("emoji_push", info);
		});

		pc.on ("demoji_push", data => {
			Debug.Log("get demoji_push");
			DEmojiPush info = JsonUtility.FromJson<DEmojiPush>(data.ToString());
			DispatchEvent("demoji_push", info);
		});

		pc.on ("dissolve_notice_push", data => {
			DissolveInfo dv = JsonUtility.FromJson<DissolveInfo>(data.ToString());
			rm.dissolve = dv;

			DispatchEvent("dissolve_notice", dv);
		});

		pc.on ("dissolve_done_push", data => {
			Debug.Log("dissolve_done_push");
			DispatchEvent("dissolve_done");
		});

		pc.on ("dissolve_cancel_push", data => {
			Debug.Log("dissolve_cancel_push");

			DissolveCancel dc = JsonUtility.FromJson<DissolveCancel>(data.ToString());

			DispatchEvent("dissolve_cancel", dc);
		});

		pc.on ("voice_msg_push", data => {
			VoiceMsgPush vm = JsonUtility.FromJson<VoiceMsgPush>(data.ToString());
			DispatchEvent("voice_msg", vm);
		});

		pc.on ("start_club_room", data => {

		});

		pc.on ("club_room_updated", data => {
			DispatchEvent("club_room_updated", data);
		});

		pc.on ("clu_room_removed", data => {
			DispatchEvent("clu_room_removed", data);
		});

		pc.on ("club_message_notify", data => {
			ClubMessageNotify ret = JsonUtility.FromJson<ClubMessageNotify>(data.ToString());

			sclub_message_notify = ret;

			DispatchEvent("club_message_notify", ret);
		});

		pc.on ("sys_message_updated", data => {
			DispatchEvent("sys_message_updated", data);
		});

		pc.on ("recommend_room_updated", data => {
			Debug.Log("recommend_room_updated");
			DispatchEvent("recommend_room_updated", data);
		});

		pc.on ("location_warning", data => {
			var lw = JsonUtility.FromJson<LocationWarning>(data.ToString());

			DispatchEvent("location_warning", lw);
		});
	}

	public void Reset() {
		mHandlerMap.Clear ();
	}

	public void QueueEvent() {

	}

	public void DispatchEvent(string msg, object data = null) {
		MsgHandler val = GetHandler (msg);

		if (val == null)
			return;

		val (data);
	}

	MsgHandler GetHandler (string msg) {
		if (!mHandlerMap.ContainsKey (msg))
			return null;
		else
			return mHandlerMap [msg];
	}

	public void AddHandler(string msg, MsgHandler handler) {
		MsgHandler val = GetHandler (msg);
		if (val == null)
			mHandlerMap.Add (msg, handler);
		else
			mHandlerMap[msg] += new MsgHandler(handler);
	}

	public void RemoveHandler(string msg, MsgHandler handler) {
		MsgHandler val = GetHandler (msg);

		if (val == null)
			return;

		val -= new MsgHandler (handler);

		if (val == null)
			mHandlerMap.Remove (msg);
		else
			mHandlerMap [msg] = val;
	}

	public void onLogin(JsonObject data) {
		//string sign = userMgr.sign;
		userMgr = JsonUtility.FromJson<UserMgr>(data.ToString());
		//userMgr.sign = sign;

		Debug.Log ("userName " + userMgr.username);
		Debug.Log ("ip: " + userMgr.ip);

		InitHandler ();

		LoadingScene.LoadNewScene ("02.lobby");
	}

	public void onResume(JsonObject data) {
		//string sign = userMgr.sign;
		userMgr = JsonUtility.FromJson<UserMgr>(data.ToString());
		//userMgr.sign = sign;

		InitHandler ();

		string roomid = userMgr.roomid;

		if (roomid != null && roomid.Length == 6) {
			enterRoom (roomid, code => {
				string content = "房间[" + roomid + "]已解散";

				if (code == 2224)
					content = "房间[" + roomid + "]已满";
				else if (code == 2231)
					content = "您的IP和其他玩家相同";
				else if (code == 2232)
					content = "您的位置和其他玩家太近";
				else if (code == 2233)
					content = "您的定位信息无效，请检查是否开启定位";

				if (code != 0) {
					GameAlert.Show(content, ()=>{
						if (SceneManager.GetActiveScene().name == "04.table3d")
							GameManager.GetInstance().exit();
					});
				}
			});

			userMgr.roomid = "";
		} else {
			if (SceneManager.GetActiveScene ().name == "04.table3d" && !ReplayMgr.GetInstance().isReplay()) {
				GameAlert.Show ("房间已结束，点确定返回大厅", () => {
					GameManager.GetInstance ().exit ();
				});
			}
		}
	}

	public void createRoom (JsonObject conf, Action<JsonObject> cb) {
		NetMgr net = NetMgr.GetInstance ();

		JsonObject args = new JsonObject ();
		args.Add ("conf", conf);

		Debug.Log ("gamemgr createRoom");

		net.request_connector ("create_private_room", args, ret=>{
			int code = Convert.ToInt32(ret["errcode"]);
			if (code != 0) {
				if (code == 2222) {
					Debug.Log ("房卡不足");
				} else {
					Debug.Log ("创建房间失败，错误码:" + code);
				}
			}

			cb(ret);
		});
	}

	public void enterRoom(string roomid, Action<int> cb = null) {
		NetMgr net = NetMgr.GetInstance ();
		LocationMgr lm = LocationMgr.GetInstance();

		JsonObject args = new JsonObject ();
		args.Add ("roomid", roomid);

		LocationInfo loc = lm.Get();

		if (loc != null && loc.valid ()) {
			JsonObject gps = new JsonObject ();
			gps.Add ("lat", loc.latitude);
			gps.Add ("lon", loc.longitude);
			gps.Add ("valid", true);
			args.Add ("gps", gps);
		}

		Debug.Log ("entering... " + roomid);

		net.request_connector ("enter_private_room", args, ret => {
			int code = Convert.ToInt32(ret["errcode"]);

			if (code != 0) {
				Debug.Log("enter room failed, code=" + code);

				if(cb != null) cb(code);
			} else {
				if (cb != null) cb(code);

				net.send("login", args);
			}
		});
	}

	public event Action eventUpCoins = null;

	public void get_coins(Action cb = null) {
		NetMgr nm = NetMgr.GetInstance ();

		nm.request_apis ("get_coins", null, ret => {
			GetCoins gc = JsonUtility.FromJson<GetCoins> (ret.ToString ());
			if (gc.errcode != 0) {
				Debug.Log("get_coins fail: " + gc.errmsg);
				return;
			}

			userMgr.gems = gc.gems;
			userMgr.coins = gc.golds;

			if (eventUpCoins != null)
				eventUpCoins();

			if (cb != null)
				cb.Invoke();
		});
	}

	public int get_gems() {
		return userMgr.gems;
	}

	public LoginInfo mLogin = null;
	public event Action eventUpDealerCoins = null;

	public void dealerLogin(Action<bool> cb = null) {
		var http = Http.GetInstance ();

		http.Post ("/dealer/login", null, text => {
			var ret = JsonUtility.FromJson<LoginInfo>(text);

			if (ret.errcode == 0) {
				mLogin = ret;

				if (eventUpDealerCoins != null)
					eventUpDealerCoins();

				if (cb != null)
					cb.Invoke(true);
			} else {
				if (cb != null)
					cb.Invoke(false);
			}
		}, err => {
			Debug.Log("login failed");
			if (cb != null)
				cb.Invoke(false);
		});
	}

	public static void share_club(int club_id, bool tl) {
		Debug.Log ("share_club: " + club_id);
		if (club_id == 0)
			return;

		string title = "<" + GameSettings.Instance.appname + ">";
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis ("get_club_detail", "club_id", club_id, data => {
			GetClubDetail ret = JsonUtility.FromJson<GetClubDetail> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("get_club_detail fail");
				return;
			}

			string content = ret.data.name + "俱乐部(ID:" + club_id + ")邀请您加入\n" + ret.data.desc;

			Dictionary<string, object> args = new Dictionary<string, object>();
			args.Add("club", club_id);

			AnysdkMgr.GetInstance().share(title, content, args, tl);
		});
	}

	public static ListClubRoom slist_club_rooms = null;

	public static void list_club_rooms(int club_id, Action<bool> cb) {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis ("list_club_rooms", "club_id", club_id, data => {
			ListClubRoom ret = JsonUtility.FromJson<ListClubRoom> (data.ToString ());
			if (ret.errcode != 0) {
				if (cb != null) cb(false);
				return;
			}

			slist_club_rooms = ret;

			if (cb != null)
				cb(true);
		});
	}

	public static void start_room(string room_tag, Action<bool> cb) {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_connector ("start_room", "room_tag", room_tag, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log ("start room fail");
				if (cb != null)
					cb (false);
				return;
			}

			if (cb != null)
				cb (true);
		});
	}

	public static void stop_room(string room_tag, Action<bool> cb) {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_connector ("stop_room", "room_tag", room_tag, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log ("start room fail");
				if (cb != null)
					cb (false);
				return;
			}

			if (cb != null)
				cb (true);
		});
	}


	public static void destroy_club_room(int id, string room_tag, int club_id, Action<bool> cb) {
		NetMgr nm = NetMgr.GetInstance();

		JsonObject ob = new JsonObject();
		ob["roomid"] = id;
		ob["room_tag"] = room_tag;
		ob["club_id"] = club_id;

		nm.request_apis("destroy_club_room", ob, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString());
			if (ret.errcode != 0) {
				Debug.Log("destroy club room fail");
				if (cb != null)
					cb (false);
				return;
			}

			if (cb != null)
				cb (true);
		});
	}

	public static void kick(int uid, int roomid, string room_tag, Action<bool> cb) {
		NetMgr nm = NetMgr.GetInstance ();

		JsonObject ob = new JsonObject(); 
		ob["uid"] = uid;
		ob["roomid"] = roomid;
		ob["room_tag"] = room_tag;

		nm.request_connector ("kick", ob, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("kick fail");
				if (cb != null)
					cb (false);
				return;
			}

			if (cb != null)
				cb (true);
		});
	}

	public static void join_club_channel(int club_id) {
		NetMgr nm = NetMgr.GetInstance();
		nm.request_apis ("join_club_channel", "club_id", club_id, data => {
			GameMgr.GetInstance().club_channel = club_id;
		});
	}

	public static void leave_club_channel(int club_id) {
		NetMgr nm = NetMgr.GetInstance();
		nm.request_apis ("leave_club_channel", "club_id", club_id, data => {
			GameMgr.GetInstance().club_channel = 0;
		});
	}

	public static void get_club_message_cnt(int club_id, Action<int> cb) {
		NetMgr.GetInstance ().request_apis ("get_club_message_cnt", "club_id", club_id, data => {
			GetClubMessageCnt ret = JsonUtility.FromJson<GetClubMessageCnt> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("get_club_message_cnt fail");
				if (cb != null) cb(0);
				return;
			}

			if (cb != null) cb(ret.data.cnt);
		});
	}

	public static ListGoodsFromShop sListGoodsFromShop = null;

	public static void list_goods_from_shop(Action<bool> cb) {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis ("list_goods_from_shop", null, data => {
			ListGoodsFromShop ret = JsonUtility.FromJson<ListGoodsFromShop> (data.ToString ());

			if (ret.errcode != 0) {
				Debug.Log("list_goods_from_shop fail");
				if (cb != null) cb(false);
				return;
			}

			sListGoodsFromShop = ret;

			if (cb != null) cb(true);
		});
	}
		
	public static GetRoomCosts sGetRoomCosts = null;

	public static void get_room_costs(Action<bool> cb) {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis ("get_room_costs", null, data => {
			GetRoomCosts ret = JsonUtility.FromJson<GetRoomCosts> (data.ToString ());

			if (ret.errcode != 0) {
				Debug.Log("get_room_costs fail");
				if (cb != null) cb(false);
				return;
			}

			sGetRoomCosts = ret;

			if (cb != null) cb(true);
		});
	}
}

