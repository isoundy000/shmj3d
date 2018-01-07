using UnityEngine;
using System.Collections;

public class Seat : MonoBehaviour {

	Transform mOffline = null;
	Transform mReady = null;
	Transform mButton = null;
	UILabel mName = null;
	UILabel mScore = null;
	IconLoader mIcon = null;
	Transform mChat = null;
	UILabel mID = null;

	string _username = "";
	int _score = 0;
	bool _offline = false;
	bool _ready = false;
	bool _button = false;
	int _userid = 0;

	void Awake() {
		Transform _icon = transform.FindChild("bghead/icon");
		if (_icon != null) {
			IconLoader i = _icon.GetComponent<IconLoader>();
			if (i != null)
				mIcon = i;
		}

		Transform nm = transform.FindChild ("name");
		if (nm != null)
			mName = nm.GetComponent<UILabel>();

		Transform sc = transform.FindChild ("score");
		if (sc != null)
			mScore = sc.GetComponent<UILabel>();

		mButton = transform.FindChild("button");
		mReady = transform.FindChild("ready");
		mOffline = transform.FindChild("offline");
		mChat = transform.FindChild("chat");

		Transform id = transform.FindChild ("id");
		if (id != null)
			mID = id.GetComponent<UILabel>();

		if (mIcon != null && _userid > 0)
			mIcon.setUserID(_userid);
	}
		
	string SubString(string str, int max) {
		if (str.Length >= max)
			return str.Substring (0, max);
		else
			return str;
	}

	void refresh() {
		if (mID != null)
			mID.text = "ID:" + _userid;

		if (mName != null)
			mName.text = SubString(_username, 5);

		if (mScore != null)
			mScore.text = _userid > 0 ? "" + _score : "";

		if (mButton != null)
			mButton.gameObject.SetActive(_button);

		if (mReady != null)
			mReady.gameObject.SetActive(_ready);

		if (mOffline != null)
			mOffline.gameObject.SetActive(_offline && _userid > 0);
	}

	public void setInfo(int uid, string name, int score) {
		_userid = uid;
		_username = name;
		_score = score;

		refresh ();

		if (mIcon != null)
			mIcon.setUserID(_userid);
	}

	public void setButton(bool val) {
		_button = val;

		if (mButton != null)
			mButton.gameObject.SetActive(val);
	}

	public void setReady(bool val) {
		_ready = val;

		if (mReady != null)
			mReady.gameObject.SetActive(val);
	}

	public void setOffline(bool val) {
		_offline = val;

		if (mOffline != null)
			mOffline.gameObject.SetActive(val);
	}

	public void setID(int uid) {
		_userid = uid;

		if (mID != null)
			mID.text = "ID:" + _userid;

		if (mIcon != null)
			mIcon.setUserID(_userid);
	}

	public void chat(string content) {

	}

	public void emoji(int id) {

	}

	public void reset() {
		_userid = 0;
		_username = "";
		_score = 0;
		_offline = false;
		_ready = false;
		_button = false;

		refresh();

		if (mIcon != null)
			mIcon.setUserID(_userid);
	}

	void Update() {

	}
}



