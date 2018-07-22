

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : ListBase {

	bool inited = false;

	void Awake() {
		base.Awake ();

		var gm = GameMgr.GetInstance ();

		updateGems ();

		gm.eventUpCoins += updateGems;
	}

	void updateGems() {
		var gm = GameMgr.GetInstance ();

		setText(transform, "bottom/gems", "" + gm.get_gems());
	}

	void OnDestroy() {
		var gm = GameMgr.GetInstance ();

		gm.eventUpCoins -= updateGems;
	}

	public void enter() {
		if (!inited) {
			refresh ();
			inited = true;
		}

		show ();
	}

	void refresh() {
		GameMgr.list_goods_from_shop (ret => {
			Debug.Log("list_goods_from_shop ret");
			if (ret)
				showGoods();
		});
	}

	void showGoods() {
		var request = GameMgr.sListGoodsFromShop;

		Debug.Log ("showGoods");

		if (request == null)
			return;

		var goods = request.data;
		int cnt = goods.Count;

		List<string> products = new List<string> ();

		Debug.Log ("cnt=" + cnt);

		for (int i = 0; i < cnt; i++) {
			var good = goods[i];
			var item = getItem (i);

			setText(item, "title", good.quantity + "钻石");
			setText (item, "btn_buy/price", "¥ " + (good.price / 100));

			setBtnEvent(item, "btn_buy", () => {
				StoreMgr.pay(good);
			});

			products.Add (good.product);
		}

		updateItems (cnt);

		var ids = string.Join (",", products.ToArray ());

		StoreMgr.InitIAP (ids);
	}
}


