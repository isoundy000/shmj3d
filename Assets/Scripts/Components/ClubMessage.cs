
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

[Serializable]
public class ClubMsg {
	public int id;
	public int user_id;
	public int club_id;
	public string name;
	public string logo;
	public string type;
	public string sign;
	public bool read;
	public int uptime;
}

[Serializable]
public class ListClubMsg {
	public int errcode;
	public string errmsg;
	public List<ClubMsg> data;
}

public class ClubMessage : ListBase {
	int mClubID = 0;

	void Awake() {
		base.Awake();

		GameMgr gm = GameMgr.GetInstance();

		gm.AddHandler ("club_message_notify", data => {
			ClubMessageNotify notify = (ClubMessageNotify)data;

			if (notify.club_id == mClubID)
				refresh();
		});
	}

	public void enter(int club_id) {
		mClubID = club_id;
		refresh();
		show();
	}

	void refresh() {
		NetMgr.GetInstance().request_apis ("list_club_message", "club_id", mClubID, data => {
			ListClubMsg ret = JsonUtility.FromJson<ListClubMsg> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("list_club_message fail");
				return;
			}

			showMessages(ret.data);
		});
	}

	void showMessages(List<ClubMsg> messages) {
		messages.Sort ((a, b) => {
			bool wait_a = a.type == "apply" && a.sign == "wait";
			bool wait_b = b.type == "apply" && b.sign == "wait";

			if (wait_a ==  wait_b)
				return b.uptime - a.uptime;

			if (wait_a && !wait_b)
				return -1;
			else
				return 1;
		});

		for (int i = 0; i < messages.Count; i++) {
			ClubMsg msg = messages[i];
			Transform item = getItem(i);

			setText (item, "name", msg.name);
			setText (item, "id", "" + msg.user_id);
			setText (item, "time", PUtils.formatTime (msg.uptime));
			setIcon (item, "bghead/icon", msg.logo);

			string type = msg.type;
			string sign = msg.sign;
			string status = "";

			Dictionary<string, string> msgs = new Dictionary<string, string> ();
			msgs["join"] = "申请加入俱乐部";
			msgs["leave"] = "离开了俱乐部";
			msgs["apply"] = "申请加入俱乐部";

			setText(item, "message", msgs[type]);

			setActive (item, "btn_approve", type == "apply" && sign == "wait");
			setActive (item, "btn_reject", type == "apply" && sign == "wait");
			setActive (item, "approved", type == "apply" || type == "join");

			if (sign == "approved")
				status = "已通过";
			else if (sign == "rejected")
				status = "已拒绝";

			setText (item, "approved", status);
			setIcon (item, "bghead/icon", msg.logo);

			setBtnEvent (item, "btn_approve", () => {
				Sign(msg.id, "approved");
			});

			setBtnEvent (item, "btn_reject", () => {
				Sign(msg.id, "rejected");
			});
		}

		updateItems(messages.Count);
	}

	void Sign(int id, string result) {
		JsonObject ob = new JsonObject();

		ob["id"] = id;
		ob["sign"] = result;
		ob["score"] = 0;
		ob["limit"] = 0;

		NetMgr.GetInstance().request_apis ("sign_club_message", ob, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("sign_club_message fail");
				return;
			}

			refresh();
		});
	}
}
