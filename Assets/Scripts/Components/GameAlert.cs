
using System;
using UnityEngine;
using System.Collections.Generic;

public class GameAlert : MonoBehaviour {

	public static GameAlert _instance = null;
	public static GameAlert GetInstance() { return Instance; }

	UILabel mContent = null;

	UIGrid grid;
	GameObject btnOk;
	GameObject btnCancel;
	Action mAct = null;

	public static GameAlert Instance {
		get {
			if (_instance == null) {
				GameObject obj = Instantiate(Resources.Load ("Prefab/UI/Alert")) as GameObject;
				_instance = obj.AddComponent<GameAlert>();

				_instance.transform.SetParent(GameObject.FindObjectOfType<UIRoot>().transform);
				_instance.transform.localScale = Vector3.one;
			}

			return _instance;
		}
	}

	void Awake() {
		InitView();
	}

	void InitView() {
		mContent = transform.Find("content").GetComponent<UILabel>();

		Transform _grid = transform.Find ("btns");
		grid = _grid.GetComponent<UIGrid>();

		btnOk = _grid.Find ("btn_ok").gameObject;
		btnCancel = _grid.Find ("btn_cancel").gameObject;

		btnOk.GetComponent<UIButton> ().onClick.Add (new EventDelegate(this, "onBtnOK"));
		btnCancel.GetComponent<UIButton> ().onClick.Add (new EventDelegate(this, "onBtnCancel"));

		gameObject.SetActive(false);
	}

	void hide() {
		gameObject.SetActive(false);
		Destroy(gameObject);
	}

	void onBtnOK() {
		if (mAct != null)
			mAct.Invoke();

		mAct = null;
		hide ();
	}

	void onBtnCancel() {
		mAct = null;
		hide ();
	}

	public void show(string content, Action act = null, bool needCancel = false) {
		gameObject.SetActive (true);

		mAct = act;
		mContent.text = content;

		btnCancel.SetActive(needCancel);
		grid.Reposition();
	}

	public static void Show(string content, Action act = null, bool needCancel = false) {
		GameAlert.GetInstance ().show (content, act, needCancel);
	}
}



