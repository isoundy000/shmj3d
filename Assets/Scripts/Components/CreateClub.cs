
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

public class CreateClub : ListBase {

	public UIInput name;
	public UIInput desc;

	void OnEnable() {
		reset();
	}

	void reset() {
		name.value = "";
		desc.value = "";
	}

	public void onBtnIcon() {

	}

	public void onBtnCreate() {
		string _name = name.value;
		string _desc = desc.value;

		string msg = null;

		if (_name == "")
			msg = "俱乐部名字不能为空";
		else if (_desc == "")
			msg = "请填写俱乐部介绍";

		if (msg != null) {
			GameAlert.Show(msg);
			return;
		}

		JsonObject ob = new JsonObject ();
		ob ["name"] = _name;
		ob ["desc"] = _desc;

		NetMgr nm = NetMgr.GetInstance ();

		nm.request_apis ("create_club", ob, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString ());
			if (ret.errcode != 0) {
				GameAlert.Show(ret.errmsg);
				return;
			}

			GameAlert.Show("俱乐部创建成功！", ()=>{
				reset();
				back();
			});
		});
	}
}
