using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using System.IO;

/*
public class ShmjItems {
	public UISlider uHuaFen = null;
	public UILabel uFlowers = null;
	public int flowers = 1;
	public List<UIToggle> uGameNum = new List<UIToggle>();
	public List<UIToggle> uPlayerNum = new List<UIToggle>();
	public List<UIToggle> uLimits = new List<UIToggle>();
	public List<UILabel> uGameCost = new List<UILabel> ();
	public UIToggle uMaima = null;
	public UIToggle uAllPairs = null;
	public UIToggle uIP = null;
	public UIToggle uLocation = null;

	public bool active = false;

	public void Init(Transform parent) {
		var grid = parent.Find ("grid");

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
	}
}

public class GzmjItems {

}
*/

public class CreateRoom : ListBase {
	int mClubID = 0;

	public List<UIToggle> uGameNum = new List<UIToggle>();
	public List<UIToggle> uPlayerNum = new List<UIToggle>();
	public List<UILabel> uGameCost = new List<UILabel> ();
	public UIToggle uJYW = null;
	public UIToggle uJ7W = null;
	public UIToggle uRYJ = null;
	public UIToggle uIP = null;
	public UIToggle uLocation = null;

	void Awake() {
		base.Awake ();

		Transform btnCreate = transform.Find ("Bottom/BtnCreate");
		btnCreate.GetComponent<UIButton> ().onClick.Add (new EventDelegate (this, "onBtnCreate"));

		var grid = transform.Find ("items_gzmj/grid");

		uGameNum.Add (grid.Find ("gamenum/gn4").GetComponent<UIToggle> ());
		uGameNum.Add (grid.Find ("gamenum/gn8").GetComponent<UIToggle> ());
		uGameNum.Add (grid.Find ("gamenum/gn16").GetComponent<UIToggle> ());

		uGameCost.Add (grid.Find ("gamenum/gn4/title").GetComponent<UILabel> ());
		uGameCost.Add (grid.Find ("gamenum/gn8/title").GetComponent<UILabel> ());
		uGameCost.Add (grid.Find ("gamenum/gn16/title").GetComponent<UILabel> ());

		uPlayerNum.Add (grid.Find ("playernum/pn4").GetComponent<UIToggle> ());
		uPlayerNum.Add (grid.Find ("playernum/pn3").GetComponent<UIToggle> ());
		uPlayerNum.Add (grid.Find ("playernum/pn2").GetComponent<UIToggle> ());

		uJYW = grid.Find ("wanfa/jyw").GetComponent<UIToggle> ();
		uJ7W = grid.Find ("wanfa/j7w").GetComponent<UIToggle> ();
		uRYJ = grid.Find ("wanfa/ryj").GetComponent<UIToggle> ();
		uIP = grid.Find ("limit/ip").GetComponent<UIToggle> ();
		uLocation = grid.Find ("limit/location").GetComponent<UIToggle> ();
	}

	void Start() {
		var grid = transform.Find ("items_gzmj/grid");

		updateGems ();

		GameMgr.GetInstance ().eventUpCoins += updateGems;

		GameMgr.get_room_costs (ret => {
			if (ret)
				updateCosts();
		});
			
		PUtils.setToggleEvent (grid, "playernum/pn4", val => updateCosts ());
		PUtils.setToggleEvent (grid, "playernum/pn3", val => updateCosts ());
		PUtils.setToggleEvent (grid, "playernum/pn2", val => updateCosts ());

		PUtils.setToggleEvent (grid, "wanfa/jyw", null);
		PUtils.setToggleEvent (grid, "wanfa/j7w", null);
		PUtils.setToggleEvent (grid, "wanfa/ryj", null);
		PUtils.setToggleEvent (grid, "gamenum/gn4", null);
		PUtils.setToggleEvent (grid, "gamenum/gn8", null);
		PUtils.setToggleEvent (grid, "gamenum/gn16", null);
		PUtils.setToggleEvent (grid, "limit/ip", null);
		PUtils.setToggleEvent (grid, "limit/location", null);
	}

	void refresh() {
		uJYW.value = PlayerPrefs.HasKey ("jyw") ? (PlayerPrefs.GetInt ("jyw") > 0) : true;
		uJ7W.value = PlayerPrefs.HasKey ("j7w") ? (PlayerPrefs.GetInt ("j7w") > 0) : true;
		uRYJ.value = PlayerPrefs.HasKey ("ryj") ? (PlayerPrefs.GetInt ("ryj") > 0) : false;
		uIP.value = PlayerPrefs.HasKey ("limit_ip") ? (PlayerPrefs.GetInt ("limit_ip") > 0) : false;
		uLocation.value = PlayerPrefs.HasKey ("limit_gps") ? (PlayerPrefs.GetInt ("limit_gps") > 0) : false;

		if (PlayerPrefs.HasKey ("gamenum")) {
			int[] gns = new int[] { 4, 8, 16 };
			int gn = PlayerPrefs.GetInt ("gamenum");
			for (int i = 0; i < uGameNum.Count; i++)
				uGameNum [i].value = (gns [i] == gn);
		} else {
			uGameNum [0].value = true;
			uGameNum [1].value = false;
			uGameNum [2].value = false;
		}

		if (PlayerPrefs.HasKey ("playernum")) {
			int[] pns = new int[] { 4, 3, 2 };
			int pn = PlayerPrefs.GetInt ("playernum");
			for (int i = 0; i < uPlayerNum.Count; i++)
				uPlayerNum [i].value = (pns [i] == pn);
		} else {
			uPlayerNum [0].value = true;
			uPlayerNum [1].value = false;
			uPlayerNum [2].value = false;
		}
	}

	void OnDestroy() {
		GameMgr.GetInstance ().eventUpCoins -= updateGems;
	}

	void updateCosts() {
		int pn = getPlayerNum();
		string game = "gzmj";

		var gc = GameMgr.sGetRoomCosts;
		if (gc == null)
			return;

		var costs = gc.data;
		int id = 0;

		for (int i = 0; i < costs.Count; i++) {
			var ct = costs[i];

			if (ct.game == game && ct.players_num == pn) {
				uGameCost[id].text = ct.round + "局(钻石x" + ct.gem + ")";
				id++;
			}
		}
	}

	void onScoreChanged() {
/*
		int[] range = { 1, 5, 10, 20, 30, 50, 100, 200, 300 };
		int id = (int)(uHuaFen.value * (range.Length - 1) + 0.5);

		uFlowers.text = range[id].ToString();
		flowers = range[id];

		Debug.Log ("flowers=" + flowers);
*/
	}

	int getPlayerNum() {
		int playernum = 4;
		int[] playernums = { 4, 3, 2 };

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

/*
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
*/
		bool jyw = uJYW.value;
		bool j7w = uJ7W.value;
		bool ryj = uRYJ.value;

		bool limit_ip = uIP.value;
		bool limit_gps = uLocation.value;

		JsonObject conf = new JsonObject ();
		conf.Add ("type", "gzmj");
		conf.Add ("gamenum", gamenum);
		//conf.Add ("maxfan", maxfan);
		//conf.Add ("huafen", flowers);
		conf.Add ("playernum", playernum);
		//conf.Add ("maima", maima);
		//conf.Add ("qidui", allpairs);
		conf.Add ("jyw", jyw);
		conf.Add ("j7w", j7w);
		conf.Add ("ryj", ryj);
		conf.Add ("limit_ip", limit_ip);
		conf.Add ("limit_gps", limit_gps);

		PlayerPrefs.SetInt ("jyw", jyw ? 1 : 0);
		PlayerPrefs.SetInt ("j7w", j7w ? 1 : 0);
		PlayerPrefs.SetInt ("ryj", ryj ? 1 : 0);

		PlayerPrefs.SetInt ("playernum", playernum);
		PlayerPrefs.SetInt ("gamenum", gamenum);

		PlayerPrefs.SetInt ("limit_ip", limit_ip ? 1 : 0);
		PlayerPrefs.SetInt ("limit_gps", limit_gps ? 1: 0);

		if (mClubID > 0) {
			conf.Add("club_id", mClubID);
			createClubRoom(conf);
			return;
		}

		if (limit_gps) {
			var lm = LocationMgr.GetInstance ();
			if (!lm.Get ().valid ()) {
				string content = "您的定位信息无效，无法创建限制距离的房间";

				GameAlert.Show(content);
				return;
			}
		}

		GameMgr game = GameMgr.GetInstance ();

		game.createRoom (conf, ret => {
			int errcode = Convert.ToInt32 (ret ["errcode"]);
			if (errcode != 0) {
				string msg = "创建房间失败，错误码" + errcode;
				if (errcode == 2222)
					msg = "钻石不足";

				GameAlert.Show(msg);

				return;
			}
			
			JsonObject data = (JsonObject)ret ["data"];
			string id = (string)data ["roomid"];

			game.enterRoom (id, code => {
				if (code != 0) {
					string content = "加入房间失败[" + code + "]";

					if (code == 2224)
						content = "房间已满！";
					else if (code == 2222)
						content = "房主钻石不足";
					else if (code == 2231)
						content = "您的IP和其他玩家相同";
					else if (code == 2232)
						content = "您的位置和其他玩家太近";
					else if (code == 2233)
						content = "您的定位信息无效，请检查是否开启定位";
					else if (code == 2251)
						content = "您不是俱乐部成员，无法加入俱乐部房间";
					else if (code == 2225)
						content = "房间不存在";

					GameAlert.Show(content);
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

			if (this != null)
				back();
		});
	}

	public void enter(int club_id = 0) {
		mClubID = club_id;
		refresh();
		show();

		var grid = transform.Find("items_gzmj/grid");
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


