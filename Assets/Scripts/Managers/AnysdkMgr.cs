
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;
using System.IO;

public class AnysdkMgr : MonoBehaviour {

	static AnysdkMgr mInstance = null;
	public static AnysdkMgr GetInstance() { return mInstance; }
	static Action<int, string> mPickNotify = null;

	void Awake() {
		mInstance = this;
	}
		
	public static void setPortait() {
		Screen.orientation = ScreenOrientation.Portrait;
		Screen.SetResolution (1080, 1920, false);
	}

	public static void setLandscape() {
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		Screen.SetResolution (1920, 1080, false);
	}

	public static void Login() {
		AndroidJavaClass wxapi = new AndroidJavaClass("com.dinosaur.shmj.WXAPI");
		wxapi.CallStatic("Login");
	}

	public void onLoginResp(string code) {
		if (code.Length == 0)
			return;

		Debug.Log ("onLoginResp: " + code);

		Dictionary<string, object> args = new Dictionary<string, object> ();
		args["code"] = code;
		args["os"] = "Android";

		Http.GetInstance().Get ("/wechat_auth", args, ret => {
			int errcode = Convert.ToInt32(ret["errcode"]);

			Debug.Log("errcode=" + errcode);

			if (errcode == 0) {
				string account = (string)ret["account"];
				string token = (string)ret["token"];

				PlayerPrefs.SetString("wx_account", account);
				PlayerPrefs.SetString("wx_sign", token);

				NetMgr.GetInstance().Login(account, token);
			}
		}, err => {
			Debug.Log("login fail: " + err);
		});
	}

	public static void pick(Action<int, string> notify) {
		string path = Application.persistentDataPath + "/ImageCache/";
		string file = path + "icon.jpg";

		if (File.Exists(file))
			File.Delete (file);

		mPickNotify = notify;

		AndroidJavaClass image = new AndroidJavaClass("com.dinosaur.shmj.Image");
		image.CallStatic("pickImage", path);
	}

	public void onPickResp(string res) {
		int ret = int.Parse (res);
		string file = Application.persistentDataPath + "/ImageCache/icon.jpg";

		Debug.Log ("onPickResp ret=" + ret);

		if (mPickNotify != null)
			mPickNotify(ret, file);
	}
}
