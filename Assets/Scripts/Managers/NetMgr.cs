using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pomelo.DotNetClient;
using SimpleJson;
using System.IO;


public class NetMgr {
	static NetMgr mInstance = null;

	string mGateUrl;
	int mGatePort;

	bool inited = false;

	public PomeloClient pc = null;
	string mAccount = null;
	string mToken = null;

	string mHost = null;
	int mPort = 0;

	bool mConnecting = false;
	bool mConnected = false;
	bool mLogin = false;
	int mRetry = 0;

	public static NetMgr GetInstance () {
		if (mInstance == null)
			mInstance = new NetMgr ();

		return mInstance;
	}

	public NetMgr () {
		mGateUrl = GameSettings.Instance.gateUrl;
		mGatePort = GameSettings.Instance.gatePort;
	}

	public void Init() {
		if (inited)
			return;

		inited = true;
	}

	public string getToken() {
		return mToken;
	}

	public void TestLogin (string account) {
		Dictionary<string, string> db = new Dictionary<string, string> ();

		db.Add ("test1", "73fe5768026b46245454e5cd6c0a631a7c3926a4ac7ec0b47ed81c0606ab406e");
		db.Add ("test2", "36e2e5f328cb1f3fc3c9e5ba7e4a169308f17f01ceb93cd179c8e71c8566fea5");
		db.Add ("test3", "096097de21ebd0cfa702bc4117123d8e56e9c46189d2a126cac0a725e4d80d01");
		db.Add ("test4", "f61510b8054c08db6f5c621b84cb6b96eb5607688d1018e06420690029f95553");
		db.Add ("test5", "8ab8d9bc0a3ef16021fa597474968b7f2b9c51c0fa4d2a649cb70a6e11703aea");
		db.Add ("test6", "5e8024d44b04284032733cd22329b5470af20bb2e1e42c94947231d29a87317a");
		db.Add ("test7", "8b3a296c6e1579da77cfb3678225f534223d654c2bc2e446ab3181278ce3f42e");
		db.Add ("test8", "3b9d9482b10d080eae7613bb9874c29600cdd0b5fab0b362248a9ce6c4bae71f");
		db.Add ("test9", "676caeb029cc6b4995c0cb053fe23093aa1666d11ec459a7868cde55ecafeebf");
		db.Add ("test10", "72cd5a4a6c3d151a207a0a7206ffbf028dceae4060a9887e006a4b457e3fa7b7");
		db.Add ("test11", "d7fdd268cdbf4f8b18b37e5433b25658b7be85673b8b5a5508bb059088fc50dc");
		db.Add ("test12", "e98df9f3de8aca620bf03b26ffc9e2b478d239b33832543fbc1f2dfc7ec2c11e");

		string token = null;

		if (db.TryGetValue (account, out token))
			Login (account, token);
	}

	public void Login (string account, string token) {
		mAccount = account;
		mToken = token;

		Debug.Log ("Login");

		pc = new PomeloClient ();

		pc.initClient (mGateUrl, mGatePort, ret => {
			if (!ret) {
				Debug.Log("Login initClient fail");
				Loom.QueueOnMainThread(() => GameAlert.Show("连接服务器失败，请检查网络"));
				pc = null;
				return;
			}

			Debug.Log("connect");

			pc.connect (null, data => {
				JsonObject msg = new JsonObject ();
				msg ["uid"] = account;

				mConnected = true;
				pc.request ("gate.gateHandler.queryEntry", msg, OnQuery);
			});
		});
	}

	void OnQuery(JsonObject ret) {
		Debug.Log("OnQuery disconnect");
		pc.release = true;
		pc = null;
		mConnected = false;

		if (!ret.ContainsKey ("code") || !ret.ContainsKey ("host") || !ret.ContainsKey ("port")) {
			Debug.Log ("OnQuery key not found!");
			return;
		}
			
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

		pc = new PomeloClient ();

		pc.initClient (mHost, port, result => {
			if (!result) {
				Debug.Log("Entry initClient fail");
				pc.Dispose();
				pc = null;
				return;
			}

			pc.connect (null, data => {
				JsonObject obj = new JsonObject ();

				obj.Add ("token", mToken);

				mConnected = true;

				pc.request ("connector.entryHandler.entry", obj, ret => {
					if (ret == null || !ret.ContainsKey("code")) {
						if (pc != null) {
							pc.release = true;
							pc = null;
						}

						mConnected = false;
						return;
					}

					int code = Convert.ToInt32 (ret ["code"]);
					if (code != 0) {
						Debug.Log("entry disconnect");
						if (pc != null) {
							pc.release = true;
							pc = null;
						}

						mConnected = false;
						return;
					}

					// TODO
					Debug.Log ("login done");
					mLogin = true;

					pc.NetWorkStateChangedEvent += OnStateChanged;

					GameMgr gm = GameMgr.GetInstance();
					gm.onLogin(ret);
				});
			});
		});
	}

	void reconnect(Action<bool> cb) {
		Debug.Log ("reconnect");

		pc = new PomeloClient();

		pc.initClient (mHost, mPort, result => {
			if (!result) {
				Debug.Log("reconnect initClient fail");
				pc.Dispose();
				pc = null;
				cb(false);
				return;
			}

			pc.connect (null, data => {
				JsonObject obj = new JsonObject ();

				obj.Add ("token", mToken);

				mConnected = true;

				pc.request ("connector.entryHandler.entry", obj, ret => {
					if (ret == null || !ret.ContainsKey("code")) {
						pc.release = true;
						pc = null;
						mConnected = false;
						cb(false);
						return;
					}

					int code = Convert.ToInt32 (ret ["code"]);

					Debug.Log("entry return:" + code);

					if (code != 0) {
						if (pc != null) {
							pc.release = true;
							pc = null;
						}
							
						mConnected = false;
						cb(false);
						return;
					}

					Debug.Log ("reconnect done");
					mLogin = true;

					pc.NetWorkStateChangedEvent += OnStateChanged;

					GameMgr gm = GameMgr.GetInstance();

					gm.onResume(ret);
					cb(true);
				});
			});
		});
	}

	public void clearCallBack() {
		if (pc != null)
			pc.clearCallBack ();
	}

	public void logout() {
		ReplayMgr rm = ReplayMgr.GetInstance();
		GameMgr gm = GameMgr.GetInstance();
		RoomMgr room = RoomMgr.GetInstance();

		if (pc != null) {
			pc.NetWorkStateChangedEvent -= OnStateChanged;
			pc.release = true;
			mConnected = false;
			mLogin = false;
			pc = null;
		}

		ResourcesMgr.GetInstance().release();

		PlayerPrefs.DeleteKey ("wx_account");
		PlayerPrefs.DeleteKey ("wx_sign");

		rm.clear();
		gm.Reset();
		room.reset();

		gm.Clear();

		AudioManager.GetInstance ().StopBGM ();

		LoadingScene.LoadNewScene("01.login");
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
			if (ret) {
				WaitMgr.Hide();
				return;
			}

			mRetry++;

			Debug.Log("reconnect fail: retry=" + mRetry);

			if (mRetry >= 10) {
				WaitMgr.Hide();

				Loom.QueueOnMainThread(()=>{
					GameAlert.Show("网络重连失败，即将返回登陆界面");
				});

				Loom.QueueOnMainThread(()=>{
					logout();
				}, 3.0f);

				return;
			}

			Loom.QueueOnMainThread(doReconnect, 1.0f);
		});

	}

	void OnStateChanged(NetWorkState state) {
		Debug.Log ("onStateChanged: " + state);

		if (pc == null)
			return;

		if (state == NetWorkState.DISCONNECTED) {
			mLogin = false;

			if (pc.getKicked()) {
				GameAlert.Show ("您的帐号已在另一台设备上登录，即将登出", () => {
					logout();
				});
			} else {
				mRetry = 0;
				WaitMgr.Show ("网络重连中，请等待...", 60, () => {
					GameAlert.Show ("网络重连超时，即将登出", () => {
						logout();
					});
				});

				if (pc != null) {
					pc.NetWorkStateChangedEvent -= OnStateChanged;
					pc.release = true;
					pc = null;
				}

				mConnected = false;
				doReconnect ();
			}
		}
	}

	public void Update() {
		if (pc != null && mConnected)
			pc.poll ();
	}



	public void request_apis(string route, string key, int value, Action<JsonObject> cb) {
		if (!mLogin || pc == null) {
			if (cb != null) {
				JsonObject ret = new JsonObject ();
				ret["errcode"] = 9001;
				ret["errmsg"] = "not connect";
				cb (ret);
			}

			return;
		}

		JsonObject data = new JsonObject ();
		data [key] = value;

		pc.request ("apis.apisHandler." + route, data, cb);
	}

	public void request_apis(string route, string key, string value, Action<JsonObject> cb) {
		if (!mLogin || pc == null) {
			if (cb != null) {
				JsonObject ret = new JsonObject ();
				ret["errcode"] = 9001;
				ret["errmsg"] = "not connect";
				cb (ret);
			}

			return;
		}
		
		JsonObject data = new JsonObject ();
		data [key] = value;

		pc.request ("apis.apisHandler." + route, data, cb);
	}

	public void request_apis(string route, JsonObject data, Action<JsonObject> cb) {
		if (!mLogin || pc == null) {
			if (cb != null) {
				JsonObject ret = new JsonObject ();
				ret["errcode"] = 9001;
				ret["errmsg"] = "not connect";
				cb (ret);
			}

			return;
		}
		
		if (data == null)
			data = new JsonObject ();

		pc.request ("apis.apisHandler." + route, data, cb);
	}

	public void request_connector(string route, string key, int value, Action<JsonObject> cb) {
		if (!mLogin || pc == null) {
			Debug.Log ("not connect.");
			return;
		}
		
		JsonObject data = new JsonObject ();
		data [key] = value;

		pc.request ("connector.entryHandler." + route, data, cb);
	}

	public void request_connector(string route, string key, string value, Action<JsonObject> cb) {
		if (!mLogin || pc == null) {
			Debug.Log ("not connect.");
			return;
		}
		
		JsonObject data = new JsonObject ();
		data [key] = value;

		pc.request ("connector.entryHandler." + route, data, cb);
	}

	public void request_connector(string route, JsonObject data, Action<JsonObject> cb) {
		if (!mLogin || pc == null) {
			Debug.Log ("not connect.");
			return;
		}
		
		if (data == null)
			data = new JsonObject ();

		pc.request ("connector.entryHandler." + route, data, cb);
	}

	public void send(string route, JsonObject data) {
		if (!mLogin || pc == null) {
			Debug.Log ("not connect.");
			return;
		}
		
		pc.notify ("game.gameHandler." + route, data);
	}

	public void send(string route, string data = null) {
		if (!mLogin || pc == null) {
			Debug.Log ("not connect.");
			return;
		}
		
		JsonObject body = data != null ? (JsonObject)SimpleJson.SimpleJson.DeserializeObject (data) : new JsonObject();

		send (route, body);
	}

	public void send(string route, string key, int value) {
		if (!mLogin || pc == null) {
			Debug.Log ("not connect.");
			return;
		}

		JsonObject body = new JsonObject ();
		body [key] = value;

		send (route, body);
	}

	public void send(string route, string key, string value) {
		if (!mLogin || pc == null) {
			Debug.Log ("not connect.");
			return;
		}
		
		JsonObject body = new JsonObject ();
		body [key] = value;

		send (route, body);
	}
}
