
using UnityEngine;
using System.Collections.Generic;

public class GameResult : MonoBehaviour {

	public void doGameResult() {
		RoomMgr rm = RoomMgr.GetInstance ();
		List<GameEndInfo> endinfo = rm.overinfo.endinfo;
		List<PlayerInfo> players = rm.players;

		int maxScore = -1;
		foreach (PlayerInfo p in players) {
			if (p.score > maxScore)
				maxScore = p.score;
		}

		Transform seats = transform.FindChild("seats");
		int index = 0;

		for (int i = 0; i < players.Count; i++, index++) {
			PlayerInfo p = players[i];
			Transform seat = seats.GetChild (i);
			GameEndInfo ei = endinfo[i];
			bool bigwin = p.score > 0 && p.score == maxScore;

			seat.gameObject.SetActive (true);

			seat.FindChild ("icon").GetComponent<IconLoader> ().setUserID (p.userid);
			seat.FindChild("name").GetComponent<UILabel>().text = p.name;
			seat.FindChild("id").GetComponent<UILabel>().text = "ID:" + p.userid;
			seat.FindChild("score").GetComponent<UILabel>().text = "" + p.score;
			seat.FindChild ("winner").gameObject.SetActive (bigwin);
			seat.FindChild ("owner").gameObject.SetActive (p.userid == rm.conf.creator);

			Transform stats = seat.FindChild ("stats");
			string[] statNames = new string[]{ "自摸次数", "接炮次数", "点炮次数", "暗杠次数", "明杠次数" };
			for (int j = 0; j < statNames.Length; j++)
				stats.GetChild(j).GetComponent<UILabel>().text = statNames[j];

			Transform vals = seat.FindChild ("values");
			int[] statVals = new int[] { ei.numzimo, ei.numjiepao, ei.numdianpao, ei.numangang, ei.numminggang };
			for (int j = 0; j < statVals.Length; j++)
				stats.GetChild(j).GetComponent<UILabel>().text = "" + statVals[j];
		}

		for (int i = index; i < seats.childCount; i++)
			seats.GetChild(i).gameObject.SetActive(false);
	}

	public void onBtnShareClicked() {
		// TODO
	}

	public void onBtnBackClicked() {
		GameMgr.GetInstance ().Reset();

		LoadingScene.LoadNewScene ("02.lobby");
	}
}
