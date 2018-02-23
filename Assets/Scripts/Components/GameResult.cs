
using UnityEngine;
using System.Collections.Generic;

public class GameResult : MonoBehaviour {

	public void doGameResult() {
		RoomMgr rm = RoomMgr.GetInstance();
		GameOverInfo info = rm.overinfo;
		List<GameEndInfo> endinfo = info.endinfo;
		List<GameOverPlayerInfo> results = info.results;

		int maxScore = -1;
		foreach (GameOverPlayerInfo p in results) {
			if (p.totalscore > maxScore)
				maxScore = p.totalscore;
		}

		Transform seats = transform.Find("seats");
		int index = 0;

		for (int i = 0; i < results.Count; i++, index++) {
			GameOverPlayerInfo p = results[i];
			Transform seat = seats.GetChild (i);
			GameEndInfo ei = endinfo[i];
			bool bigwin = p.totalscore > 0 && p.totalscore == maxScore;

			seat.gameObject.SetActive (true);

			seat.Find ("bghead/icon").GetComponent<IconLoader> ().setUserID (p.userid);
			seat.Find("name").GetComponent<UILabel>().text = p.name;
			seat.Find("id").GetComponent<UILabel>().text = "ID:" + p.userid;
			seat.Find("score").GetComponent<UILabel>().text = "" + p.totalscore;
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
	}

	public void onBtnShareClicked() {
		// TODO
	}

	public void onBtnBackClicked() {
		GameMgr.GetInstance ().Reset();
		RoomMgr.GetInstance ().reset();
		LoadingScene.LoadNewScene ("02.lobby");
	}
}
