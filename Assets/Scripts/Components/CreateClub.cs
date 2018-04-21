
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;
using System.IO;

public class CreateClub : ListBase {

	public UIInput name;
	public UIInput desc;
	public UITexture icon;

	string pickPath = null;

	public void enter() {
		pickPath = null;
		reset();

		Transform items = transform.Find("items");
		items.GetComponent<UIScrollView> ().ResetPosition ();

		show();

		updateGems();
	}

	void reset() {
		name.value = "";
		desc.value = "";
	}

	public void onBtnIcon() {
		Debug.Log("onBtnIcon");

		AnysdkMgr.pick ((ret, path) => {
			if (0 != ret)
				return;

			pickPath = path;
			Debug.Log("after pick " + path);
			ImageLoader.GetInstance().LoadLocalImage(path, icon);
		});
	}

	public void onBtnCreate() {
		string _name = name.value;
		string _desc = desc.value;
		string msg = null;
		int price = 300;

		if (_name == "")
			msg = "俱乐部名字不能为空";
		else if (_desc == "")
			msg = "请填写俱乐部介绍";
		else if (GameMgr.GetInstance ().userMgr.gems < price)
			msg = "您的麻油不够";

		if (msg != null) {
			GameAlert.Show(msg);
			return;
		}

		GameAlert.Show ("创建俱乐部将立即扣除" + price + "麻油，要继续吗？", () => {

			JsonObject ob = new JsonObject ();
			ob ["name"] = _name;
			ob ["desc"] = _desc;

			if (pickPath != null) {
				byte[] bytes = File.ReadAllBytes (pickPath);
				string base64 = Convert.ToBase64String (bytes);
				ob ["logo"] = base64;
			}

			NetMgr nm = NetMgr.GetInstance ();

			nm.request_apis ("create_club", ob, data => {
				NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString ());
				if (ret.errcode != 0) {
					GameAlert.Show (ret.errmsg);
					return;
				}

				GameAlert.Show ("俱乐部创建成功！", () => {
					reset ();
					back ();
				});
			});
		}, true);
	}

	void updateGems() {
		var gm = GameMgr.GetInstance ();
		var gems = transform.Find("Bottom/gems").GetComponent<UILabel>();

		gm.get_coins (() => {
			gems.text = "" + gm.userMgr.gems;
		});
	}
}
