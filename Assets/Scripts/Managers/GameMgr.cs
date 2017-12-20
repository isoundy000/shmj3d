using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Pomelo.DotNetClient;
using SimpleJson;
using System.IO;

public class GameMgr {
	static GameMgr mInstance = null;

	bool inited = false;

	public delegate void MsgHandler(JsonObject data);

	Dictionary<string, MsgHandler> mHandlerMap = new Dictionary<string, MsgHandler>();

	public string account = null;
	public int userId = 0;
	public string userName = null;
	public int level = 0;
	public int gems = 0;
	public int coins = 0;
	public string roomData = null;
	public int sex = 0;
	public string ip = null;
	public bool is_agent = false;

	public static GameMgr GetInstance () {
		if (mInstance == null)
			mInstance = new GameMgr ();

		return mInstance;
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
		PomeloClient pc = net.pc;

		pc.on ("login_result", data => {
			Debug.Log("login_result");
		});

		pc.on ("login_finished", data => {
			Debug.Log("login_finished");
		});
	}

	public void Reset() {
		mHandlerMap.Clear ();
	}

	public void DispatchEvent(string msg, JsonObject data) {
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
		account = (string)data["account"];
		userId = Convert.ToInt32(data ["userid"]);
		userName = (string)data ["username"];
		level = Convert.ToInt32(data["lv"]);
		coins = Convert.ToInt32(data ["coins"]);
		gems = Convert.ToInt32(data["gems"]);
		roomData = (string)data ["roomid"];
		sex = Convert.ToInt32(data["sex"]);
		ip = (string)data["ip"];

		Debug.Log ("userName " + userName);
		Debug.Log ("ip: " + ip);

		InitHandler ();

		LoadingScene.LoadNewScene ("02.lobby");
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

	public void enterRoom(string roomid, Action<int> cb) {
		NetMgr net = NetMgr.GetInstance ();

		JsonObject args = new JsonObject ();
		args.Add ("roomid", roomid);

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

