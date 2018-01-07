
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

public class Rank : ListBase {
	int mClubID = 0;

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
			setIcon(item, "icon", mb.logo);
		}

		updateItems(members.Count);
	}
}


