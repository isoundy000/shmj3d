
using UnityEngine;
using System.Collections;
using AssetBundles;

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
		transform.Find("version").GetComponent<UILabel>().text = text;

		bool wechat = AnysdkMgr.GetInstance().CheckWechat();
		bool native = AnysdkMgr.isNative();
		bool guest = !native || text.EndsWith ("S");

		btnLogin.SetActive(wechat);
		btnGuest.SetActive(guest);
	}

	void Start() {
		string account = PlayerPrefs.GetString ("wx_account");
		string token = PlayerPrefs.GetString ("wx_sign");

		if (account != null && account.Length > 0 && token != null && token.Length > 0)
			NetMgr.GetInstance().Login(account, token);

		bool wechat = AnysdkMgr.GetInstance().CheckWechat();
		bool native = AnysdkMgr.isNative();

		btnLogin.SetActive(wechat);
		btnGuest.SetActive(!native || !wechat);
	}

	public void onBtnGuestClicked() {
		AudioManager.PlayButtonClicked();
		//input.SetActive (true);
		NetMgr.GetInstance().TestLogin("test2");
	}

	public void onBtnLoginClicked() {
		AudioManager.PlayButtonClicked();
		AnysdkMgr.Login();
	}
}
