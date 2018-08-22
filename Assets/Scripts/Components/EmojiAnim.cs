using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiAnim : MonoBehaviour {

	float rate = 0;
	float nextFire = 0;

	List<string> names = new List<string>();
	int current = 0;

	public UIAtlas atlas = null;
	public bool PlayOnLoad = false;
	public bool loop = false;

	float endtime = 0;

	void Start() {
		if (PlayOnLoad)
			run (1);
	}

	public void run(int id) {
		gameObject.SetActive (true);
		names.Clear();

		int index = 1;
		do {
			string name = "fc" + (id * 100 + index);
			UISpriteData sp = atlas.GetSprite(name);

			if (sp == null)
				break;

			Debug.Log("add " + name);
			names.Add(name);
			index++;
		} while (true);

		int cnt = names.Count;
		if (cnt == 0) {
			Debug.Log ("names null");
			reset ();
			return;
		}

		float now = Time.time;

		current = 0;

		var sprite = transform.GetComponent<UISprite> ();
		if (sprite != null)
			sprite.spriteName = names[0];
		
		rate = 1.0f / cnt;
		nextFire = now + rate;
		endtime = now + 3.0f;
	}
		
	void reset() {
		current = 0;
		endtime = 0;
		rate = 0;
		nextFire = 0;

		var sprite = transform.GetComponent<UISprite> ();
		if (sprite != null)
			sprite.spriteName = null;

		names.Clear ();
		gameObject.SetActive (false);
	}

	void Update () {
		float now = Time.time;
		if (endtime == 0)
			return;

		if (now > endtime && !loop) {
			reset ();
			return;
		}

		if (now < nextFire)
			return;

		nextFire = now + rate;

		int cnt = names.Count;
		if (cnt == 0)
			return;

		current = (current + 1) % cnt;

		var sprite = transform.GetComponent<UISprite> ();
		if (sprite != null) {
			sprite.spriteName = names [current];
			sprite.MakePixelPerfect ();
		}
	}
}
