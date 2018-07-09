using UnityEngine;
using System.Collections;

public class Mine : ListBase {

	void Start() {
		Transform me = transform.Find ("me");

		setText(me, "name", GameMgr.getUserMgr ().username);
		setIcon (me, "icon", GameMgr.getUserMgr ().userid);

		PUtils.setTimeout (() => {
			Transform items = transform.Find ("items");
			items.GetComponentInChildren<UIGrid> ().Reposition ();
			items.GetComponent<UIScrollView> ().ResetPosition ();
		}, 0.1f);
	}

	void Awake() {
		base.Awake ();

		updateGems();

		GameMgr.GetInstance ().eventUpCoins += updateGems;
	}

	void OnDestroy() {
		GameMgr.GetInstance ().eventUpCoins -= updateGems;
	}

	void updateGems() {
		var gm = GameMgr.GetInstance ();
		var gems = transform.Find("items/grid_ign").GetChild(0).Find("gems").GetComponent<UILabel>();

		gems.text = "" + gm.get_gems ();
	}

	public void onBtnClub() {
		ClubList cl = GameObject.Find("PClubList").GetComponent<ClubList>();
		cl.enter();
	}

	public void onBtnShop() {
		GameObject ob = GameObject.Find ("PShop");
		ob.GetComponent<Shop>().enter();
	}

	public void onBtnBag() {
		Debug.Log ("onBtnBag");
	}

	public void onBtnAchieve() {

	}

	public void onBtnSetting() {
		GameObject ob = GameObject.Find ("PSetting");
		ob.GetComponent<LuaListBase>().enter();
	}

	public void onBtnFeedback() {
		GameObject ob = GameObject.Find ("PFeedback");
		ob.GetComponent<Feedback>().enter();
	}
}
