using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

	public GameObject msg_num = null;

	void Awake() {
		layout ();

		GameMgr gm = GameMgr.GetInstance();

		gm.AddHandler("club_message_notify", data => {
			if (this != null)
				refresh();
		});
	}

	void layout() {
		Transform bg = transform.Find("bg_portait");

		if (bg != null) {
			UISprite sp = bg.GetComponent<UISprite> ();
			Transform root = GameObject.Find("UI Root").transform;
			sp.topAnchor.Set(root, 1, 0);
			sp.bottomAnchor.Set(root, 0, 0);
		}
	}

	void Start() {
		refresh();

		UIToggle[] tabs = transform.Find ("Bottom/tabs").GetComponentsInChildren<UIToggle> ();
		var body = transform.Find ("Body");

		tabs [0].value = true;

		for (int i = 0; i < tabs.Length; i++) {
			int id = i;
			PUtils.setToggleEvent (tabs [i].transform, null, (val) => {
				if (val) {
					for (int j = 0; j < body.childCount; j++)
						body.GetChild (j).gameObject.SetActive (j == id);
				}
			});
		}
	}

	void setCount(int cnt) {
		msg_num.SetActive(cnt > 0);
		msg_num.transform.Find("tile").GetComponent<UILabel>().text = "" + cnt;
	}

	public void refresh() {
		NetMgr.GetInstance ().request_apis ("get_club_message_cnt", null, data => {
			GetClubMessageCnt ret = JsonUtility.FromJson<GetClubMessageCnt> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("get_club_message_cnt fail");
				return;
			}

			if (this != null && ret.data != null)
				setCount(ret.data.cnt);
		});
	}
}
