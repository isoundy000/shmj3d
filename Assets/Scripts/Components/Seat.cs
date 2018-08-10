using UnityEngine;
using System.Collections;

public class Seat : MonoBehaviour {

	public GameObject cigarette = null;

	Transform mOffline = null;
	Transform mReady = null;
	Transform mButton = null;
	UILabel mName = null;
	UILabel mScore = null;
	IconLoader mIcon = null;
	GameObject mChat = null;
	GameObject mEmoji = null;
	GameObject mFire = null;
	GameObject mVoice = null;
	GameObject mTing = null;
	GameObject mHu = null;
	GameObject mQue = null;
	UILabel mID = null;

	string _username = "";
	int _score = 0;
	bool _offline = false;
	bool _ready = false;
	bool _button = false;
	bool _ting = false;
	bool _hu = false;
	int _userid = 0;
	int _que = 0;

	float _lastChatTime = 0;
	float _lastVoiceTime = 0;

	void Awake() {
		Transform _icon = transform.Find("bghead/icon");
		if (_icon != null) {
			IconLoader i = _icon.GetComponent<IconLoader>();
			if (i != null)
				mIcon = i;
		}

		Transform nm = transform.Find ("name");
		if (nm != null)
			mName = nm.GetComponent<UILabel>();

		Transform sc = transform.Find ("score");
		if (sc != null)
			mScore = sc.GetComponent<UILabel>();

		mButton = transform.Find("button");
		mReady = transform.Find("ready");
		mOffline = transform.Find("offline");
		mChat = transform.Find("chat").gameObject;
		mEmoji = transform.Find("emoji").gameObject;

		Transform id = transform.Find ("id");
		if (id != null)
			mID = id.GetComponent<UILabel>();

		if (mIcon != null && _userid > 0)
			mIcon.setUserID(_userid);

		Transform fire = transform.Find("bghead/fire");
		if (fire != null)
			mFire = fire.gameObject;

		Transform voice = transform.Find("voice");
		if (voice != null)
			mVoice = voice.gameObject;

		Transform ting = transform.Find("ting");
		if (ting != null)
			mTing = ting.gameObject;

		Transform hu = transform.Find("hu");
		if (hu != null)
			mHu = hu.gameObject;

		var que = transform.Find("que");
		if (que != null)
			mQue = que.gameObject;
	}

	void refresh() {
		if (mID != null)
			mID.text = "ID:" + _userid;

		if (mName != null)
			mName.text = PUtils.subString(_username, 5);

		if (mScore != null)
			mScore.text = _userid > 0 ? "" + _score : "";

		if (mButton != null)
			mButton.gameObject.SetActive(_button);

		if (mTing != null)
			mTing.SetActive(_ting);

		if (mHu != null)
			mHu.SetActive(_hu);

		if (cigarette != null)
			cigarette.SetActive(_ting);

		if (mReady != null)
			mReady.gameObject.SetActive(_ready);

		if (mOffline != null)
			mOffline.gameObject.SetActive(_offline && _userid > 0);

		if (mFire != null)
			mFire.SetActive(false);

		if (mQue != null) {
			bool show = _que > 0;
			mQue.SetActive(show);
			if (show)
				mQue.GetComponent<SpriteMgr>().setIndex(_que - 1);
		}
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

	public void setFire(bool fire) {
		if (mFire != null)
			mFire.SetActive(fire);
	}

	public void setTing(bool ting) {
		_ting = ting;
		if (mTing != null)
			mTing.SetActive(ting);

		if (cigarette != null)
			cigarette.SetActive(ting);
	}

	public void setHu(bool hu) {
		_hu = hu;
		if (mHu != null)
			mHu.SetActive(hu);

		if (hu) {
			_ting = false;
			setTing(false);
		}
	}

	public void setQue(int que) {
		_que = que;
		if (mQue != null) {
			bool show = que > 0;
			mQue.SetActive(show);
			if (show)
				mQue.GetComponent<SpriteMgr>().setIndex (que - 1);
		}
	}

	public void chat(string content) {
		if (mChat == null)
			return;

		mChat.SetActive(true);
		mChat.GetComponentInChildren<UILabel>().text = content;
		_lastChatTime = Time.time + 3.0f;
	}

	public void voice(float duration) {
		if (mVoice == null)
			return;

		mVoice.SetActive(true);
		_lastVoiceTime = Time.time + duration;
	}

	public void emoji(int id) {
		if (mEmoji == null)
			return;

		mEmoji.active = true;
		mEmoji.GetComponent<EmojiAnim>().run(id);
	}

	public void reset() {
		Debug.Log ("seat reset");
		_userid = 0;
		_username = "";
		_score = 0;
		_offline = false;
		_ready = false;
		_button = false;
		_ting = false;
		_hu = false;

		refresh();

		if (mIcon != null)
			mIcon.setUserID(_userid);
	}

	void Update() {
		if (_lastChatTime > 0 && Time.time > _lastChatTime) {
			mChat.SetActive(false);
			_lastChatTime = 0;
		}

		if (_lastVoiceTime > 0 && Time.time > _lastVoiceTime) {
			mVoice.SetActive(false);
			_lastVoiceTime = 0;
		}
	}
}



