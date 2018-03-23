
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class BatteryInfo {
	public int power;
	public string state;
}

public class NetworkInfo {
	public string type;
	public int strength;
}


public class AnysdkMgr : MonoBehaviour {

	static AnysdkMgr mInstance = null;
	public static AnysdkMgr GetInstance() { return mInstance; }
	static Action<int, string> mPickNotify = null;

#if UNITY_IPHONE
	[DllImport("__Internal")]
	private static extern void wechatLogin();

	[DllImport("__Internal")]
	private static extern void pickImage(string path);

	[DllImport("__Internal")]
	private static extern bool checkWechat();

	[DllImport("__Internal")]
	private static extern int getBatteryInfo();

	[DllImport("__Internal")]
	private static extern int getNetworkInfo();

	[DllImport("__Internal")]
	private static extern void shareIOS(string url, string title, string desc, bool tl);

	[DllImport("__Internal")]
	private static extern void shareImgIOS(string path, int weight, int height, bool tl);

	[DllImport("__Internal")]
	private static extern string getQuery();

	[DllImport("__Internal")]
	private static extern void clearQuery();
#endif

	void Awake() {
		mInstance = this;
	}

	static bool isAndroid() {
		return Application.platform == RuntimePlatform.Android;
	}

	static bool isIOS() {
		return Application.platform == RuntimePlatform.IPhonePlayer;
	}

	static bool isNative() {
		return isAndroid() || isIOS();
	}

	static string getOS() {
		return isIOS () ? "iOS" : "Android";
	}

	public static void setPortait() {
		Screen.orientation = ScreenOrientation.Portrait;
		//Screen.SetResolution(1080, 1920, false);
	}

	public static void setLandscape() {
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		//Screen.SetResolution (1920, 1080, false);
	}

	public static void Login() {
		if (isAndroid ()) {
			AndroidJavaClass wxapi = new AndroidJavaClass ("com.dinosaur.shmj.WXAPI");
			wxapi.CallStatic ("Login");
		} else if (isIOS()) {
			#if UNITY_IPHONE
			wechatLogin();
			#endif
		}
	}

	public void onLoginResp(string code) {
		if (code.Length == 0)
			return;

		Debug.Log ("onLoginResp: " + code);

		Dictionary<string, object> args = new Dictionary<string, object> ();
		args["code"] = code;
		args["os"] = getOS();

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

		if (isAndroid ()) {
			AndroidJavaClass image = new AndroidJavaClass ("com.dinosaur.shmj.Image");
			image.CallStatic ("pickImage", path);
		} else if (isIOS()) {
			#if UNITY_IPHONE
			pickImage(path);
			#endif
		}
	}

	public void onPickResp(string res) {
		int ret = int.Parse (res);
		string file = Application.persistentDataPath + "/ImageCache/icon.jpg";

		Debug.Log ("onPickResp ret=" + ret);

		if (mPickNotify != null)
			mPickNotify(ret, file);
	}

	public bool CheckWechat() {
		if (isAndroid ())
			return true;
		else if (isIOS ()) {
			#if UNITY_IPHONE
			return checkWechat ();
			#endif
		}

		return false;
	}

	public void share(string title, string desc, Dictionary<string, object> args, bool tl = false) {
		string url = "http://www.queda88.com/share.html";
		string parameters = "";

		if (args.Count > 0) {
			bool first = true;

			parameters += "?";
			foreach (KeyValuePair<string, object> arg in args) {
				if (first)
					first = false;
				else
					parameters += "&";

				parameters += arg.Key + "=" + arg.Value.ToString();
			}
		}

		url += parameters;

		if (isAndroid ()) {
			AndroidJavaClass wxapi = new AndroidJavaClass ("com.dinosaur.shmj.WXAPI");
			wxapi.CallStatic ("Share", url, title, desc, tl);
		} else if (isIOS ()) {
			#if UNITY_IPHONE
			shareIOS (url, title, desc, tl);
			#endif
		}
	}

	public void shareImg(bool tl) {
		// TODO
	}

	public void onInvite(string query) {
		Debug.Log ("onInvite: " + query);

		if (query.Length == 0)
			return;

		Dictionary<string, string> ps = Utils.parseQuery (query);

		string scene = SceneManager.GetActiveScene ().name;

		if (scene == "02.lobby") {
			Lobby lb = GameObject.Find ("UI Root").GetComponent<Lobby> ();

			if (lb != null)
				lb.checkQuery ();
		} else if (scene == "04.table3d") {
			ClearQuery();
		}
	}

	public string GetQuery() {

		if (isIOS ()) {
			#if UNITY_IPHONE
			return getQuery ();
			#endif
		}

		return "";
	}

	public void ClearQuery() {
		if (isAndroid ())
			return;	// TODO
		else if (isIOS ()) {
			#if UNITY_IPHONE
			clearQuery ();
			#endif
		}
	}

	public BatteryInfo GetBatteryInfo() {
		BatteryInfo ret = new BatteryInfo();

		ret.power = 100;
		ret.state = "full";

		if (isAndroid ())
			return ret; // TODO
		else if (isIOS ()) {
			#if UNITY_IPHONE
			int battery = getBatteryInfo ();

			ret.power = battery % 1000;

			int tmp = battery / 1000;
			if (tmp == 2)
				ret.state = "charging";
			else if (tmp == 3)
				ret.state = "full";
			else
				ret.state = "unplugged";
			#endif
		}

		return ret;
	}

	public NetworkInfo GetNetworkInfo() {
		NetworkInfo ret = new NetworkInfo ();

		ret.type = "wifi";
		ret.strength = 4;

		if (isAndroid ())
			return ret;
		else if (isIOS ()) {
			#if UNITY_IPHONE
			int net = getNetworkInfo();

			ret.strength = net % 1000;
			int tmp = net / 1000;

			string[] types = new string[]{ "N", "wifi", "2G", "3G", "4G", "5G" };

			if (tmp >= types.Length)
				ret.type = "N";
			else
				ret.type = types[tmp];
			#endif
		}

		return ret;
	}
}



