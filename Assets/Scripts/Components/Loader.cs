
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {

	UIProgressBar loading = null;
	UILabel percent = null;

	void Start () {
		Transform ld = transform.Find("loading");

		if (ld != null) {
			loading = ld.GetComponent<UIProgressBar> ();
			loading.value = 0;

			percent = ld.Find ("percent").GetComponent<UILabel>();
		}

		AnysdkMgr.InitBuglySDK ();
	}

	public void setProgress(float progress) {
		if (loading != null)
			loading.value = progress;

		if (percent != null)
			percent.text = string.Format ("{0:P0}", progress);
	}
}

