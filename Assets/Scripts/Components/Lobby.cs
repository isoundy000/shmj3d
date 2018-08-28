
using System;
using UnityEngine;
using System.Collections;
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

		//StartCoroutine(LoadAssets());
	}

	IEnumerator LoadAssets() {
		string[] assets = new string[]{ "PMain", "PJoinRoom", "PHall", "PAdmin", "PCreateRoom", "PClubList", "PClubDetail",
										"PClubMessage", "PEditRoom", "PCreateClub", "PJoinClub", "PSetMember", "PRank", "PSetting",
										"PFeedback", "PClubHistory", "PDetailHistory", "PMessage", "PShare", "PShop" };

		foreach (var ab in assets)
			yield return StartCoroutine(PUtils.LoadAsset(transform, ab));
	}

	void Start () {

		AudioManager.GetInstance().PlayBackgroundAudio("hall_bgm");

		GameMgr game = GameMgr.GetInstance();
		string roomid = game.userMgr.roomid;

		PUtils.setTimeout (() => {
			if (roomid != null && roomid.Length >= 6) {
				game.enterRoom (roomid, code => {
					Debug.Log ("enter ret=" + code);
				});

				game.userMgr.roomid = "";
			} else if (!checkQuery ()) {
				//resumeClub ();
/*
				var cb = AnysdkMgr.getClipBoard();
				int id = cb.IndexOf("房号:");

				if (id >= 0 && cb.Length > id + 9)
					enterRoom(cb.Substring(id + 3, 6));
*/
			}
		}, 1.0f);
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

		Dictionary<string, string> ps = PUtils.parseQuery (query);

		string roomid = "";
		int clubid = 0;
		int gameid = 0;

		PUtils.setTimeout (() => {
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
				var script = ListBase.getPage<Hall>("PHall");
				if (script != null)
					script.enter(clubid);
			} else if (role == "admin") {
				var script = ListBase.getPage<Admin>("PAdmin");
				if (script != null)
					script.enter(clubid);
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
