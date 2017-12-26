using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pomelo.DotNetClient;
using SimpleJson;
using System.IO;


public class NetMgr {
	static NetMgr mInstance = null;
	static string mServer = "ip.queda88.com";

	bool inited = false;

	public PomeloClient pc = null;
	string mAccount = null;
	string mToken = null;

	public static NetMgr GetInstance () {
		if (mInstance == null)
			mInstance = new NetMgr ();

		return mInstance;
	}

	public NetMgr () {

	}

	public void Init() {
		if (inited)
			return;

		pc = new PomeloClient ();

		inited = true;
	}

	public void TestLogin (string account) {
		Dictionary<string, string> db = new Dictionary<string, string> ();

		db.Add ("test1", "0d648f562a37229dde3b0c95e083213d6152ecb319a42468f04528d985473b10");
		db.Add ("test2", "7358d8c19b8f7f60cd086ccb3614c03ab43f3cbaa3727e00aaf5908dac4540a1");
		db.Add ("test3", "8cf59929351ba7201c55072ed7a12b88d2b0225da8a8afb4cbabdc8371f335ad");
		db.Add ("test4", "998331947b6f82971aae44518322ff862741f3bd90a9c59e0cfd564d5f6922ee");
		db.Add ("test5", "9970d6b7aaaa42d5f138a12bc91176d5d5a9a7641df6f5696d8feda2f92e35c1");

		string token = null;

		if (db.TryGetValue (account, out token))
			Login (account, token);
	}

	public void Login (string account, string token) {
		int port = 5005;

		mAccount = account;
		mToken = token;

		pc.initClient (mServer, port, () => {
			pc.connect (null, data => {
				JsonObject msg = new JsonObject ();
				msg ["uid"] = account;

				pc.request ("gate.gateHandler.queryEntry", msg, OnQuery);
			});
		});
	}

	void OnQuery(JsonObject ret) {
		pc.disconnect ();

		int code = Convert.ToInt32 (ret ["code"]);
		if (code != 0)
			return;

		string host = (string)ret["host"];
		int port = Convert.ToInt32 (ret["port"]);

		Entry (host, port);
	}

	void Entry (string host, int port) {
		//pc = new PomeloClient ();

		pc.NetWorkStateChangedEvent += OnStateChanged;

		pc.initClient (mServer, port, () => {
			pc.connect (null, data => {
				JsonObject obj = new JsonObject ();

				obj.Add ("token", mToken);

				pc.request ("connector.entryHandler.entry", obj, ret => {
					int code = Convert.ToInt32 (ret ["code"]);

					if (code != 0) {
						pc.disconnect ();
						return;
					}

					// TODO
					Debug.Log ("login done");

					GameMgr gm = GameMgr.GetInstance();
					gm.onLogin(ret);
				});
			});
		});
	}

	void OnStateChanged(NetWorkState state) {
		Debug.Log (state);

		// TODO
	}

	public void Update() {
		if (pc != null)
			pc.poll ();
	}

	public void request_apis(string route, JsonObject data, Action<JsonObject> cb) {
		pc.request ("apis.apisHandler." + route, data, cb);
	}

	public void request_connector(string route, JsonObject data, Action<JsonObject> cb) {
		pc.request ("connector.entryHandler." + route, data, cb);
	}

	public void send(string route, JsonObject data) {
		pc.notify ("game.gameHandler." + route, data);
	}

	public void send(string route, string data = null) {
		JsonObject body = data != null ? (JsonObject)SimpleJson.SimpleJson.DeserializeObject (data) : new JsonObject();

		send (route, body);
	}

	public void send(string route, string key, int value) {
		JsonObject body = new JsonObject ();
		body [key] = value;

		send (route, body);
	}

	public void send(string route, string key, string value) {
		JsonObject body = new JsonObject ();
		body [key] = value;

		send (route, body);
	}
}
