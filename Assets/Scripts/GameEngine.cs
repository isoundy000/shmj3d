﻿using UnityEngine;
using System.Collections;

public class GameEngine : MonoBehaviour {
	static GameEngine mInstance = null;

	GameMgr mGameMgr = null;
	NetMgr mNetMgr = null;
	RoomMgr mRoomMgr = null;
	VoiceMgr mVoiceMgr = null;
	ReplayMgr mReplayMgr = null;
	Http mHttp = null;

	public static GameEngine GetInstance() {
		return mInstance;
	}

	void Awake() {
		PUtils ut = PUtils.GetInstance ();

		Loom lm = Loom.Current;

		mNetMgr = NetMgr.GetInstance ();
		mNetMgr.Init ();

		mGameMgr = GameMgr.GetInstance ();
		mGameMgr.Init ();

		mRoomMgr = RoomMgr.GetInstance ();
		mRoomMgr.Init ();

		mVoiceMgr = VoiceMgr.GetInstance ();
		mVoiceMgr.Init ();

		mReplayMgr = ReplayMgr.GetInstance ();
		mReplayMgr.Init ();
	}

	void Start () {
		//mNetMgr.TestLogin ();
	}
	
	// Update is called once per frame
	void Update () {
		if (mNetMgr != null)
			mNetMgr.Update ();
	}
}
