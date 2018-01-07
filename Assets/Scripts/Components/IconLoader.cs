using UnityEngine;
using System.Collections;

public class IconLoader : MonoBehaviour {
	UITexture texture = null;

	public int width = 0;
	public int height = 0;

	void Awake() {
		texture = transform.GetComponent<UITexture> ();
	}

	public void setUserID(int uid) {
		if (texture == null)
			return;

		if (uid <= 0) {
			texture.mainTexture = null;
			return;
		}

		UserInfoMgr.GetInstance ().getBaseInfo (uid, info => {
			if (info != null)
				ImageLoader.GetInstance().LoadImage(info.headimgurl, texture);
		});
	}
}


