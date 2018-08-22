
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[Serializable]
public class NoticeInfo {
	public string content;
	public int playtimes;
};

[Serializable]
public class GetMessage {
	public int errcode;
	public string msg;
}

public class NoticeCtl : MonoBehaviour {

	Transform label;
	Transform mask;

	List<NoticeInfo> notices;
	int index = 0;

	Vector3 oldPos;

	float nextTime = 0;
	Sequence mSeq = null;

	void Start () {
		mask = transform.Find("mask");
		label = mask.Find("text");

		notices = new List<NoticeInfo>();
		oldPos = label.localPosition;

		refreshNotice();
	}

	void refreshNotice() {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis ("get_message", "type", "notice", data => {
			GetMessage msg = JsonUtility.FromJson<GetMessage> (data.ToString ());
			if (msg.errcode != 0) {
				Debug.Log("get_message fail " + msg.errcode);
				return;
			}

			if (this != null) {
				NoticeInfo info = new NoticeInfo();
				info.content = msg.msg;
				info.playtimes = 0;

				notices.Add(info);
				play();
			}
		});
	}

	void play() {
		int cnt = notices.Count;
		if (cnt < 1)
			return;

		if (index >= cnt)
			index = 0;

		var info = notices[index];
		int times = info.playtimes;

		if (times == 1)
			notices.RemoveAt (index);
		else {
			if (times > 1)
				info.playtimes--;
		
			index++;
		}

		nextTime = 0;
		var seq = DOTween.Sequence ();

		var text = label.GetComponent<UILabel>();
		text.text = info.content;

		float duration = 5.0f;
		float masksize = mask.GetComponent<UIPanel> ().GetViewSize ().x;
		float textsize = text.localSize.x;

		label.localPosition = oldPos;
		duration *= (1 + textsize / masksize);

		seq.Insert(0, label.DOLocalMoveX(0 - textsize - masksize / 2, duration).SetEase(Ease.Linear));
		seq.InsertCallback(duration, ()=>{
			mSeq = null;
			nextTime = Time.time + 3.0f;
		});

		mSeq = seq;
		seq.Play();
	}

	void Update() {
		float now = Time.time;

		if (nextTime > 0 && nextTime < now)
			play ();
	}
}


