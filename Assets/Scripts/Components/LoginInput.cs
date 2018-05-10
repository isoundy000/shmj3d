using UnityEngine;
using System.Collections;

public class LoginInput : MonoBehaviour {

	public UIInput name;
	public UIInput passwd;

	void Awake() {
		UIButton btn_close = transform.Find ("btn_close").GetComponent<UIButton> ();
		btn_close.onClick.Add (new EventDelegate(this, "onBtnClose"));

		UIButton btn_submit = transform.Find ("btn_submit").GetComponent<UIButton> ();
		btn_submit.onClick.Add (new EventDelegate(this, "onBtnSubmit"));
	}

	void onBtnClose() {
		gameObject.SetActive (false);
	}

	void onBtnSubmit() {
		NetMgr net = NetMgr.GetInstance ();
		string account = name.value.Replace(" ", "");
		string pass = passwd.value;

		string[] accounts = new string[] { "test1", "test2", "test3", "test4", "test8" };

		bool found = false;

		for (int i = 0; i < accounts.Length; i++) {
			if (accounts [i] == account) {
				found = true;
				break;
			}
		}

		if (!found) {
			GameAlert.Show("用户不存在，请重新输入！", () => {
				name.value = "";
			});

			return;
		}

		if (pass != "654123") {
			GameAlert.Show ("密码错误, 请重新输入!", () => {
				passwd.value = "";
			});

			return;
		}

		net.TestLogin (account);
	}
}

