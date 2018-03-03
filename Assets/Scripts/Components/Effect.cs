using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour {

	UISprite sp1 = null;
	UISprite sp2 = null;
	UISprite sp3 = null;
	Animation anim = null;

	void Awake() {
		sp1 = transform.Find("sp1").GetComponent<UISprite>();
		sp2 = transform.Find("sp2").GetComponent<UISprite>();
		sp3 = transform.Find("sp3").GetComponent<UISprite>();

		anim = GetComponent<Animation>();
	}

	void updateSize(UISprite sp) {
		UISpriteData data = sp.GetAtlasSprite();
		sp.width = data.width;
		sp.height = data.height;
	}

	public void playAction(string action) {
		sp1.spriteName = action + "1";
		updateSize(sp1);

		sp2.spriteName = action + "2";
		updateSize(sp2);

		sp3.spriteName = action + "3";
		updateSize(sp3);

		anim.Play(action);
	}
}
