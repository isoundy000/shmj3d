
using System;
using UnityEngine;
using System.Collections;
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

	static string appid;

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

	[DllImport("__Internal")]
	private static extern void copyTextToClipboard(string text);

	[DllImport("__Internal")]
	private static extern string getTextFromClipboard();
#endif

	void Awake() {
		mInstance = this;

		appid = GameSettings.Instance.appid;
	}

	public static bool isAndroid() {
		return Application.platform == RuntimePlatform.Android;
	}

	public static bool isIOS() {
		return Application.platform == RuntimePlatform.IPhonePlayer;
	}

	public static bool isNative() {
		return isAndroid() || isIOS();
	}

	public static string getOS() {
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
			AndroidJavaClass wxapi = new AndroidJavaClass (appid + ".WXAPI");

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
		args["version"] = GameSettings.Instance.version;

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
			AndroidJavaClass image = new AndroidJavaClass (appid + ".Image");
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
		string url = "http://w.ztvps.com//share.html";
		string parameters = "";

		Debug.Log ("anysdk share");

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
			AndroidJavaClass wxapi = new AndroidJavaClass (appid + ".WXAPI");
			wxapi.CallStatic ("Share", url, title, desc, tl);
		} else if (isIOS ()) {
			#if UNITY_IPHONE
			shareIOS (url, title, desc, tl);
			#endif
		}
	}

	Texture2D captureScreen(Rect rect, string file) {
		Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24,false);    

		screenShot.ReadPixels(rect, 0, 0);
		screenShot.Apply();

		byte[] bytes = screenShot.EncodeToJPG();

		File.WriteAllBytes(file, bytes);

		return screenShot;
	}

	public void shareImg(bool tl, Action cb) {
		StartCoroutine (_shareImg(tl, cb));
	}

	IEnumerator _shareImg(bool tl, Action cb) {
		string file = Application.persistentDataPath + "/screenshot.jpg";
		Rect rect = new Rect(0, 0, Screen.width, Screen.height);

		yield return new WaitForEndOfFrame();

		var screenshot = captureScreen(rect, file);

		int h = 100;
		int w = (int)Math.Floor ((double)Screen.width * h / Screen.height);

		Debug.Log ("shareImg, w=" + w + " h=" + h);

		if (isAndroid ()) {
			AndroidJavaClass wxapi = new AndroidJavaClass (appid + ".WXAPI");
			wxapi.CallStatic ("ShareIMG", file, w, h, tl);
		} else if (isIOS ()) {
			#if UNITY_IPHONE
			shareImgIOS (file, w, h, tl);
			#endif
		}

		if (cb != null)
			cb();
	}

	public void onInvite(string query) {
		Debug.Log ("onInvite: " + query);

		if (query.Length == 0)
			return;

		Dictionary<string, string> ps = PUtils.parseQuery (query);

		string scene = SceneManager.GetActiveScene ().name;

		if (scene == "02.lobby") {
			GameObject ob = GameObject.Find ("UI Root");

			if (ob != null) {
				Lobby lb = ob.GetComponent<Lobby> ();

				if (lb != null)
					lb.checkQuery ();
			}
		} else if (scene == "04.table3d") {
			ClearQuery();
		}
	}

	public string GetQuery() {
		if (isIOS ()) {
			#if UNITY_IPHONE
			return getQuery ();
			#endif
		} else if (isAndroid ()) {
			#if UNITY_ANDROID
			AndroidJavaClass ma = new AndroidJavaClass (appid + ".MainActivity");
			string query =  ma.CallStatic<string>("getQuery");
			Debug.Log("getQuery: " + query);
			#endif
		}

		return "";
	}

	public void ClearQuery() {
		if (isAndroid ()) {
			#if UNITY_ANDROID
			AndroidJavaClass ma = new AndroidJavaClass (appid + ".MainActivity");
			ma.CallStatic("clearQuery");
			#endif
		} else if (isIOS ()) {
			#if UNITY_IPHONE
			clearQuery ();
			#endif
		}
	}

	public static BatteryInfo GetBatteryInfo() {
		BatteryInfo ret = new BatteryInfo();

		ret.power = 100;
		ret.state = "full";
	
		if (isAndroid ()) {
			#if UNITY_ANDROID
			string bat = "";

			try
			{
				AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
				AndroidJavaClass unityPluginLoader = new AndroidJavaClass(appid + ".UnitySystem");
				bat = unityPluginLoader.CallStatic<string>("GetBatteryState", currentActivity);
			} catch (Exception e) {}

			string[] arr = bat.Split('|');

			if (arr.Length == 3) {
				ret.power = Convert.ToInt32(arr[0]);

				if (arr[2] == "2")
					ret.state = "charging";
				else if (arr[2] == "5")
					ret.state = "full";
				else
					ret.state = "unplugged";
			}

			#endif
		} else if (isIOS ()) {
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

	public static NetworkInfo GetNetworkInfo() {
		NetworkInfo ret = new NetworkInfo ();

		ret.type = "wifi";
		ret.strength = 4;

		if (isAndroid ()) {
			#if UNITY_ANDROID


			#endif
		} else if (isIOS ()) {
			#if UNITY_IPHONE
/*
			int net = getNetworkInfo();

			ret.strength = net % 1000;
			int tmp = net / 1000;

			string[] types = new string[]{ "N", "wifi", "2G", "3G", "4G", "5G" };

			if (tmp >= types.Length)
				ret.type = "N";
			else
				ret.type = types[tmp];
*/
			#endif
		}

		return ret;
	}

	public static void setClipBoard(string text) {
		#if UNITY_ANDROID
		AndroidJavaObject androidObject = new AndroidJavaObject(appid + ".ClipBoardTools");     
		AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
		if (activity == null)
			return;

		androidObject.Call("copyTextToClipboard", activity, text);
		#endif

		#if UNITY_IPHONE
		copyTextToClipboard(text);
		#endif
	}

	public static string  getClipBoard() {
		#if UNITY_ANDROID
		AndroidJavaObject androidObject = new AndroidJavaObject(appid + ".ClipBoardTools");     
		AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
		if (activity == null)
			return "";

		String text = androidObject.Call<String>("getTextFromClipboard");

		return text;
		#endif

		#if UNITY_IPHONE
		String text = getTextFromClipboard();

		return text;
		#endif

		return "";
	}

	public static bool checkRecordPermissions() {
		#if UNITY_ANDROID
		AndroidJavaClass ma = new AndroidJavaClass (appid + ".MainActivity");
		return ma.CallStatic<bool>("checkRecordPermissions");
		#endif

		return true;
	}

	public static void requestRecordPermissions() {
		#if UNITY_ANDROID
		AndroidJavaClass ma = new AndroidJavaClass (appid + ".MainActivity");
		ma.CallStatic("requestRecordPermissions");
		#endif
	}

	public static void InitBuglySDK() {
		BuglyAgent.ConfigDebugMode (true);

		#if UNITY_IPHONE || UNITY_IOS
		BuglyAgent.InitWithAppId ("f71aaa33e2");
		#elif UNITY_ANDROID
		BuglyAgent.InitWithAppId ("c1a7b714ed");
		#endif

		BuglyAgent.EnableExceptionHandler ();
	}
}



