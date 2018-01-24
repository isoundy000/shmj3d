using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatItem {
	public int type; // 0: text, 1: voice
	public int sender;
	public string text;
	public VoiceMsg voice;
	public string path;
	public bool read;

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
}

public class Chat : MonoBehaviour {

	public GameObject mChat = null;
	public UIInput mInput = null;

	Transform mGrid = null;

	string listPath = "Chat/msgs/grid";

	List<ChatItem> mChatItems = new List<ChatItem>();

	Queue<ChatItem> mVoiceQueue = new Queue<ChatItem>();
	int _playingSeat = -1;
	long _lastPlayTime = 0;

	void Awake() {
		mGrid = transform.Find (listPath);
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
		current.Find ("icon").GetComponent<IconLoader>().setUserID(item.sender);

		bool isVoice = item.type == 1;

		current.Find ("text").gameObject.SetActive(!isVoice);
		current.Find ("btn_voice").gameObject.SetActive(isVoice);
		current.Find ("len").gameObject.SetActive(isVoice);

		if (isVoice) {
			Utils.onClick (current.Find ("btn_voice"), () => {
				playVoiceItem(item);
			});

			current.Find ("len").GetComponent<UILabel> ().text = (item.voice.time / 1000) + "''";
		} else {
			current.Find ("text").GetComponent<UILabel> ().text = item.text;
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
	}


}
