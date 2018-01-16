using UnityEngine;
using System.Collections;

public class Lobby : MonoBehaviour {

	UITweener tweener = null;

	void Awake() {
		AnysdkMgr.setPortait ();
	}

	void Start () {
		GameMgr game = GameMgr.GetInstance();
		string roomid = game.userMgr.roomid;

		if (roomid != null && roomid.Length >= 6) {
			game.enterRoom(roomid, code=>{
				Debug.Log("enter ret=" + code);
			});

			game.userMgr.roomid = null;
		}
	}
}
