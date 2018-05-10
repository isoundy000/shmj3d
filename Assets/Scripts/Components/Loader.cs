
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {

	UITexture loading = null;

	void Start () {
		Transform ld = transform.Find("loading");

		if (ld != null) {
			loading = ld.GetComponent<UITexture> ();
			loading.fillAmount = 0;
		}
	}

	public void setProgress(float progress) {
		if (loading != null)
			loading.fillAmount = progress;
	}
}

