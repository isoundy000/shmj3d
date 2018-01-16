using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleAnim : MonoBehaviour {

	public bool clockwise = true;
	public float step = 10.0f;
	public float rate = 0.08f;

	float nextFire = 0;
	UISprite target = null;

	float hf = 0;
	float wf = 0;
	int side = 0;

	void Awake() {
		target = transform.parent.GetComponent<UISprite>();
		int width = target.width;
		int height = target.height;

		hf = (float)height / 2;
		wf = (float)width / 2;

		transform.localPosition = new Vector3(clockwise ? 0 - wf : wf, hf, 0);
	}

	void move() {
		Vector3 pos = transform.localPosition;

		float x = pos.x;
		float y = pos.y;

		// 上边
		if (side == 0) {
			x += step;
			if (x > wf) {
				x = wf;
				y = hf - 1;
				side = 1;
			}
		} else if (side == 1) { // 右边
			y -= step;
			if (y < 0 - hf) {
				y = 0 - hf;
				x = wf - 1;
				side = 2;
			}
		} else if (side == 2) { // 下边
			x -= step;
			if (x < 0 - wf) {
				x = 0 - wf;
				y = 1 - hf;
				side = 3;
			}
		} else if (side == 3) {
			y += step;
			if (y > hf) {
				y = hf;
				x = 1 - wf;
				side = 0;
			}
		}

		pos.x = x;
		pos.y = y;
		transform.localPosition = pos;

		float rotation = side % 2 == 0 ? 0 : 90;
		transform.localRotation = Quaternion.Euler(0, 0, rotation);
	}

	void move_reverse() {
		Vector3 pos = transform.localPosition;

		float x = pos.x;
		float y = pos.y;

		// 上边
		if (side == 0) {
			x -= step;
			if (x < 0 - wf) {
				x = 0 - wf;
				y = hf - 1;
				side = 1;
			}
		} else if (side == 1) { // 左边
			y -= step;
			if (y < 0 - hf) {
				y = 0 - hf;
				x = 1 - wf;
				side = 2;
			}
		} else if (side == 2) { // 下边
			x += step;
			if (x > wf) {
				x = wf;
				y = 1 - hf;
				side = 3;
			}
		} else if (side == 3) {
			y += step;
			if (y > hf) {
				y = hf;
				x = wf;
				side = 0;
			}
		}

		pos.x = x;
		pos.y = y;
		transform.localPosition = pos;

		float rotation = side % 2 == 0 ? 0 : 90;
		transform.localRotation = Quaternion.Euler(0, 0, rotation);
	}

	void Update () {
		if (Time.time < nextFire)
			return;

		nextFire = Time.time + rate;
		if (clockwise)
			move ();
		else
			move_reverse ();
	}
}
