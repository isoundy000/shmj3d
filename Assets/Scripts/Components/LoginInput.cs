using UnityEngine;
using System.Collections;

public class LoginInput : MonoBehaviour {

	public UIInput name;
	public UILabel lblName;

	public UIInput passwd;
	public UILabel lblPasswd;

	void Awake() {
		UIButton btn_close = transform.FindChild ("btn_close").GetComponent<UIButton> ();
		btn_close.onClick.Add (new EventDelegate(this, "onBtnClose"));

		UIButton btn_submit = transform.FindChild ("btn_submit").GetComponent<UIButton> ();
		btn_submit.onClick.Add (new EventDelegate(this, "onBtnSubmit"));
	}

	void onBtnClose() {
		gameObject.SetActive (false);
	}

	void onBtnSubmit() {
		NetMgr net = NetMgr.GetInstance ();
		string account = name.value;

		net.TestLogin (account);
	}
}

