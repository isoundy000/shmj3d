using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

	public GameObject msg_num = null;

	void Awake() {
		layout ();

		GameMgr gm = GameMgr.GetInstance();

		gm.AddHandler("club_message_notify", data => {
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

			if (ret.data != null)
				setCount(ret.data.cnt);
			else
				Debug.LogError("get_my_message_cnt failed");
		});
	}
}
