
using System;
using UnityEngine;
using System.Collections.Generic;

public class ClubList : ListBase {
	List<ClubInfo> mClubs = null;

	public void enter() {
		refresh();
		show();
	}

	void refresh() {
		NetMgr.GetInstance ().request_apis ("list_clubs", null, data => {
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
		ClubDetail cd = GameObject.Find ("PClubDetail").GetComponent<ClubDetail>();
		cd.UpdateEvents += refresh;
		cd.enter(club.id, club.is_admin);
	}
}


