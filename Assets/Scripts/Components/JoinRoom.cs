﻿
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JoinRoom : ListBase {
	List<UILabel> inputs;
	int index = 0; 
	List<int> roomid;

	UIToggle is_club;

	void Start() {
		roomid = new List<int>();
		inputs = new List<UILabel>();

		Transform _inputs = transform.Find ("Body/inputs");
		for (int i = 0; i < 6; i++) {
			inputs.Add(_inputs.GetChild (i).GetComponentInChildren<UILabel>());
		}

		var club = transform.Find ("Body/club");
		is_club = club.GetComponent<UIToggle>();

		if (!PlayerPrefs.HasKey ("JOIN_CLUB_ROOM"))
			PlayerPrefs.SetInt ("JOIN_CLUB_ROOM", 0);

		is_club.value = PlayerPrefs.GetInt ("JOIN_CLUB_ROOM") == 1;
	}
		
	void setInput(int id, int num) {
		if (id < 0 || id >= 6)
			return;

		UILabel label = inputs[id];

		if (num >= 0 && num < 10)
			label.text = "" + num;
		else
			label.text = "";

		label.MakePixelPerfect ();
	}

	void onInputFinished(string id) {
		Debug.Log ("input finished: " + id);

		if (is_club.value)
			id = "c" + id;

		PlayerPrefs.SetInt("JOIN_CLUB_ROOM", is_club.value ? 1 : 0);

		GameMgr gm = GameMgr.GetInstance ();
		gm.enterRoom(id, code => {
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

	void onInput(int num) {
		if (index >= 6)
			return;

		setInput (index++, num);
		roomid.Add (num);

		if (index == 6) {
			string id = "";

			foreach (int i in roomid) {
				id += i.ToString ();
			}

			onInputFinished (id);
		}
	}

	public void onN0Clicked() {
		onInput (0);
	}

	public void onN1Clicked() {
		onInput (1);
	}

	public void onN2Clicked() {
		onInput (2);
	}

	public void onN3Clicked() {
		onInput (3);
	}

	public void onN4Clicked() {
		onInput (4);
	}

	public void onN5Clicked() {
		onInput (5);
	}

	public void onN6Clicked() {
		onInput (6);
	}

	public void onN7Clicked() {
		onInput (7);
	}

	public void onN8Clicked() {
		onInput (8);
	}

	public void onN9Clicked() {
		onInput (9);
	}

	public void onResetClicked() {
		for (int i = 0; i < 6; i++)
			setInput (i, -1);

		index = 0;
		roomid.Clear ();
	}

	public void onDelClicked() {
		if (index > 0) {
			index--;
			setInput(index, -1);
			roomid.RemoveAt (index);
		}
	}

	public void enter() {
		onResetClicked();
		show();
	}
}
