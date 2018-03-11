using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using System.IO;

public class CreateRoom : ListBase {

	UISlider uHuaFen = null;
	UILabel uFlowers = null;
	int flowers = 1;
	List<UIToggle> uGameNum = new List<UIToggle>();
	List<UIToggle> uPlayerNum = new List<UIToggle>();
	List<UIToggle> uLimits = new List<UIToggle>();
	UIToggle uMaima = null;
	UIToggle uAllPairs = null;
	UIToggle uIP = null;
	UIToggle uLocation = null;

	int mClubID = 0;

	void Awake() {
		base.Awake ();

		Transform btnCreate = transform.Find ("Bottom/BtnCreate");
		btnCreate.GetComponent<UIButton> ().onClick.Add (new EventDelegate(this, "onBtnCreate"));

		uHuaFen = transform.Find ("Body/huafen/score").GetComponent<UISlider>();
		uHuaFen.onChange.Add(new EventDelegate(this, "onScoreChanged"));
		uFlowers = transform.Find ("Body/huafen/value").GetComponent<UILabel>();

		uGameNum.Add(transform.Find ("Body/gamenum/gn4").GetComponent<UIToggle>());
		uGameNum.Add(transform.Find ("Body/gamenum/gn8").GetComponent<UIToggle>());
		uGameNum.Add(transform.Find ("Body/gamenum/gn16").GetComponent<UIToggle>());

		uPlayerNum.Add(transform.Find ("Body/playernum/pn4").GetComponent<UIToggle>());
		uPlayerNum.Add(transform.Find ("Body/playernum/pn2").GetComponent<UIToggle>());
		uPlayerNum.Add(transform.Find ("Body/playernum/pn3").GetComponent<UIToggle>());

		uLimits.Add(transform.Find("Body/maxfan/limit2").GetComponent<UIToggle>());
		uLimits.Add(transform.Find("Body/maxfan/limit3").GetComponent<UIToggle>());
		uLimits.Add(transform.Find("Body/maxfan/limit4").GetComponent<UIToggle>());
		uLimits.Add(transform.Find("Body/maxfan/limitno").GetComponent<UIToggle>());
		uMaima = transform.Find ("Body/wanfa/horse").GetComponent<UIToggle> ();
		uAllPairs = transform.Find ("Body/wanfa/allpairs").GetComponent<UIToggle> ();
		uIP = transform.Find ("Body/limit/ip").GetComponent<UIToggle> ();
		uLocation = transform.Find ("Body/limit/location").GetComponent<UIToggle> ();
	}

	void onScoreChanged() {
		int[] range = { 1, 5, 10, 20, 30, 50, 100, 200, 300 };
		int id = (int)(uHuaFen.value * (range.Length - 1) + 0.5);

		uFlowers.text = range[id].ToString();
		flowers = range[id];

		Debug.Log ("flowers=" + flowers);
	}

	void onBtnCreate() {
		int gamenum = 0;
		int[] gamenums = { 4, 8, 16 };

		for (int i = 0; i < uGameNum.Count; i++) {
			if (uGameNum [i].value) {
				gamenum = gamenums [i];
				break;
			}
		}

		int playernum = 4;
		int[] playernums = { 4, 2, 3 };

		for (int i = 0; i < uPlayerNum.Count; i++) {
			if (uPlayerNum [i].value) {
				playernum = playernums [i];
				break;
			}
		}

		int maxfan = 0;
		int[] maxfans = { 2, 3, 4, 100 };
		for (int i = 0; i < uLimits.Count; i++) {
			if (uLimits [i].value) {
				maxfan = maxfans[i];
				break;
			}
		}

		bool maima = uMaima.value;
		bool allpairs = uAllPairs.value;

		JsonObject conf = new JsonObject ();
		conf.Add ("type", "shmj");
		conf.Add ("gamenum", gamenum);
		conf.Add ("maxfan", maxfan);
		conf.Add ("huafen", flowers);
		conf.Add ("playernum", playernum);
		conf.Add ("maima", maima);
		conf.Add ("qidui", allpairs);

		if (mClubID > 0) {
			conf.Add("club_id", mClubID);
			createClubRoom(conf);
			return;
		}

		GameMgr game = GameMgr.GetInstance ();

		game.createRoom (conf, ret => {
			int code = Convert.ToInt32 (ret ["errcode"]);
			if (code != 0)
				return;
			
			JsonObject data = (JsonObject)ret ["data"];
			string roomid = (string)data ["roomid"];

			game.enterRoom (roomid, errcode => {
				if (errcode != 0) {
					
				} else {
					Debug.Log("enter room success");
				}
			});
		});
	}

	void createClubRoom(JsonObject conf) {
		NetMgr nm = NetMgr.GetInstance();

		JsonObject ob = new JsonObject();
		ob ["conf"] = conf;

		nm.request_connector ("create_private_room", ob, data => {
			NormalReturn ret = 	JsonUtility.FromJson<NormalReturn> (data.ToString());
			if (ret.errcode != 0) {
				GameAlert.Show(ret.errmsg);
				return;
			}

			back();
		});
	}

	public void enter(int club_id = 0) {
		mClubID = club_id;
		show();
	}
}


