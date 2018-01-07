
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

[Serializable]
public class ClubOwner {
	public int id;
	public string name;
	public string logo;
}

[Serializable]
public class ClubDetailInfo {
	public int id;
	public string name;
	public string desc;
	public string logo;
	public int member_num;
	public int max_member_num;
	public int create_time;
	public bool auto_start;
	public ClubOwner owner;
}

[Serializable]
public class GetClubDetail {
	public int errcode;
	public string errmsg;
	public ClubDetailInfo data;
}

public class SetClub : ListBase {
	public UIInput name;
	public UIInput desc;

	int mClubID = 0;

	public void enter(int club_id) {
		mClubID = club_id;
		refresh();
		show();
	}

	void refresh() {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis ("get_club_detail", "club_id", mClubID, data => {
			GetClubDetail ret = JsonUtility.FromJson<GetClubDetail> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("get_club_detail fail");
				return;
			}

			showClub(ret.data);
		});
	}

	void showClub(ClubDetailInfo club) {
		Transform body = transform.FindChild("Body");

		setInput(body, "name/input", club.name);
		setInput(body, "desc/input", club.desc);

		UIToggle auto_start = body.FindChild("params/auto_start").GetComponent<UIToggle>();
		auto_start.value = club.auto_start;

		setIcon(body, "logo/bghead/icon", club.logo);
	}

	public void onBtnIcon() {

	}

	public void onBtnSave() {
		Transform body = transform.FindChild("Body");
		string _name = getInput(body, "name/input");
		string _desc = getInput(body, "desc/input");

		string msg = null;

		if (_name == "")
			msg = "俱乐部名字不能为空";
		else if (_desc == "")
			msg = "请填写俱乐部介绍";

		if (msg != null) {
			GameAlert.Show(msg);
			return;
		}

		bool auto_start = body.FindChild("params/auto_start").GetComponent<UIToggle>().value;

		JsonObject ob = new JsonObject();
		ob["id"] = mClubID;
		ob["name"] = _name;
		ob["desc"] = _desc;
		ob["auto_start"] = auto_start;

		// TODO: set logo

		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis("set_club", ob, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString());
			if (ret.errcode != 0) {
				Debug.Log("set_club fail: " + ret.errmsg);
				return;
			}

			GameAlert.Show("俱乐部设置成功!", ()=>{
				back();
			});
		});
	}
}


