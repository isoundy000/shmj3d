
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
		string name = string.Empty;
		if (id >= 0 && id < sprites.Count)
			name = sprites [id];

		uiSprite.spriteName = name;

		UISpriteData sp = uiSprite.GetAtlasSprite();
		uiSprite.width = sp.width;
		uiSprite.height = sp.height;

		index = id;
	}
}
