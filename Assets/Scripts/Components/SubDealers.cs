
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

[Serializable]
public class ListSubDealers {
	public int errcode;
	public string errmsg;
	public List<SubDealer> data;
}

[Serializable]
public class SubDealer {
	public string name;
	public int recharge;
	public int rebate;
};

public class SubDealers : ListBase {

	void OnEnable() {
		refresh ();
	}

	void refresh() {
		var http = Http.GetInstance ();

		http.Post ("/dealer/list_sub_dealers", null, text => {
			var ret = JsonUtility.FromJson<ListSubDealers> (text);

			if (ret.errcode != 0) {
				Debug.Log("ListSubDealers fail");
				return;
			}

			showDealers(ret.data);
		}, err => {
			Debug.Log("ListSubDealers fail");
		});
	}

	void showDealers(List<SubDealer> dealers) {
		for (int i = 0; i < dealers.Count; i++) {
			var dealer = dealers[i];
			var item = getItem(i);

			PUtils.setText (item, "name", dealer.name);
			PUtils.setText (item, "recharge", "" + dealer.recharge);
			PUtils.setText (item, "rebate", "" + dealer.rebate);
		}

		updateItems(dealers.Count);
	}
}

