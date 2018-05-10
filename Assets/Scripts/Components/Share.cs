
using UnityEngine;
using System.Collections.Generic;

public class Share : MonoBehaviour {

	public int club_id = 0;

	public void onBtnCancel() {
		gameObject.SetActive(false);
	}

	public void onBtnWC() {
		GameMgr.share_club(club_id, false);
	}

	public void onBtnTL() {
		GameMgr.share_club(club_id, true);
	}
}
