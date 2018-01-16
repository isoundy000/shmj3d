
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;
using System.IO;

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
	public UITexture icon;

	int mClubID = 0;
	string pickPath = null;

	public void enter(int club_id) {
		mClubID = club_id;
		pickPath = null;
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
		Transform body = transform.Find("Body");

		setInput(body, "name/input", club.name);
		setInput(body, "desc/input", club.desc);

		UIToggle auto_start = body.Find("params/auto_start").GetComponent<UIToggle>();
		auto_start.value = club.auto_start;

		setIcon(body, "logo/bghead/icon", club.logo);
	}

	public void onBtnIcon() {
		Debug.Log("onBtnIcon");

		AnysdkMgr.pick ((ret, path) => {
			if (0 != ret)
				return;

			pickPath = path;
			Debug.Log("after pick " + path);
			ImageLoader.GetInstance().LoadLocalImage(path, icon);
		});
	}

	public void onBtnSave() {
		Transform body = transform.Find("Body");
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

		bool auto_start = body.Find("params/auto_start").GetComponent<UIToggle>().value;

		JsonObject ob = new JsonObject();
		ob["id"] = mClubID;
		ob["name"] = _name;
		ob["desc"] = _desc;
		ob["auto_start"] = auto_start;

		if (pickPath != null) {
			byte[] bytes = File.ReadAllBytes (pickPath);
			string base64 = Convert.ToBase64String (bytes);
			ob["logo"] = base64;
		}

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


