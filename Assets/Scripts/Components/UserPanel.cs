
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

	void Awake() {
		info = transform.Find("UserPanel");

		nick = info.Find("nick").GetComponent<UILabel>();
		id = info.Find("id").GetComponent<UILabel>();
		ip = info.Find("ip").GetComponent<UILabel>();
		addr = info.Find("addr").GetComponent<UILabel>();
		icon = info.Find("bghead/icon").GetComponent<IconLoader>();

		Transform btnclose = info.Find("btn_close");
		Utils.onClick (btnclose, () => {
			info.gameObject.SetActive(false);
		});

		Transform grid = info.Find ("emojis/Grid");
		for (int i = 0; i < grid.childCount; i++) {
			Transform child = grid.GetChild (i);
			int j = i;
			Utils.onClick (child, () => {
				JsonObject ob = new JsonObject();
				ob.Add("id", j);
				ob.Add("target", userid);
				NetMgr.GetInstance().send("demoji", ob);
			});
		}
	}

	public void show(int uid) {
		userid = uid;

		info.gameObject.SetActive(true);

		RoomMgr rm = RoomMgr.GetInstance();
		PlayerInfo p = rm.findPlayer(uid);

		nick.text = p.name;
		id.text = "" + uid;
		ip.text = p.ip.StartsWith("::ffff:") ? p.ip.Substring(7) : p.ip;
		addr.text = "";

		icon.setUserID(userid);
	}
}


