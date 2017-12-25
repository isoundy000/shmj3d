using UnityEngine;
using System.Collections;

public class LoginInput : MonoBehaviour {

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
		
	}
}
