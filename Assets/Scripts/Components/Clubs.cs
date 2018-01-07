
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
	public GameObject mJoin = null;

	void Awake() {
		base.Awake();

		mPopup = transform.FindChild("popup").gameObject;
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

			mClubs = ret.data;
			showClubs();
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

			UIButton btn = item.GetComponent<UIButton>();
			Color cl = club.is_admin ? new Color (0.26f, 0.26f, 0.26f) : new Color (0.06f, 0.06f, 0.06f);
			item.GetComponent<UISprite> ().color = cl;
			btn.hover = cl;
			btn.pressed = cl;

			Utils.onClick (item, () => {
				onBtnClub(club);
			});
		}

		updateItems(mClubs.Count);
	}

	void onBtnClub(ClubInfo club) {
		bool admin = club.is_admin;

		if (admin) {
			GameObject padmin = GameObject.Find ("PAdmin");
			padmin.GetComponent<Admin> ().enter(club);
		} else {
			GameObject hall = GameObject.Find ("PHall");
			hall.GetComponent<Hall>().enter(club);
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
		mJoin.SetActive (true);
		mPopup.SetActive(false);
	}

	public void onBtnCreate() {
		CreateClub create = GameObject.Find ("PCreateClub").GetComponent<CreateClub>();

		create.UpdateEvents += refresh;
		create.enter();

		mPopup.SetActive(false);
	}
}
