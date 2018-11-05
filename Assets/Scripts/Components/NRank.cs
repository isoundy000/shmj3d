
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

[Serializable]
public class RankData {
	public int id;
	public int score;
	public string name;
	public string logo;
}

[Serializable]
public class GetRank {
	public int errcode;
	public string errmsg;
	public List<RankData> data;
}

public class NRank : ListBase {

	UIToggle uPlayer;
	UIToggle uClub;
	UIToggle uClubDay;

	void Awake() {
		base.Awake ();

		var bottom = transform.Find ("bottom");

		uPlayer = bottom.Find ("BtnPlayer").GetComponent<UIToggle>();
		uClub = bottom.Find ("BtnClub").GetComponent<UIToggle>();
		uClubDay = bottom.Find ("BtnClubDay").GetComponent<UIToggle>();
	}

	public void enter() {
		Debug.Log ("enter NRank");

		var type = uPlayer.value ? "player" : "club";
		var period = uClubDay.value ? "day" : "month";

		getRank (type, period);

		show();
	}

	void getRank(string type, string period) {
		NetMgr nm = NetMgr.GetInstance ();

		JsonObject ob = new JsonObject();
		ob ["type"] = type;
		ob ["period"] = period;

		nm.request_apis ("get_rank", ob, data => {
			GetRank ret = JsonUtility.FromJson<GetRank> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("GetRank fail");
				return;
			}

			if (this != null)
				showRank(ret.data);
		});
	}

	void showRank(List<RankData> ranks) {
		for (int i = 0; i < ranks.Count; i++) {
			Transform item = getItem(i);
			var rk = ranks[i];

			setText(item, "name", rk.name);
			setText(item, "id", "" + rk.id);
			setText(item, "score", "" + rk.score);
			setIcon(item, "icon", rk.logo);
			setText (item, "rank", "" + (i + 1));
		}

		updateItems(ranks.Count);
	}

	public void onBtnSel() {
		if (!valid ())
			return;

		if (!UIToggle.current.value)
			return;

		var type = uPlayer.value ? "player" : "club";
		var period = uClubDay.value ? "day" : "month";

		getRank (type, period);
	}
}


