

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJson;

[Serializable]
public class GetUserInfo {
	public int errcode;
	public int id;
	public string auth_id;
	public string nickname;
	public int gem_coin;
}

[Serializable]
public class LoginInfo {
	public int errcode;
	public int id;
	public string auth_id;
	public int gem_coin;
	public int score;
	public int upper;
	public int level;
	public int all_gems;
	public int all_score;
	public int all_subs;

	public bool valid() {
		return errcode == 0;
	}
}

public class Dealer : ListBase {

	bool inited = false;

	UIInput userid;
	UILabel nick;
	UILabel diamonds;
	UIInput transfer_gems;

	void Awake() {
		base.Awake ();

		var gm = GameMgr.GetInstance ();

		gm.eventUpDealerCoins += updateGems;

		gm.dealerLogin ();
	}

	void updateGems() {
		var gm = GameMgr.GetInstance ();
		var login = gm.mLogin;

		setText(transform, "bottom/gems", "" + login.gem_coin);
		setText(transform, "bottom/score", "" + login.all_score);
	}

	void Start() {
		var pages = transform.Find ("body/pages");
		var player = pages.Find ("player");

		userid = player.Find ("inputUID").GetComponent<UIInput> ();
		nick = player.Find ("nickname").GetComponent<UILabel> ();
		diamonds = player.Find ("diamonds").GetComponent<UILabel> ();
		transfer_gems = player.Find ("inputGems").GetComponent<UIInput> ();
	}

	public void enter() {
		var gm = GameMgr.GetInstance ();

		getGoods ();

		show();
	}

	void OnDestroy() {
		var gm = GameMgr.GetInstance ();

		gm.eventUpDealerCoins -= updateGems;
	}

	void getGoods() {
		var http = Http.GetInstance ();

		http.Post ("/dealer/list_goods", null, text => {
			var ret = JsonUtility.FromJson<ListGoodsFromShop> (text);

			if (ret.errcode != 0) {
				Debug.Log("getGoods fail");
				return;
			}

			showGoods(ret.data);
		}, err => {
			Debug.Log("getGoods fail");
		});
	}

	void showGoods(List<GoodsInfo> goods) {
		int cnt = goods.Count;

		for (int i = 0; i < cnt; i++) {
			var good = goods[i];
			var item = getItem (i);

			setText(item, "title", good.quantity + "钻石");
			setText (item, "btn_buy/price", "¥ " + (good.price / 100));

			setBtnEvent(item, "btn_buy", () => {
				StoreMgr.pay(good);
			});
		}

		updateItems (cnt);
	}

	public void onBtnQuery() {
		var uid = userid.value;
		if (string.IsNullOrEmpty (uid))
			return;

		var http = Http.GetInstance ();
		var args = new JsonObject ();

		args.Add ("uid", uid);

		http.Post ("/dealer/get_user_info", args, text => {
			var info = JsonUtility.FromJson<GetUserInfo>(text);

			if (info.errcode != 0) {
				GameAlert.Show("获取玩家信息失败: " + info.errcode);
				return;
			}

			nick.text = info.nickname;
			diamonds.text = info.gem_coin + "颗";
		}, err => {
			GameAlert.Show("获取玩家信息失败: " + err);
		});
	}

	public void onBtnTransfer() {
		var uid = userid.value;
		var count = transfer_gems.value;
		if (string.IsNullOrEmpty (uid) || string.IsNullOrEmpty(count))
			return;

		var gm = GameMgr.GetInstance ();
		var login = gm.mLogin;
		if (login == null || login.errcode != 0)
			return;

		var http = Http.GetInstance ();
		var args = new JsonObject ();

		args.Add ("uid", uid);
		args.Add ("count", count);
		args.Add ("from", login.id);

		http.Post ("/dealer/transfer_gems2user", args, text => {
			var ret = JsonUtility.FromJson<NormalReturn>(text);

			if (ret.errcode != 0) {
				GameAlert.Show("赠钻失败: " + ret.errmsg);
				return;
			}
	
			gm.dealerLogin();
			onBtnQuery();
		}, err => {
			GameAlert.Show("赠钻失败: " + err);
		});
	}

	public void onBtnTransferRecords() {
		var uid = userid.value;
		if (string.IsNullOrEmpty (uid))
			return;

		var script = getPage<TransferList>("PTransferList");
		if (script != null)
			script.enter(Convert.ToInt32(uid));
	}
}


