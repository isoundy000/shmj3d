using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Message : ListBase {

	float nextUp = -1;

/*
	void Awake() {
		base.Awake();

		GameMgr gm = GameMgr.GetInstance();

		gm.AddHandler ("sys_message_updated", data => {
			if (this == null || !mShow) return;

			refresh();
		});
	}
*/

	public void enter() {
		refresh();
		show();
	}

	void refresh() {
		NetMgr.GetInstance().request_apis ("list_my_message", null, data => {
			if (this != null)
				nextUp = 0;

			ListClubMsg ret = JsonUtility.FromJson<ListClubMsg> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("list_my_message fail");
				return;
			}

			if (this != null)
				showMessages(ret.data);
		});
	}
/*
	void Update() {
		if (!mShow || !gameObject.activeInHierarchy || nextUp < 0)
			return;

		nextUp += Time.deltaTime;
		if (nextUp < 5)
			return;

		nextUp = -1;
		refresh();
	}
*/
	void showMessages(List<ClubMsg> messages) {
		messages.Sort ((a, b) => {
			return b.uptime - a.uptime;
		});

		for (int i = 0; i < messages.Count; i++) {
			ClubMsg msg = messages[i];
			Transform item = getItem(i);

			setText (item, "name", PUtils.subString(msg.name, 5));
			setText (item, "id", "" + msg.user_id);
			setText (item, "time", PUtils.formatTime (msg.uptime));
			setIcon (item, "bghead/icon", msg.logo);

			string type = msg.type;
			string sign = msg.sign;
			string status = "";

			Dictionary<string, string> msgs = new Dictionary<string, string> ();
			msgs["join"] = "申请加入俱乐部[" + msg.club_id + "]";
			msgs["leave"] = "离开了俱乐部[" + msg.club_id + "]";
			msgs["apply"] = "申请加入俱乐部[" + msg.club_id + "]";

			setActive(item, "new", !msg.read);
			setText(item, "message", msgs[type]);
			setActive (item, "approved", type == "apply" || type == "join");

			if (sign == "approved")
				status = "已通过";
			else if (sign == "rejected")
				status = "已拒绝";

			setText (item, "approved", status);
			setIcon (item, "bghead/icon", msg.logo);
		}

		updateItems(messages.Count);
	}
}
