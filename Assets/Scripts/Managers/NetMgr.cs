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

	string mHost = null;
	int mPort = 0;

	bool mConnecting = false;
	bool mConnected = false;
	int mRetry = 0;

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
		db.Add ("test6", "46eb26925fbb6ed9da1c63c8a97e15f9481402af2a111f780b221ee90000c279");
		db.Add ("test7", "8a43c7ce4476094c324621c040f27a577e9404168ed4b978e2bdc7f6f90f635b");
		db.Add ("test8", "6786ced4d52bc6da7281103842e4cae2000b154e981c0ec2e019fce187913270");
		db.Add ("test9", "f87c9c53b46f81cfa9a2bed1902c931e5d6859abaa5091405a8058f4b823d362");
		db.Add ("test10", "de50407eb3693f5cf1b6e66ef574f14eaa0656907cddb35ad05e7f8751a69b1c");
		db.Add ("test11", "19e1fe0de30d60fe3c2c8c936e8ab36fb78ccda18ef3c442b354443911217442");
		db.Add ("test12", "a0361432240e3f2168ee577cbf2fe34456e37af76fbbb96196e52fc6c84315a8");

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
		mHost = host;
		mPort = port;

		pc.NetWorkStateChangedEvent += OnStateChanged;

		pc.initClient (host, port, () => {
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

					mConnected = true;

					GameMgr gm = GameMgr.GetInstance();
					gm.onLogin(ret);
				});
			});
		});
	}

	void reconnect(Action<bool> cb) {
		Debug.Log ("reconnect");

		pc.initClient (mHost, mPort, () => {
			pc.connect (null, data => {
				JsonObject obj = new JsonObject ();

				obj.Add ("token", mToken);

				pc.request ("connector.entryHandler.entry", obj, ret => {
					int code = Convert.ToInt32 (ret ["code"]);

					Debug.Log("entry return:" + code);

					if (code != 0) {
						cb(false);
						return;
					}


					Debug.Log ("login done");

					mConnected = true;

					GameMgr gm = GameMgr.GetInstance();
					gm.onResume(ret);
					cb(true);
				});
			});
		});
	}

	void doReconnect() {
		if (mConnecting) {
			Debug.Log ("isConnecting return");
			return;
		}

		Debug.Log ("doReconnect");

		mConnected = false;
		mConnecting = true;

		reconnect (ret => {
			mConnecting = false;
			if (ret)
				return;

			mRetry++;

			Debug.Log("reconnect fail: retry=" + mRetry);

			if (mRetry >= 10) {
				LoadingScene.LoadNewScene("01.login");
				return;
			}

			Utils.setTimeout(doReconnect, 3.0f);
		});

	}

	void OnStateChanged(NetWorkState state) {
		Debug.Log ("onStateChanged: " + state);

		if (state == NetWorkState.DISCONNECTED) {
			mRetry = 0;
			doReconnect();
		}
	}

	public void Update() {
		if (pc != null)
			pc.poll ();
	}

	public void request_apis(string route, string key, int value, Action<JsonObject> cb) {
		JsonObject data = new JsonObject ();
		data [key] = value;

		pc.request ("apis.apisHandler." + route, data, cb);
	}

	public void request_apis(string route, string key, string value, Action<JsonObject> cb) {
		JsonObject data = new JsonObject ();
		data [key] = value;

		pc.request ("apis.apisHandler." + route, data, cb);
	}

	public void request_apis(string route, JsonObject data, Action<JsonObject> cb) {
		if (data == null)
			data = new JsonObject ();

		pc.request ("apis.apisHandler." + route, data, cb);
	}

	public void request_connector(string route, string key, int value, Action<JsonObject> cb) {
		JsonObject data = new JsonObject ();
		data [key] = value;

		pc.request ("connector.entryHandler." + route, data, cb);
	}

	public void request_connector(string route, string key, string value, Action<JsonObject> cb) {
		JsonObject data = new JsonObject ();
		data [key] = value;

		pc.request ("connector.entryHandler." + route, data, cb);
	}

	public void request_connector(string route, JsonObject data, Action<JsonObject> cb) {
		if (data == null)
			data = new JsonObject ();

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
