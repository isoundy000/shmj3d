
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJson;

public class UserPanel : MonoBehaviour {

	Transform info;
	UILabel nick;
	UILabel id;
	UILabel ip;
	UILabel addr;
	IconLoader icon;

	int userid;

	void Start() {
		info = transform.Find("UserPanel");

		nick = info.Find("nick").GetComponent<UILabel>();
		id = info.Find("id").GetComponent<UILabel>();
		ip = info.Find("ip").GetComponent<UILabel>();
		addr = info.Find("addr").GetComponent<UILabel>();
		icon = info.Find("bghead/icon").GetComponent<IconLoader>();

		Transform btnclose = info.Find("btn_close");
		PUtils.onClick (btnclose, () => {
			info.gameObject.SetActive(false);
		});

		Transform grid = info.Find ("emojis/grid");
		for (int i = 0; i < grid.childCount; i++) {
			Transform child = grid.GetChild (i);
			int j = i;
			PUtils.onClick (child, () => {
				JsonObject ob = new JsonObject();
				ob.Add("id", j);
				ob.Add("target", userid);
				NetMgr.GetInstance().send("demoji", ob);
				close();

			});
		}

		grid = info.Find ("semojis/grid");
		for (int i = 0; i < grid.childCount; i++) {
			Transform child = grid.GetChild (i);
			int j = i + 100;
			PUtils.onClick (child, () => {
				JsonObject ob = new JsonObject();
				ob.Add("id", j);
				ob.Add("target", userid);
				NetMgr.GetInstance().send("demoji", ob);
				close();
			});
		}
	}

	void close() {
		info.gameObject.SetActive (false);
	}

	public void show(int uid) {
		userid = uid;

		RoomMgr rm = RoomMgr.GetInstance();
		PlayerInfo p = rm.findPlayer(uid);

		if (p == null)
			return;

		info.gameObject.SetActive(true);

		nick.text = p.name;
		id.text = "" + uid;
		ip.text = p.ip.StartsWith("::ffff:") ? p.ip.Substring(7) : p.ip;
		addr.text = "";

		icon.setUserID(userid);

		bool myself = GameMgr.myself (uid);

		info.Find ("emojis").gameObject.SetActive (!myself);
		info.Find ("semojis").gameObject.SetActive (myself);
	}
}


