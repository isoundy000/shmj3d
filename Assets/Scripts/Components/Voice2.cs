

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJson;

public class Voice2 : MonoBehaviour {
	public GameObject voice;
	public GameObject time;
	public UILabel notice;
	public GameObject mic;
	public GameObject cancel;
	public GameObject warning;
	public GameObject volume;
	public GameObject button;
	public GameObject[] volumes;

	enum TOUCH_STATE {
		START,
		MOVE_OUT,
		MOVE_IN,
		CANCEL,
		END
	};

	float lastTouchTime = 0;
	float lastCheckTime = 0;
	long MAXTIME = 15000;

	bool draging = false;
	bool inButton = false;

	Camera cam;

	void Start () {
		cam = GameObject.Find("UI Root").GetComponentInChildren<Camera>();

		voice.SetActive(false);
	}

	void enter(TOUCH_STATE state) {
		VoiceMgr vm = VoiceMgr.GetInstance();

		Debug.Log("enter " + state);

		switch (state) {
		case TOUCH_STATE.START:
			{
				bool done = vm.prepare ("record.amr");
				if (!done) {
					lastTouchTime = 0;
					return;
				}

				lastTouchTime = Time.time;

				voice.SetActive (true);
				mic.SetActive (true);
				volume.SetActive (true);
				cancel.SetActive (false);
				warning.SetActive (false);

				time.transform.localScale = new Vector2 (0, 1);
				notice.text = "滑动手指，取消发送";
				break;
			}
		case TOUCH_STATE.MOVE_OUT:
			if (lastTouchTime > 0) {
				mic.SetActive (false);
				volume.SetActive (false);
				cancel.SetActive (true);
				warning.SetActive (false);

				notice.text = "松开手指，取消发送";
			}

			break;
		case TOUCH_STATE.MOVE_IN:
			if (lastTouchTime > 0) {
				mic.SetActive (true);
				volume.SetActive (true);
				cancel.SetActive (false);
				warning.SetActive (false);

				notice.text = "滑动手指，取消发送";
			}

			break;
		case TOUCH_STATE.CANCEL:
			if (lastTouchTime > 0) {
				vm.cancel();
				lastTouchTime = 0;
				voice.SetActive (false);
			}

			break;
		case TOUCH_STATE.END:
			if (lastTouchTime > 0) {
				if (Time.time - lastTouchTime < 1.0f) {
					vm.cancel ();

					voice.SetActive (true);
					mic.SetActive (false);
					volume.SetActive (false);
					cancel.SetActive (false);
					warning.SetActive (true);
					time.transform.localScale = new Vector2(0, 1);
					notice.text = "录制时间太短";

					PUtils.setTimeout (() => {
						voice.SetActive (false);
					}, 1.0f);
				} else {
					onVoiceOK ();
				}

				lastTouchTime = 0;
			}

			break;
		default:
			break;
		}
	}

	void onVoiceOK() {
		VoiceMgr vm = VoiceMgr.GetInstance();

		if (lastTouchTime > 0) {
			vm.release();

			long t = (long)((Time.time - lastTouchTime) * 1000);
			string msg = vm.getVoiceData("record.amr");

			if (msg != null && msg.Length > 0) {
				JsonObject ob = new JsonObject ();
				ob ["msg"] = msg;
				ob ["time"] = t;

				NetMgr.GetInstance ().send ("voice_msg", ob);
			}
		}

		voice.SetActive (false);
	}

	void Update () {
		VoiceMgr vm = VoiceMgr.GetInstance();
		float now = Time.time;

		if (Input.GetMouseButtonDown(0)) {
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit)) {
				GameObject ob = hit.collider.gameObject;

				if (ob.Equals (button)) {
					enter (TOUCH_STATE.START);
					inButton = true;
					draging = true;
				}
			}
		}

		if (draging) {
			bool found = false;
			Ray ray = cam.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit)) {
				GameObject ob = hit.collider.gameObject;

				if (ob.Equals (button))
					found = true;
			}

			if (Input.GetMouseButtonUp (0)) {
				enter (found ? TOUCH_STATE.END : TOUCH_STATE.CANCEL);

				draging = false;
			} else if (draging) {
				if (found != inButton) {
					enter (found ? TOUCH_STATE.MOVE_IN : TOUCH_STATE.MOVE_OUT);

					inButton = found;
				}
			}
		}

		if (voice.activeSelf && volume.activeSelf) {
			if (now - lastCheckTime > 0.3f) {
				int v = vm.getVoiceLevel (5);

				for (int i = 0; i < volumes.Length; i++) {
					volumes [i].SetActive (v > i);
				}

				lastCheckTime = now;
			}
		}

		if (lastTouchTime > 0) {
			long t = (long)((Time.time - lastTouchTime) * 1000);

			if (t >= MAXTIME) {
				onVoiceOK ();
				lastTouchTime = 0;
			} else {
				time.transform.localScale = new Vector2((float)t / MAXTIME, 1);
			}
		}
	}
}
