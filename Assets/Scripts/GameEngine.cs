using UnityEngine;
using System.Collections;

public class GameEngine : MonoBehaviour {
	static GameEngine mInstance = null;

	GameMgr mGameMgr = null;
	NetMgr mNetMgr = null;
	RoomMgr mRoomMgr = null;

	public static GameEngine GetInstance() {
		return mInstance;
	}

	void Awake() {
		mNetMgr = NetMgr.GetInstance ();
		mNetMgr.Init ();

		mGameMgr = GameMgr.GetInstance ();
		mGameMgr.Init ();

		mRoomMgr = RoomMgr.GetInstance ();
		mRoomMgr.Init ();
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
