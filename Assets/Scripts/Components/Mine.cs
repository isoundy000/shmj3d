using UnityEngine;
using System.Collections;

public class Mine : ListBase {

	void Start() {
		Transform me = transform.FindChild ("me");

		setText(me, "name", GameMgr.getUserMgr ().username);
		setIcon (me, "icon", GameMgr.getUserMgr ().userid);
	}

	public void onBtnClub() {
		ClubList cl = GameObject.Find("PClubList").GetComponent<ClubList>();
		cl.enter();
	}

	public void onBtnShop() {

	}

	public void onBtnBag() {

	}

	public void onBtnAchieve() {

	}

	public void onBtnSetting() {
		GameObject ob = GameObject.Find ("PSetting");
		ob.GetComponent<Setting>().enter();
	}

	public void onBtnFeedback() {
		GameObject ob = GameObject.Find ("PFeedback");
		ob.GetComponent<Feedback>().enter();
	}
}
