
using UnityEngine;
using System.Collections;
using AssetBundles;

public class Login : MonoBehaviour {

	public GameObject input;
	public GameObject btnLogin;
	public GameObject btnGuest;

	void Awake() {
		AnysdkMgr.setPortait ();

		StartCoroutine (LoadAsset());
	}

	IEnumerator LoadAsset() {
/*
		var request = AssetBundleManager.LoadAssetAsync("ab_ui_prefabs_logincenter", "Center", typeof(GameObject));
		if (request == null)
			yield break;

		yield return StartCoroutine(request);

		GameObject prefab = request.GetAsset<GameObject>();

		if (prefab != null)
			GameObject.Instantiate(prefab, transform);
*/
		var request = AssetBundleManager.LoadAssetAsync("ab_ui_prefabs_version", "version", typeof(TextAsset));
		if (request == null)
			yield break;

		yield return StartCoroutine(request);
		string text = request.GetAsset<TextAsset> ().text.Trim();
		transform.Find("version").GetComponent<UILabel>().text = text;

		bool wechat = AnysdkMgr.GetInstance().CheckWechat();
		bool native = AnysdkMgr.isNative();
		bool guest = !native || text.EndsWith ("S");

		btnLogin.SetActive(wechat);
		btnGuest.SetActive(guest);
	}

	void InitBuglySDK() {
		// 开启SDK的日志打印，发布版本请务必关闭
		BuglyAgent.ConfigDebugMode (true);
		// 注册日志回调，替换使用 'Application.RegisterLogCallback(Application.LogCallback)'注册日志回调的方式
		// BuglyAgent.RegisterLogCallback (CallbackDelegate.Instance.OnApplicationLogCallbackHandler);

		#if UNITY_IPHONE || UNITY_IOS
		BuglyAgent.InitWithAppId ("f71aaa33e2");
		#elif UNITY_ANDROID
		BuglyAgent.InitWithAppId ("Your App ID");
		#endif

		// 如果你确认已在对应的iOS工程或Android工程中初始化SDK，那么在脚本中只需启动C#异常捕获上报功能即可
		BuglyAgent.EnableExceptionHandler ();
	}

	void Start() {
		InitBuglySDK ();

		string account = PlayerPrefs.GetString ("wx_account");
		string token = PlayerPrefs.GetString ("wx_sign");

		if (account != null && account.Length > 0 && token != null && token.Length > 0)
			NetMgr.GetInstance().Login(account, token);
/*
		bool wechat = AnysdkMgr.GetInstance().CheckWechat();
		bool native = AnysdkMgr.isNative();

		btnLogin.SetActive(wechat);
		btnGuest.SetActive(!native || !wechat);
*/
	}

	public void onBtnGuestClicked() {
		//input.SetActive (true);
		NetMgr.GetInstance().TestLogin("test1");
	}

	public void onBtnLoginClicked() {
		AnysdkMgr.Login();
	}
}
