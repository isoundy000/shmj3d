using UnityEngine;
using System.Collections;

public class Mine : ListBase {

	void Start() {
		Transform me = transform.Find ("me");

		var um = GameMgr.getUserMgr();

		setText (me, "name", um.username);
		setIcon (me, "icon", um.userid);
		setText (me, "id", "ID: " + um.userid);

		PUtils.setTimeout (() => {
			Transform items = transform.Find ("items");
			if (items) {
				var grid = items.GetComponentInChildren<UIGrid>();
				if (grid)
					grid.Reposition();
				
				var scroll = items.GetComponent<UIScrollView>();
				if (scroll)
					scroll.ResetPosition();
			}
		}, 0.5f);
	}

	void Awake() {
		base.Awake ();

		updateGems();

		GameMgr.GetInstance ().eventUpCoins += updateGems;
	}

	void OnDestroy() {
		GameMgr.GetInstance ().eventUpCoins -= updateGems;
	}

	void OnEnable() {
		GameMgr.GetInstance().get_coins();
	}

	void updateGems() {
		var gm = GameMgr.GetInstance ();
		var gems = transform.Find("items/grid_ign").GetChild(0).Find("gems").GetComponent<UILabel>();

		gems.text = "" + gm.get_gems ();
	}

	public void onBtnClub() {
		var ob = getPage<ClubList>("PClubList");
		if (ob != null)
			ob.enter();
	}

	public void onBtnShop() {
		var ob = getPage<Shop>("PShop");
		if (ob != null)
			ob.enter();
	}

	public void onBtnBag() {
		Debug.Log ("onBtnBag");
	}

	public void onBtnAchieve() {

	}

	public void onBtnSetting() {
		var ob = getPage<LuaListBase>("PSetting");
		if (ob != null)
			ob.enter();
	}

	public void onBtnFeedback() {
		var ob = getPage<Feedback>("PFeedback");
		if (ob != null)
			ob.enter();
	}

	public void onBtnDealer() {
		var gm = GameMgr.GetInstance ();

		gm.dealerLogin (ret => {
			var login = gm.mLogin;
			bool dealer = ret && login != null && login.valid();

			if (dealer) {
				var ob = getPage<Dealer>("PDealer");
				if (ob != null)
					ob.enter();
			} else {
				var ob = getPage<Invest>("PInvest");
				if (ob != null)
					ob.enter();
			}
		});
	}

	public void onBtnOfficial() {
		var ob = getPage<ListBase>("POfficial");
		if (ob != null)
			ob.enter();
	}
}
