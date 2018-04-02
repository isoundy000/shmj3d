using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatItem {
	public int type; // 0: text, 1: voice, 2: emoji
	public int sender;
	public string text;
	public VoiceMsg voice;
	public string path;
	public bool read;
	public int emoji;
	public GameObject vobj = null;

	public ChatItem(VoiceMsgPush vmp) {
		type = 1;
		sender = vmp.sender;
		voice = vmp.content;
		path = "vm-" + sender + "-" + Utils.getSeconds() + ".amr";
		read = false;
	}

	public ChatItem(ChatInfo info) {
		type = 0;
		sender = info.sender;
		text = info.content;
	}

	public ChatItem(EmojiPush info) {
		type = 2;
		sender = info.sender;
		emoji = info.content;
	}
}

public class Chat : MonoBehaviour {

	public GameObject mChat = null;
	public UIInput mInput = null;

	UIScrollView scroll = null;
	Transform mGrid = null;

	string listPath = "Chat/msgs";

	List<ChatItem> mChatItems = new List<ChatItem>();

	Queue<ChatItem> mVoiceQueue = new Queue<ChatItem>();
	int _playingSeat = -1;
	long _lastPlayTime = 0;

	GameObject playing = null;

	void Awake() {

		scroll = transform.Find(listPath).GetComponent<UIScrollView>();
		mGrid = transform.Find (listPath + "/grid");
		InitEventHandler();

		Transform emojis = transform.Find("Chat/emoji/table");

		for (int i = 0; i < 20; i++) {
			int j = i;
			Utils.onClick (emojis.GetChild(j), () => {
				onEmojiClicked(j);
			});
		}
	}

	void InitEventHandler() {
		GameMgr gm = GameMgr.GetInstance();

		gm.AddHandler ("voice_msg", data => {
			onVoiceMsg((VoiceMsgPush)data);
		});

		gm.AddHandler ("chat", data => {
			onChat((ChatInfo)data);
		});

		gm.AddHandler("emoji_push", data => {
			onEmoji((EmojiPush)data);
		});
	}

	void onEmojiClicked(int idx) {
		Debug.Log ("onEmojiClicked: " + idx);
		NetMgr.GetInstance ().send ("emoji", "id", idx);
	}

	public void onBtnChat() {
		mChat.SetActive(true);
	}

	void onVoiceMsg(VoiceMsgPush vmp) {
		ChatItem item = new ChatItem(vmp);
		addItem(item);

		mVoiceQueue.Enqueue(item);
		playVoice();
	}

	void onEmoji(EmojiPush ep) {
		ChatItem item = new ChatItem(ep);
		addItem(item);
	}

	void playVoice() {
		VoiceMgr vmr = VoiceMgr.GetInstance();
		RoomMgr rm = RoomMgr.GetInstance();

		if (_playingSeat < 0 && mVoiceQueue.Count > 0) {
			ChatItem vm = mVoiceQueue.Dequeue();
			string msg = vm.voice.msg;
			string file = vm.path;
			int si = rm.getSeatIndexByID(vm.sender);
			int local = rm.getLocalIndex(si);
			_playingSeat = local;

			vmr.writeVoice(file, msg);
			vmr.play(file);

			if (playing != null) {
				playing.SetActive (false);
				playing = null;
			}

			playing = vm.vobj;
			if (playing != null)
				playing.SetActive(true);

			_lastPlayTime = Utils.getMilliSeconds() + vm.voice.time;
		}
	}

	void Update() {
		long now = Utils.getMilliSeconds();

		if (_lastPlayTime != 0) {
			if (now > _lastPlayTime + 200) {
				onPlayerOver ();
				_lastPlayTime = 0;
			}
		} else {
			playVoice();
		}
	}

	void onPlayerOver() {
		AudioManager.resumeAll();
		_playingSeat = -1;

		if (playing != null) {
			playing.SetActive (false);
			playing = null;
		}
	}

	void onChat(ChatInfo info) {
		ChatItem item = new ChatItem(info);
		addItem(item);
	}

	void playVoiceItem(ChatItem item) {
		_playingSeat = -1;
		_lastPlayTime = 0;
		mVoiceQueue.Clear();

		VoiceMgr vm = VoiceMgr.GetInstance ();
		vm.stop();

		mVoiceQueue.Enqueue(item);
		playVoice();
	}

	void addItem(ChatItem item) {
		Transform _item = getItem(mChatItems.Count);

		bool self = item.sender == GameMgr.getUserMgr().userid;

		Transform left = _item.Find ("left");
		Transform right = _item.Find ("right");
		Transform current = null;

		left.gameObject.SetActive(!self);
		right.gameObject.SetActive(self);
		current = self ? right : left;

		Debug.Log("setIcon: " + item.sender);
		GameObject icon = current.Find ("icon").gameObject;
		IconLoader loader = icon.AddComponent<IconLoader>();
		loader.setUserID(item.sender);
		//icon.GetComponent<IconLoader>().setUserID(item.sender);

		int type = item.type;
		GameObject btn_voice = current.Find("btn_voice").gameObject;
		GameObject voice = current.Find("voice").gameObject;
		GameObject text = current.Find("text").gameObject;
		GameObject len = current.Find("len").gameObject;
		GameObject emoji = current.Find("emoji").gameObject;

		item.vobj = voice;

		text.SetActive(type == 0);
		btn_voice.SetActive(type != 2);
		len.SetActive(type == 1);
		voice.SetActive(false);
		emoji.SetActive(type == 2);

		if (type == 1) {
			Utils.onClick (current.Find("btn_voice"), () => {
				playVoiceItem (item);
			});

			len.GetComponent<UILabel> ().text = (item.voice.time / 1000) + "''";
		} else if (type == 0) {
			text.GetComponent<UILabel> ().text = item.text;
		} else if (type == 2) {
			emoji.GetComponent<UISprite>().spriteName = "face_" + (item.emoji + 1);
		}

		mChatItems.Add(item);
		updateItems (mChatItems.Count);
	}

	public void onBtnMask() {
		mChat.SetActive(false);
	}

	public void onBtnSend() {
		string text = mInput.value;
		if (text == "")
			return;

		NetMgr.GetInstance().send("chat", "msg", text);
		mInput.value = "";
	}

	Transform getItem(int id) {
		if (mGrid.childCount > id)
			return mGrid.GetChild(id);

		GameObject ob = Instantiate(Resources.Load("Prefab/UI/ChatItem"), mGrid) as GameObject;
		return ob.transform;
	}

	void shrinkContent(int num) {
		while (mGrid.childCount > num)
			DestroyImmediate(mGrid.GetChild(mGrid.childCount - 1).gameObject);
	}

	void updateItems(int count) {
		shrinkContent(count);

		UIGrid grid = mGrid.GetComponent<UIGrid>();
		if (grid != null)
			grid.Reposition();

		scroll.ResetPosition();
	}
}

