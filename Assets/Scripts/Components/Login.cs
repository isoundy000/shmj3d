using UnityEngine;
using System.Collections;

public class Login : MonoBehaviour {

	public void onBtnGuestClicked() {
		NetMgr net = NetMgr.GetInstance ();

		net.TestLogin ();
	}

	public void onBtnLoginClicked() {

	}

}
