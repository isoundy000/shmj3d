using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Pomelo.DotNetClient;
using SimpleJson;
using System.IO;

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
public class EmojiPush {
	public int sender;
	public int content;
}

public class GameMgr {
	static GameMgr mInstance = null;

	bool inited = false;

	public delegate void MsgHandler(object data);

	Dictionary<string, MsgHandler> mHandlerMap = new Dictionary<string, MsgHandler>();

	public UserMgr userMgr = new UserMgr();

	public int club_channel = 0;

	public static GameMgr GetInstance () {
		if (mInstance == null)
			mInstance = new GameMgr ();

		return mInstance;
	}

	public static UserMgr getUserMgr() {
		return mInstance.userMgr;
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
		});

		pc.on ("login_finished", data => {
			Debug.Log("login_finished");

			mHandlerMap.Clear();
			LoadingScene.LoadNewScene("04.table3d");
		});

		pc.on ("exit_result", data=>{
			string reason = (string)data["reason"];

			if (reason == "kick") {
				GameAlert.GetInstance().show("您已被管理员请出房间", ()=>{
					mHandlerMap.Clear();
					rm.reset();
					LoadingScene.LoadNewScene("02.lobby");
				});
			} else if (reason == "request") {
				mHandlerMap.Clear();
				rm.reset();
				LoadingScene.LoadNewScene("02.lobby");
			}
		});

		pc.on ("exit_notify_push", data => {
			int uid = Convert.ToInt32(data["value"]);
			int seatindex = rm.userExit(uid);

			DispatchEvent("user_state_changed", seatindex);
		});

		pc.on ("dispress_push", data => {
			rm.reset();

			mHandlerMap.Clear();
			LoadingScene.LoadNewScene("02.lobby");
		});

		pc.on ("new_user_comes_push", data => {
			int seatindex = rm.newUserCome(data);

			DispatchEvent("user_state_changed", seatindex);
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

			int seatindex = rm.updateSeat(data);
			// todo protocol to be changed
			DispatchEvent("game_holds", seatindex);
		});

		pc.on ("game_holds_updated", data => {
			Debug.Log("get game_holds_updated");

			int seatindex = rm.updateSeat(data);

			DispatchEvent("game_holds", seatindex);
		});

		pc.on ("game_state_push", data => {
			rm.updateState(data);

			DispatchEvent("game_state");
		});

		pc.on ("game_begin_push", data => {
			Debug.Log("get game_begin_push");

			rm.updateState(data);
			rm.newRound();

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

			rm.updateState(data);
			rm.updateSeats((JsonArray)data["seats"]);

			DispatchEvent("user_hf_updated");
			DispatchEvent("game_sync");
		});

		pc.on ("hangang_notify_push", data => {
			int seatindex = Convert.ToInt32(data["seatindex"]);
			DispatchEvent("hangang_notify", seatindex);
		});
	
		pc.on ("game_action_push", data => {
			Debug.Log("get game_action_push");
			rm.updateAction(data);

			DispatchEvent("game_action");
		});

		pc.on ("game_chupai_push", data => {
			Debug.Log("get game_chupai_push");
			rm.updateState(data);

			DispatchEvent("game_turn_change");
		});

		pc.on ("game_num_push", data => {
			rm.updateRoomInfo(data);

			DispatchEvent("game_num");
		});

		pc.on ("game_over_push", data => {
			rm.updateOverInfo(data);

			DispatchEvent("game_over");
		});

		pc.on ("mj_count_push", data => {
			rm.updateState(data);

			DispatchEvent("mj_count");
		});

		pc.on ("hu_push", data => {
			HuPushInfo info = JsonUtility.FromJson<HuPushInfo>(data.ToString());
			DispatchEvent("hupai", info);
		});

		pc.on ("game_chupai_notify_push", data => {
			Debug.Log("get game_chupai_notify_push");
			ActionInfo info = rm.doChupai(data);

			DispatchEvent("game_chupai_notify", info);
		});

		pc.on ("game_hf_push", data => {
			rm.updateFlowers(data);

			DispatchEvent("user_hf_updated");
		});

		pc.on ("game_af_push", data => {
			ActionInfo info = rm.doAddFlower(data);

			DispatchEvent("user_hf_updated", info);
		});

		pc.on ("game_mopai_push", data => {
			Debug.Log("get game_mopai_push");
			ActionInfo info = rm.doMopai(data);

			DispatchEvent("game_mopai", info);
		});

		pc.on ("guo_notify_push", data => {
			Debug.Log("get guo_notify_push");
			ActionInfo info = rm.doGuo(data);

			DispatchEvent("guo_notify", info);
		});

		pc.on ("guo_result", data => {
			Debug.Log("get guo_result");
			DispatchEvent("guo_result");
		});

		pc.on ("peng_notify_push", data => {
			ActionInfo info = rm.doPeng(data);

			DispatchEvent("peng_notify", info);
		});

		pc.on ("chi_notify_push", data => {
			ActionInfo info = rm.doChi(data);

			DispatchEvent("chi_notify", info);
		});

		pc.on ("gang_notify_push", data => {
			GangInfo info = rm.doGang(data);

			DispatchEvent("gang_notify", info);
		});

		pc.on ("ting_notify_push", data => {
			TingInfo info = rm.doTing(data);

			DispatchEvent("ting_notify", info.seatindex);
		});

		pc.on ("chat_push", data => {
			ChatInfo info = JsonUtility.FromJson<ChatInfo>(data.ToString());

			DispatchEvent("chat", info);
		});

		pc.on ("quick_chat_push", data => {

		});

		pc.on ("emoji_push", data => {
			EmojiPush info = JsonUtility.FromJson<EmojiPush>(data.ToString());
			DispatchEvent("emoji_push", info);
		});

		pc.on ("demoji_push", data => {

		});

		pc.on ("dissolve_notice_push", data => {
			DissolveInfo dv = JsonUtility.FromJson<DissolveInfo>(data.ToString());

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

			DispatchEvent("club_message_notify", ret);
		});

		pc.on ("sys_message_updated", data => {
			DispatchEvent("sys_message_updated", data);
		});
	}

	public void Reset() {
		mHandlerMap.Clear ();
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
		string sign = userMgr.sign;
		userMgr = JsonUtility.FromJson<UserMgr>(data.ToString());
		userMgr.sign = sign;

		Debug.Log ("userName " + userMgr.username);
		Debug.Log ("ip: " + userMgr.ip);

		InitHandler ();

		LoadingScene.LoadNewScene ("02.lobby");
	}

	public void onResume(JsonObject data) {
		string sign = userMgr.sign;
		userMgr = JsonUtility.FromJson<UserMgr>(data.ToString());
		userMgr.sign = sign;

		string roomid = userMgr.roomid;

		if (roomid != null && roomid.Length == 6) {
			enterRoom(roomid);
			userMgr.roomid = null;
		}
			
	}

	public void createRoom (JsonObject conf, Action<JsonObject> cb) {
		NetMgr net = NetMgr.GetInstance ();

		JsonObject args = new JsonObject ();
		args.Add ("conf", conf);

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

		JsonObject args = new JsonObject ();
		args.Add ("roomid", roomid);

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


}

