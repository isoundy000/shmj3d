using UnityEngine;
using System.Collections;

public class GameEngine : MonoBehaviour {
	static GameEngine mInstance = null;

	GameMgr mGameMgr = null;
	NetMgr mNetMgr = null;

	public static GameEngine GetInstance() {
		return mInstance;
	}

	void Awake() {
		mNetMgr = NetMgr.GetInstance ();
		mNetMgr.Init ();

		mGameMgr = GameMgr.GetInstance ();
		mGameMgr.Init ();
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
