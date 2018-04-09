
using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class ClubRole {
	public string role;
}

[Serializable]
public class GetClubRole {
	public int errcode;
	public string errmsg;
	public ClubRole data;
}

public class Lobby : MonoBehaviour {

	UITweener tweener = null;

	void Awake() {
		AnysdkMgr.setPortait ();
	}

	void Start () {

		AudioManager.GetInstance ().StopBGM ();

		GameMgr game = GameMgr.GetInstance();
		string roomid = game.userMgr.roomid;

		if (roomid != null && roomid.Length >= 6) {
			game.enterRoom(roomid, code=>{
				Debug.Log("enter ret=" + code);
			});

			game.userMgr.roomid = null;
		} else if (!checkQuery()) {
			resumeClub();
		}
	}

	void resumeClub() {
		int cid = GameMgr.GetInstance ().club_channel;
		if (cid == 0)
			return;

		enterClub(cid);
	}

	public bool checkQuery() {
		AnysdkMgr am = AnysdkMgr.GetInstance();
		string query = am.GetQuery ();

		if (query == null || query.Length == 0)
			return false;

		Dictionary<string, string> ps = Utils.parseQuery (query);

		string roomid = "";
		int clubid = 0;
		int gameid = 0;

		Utils.setTimeout (() => {
			am.ClearQuery();
		}, 0.1f);

		if (ps.ContainsKey("room"))
			roomid = ps["room"];

		if (ps.ContainsKey("club"))
			clubid = int.Parse(ps["club"]);

		if (ps.ContainsKey("game"))
			gameid = int.Parse(ps["game"]);

		if (roomid != "") {
			enterRoom(roomid);
		} else if (clubid > 0) {
			enterClub(clubid);
		} else if (gameid > 0) {
			enterGame(gameid);
		}

		return true;
	}

	void enterRoom(string roomid) {
		GameMgr gm = GameMgr.GetInstance();

		gm.enterRoom (roomid, code => {
			if (code != 0) {
				string content = "房间[" + roomid + "]不存在";

				if (code == 2224)
					content = "房间[" + roomid + "]已满！";
				else if (code == 2222)
					content = "钻石不足";
				else if (code == 2231)
					content = "您的IP和其他玩家相同";
				else if (code == 2232)
					content = "您的位置和其他玩家太近";
				else if (code == 2233)
					content = "您的定位信息无效，请检查是否开启定位";
				else if (code == 2251)
					content = "您不是俱乐部普通成员，无法加入俱乐部房间";

				GameAlert.Show(content);
			}
		});
	}

	void enterClub(int clubid) {
		/* 通过俱乐部名片进入
			1) 检查是否俱乐部成员
			2) 如果是普通会员，进入hall界面
			3) 如果是管理员，进入admin界面
			4) 如果不是俱乐部成员，发送加入申请
		*/

		NetMgr nm = NetMgr.GetInstance ();

		nm.request_apis ("get_club_role", "club_id", clubid, data => {
			GetClubRole ret = JsonUtility.FromJson<GetClubRole> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("get_club_role fail: " + ret.errcode);
				return;
			}

			string role = ret.data.role;
			if (role == "member") {
				GameObject hall = GameObject.Find ("PHall");
				hall.GetComponent<Hall>().enter(clubid);
			} else if (role == "admin") {
				GameObject admin = GameObject.Find ("PAdmin");
				admin.GetComponent<Admin>().enter(clubid);
			} else if (role == "outsider") {
				nm.request_apis("apply_join_club", "club_id", clubid, data2 => {
					NormalReturn ret2 = JsonUtility.FromJson<NormalReturn>(data2.ToString());
					if (ret2.errcode != 0) {
						Debug.Log("apply_join_club fail: " + ret2.errcode);
						return;
					}

					GameAlert.Show("已成功申请加入俱乐部" + clubid + "，请等待管理员审核");
				});
			}
		});
	}

	void enterGame(int gameid) {
		NetMgr nm = NetMgr.GetInstance ();

		nm.request_apis ("get_game_detail", "id", gameid, data => {
			// TODO
		});
	}
}
