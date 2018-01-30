
using UnityEngine;
using System.Collections;

public class Login : MonoBehaviour {

	public GameObject input;
	public GameObject btnLogin;
	public GameObject btnGuest;

	void Awake() {
		AnysdkMgr.setPortait ();
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

		if (account != null && token != null)
			NetMgr.GetInstance().Login(account, token);

		btnLogin.SetActive(AnysdkMgr.GetInstance().CheckWechat());
	}

	public void onBtnGuestClicked() {
/*
		NetMgr net = NetMgr.GetInstance ();

		net.TestLogin ();
*/
		input.SetActive (true);
	}

	public void onBtnLoginClicked() {
		AnysdkMgr.Login();
	}
}
