
using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class ClubInfo {
	public int id;
	public string name;
	public string desc;
	public string logo;
	public int member_num;
	public int max_member_num;
	public bool is_admin;
}

[Serializable]
public class ClubInfoList {
	public int errcode;
	public string errmsg;
	public List<ClubInfo> data;
}

public class Clubs : ListBase {
	List<ClubInfo> mClubs = null;
	GameObject mPopup = null;

	void Awake() {
		base.Awake();

		mPopup = transform.Find("popup").gameObject;
	}

	void OnEnable() {
		refresh ();
	}

	void refresh() {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis ("list_clubs", null, data => {
			ClubInfoList ret = JsonUtility.FromJson<ClubInfoList> (data.ToString ());
			if (ret.errcode != 0)
				return;

			if (this != null) {
				mClubs = ret.data;
				showClubs();
			}
		});
	}

	void showClubs() {
		for (int i = 0; i < mClubs.Count; i++) {
			ClubInfo club = mClubs[i];
			Transform item = getItem(i);

			setText(item, "name", club.name);
			setText(item, "desc", club.desc);
			setText(item, "id", "ID:" + club.id);
			setActive(item, "admin", club.is_admin);
			setText(item, "hc", club.member_num + " / " + club.max_member_num);
			setIcon(item, "bghead/icon", club.logo);

			UIButton btn = item.GetComponent<UIButton>();
			Color cl = club.is_admin ? new Color (0.26f, 0.26f, 0.26f) : new Color (0.06f, 0.06f, 0.06f);
			item.GetComponent<UISprite> ().color = cl;
			btn.hover = cl;
			btn.pressed = cl;

			PUtils.onClick (item, () => {
				onBtnClub(club);
			});
		}

		updateItems(mClubs.Count);
	}

	void onBtnClub(ClubInfo club) {
		bool admin = club.is_admin;

		Debug.Log ("onBtnClub");

		if (admin) {
			var ob = getPage<LuaListBase>("PAdmin");
			if (ob != null)
				ob.enter(club.id);
		} else {
			var ob = getPage<Hall>("PHall");
			if (ob != null)
				ob.enter(club.id);
		}
	}

	public void onBtnAdd() {
		Debug.Log ("onBtnAdd: " + mPopup.activeSelf);

		mPopup.SetActive(!mPopup.activeSelf);
	}

	public void onBtnMask() {
		mPopup.SetActive(false);
	}

	public void onBtnJoin() {
		GameObject root = GameObject.Find("UI Root");
		GameObject ob = root.transform.Find ("PJoinClub").gameObject;

		ob.SetActive (true);
		mPopup.SetActive(false);
	}

	public void onBtnCreate() {
		var ob = getPage<CreateClub>("PCreateClub");
		if (ob != null) {
			ob.UpdateEvents += refresh;
			ob.enter ();
		}

		mPopup.SetActive(false);
	}
}
