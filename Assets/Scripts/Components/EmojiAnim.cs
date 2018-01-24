using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiAnim : MonoBehaviour {

	float rate = 0;
	float nextFire = 0;

	UISprite sprite = null;
	List<string> names = new List<string>();

	public UIAtlas atlas = null;

	void Awake() {
		sprite = transform.GetComponent<UISprite>();
	}

	void run(int id) {
		names.Clear();

		int index = 1;

		do {
			string name = "fc" + (id * 100 + index);
			UISpriteData sp = atlas.GetSprite(name);



		} while (true);
	}

	void Update () {
		
	}
}
