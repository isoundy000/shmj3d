
using UnityEngine;
using System.Collections.Generic;

public class Share : MonoBehaviour {

	public int club_id = 0;

	public void onBtnCancel() {
		gameObject.SetActive(false);
	}

	public void onBtnWC() {
		share(false);
	}

	public void onBtnTL() {
		share(true);
	}

	void share(bool tl) {
		if (club_id == 0)
			return;

		string title = "<雀达麻友圈>";
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis ("get_club_detail", "club_id", club_id, data => {
			GetClubDetail ret = JsonUtility.FromJson<GetClubDetail> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("get_club_detail fail");
				return;
			}

			string content = ret.data.name + "俱乐部(ID:" + club_id + ")邀请您加入\n" + ret.data.desc;

			Dictionary<string, object> args = new Dictionary<string, object>();
			args.Add("club", club_id);

			AnysdkMgr.GetInstance().share(title, content, args, tl);
		});
	}
}
