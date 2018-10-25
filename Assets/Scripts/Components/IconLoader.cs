using UnityEngine;
using System.Collections;

public class IconLoader : MonoBehaviour {
	UITexture texture = null;

	public int width = 0;
	public int height = 0;

	int mUID = 0;

	void Awake() {
		texture = transform.GetComponent<UITexture> ();
	}

	void Start() {
		if (mUID > 0)
			setUserID (mUID);
	}

	public void setUserID(int uid) {
		if (uid == mUID && texture != null && texture.mainTexture != null)
			return;

		mUID = uid;
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


