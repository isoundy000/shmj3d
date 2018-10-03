
using UnityEngine;
using System.Collections.Generic;

public class GameResult : MonoBehaviour {

	void OnEnable() {
		RoomMgr rm = RoomMgr.GetInstance();
		GameOverInfo info = rm.overinfo;
		List<GameEndInfo> endinfo = info.endinfo;

		int maxScore = -1;
		foreach (PlayerInfo p in rm.players) {
			if (p.score > maxScore)
				maxScore = p.score;
		}

		Transform seats = transform.Find("seats");
		UIGrid grid = seats.GetComponent<UIGrid>();
		int index = 0;

		for (int i = 0; i < rm.players.Count; i++, index++) {
			PlayerInfo p = rm.players[i];
			Transform seat = seats.GetChild (i);
			GameEndInfo ei = endinfo[i];
			bool bigwin = p.score > 0 && p.score == maxScore;

			seat.gameObject.SetActive (true);

			seat.Find ("bghead/icon").GetComponent<IconLoader> ().setUserID (p.userid);
			seat.Find("name").GetComponent<UILabel>().text = PUtils.subString(p.name, 5);
			seat.Find("id").GetComponent<UILabel>().text = "ID:" + p.userid;
			seat.Find("score").GetComponent<UILabel>().text = "" + p.score;
			seat.Find ("winner").gameObject.SetActive (bigwin);
			seat.Find ("owner").gameObject.SetActive (false);

			Transform stats = seat.Find ("stats");
			string[] statNames = new string[]{ "自摸次数", "接炮次数", "点炮次数" /* , "暗杠次数", "明杠次数" */ };
			for (int j = 0; j < statNames.Length; j++)
				stats.GetChild(j).GetComponent<UILabel>().text = statNames[j];

			Transform vals = seat.Find ("values");
			int[] statVals = new int[] { ei.numzimo, ei.numjiepao, ei.numdianpao /*, ei.numangang, ei.numminggang */ };
			for (int j = 0; j < statVals.Length; j++)
				vals.GetChild(j).GetComponent<UILabel>().text = "" + statVals[j];
		}

		for (int i = index; i < seats.childCount; i++)
			seats.GetChild(i).gameObject.SetActive(false);

		grid.Reposition();
	}

	public void onBtnShareClicked() {
		AnysdkMgr.GetInstance().shareImg(false, null);
	}

	public void onBtnShareXL() {
		AnysdkMgr.GetInstance ().shareImgXL();
	}

	public void onBtnBackClicked() {
		GameManager.GetInstance ().exit();
	}
}
