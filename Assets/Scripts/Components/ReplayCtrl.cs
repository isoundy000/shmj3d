
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayCtrl : MonoBehaviour {

	public GameObject replay = null;
	public UILabel progress = null;

	public UIButton btnFF;
	public UIButton btnPrev;

	bool isPlaying = true;
	float nextPlayTime = 0;

	bool isReplay = false;

	bool exit = false;

	float lastClickTime = 0;

	void Start() {
		isReplay = ReplayMgr.GetInstance().isReplay();

		replay.SetActive(isReplay);
		refreshBtn();

		nextPlayTime = Time.time + 5.0f;

		if (isReplay) {
			PUtils.setTimeout (() => {
				ReplayMgr.GetInstance().sync ();
			}, 2.0f);
		}
	}

	void refreshBtn() {
		SpriteMgr sp = replay.GetComponentInChildren<SpriteMgr>();
		sp.setIndex(isPlaying ? 1 : 0);
	}

	public void onBtnPrev() {
		ReplayMgr rm = ReplayMgr.GetInstance();
		bool old = isPlaying;

		isPlaying = false;

		rm.prev(2);
		rm.sync();

		updateProgress();
		nextPlayTime = Time.time + 2.0f;
		isPlaying = old;

		refreshBtn();

		lastClickTime = Time.time;
		setButton(false);
	}

	public void onBtnPlay() {
		isPlaying = !isPlaying;
		refreshBtn();
	}

	public void onBtnForward() {
		ReplayMgr rm = ReplayMgr.GetInstance();
		bool old = isPlaying;

		isPlaying = false;

		rm.forward(2);
		rm.sync();

		updateProgress ();

		nextPlayTime = Time.time + 2.0f;
		isPlaying = old;

		refreshBtn();

		lastClickTime = Time.time;
		setButton(false);
	}

	void setButton(bool status) {
		btnFF.enabled = status;
		btnPrev.enabled = status;
	}

	public void onBtnBack() {
		if (!exit) {
			exit = true;
			GameManager.GetInstance().exit(2.0f);
		}
	}

	public void updateProgress() {
		if (progress != null)
			progress.text = ReplayMgr.GetInstance().getProgress() + "%";
	}

	void Update() {
		ReplayMgr rm = ReplayMgr.GetInstance();

		if (isPlaying && rm.isReplay () && nextPlayTime > 0 && nextPlayTime < Time.time) {
			float next = rm.takeAction(false);

			if (next == 0) {
				nextPlayTime = 0;
				return;
			}

			nextPlayTime = Time.time + next;
			updateProgress();
		}

		if (lastClickTime > 0 && lastClickTime + 0.5f < Time.time) {
			setButton(true);
			lastClickTime = 0;
		}
			
	}
}

