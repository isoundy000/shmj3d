
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyShare : MonoBehaviour {

	void Start () {
		
	}

	void close() {
		gameObject.SetActive (false);
	}

	public void onBtnShare() {
		var anysdk = AnysdkMgr.GetInstance ();

		anysdk.shareAd();
		close();
	}

	public void onBtnClose() {
		close();
	}
}

