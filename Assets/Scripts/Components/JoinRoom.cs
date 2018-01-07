using UnityEngine;
using System.Collections;

public class JoinRoom : ListBase {
	ArrayList inputs = null;
	int index = 0; 
	ArrayList roomid = new ArrayList();

	void Awake() {
		base.Awake();

		inputs = new ArrayList ();

		Transform _inputs = transform.FindChild ("Body/inputs");
		for (int i = 0; i < 6; i++) {
			inputs.Add(_inputs.GetChild (i).GetComponentInChildren<UILabel>());
		}
	}

	void onInputFinished(string id) {
		Debug.Log ("input finished: " + id);

		GameMgr game = GameMgr.GetInstance ();
		game.enterRoom(id, code=>{
			Debug.Log("enter ret=" + code);
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
		show();
	}
}
