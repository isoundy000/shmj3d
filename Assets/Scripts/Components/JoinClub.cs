
using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class NormalReturn {
	public int errcode;
	public string errmsg;
}

public class JoinClub : MonoBehaviour {

	public UIInput id;

	public void onBtnClose() {
		gameObject.SetActive(false);
	}

	void reset() {
		id.value = "";
	}

	public void onBtnJoin() {
		string club_id = id.value;
		int cid = 0;

		if (club_id == "" || !int.TryParse(club_id, out cid)) {
			GameAlert.Show ("请填写俱乐部ID");
			reset();
			return;
		}

		NetMgr nm = NetMgr.GetInstance ();
		nm.request_apis ("apply_join_club", "club_id", cid, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString ());
			if (ret.errcode != 0) {
				GameAlert.Show (ret.errmsg);
				reset();
				return;
			}

			GameAlert.Show("已成功申请，请等待管理员审核", ()=>{
				gameObject.SetActive(false);
				id.value = "";
			});
		});
	}
}
