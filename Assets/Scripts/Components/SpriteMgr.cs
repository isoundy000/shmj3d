
using UnityEngine;
using System.Collections.Generic;

public class SpriteMgr : MonoBehaviour {

	public List<string> sprites;
	UISprite uiSprite;

	public int index;

	void Awake() {
		uiSprite = transform.GetComponent<UISprite> ();

		setIndex (index);
	}

	public void setIndex (int id) {
		if (!(id >= 0 && id < sprites.Count)) {
			uiSprite.spriteName = null;
			index = id;
			return;
		}

		if (uiSprite == null) {
			Debug.Log ("uiSprite null");
			return;
		}

		uiSprite.spriteName = sprites [id];

		UISpriteData sp = uiSprite.GetAtlasSprite();
		uiSprite.width = sp.width;
		uiSprite.height = sp.height;

		index = id;
	}
}
