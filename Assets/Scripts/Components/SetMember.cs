
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

[Serializable]
public class ClubMember {
	public int id;
	public string name;
	public string logo;
	public int score;
	public int limit;
	public bool admin;
};

[Serializable]
public class ListClubMembers {
	public int errcode;
	public string errmsg;
	public List<ClubMember> data;
}

public class SetMember : ListBase {
	int mClubID = 0;
	ClubMember mEditMember = null;

	public Transform mEditor = null;

	public void enter(int club_id) {
		mClubID = club_id;
		refresh();
		show();
	}

	void refresh() {
		NetMgr nm = NetMgr.GetInstance ();

		nm.request_apis ("list_club_members", "club_id", mClubID, data => {
			ListClubMembers ret = JsonUtility.FromJson<ListClubMembers> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("list club members fail");
				return;
			}

			if (this != null)
				showMembers(ret.data);
		});
	}

	void showMembers(List<ClubMember> members) {
		for (int i = 0; i < members.Count; i++) {
			Transform item = getItem(i);
			ClubMember mb = members[i];

			setText(item, "name", mb.name);
			setText(item, "id", "" + mb.id);
			setText(item, "score", "" + mb.score);
			setText(item, "limit", "" + mb.limit);
			setIcon(item, "icon", mb.logo);

			setBtnEvent(item, "btn_edit", () => {
				mEditMember = mb;
				onBtnEdit();
			});

			setBtnEvent(item, "btn_history", () => {
				var ob = getPage<ClubHistory>("PClubHistory");
				if (ob != null)
					ob.enter(mClubID, mb.id);
			});
		}

		updateItems(members.Count);
	}

	void onBtnEdit() {
		setInput(mEditor, "ip_score", "" + mEditMember.score);
		setInput(mEditor, "ip_limit", "" + (0 - mEditMember.limit));
		setText(mEditor, "btn_admin/Label", mEditMember.admin ? "取消管理员" : "设为管理员");

		mEditor.gameObject.SetActive(true);
	}

	public void onBtnClose() {
		mEditor.gameObject.SetActive(false);
	}

	public void onBtnDel() {
		JsonObject ob = new JsonObject ();
		ob["club_id"] = mClubID;
		ob["user_id"] = mEditMember.id;

		NetMgr.GetInstance ().request_apis ("leave_club", ob, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("leave_club fail");
				return;
			}

			if (this != null) {
				mEditor.gameObject.SetActive(false);
				refresh();
			}
		});
	}

	public void onBtnAdmin() {
		bool admin = !mEditMember.admin;

		JsonObject ob = new JsonObject ();
		ob["club_id"] = mClubID;
		ob["user_id"] = mEditMember.id;
		ob["admin"] = admin;

		NetMgr.GetInstance ().request_apis ("prompt_club_member", ob, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("prompt_club_member fail");
				return;
			}

			if (this != null) {
				mEditor.gameObject.SetActive(false);
				refresh();

				GameAlert.Show((admin ? "已设置" : "已取消") + mEditMember.name + "管理员权限");
			}
		});
	}

	public void onBtnCScore() {
		setInput(mEditor, "ip_score", "0");
	}

	public void onBtnCLimit() {
		setInput(mEditor, "ip_limit", "0");
	}

	public void onBtnOK() {
		int score = int.Parse(getInput(mEditor, "ip_score"));
		int limit = int.Parse(getInput(mEditor, "ip_limit"));
		if (limit < 0) {
			GameAlert.Show("活力值必须小于或者等于0");
			return;
		}

		limit = 0 - limit;

		JsonObject ob = new JsonObject ();
		ob["club_id"] = mClubID;
		ob["user_id"] = mEditMember.id;
		ob["score"] = score;
		ob["limit"] = limit;

		NetMgr.GetInstance ().request_apis ("setup_club_member", ob, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log ("setup_club_member fail");
				return;
			}

			if (this != null) {
				mEditor.gameObject.SetActive(false);
				refresh();
			}
		});
	}
}


