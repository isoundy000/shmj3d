using UnityEngine;
using System.Collections;

public class JoinRoom : ListBase {
	ArrayList inputs = null;
	int index = 0; 
	ArrayList roomid = new ArrayList();

	void Awake() {
		base.Awake();

		inputs = new ArrayList ();

		Transform _inputs = transform.Find ("Body/inputs");
		for (int i = 0; i < 6; i++) {
			inputs.Add(_inputs.GetChild (i).GetComponentInChildren<UILabel>());
		}
	}

	void onInputFinished(string id) {
		Debug.Log ("input finished: " + id);

		GameMgr gm = GameMgr.GetInstance ();
		gm.enterRoom(id, code => {
			if (code != 0) {
				string content = "房间<" + id + ">不存在";

				if (code == 2224)
					content = "房间<" + id + ">已满！";
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

	void onInput(int num) {
		if (index >= 6)
			return;

		UILabel label = (UILabel)inputs[index++];
		label.text = num.ToString ();
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
		foreach (UILabel label in inputs) {
			label.text = "";
		}

		index = 0;
		roomid.Clear ();
	}

	public void onDelClicked() {
		if (index > 0) {
			index--;
			UILabel lable = (UILabel)inputs [index];
			lable.text = "";
			roomid.RemoveAt (index);
		}
	}

	public void enter() {
		onResetClicked();
		show();
	}
}
