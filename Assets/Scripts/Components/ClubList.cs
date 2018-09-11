
using System;
using UnityEngine;
using System.Collections.Generic;

public class ClubList : ListBase {
	List<ClubInfo> mClubs = null;

	float nextUp = -1;

	public void enter() {
		refresh();
		show();
	}

	void refresh() {
		NetMgr.GetInstance ().request_apis ("list_clubs", null, data => {
			if (this != null)
				nextUp = 0;

			ClubInfoList ret = JsonUtility.FromJson<ClubInfoList> (data.ToString ());
			if (ret.errcode != 0)
				return;

			if (this != null) {
				mClubs = ret.data;
				showClubs();
			}
		});
	}

	void Update() {
		if (!mShow || !gameObject.activeInHierarchy || nextUp < 0)
			return;

		nextUp += Time.deltaTime;
		if (nextUp < 5)
			return;

		nextUp = -1;
		refresh();
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
		var ob = getPage<ClubDetail>("PClubDetail");
		if (ob != null) {
			ob.UpdateEvents += () => {
				mShow = true;
				refresh ();
			};

			mShow = false;
			ob.enter (club.id, club.is_admin);
		}
	}
}


