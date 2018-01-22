using UnityEngine;
using System.Collections;

public class Login : MonoBehaviour {

	public GameObject input;
	public GameObject btnLogin;
	public GameObject btnGuest;

	void Awake() {
		AnysdkMgr.setPortait ();
	}

	void Start() {
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
