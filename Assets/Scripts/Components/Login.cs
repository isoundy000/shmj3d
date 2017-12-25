using UnityEngine;
using System.Collections;

public class Login : MonoBehaviour {

	void Awake() {
		AnysdkMgr.setPortait ();
	}

	public void onBtnGuestClicked() {
		NetMgr net = NetMgr.GetInstance ();

		net.TestLogin ();
	}

	public void onBtnLoginClicked() {

	}

}
