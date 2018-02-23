
using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class RecommendRoom {
	public string room_tag;
	public ClubRoomBaseInfo base_info;
	public int club_id;
	public string club_name;
	public string club_logo;
	public int cnt;
}

[Serializable]
public class ListRecommendRooms {
	public int errcode;
	public string errmsg;
	public List<RecommendRoom> data;
}

public class Discover : ListBase {

	public GameObject msg_num = null;

	void Awake() {
		listPath = "Recommend/items/grid";
		base.Awake();

		InitEventHandler();
	}

	void InitEventHandler() {
		GameMgr gm = GameMgr.GetInstance();

		// TODO: listen to recommend rooms update message
		gm.AddHandler("recommend_room_updated", data=>{
			refresh();
		});

		gm.AddHandler("sys_message_updated", data => {
			updateMessageCnt();
		});
	}

	public void onBtnCreate() {
		CreateRoom jr = getPage<CreateRoom>("PCreateRoom");
		jr.enter();
	}

	public void onBtnJoin() {
		JoinRoom jr = getPage<JoinRoom>("PJoinRoom");
		jr.enter();
	}

	void OnEnable() {
		refresh();
		updateMessageCnt();
	}

	void refresh() {
		NetMgr.GetInstance ().request_apis ("list_recommend_rooms", null, data => {
			ListRecommendRooms ret = JsonUtility.FromJson<ListRecommendRooms> (data.ToString());
			if (ret.errcode != 0) {
				Debug.Log("list_recommend_rooms fail");
				return;
			}

			showItems(ret.data);
		});
	}

	void showItems(List<RecommendRoom> rooms) {
		for (int i = 0; i < rooms.Count; i++) {
			Transform item = getItem(i);
			RecommendRoom room = rooms[i];
			ClubRoomBaseInfo info = room.base_info;

			setText(item, "club", room.club_name + "俱乐部");
			setText(item, "desc", info.huafen + "/" + info.huafen + (info.maima ? "带苍蝇" : "不带苍蝇") + info.maxGames + "局");
			setText(item, "room", "房间号" + room.room_tag);
			setText(item, "hc", room.cnt + " / " + info.numOfSeats);
			setIcon(item, "bghead/icon", room.club_logo);
		}

		updateItems(rooms.Count);
	}

	void setCount(int cnt) {
		msg_num.SetActive(cnt > 0);
		msg_num.transform.Find("tile").GetComponent<UILabel>().text = "" + cnt;
	}

	void updateMessageCnt() {
		NetMgr.GetInstance ().request_apis ("get_my_message_cnt", null, data => {
			GetClubMessageCnt ret = JsonUtility.FromJson<GetClubMessageCnt> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("get_my_message_cnt fail");
				return;
			}

			if (ret.data != null)
				setCount(ret.data.cnt);
			else
				Debug.LogError("get_my_message_cnt failed");
		});
	}

	public void onBtnMessage() {
		Message msg = getPage<Message>("PMessage");
		msg.UpdateEvents += refresh;
		msg.enter();
	}

	void Update() {

	}
}
