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
	List<UILabel> uGameCost = new List<UILabel> ();
	UIToggle uMaima = null;
	UIToggle uAllPairs = null;
	UIToggle uIP = null;
	UIToggle uLocation = null;

	int mClubID = 0;

	void Awake() {
		base.Awake ();

		Transform btnCreate = transform.Find ("Bottom/BtnCreate");
		btnCreate.GetComponent<UIButton> ().onClick.Add (new EventDelegate(this, "onBtnCreate"));

		Transform grid = transform.Find("items/grid_ign");

		uHuaFen = grid.Find ("huafen/score").GetComponent<UISlider>();
		uHuaFen.onChange.Add(new EventDelegate(this, "onScoreChanged"));
		uFlowers = grid.Find ("huafen/value").GetComponent<UILabel>();

		uGameNum.Add(grid.Find ("gamenum/gn4").GetComponent<UIToggle>());
		uGameNum.Add(grid.Find ("gamenum/gn8").GetComponent<UIToggle>());
		uGameNum.Add(grid.Find ("gamenum/gn16").GetComponent<UIToggle>());

		uGameCost.Add(grid.Find ("gamenum/gn4/title").GetComponent<UILabel>());
		uGameCost.Add(grid.Find ("gamenum/gn8/title").GetComponent<UILabel>());
		uGameCost.Add(grid.Find ("gamenum/gn16/title").GetComponent<UILabel>());

		uPlayerNum.Add(grid.Find ("playernum/pn4").GetComponent<UIToggle>());
		uPlayerNum.Add(grid.Find ("playernum/pn2").GetComponent<UIToggle>());
		uPlayerNum.Add(grid.Find ("playernum/pn3").GetComponent<UIToggle>());

		uLimits.Add(grid.Find("maxfan/limit2").GetComponent<UIToggle>());
		uLimits.Add(grid.Find("maxfan/limit3").GetComponent<UIToggle>());
		uLimits.Add(grid.Find("maxfan/limit4").GetComponent<UIToggle>());
		uLimits.Add(grid.Find("maxfan/limitno").GetComponent<UIToggle>());
		uMaima = grid.Find ("wanfa/horse").GetComponent<UIToggle> ();
		uAllPairs = grid.Find ("wanfa/allpairs").GetComponent<UIToggle> ();
		uIP = grid.Find ("limit/ip").GetComponent<UIToggle> ();
		uLocation = grid.Find ("limit/location").GetComponent<UIToggle> ();

		updateGems ();

		GameMgr.GetInstance ().eventUpCoins += updateGems;

		GameMgr.get_room_costs (ret => {
			if (ret)
				updateCosts();
		});

		setToggleEvent (grid, "playernum/pn4", val => {
			updateCosts();
		});

		setToggleEvent (grid, "playernum/pn2", val => {
			updateCosts();
		});

		setToggleEvent (grid, "playernum/pn3", val => {
			updateCosts();
		});
	}

	void OnDestroy() {
		GameMgr.GetInstance ().eventUpCoins -= updateGems;
	}

	void updateCosts() {
		int pn = getPlayerNum();
		string game = "shmj";

		var gc = GameMgr.sGetRoomCosts;
		if (gc == null)
			return;

		var costs = gc.data;
		int id = 0;

		for (int i = 0; i < costs.Count; i++) {
			var ct = costs[i];

			if (ct.game == game && ct.players_num == pn) {
				uGameCost[id].text = ct.round + "局(麻油x" + ct.gem + ")";
				id++;
			}
		}
	}

	void onScoreChanged() {
		int[] range = { 1, 5, 10, 20, 30, 50, 100, 200, 300 };
		int id = (int)(uHuaFen.value * (range.Length - 1) + 0.5);

		uFlowers.text = range[id].ToString();
		flowers = range[id];

		Debug.Log ("flowers=" + flowers);
	}

	int getPlayerNum() {
		int playernum = 4;
		int[] playernums = { 4, 2, 3 };

		for (int i = 0; i < uPlayerNum.Count; i++) {
			if (uPlayerNum [i].value) {
				playernum = playernums [i];
				break;
			}
		}

		return playernum;
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

		int playernum = getPlayerNum();

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
		bool limit_ip = uIP.value;
		bool limit_gps = uLocation.value;

		JsonObject conf = new JsonObject ();
		conf.Add ("type", "shmj");
		conf.Add ("gamenum", gamenum);
		conf.Add ("maxfan", maxfan);
		conf.Add ("huafen", flowers);
		conf.Add ("playernum", playernum);
		conf.Add ("maima", maima);
		conf.Add ("qidui", allpairs);
		conf.Add ("limit_ip", limit_ip);
		conf.Add ("limit_gps", limit_gps);

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

		var grid = transform.Find("items/grid_ign");
		var gd = grid.GetComponent<UIGrid>();
		var scroll = grid.GetComponentInParent<UIScrollView>();

		//gd.keepWithinPanel = true;
		//gd.repositionNow = true;

		grid.GetComponent<UIGrid> ().Reposition ();
		scroll.ResetPosition ();

		updateGems();
	}

	void updateGems() {
		var gm = GameMgr.GetInstance ();
		var gems = transform.Find("Bottom/gems").GetComponent<UILabel>();

		gems.text = "" + GameMgr.GetInstance().get_gems();
	}
}


