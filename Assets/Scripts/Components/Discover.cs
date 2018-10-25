
using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class RecommendRoom {
	public int id;
	public string room_tag;
	public ClubRoomBaseInfo base_info;
	public int club_id;
	public string club_name;
	public string club_logo;
	public int cnt;
	public List<ClubRoomPlayer> players;
}

[Serializable]
public class ListRecommendRooms {
	public int errcode;
	public string errmsg;
	public List<RecommendRoom> data;
}

public class Discover : ListBase {

	public GameObject msg_num = null;

	bool shown = false;
	int mRoomID = 0;

	float nextUp = -1;

	void Awake() {
		listPath = "Recommend/items/grid";
		base.Awake();

		InitEventHandler();
	}

	void Start() {
		var last = LoadingScene.lastScene;

		if (last == "01.login")
			onBtnShare ();
	}

	void InitEventHandler() {
		GameMgr gm = GameMgr.GetInstance();
/*
		gm.AddHandler("recommend_room_updated", data=>{
			if (!shown) return;

			if (this != null)
				refresh();
		});

		gm.AddHandler("sys_message_updated", data => {
			if (this != null)
				updateMessageCnt();
		});
*/
	}

	public void onBtnCreate() {
		AudioManager.PlayButtonClicked();

		var page = getPage<CreateRoom>("PCreateRoom");
		if (page != null) {
			page.UpdateEvents += () => {
				mShow = true;
				updateMessageCnt();
			};

			mShow = false;
			page.enter();
		}
	}

	public void onBtnJoin() {
		AudioManager.PlayButtonClicked();

		var page = getPage<JoinRoom>("PJoinRoom");
		if (page != null) {
			page.UpdateEvents += () => {
				mShow = true;
				updateMessageCnt();
			};

			mShow = false;
			page.enter();
		}
	}

	void OnEnable() {
		//refresh();
		mShow = true;
		updateMessageCnt();

		shown = true;
	}

	void OnDisable() {
		mShow = false;
		shown = false;
	}

	void refresh() {
		if (this != null)
			return;

		NetMgr.GetInstance ().request_apis ("list_recommend_rooms", null, data => {
			ListRecommendRooms ret = JsonUtility.FromJson<ListRecommendRooms> (data.ToString());
			if (ret.errcode != 0) {
				Debug.Log("list_recommend_rooms fail");
				return;
			}

			if (this != null)
				showItems(ret.data);
		});
	}

	void showItems(List<RecommendRoom> rooms) {
		bool show = false;

		for (int i = 0; i < rooms.Count; i++) {
			Transform item = getItem(i);
			RecommendRoom room = rooms[i];
			ClubRoomBaseInfo info = room.base_info;

			setText(item, "club", room.club_name + "俱乐部");
			setText(item, "desc", info.getDesc());
			setText(item, "room", "房间号" + room.room_tag);
			setText(item, "hc", room.cnt + " / " + info.numOfSeats);
			setIcon(item, "bghead/icon", room.club_logo);

			setBtnEvent (item, null, () => {
				showDetail(room);
			});

			if (mRoomID == room.id) {
				show = true;
				showDetail (room);
			}
		}

		if (!show) {
			mRoomID = 0;
			setActive (transform, "detail", false);
		}

		updateItems(rooms.Count);
	}

	void setCount(int cnt) {
		msg_num.SetActive(cnt > 0);
		msg_num.transform.Find("tile").GetComponent<UILabel>().text = "" + cnt;
	}

	void updateMessageCnt() {
		NetMgr.GetInstance ().request_apis ("get_my_message_cnt", null, data => {
			if (this != null)
				nextUp = 0;

			GetClubMessageCnt ret = JsonUtility.FromJson<GetClubMessageCnt> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("get_my_message_cnt fail");
				return;
			}

			if (this != null && ret.data != null)
				setCount(ret.data.cnt);
		});
	}

/*
	void Update() {
		if (!mShow || !gameObject.activeInHierarchy || nextUp < 0)
			return;
			
		nextUp += Time.deltaTime;
		if (nextUp < 5)
			return;

		nextUp = -1;
		updateMessageCnt();
	}
*/

	public void onBtnMessage() {
		AudioManager.PlayButtonClicked();

		var page = getPage<Message>("PMessage");
		if (page != null) {
			page.UpdateEvents += () => {
				mShow = true;
				updateMessageCnt();
			};

			mShow = false;
			page.enter();
		}
	}

	public void onBtnShare() {
		var share = transform.Find ("share");

		share.gameObject.SetActive(true);
	}

	void showDetail(RecommendRoom room) {
		mRoomID = room.id;

		Transform detail = transform.Find("detail");
		setActive(detail, null, true);

		Transform seats = detail.Find("seats");
		UIGrid grid = seats.GetComponent<UIGrid>();

		int nseats = room.players.Count;
		int empties = 0;

		for (int i = 0; i < seats.childCount && i < nseats; i++) {
			ClubRoomPlayer p = room.players [i];
			Transform s = seats.GetChild(i);
			bool empty = p.id == 0;

			setActive(s, null, true);
			setActive(s, "name", !empty);
			setActive(s, "bghead/icon", !empty);

			setIcon(s, "bghead/icon", p.id);
			setText(s, "name", PUtils.subString(p.name, 5));
			if (empty)
				empties++;
		}

		for (int i = nseats; i < seats.childCount; i++) {
			Transform s = seats.GetChild (i);
			setActive(s, null, false);
		}

		grid.Reposition();

		ClubRoomBaseInfo info = room.base_info;
		var type = info.type;
		Transform ops = detail.Find("options");

		ops.gameObject.SetActive (true);

		setText (ops, "playernum", "" + info.numOfSeats);
		setText (ops, "gamenum", "" + info.maxGames);

		var rules = ops.Find ("rules");
		for (int i = 0; i < rules.childCount; i++)
			rules.GetChild (i).gameObject.SetActive (false);

		var rule = ops.Find("rules/" + type);
		rule.gameObject.SetActive (true);

		if (type == "shmj") {
			setText (ops, "rule", "上海敲麻");
			setText (rule, "huafen", "" + info.huafen);
			setText (rule, "maxfan", "" + info.maxFan);
			setText (rule, "maima", info.maima ? "是" : "否");
			setText (rule, "qidui", info.qidui ? "是" : "否");
		} else if (type == "gzmj") {
			setText (ops, "rule", "酒都麻将");
			setText (rule, "jyw", info.jyw ? "是" : "否");
			setText (rule, "j7w", info.j7w ? "是" : "否");
			setText (rule, "ryj", info.ryj ? "是" : "否");
		}

		setText (ops, "limit_ip", info.limit_ip ? "是" : "否");
		setText (ops, "limit_gps", info.limit_gps ? "是" : "否");

		setBtnEvent(detail, "btn_join", () => {
			if (empties == 0) {
				GameAlert.Show("房间已满，请重新选择");
				return;
			}

			mShow = false;
			GameMgr.GetInstance().enterRoom(room.room_tag, code=>{
				Debug.Log("club enterRoom code=" + code);
				if (0 != code) {
					mShow = true;

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

		setBtnEvent(detail, "btn_back", () => {
			mRoomID = 0;
			setActive(detail, null, false);
		});
	}

	public void onBtnXL() {
		AnysdkMgr.GetInstance ().shareImgXL ();
	}
}



