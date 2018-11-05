
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;
using SimpleJson;

[Serializable]
public class UpgradeInfo {
	public int code;
	public string version;
	public string url;
	public string changelog;
	public bool must;
}

public class Login : MonoBehaviour {

	public GameObject input;
	public GameObject btnLogin;
	public GameObject btnGuest;

	void Awake() {
		AnysdkMgr.setPortait ();
		//StartCoroutine (LoadAsset());
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

		transform.Find ("version").GetComponent<UILabel> ().text = GameSettings.Instance.version;

		bool wechat = AnysdkMgr.GetInstance().CheckWechat();
		bool native = AnysdkMgr.isNative();
		bool guest = !native/* || text.EndsWith ("S") */;

		btnLogin.SetActive(wechat);
		btnGuest.SetActive(guest);
	}

	void Start() {
		//bool wechat = AnysdkMgr.GetInstance().CheckWechat();
		bool native = AnysdkMgr.isNative();

		btnLogin.SetActive(true);
		btnGuest.SetActive(!native);

		transform.Find("version").GetComponent<UILabel> ().text = GameSettings.Instance.version;

		AudioManager.GetInstance().PlayBackgroundAudio("hall_bgm");

		checkUpgrade ();
	}

	void autoLogin() {
		string account = PlayerPrefs.GetString ("wx_account");
		string token = PlayerPrefs.GetString ("wx_sign");

		if (account != null && account.Length > 0 && token != null && token.Length > 0)
			NetMgr.GetInstance().Login(account, token);
	}

	int compareVersion(string a, string b) {
		string[] aa = a.Split ('.');
		string[] bb = b.Split ('.');

		if (aa.Length < 2 || bb.Length < 2)
			return 0;

		int a1, a2, b1, b2;
		if (Int32.TryParse (aa [0], out a1) && Int32.TryParse (aa [1], out a2) &&
			Int32.TryParse (bb [0], out b1) && Int32.TryParse (bb [1], out b2))
		{
			if (a1 != b1)
				return a1 - b1;
			else
				return a2 - b2;
		}

		return 0;
	}

	void showUpgrade(UpgradeInfo info) {
		var current = GameSettings.Instance.version;
		var version = info.version;
		var compare = compareVersion (current, version);

		if (compare >= 0) {
			Debug.Log ("version compare >= 0");
			autoLogin ();
			return;
		}

		var upgrade = transform.Find ("upgrade");
		var body = upgrade.Find ("body");

		upgrade.gameObject.SetActive (true);

		var context = body.Find ("content").GetComponent<UILabel> ();

		if (context != null)
			context.text = info.changelog;

		PUtils.setBtnEvent (body, "btnUpgrade", () => {
			Application.OpenURL(info.url);
		});

		PUtils.setBtnEvent (body, "btnClose", () => {
			upgrade.gameObject.SetActive(false);
			autoLogin();
		});
	}

	void checkUpgrade() {
		JsonObject args = new JsonObject ();
		args.Add("os", AnysdkMgr.getOS());

		var http = Http.GetInstance ();

		http.Post ("/check_upgrade", args, text => {
			if (string.IsNullOrEmpty(text)) {
				Debug.Log("text null");
				autoLogin();
				return;
			}

			UpgradeInfo ret = JsonUtility.FromJson<UpgradeInfo> (text);

			if (ret.code != 0) {
				autoLogin();
				return;
			}

			showUpgrade(ret);
		}, err => {
			autoLogin();
		}, false);
	}

	public void onBtnGuestClicked() {
		AudioManager.PlayButtonClicked();
		//input.SetActive (true);
		NetMgr.GetInstance().TestLogin("test1");
	}

	public void onBtnLoginClicked() {
		AudioManager.PlayButtonClicked();
		AnysdkMgr.Login();
	}
}
