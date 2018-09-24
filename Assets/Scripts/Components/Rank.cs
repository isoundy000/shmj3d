
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

public class Rank : ListBase {
	int mClubID = 0;

	public void enter(int club_id) {
		mClubID = club_id;

		resetNavigator();

		refresh();
		show();
	}

	void refresh() {
		NetMgr nm = NetMgr.GetInstance ();

		JsonObject ob = new JsonObject();
		ob ["club_id"] = mClubID;
		ob ["limit"] = mNumsPerPage;
		ob ["offset"] = mPage * mNumsPerPage;

		nm.request_apis ("list_club_members_paging", ob, data => {
			ListClubMembers ret = JsonUtility.FromJson<ListClubMembers> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("list club members fail");
				return;
			}

			if (this != null) {
				showMembers(ret.data.members);
				mTotal = ret.data.count;
				updateNavigator(refresh);
			}
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


