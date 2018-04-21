
using System;
using UnityEngine;
using System.Collections;
using System.IO;
using SimpleJson;

public class ClubDetail : ListBase {

	int mClubID = 0;
	bool mAdmin = false;
	public GameObject mShare = null;

	public Transform mDescItem;

	void Awake() {
		base.Awake();

		GameMgr gm = GameMgr.GetInstance();

		gm.AddHandler ("club_message_notify", data => {
			ClubMessageNotify notify = (ClubMessageNotify)data;

			if (notify.club_id == mClubID)
				setCount(notify.cnt);
		});
	}

	public void enter(int club_id, bool admin) {
		mClubID = club_id;
		mAdmin = admin;
		refresh();
		updateMessageCnt();
		show();
	}

	void refresh() {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis ("get_club_detail", "club_id", mClubID, data => {
			GetClubDetail ret = JsonUtility.FromJson<GetClubDetail> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("get_club_detail fail");
				return;
			}

			showClub(ret.data);
		});
	}

	void setCount(int cnt) {
		setActive(transform, "me/btn_mail/msg_num", cnt > 0);
		if (cnt > 0)
			setText(transform, "me/btn_mail/msg_num/tile", "" + cnt);
	}

	void updateMessageCnt() {
		NetMgr.GetInstance ().request_apis ("get_club_message_cnt", "club_id", mClubID, data => {
			GetClubMessageCnt ret = JsonUtility.FromJson<GetClubMessageCnt> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("get_club_message_cnt fail");
				return;
			}

			setCount(ret.data.cnt);
		});
	}

	void showClub(ClubDetailInfo club) {
		Transform me = transform.Find("me");
		Transform bottom = transform.Find("bottom");

		//setActive (transform, "top/btn_save", mAdmin);
		setActive (me, "btn_mail", mAdmin);
		if (mAdmin) {
			setBtnEvent (transform, "top/btn_edit", () => {
				SetClub sc = getPage<SetClub>("PSetClub");
				sc.UpdateEvents += refresh;
				sc.enter(mClubID);
			});

			setBtnEvent (me, "btn_mail", () => {
				ClubMessage cm = getPage<ClubMessage>("PClubMessage");
				cm.enter(mClubID);
			});
		}

		setText(me, "name", club.name);
		setText(me, "hc", club.member_num + " / " + club.max_member_num);
		setIcon(me, "icon", club.logo);

		Transform grid = transform.Find("items/grid_ign");
		Transform creator = grid.GetChild(0);

		setBtnEvent (grid.GetChild(1), null, () => {
			if (mAdmin) {
				SetMember sm = getPage<SetMember>("PSetMember");
				sm.enter(mClubID);
			} else {
				Rank rk = getPage<Rank>("PRank");
				rk.enter(mClubID);
			}
		});

		setText(grid.GetChild (2), "desc", club.desc);
		setActive(grid.GetChild (2), "btn_edit", mAdmin);

		setText(grid.GetChild (3), "id", "" + club.id);

		setText(creator, "name", club.owner.name);
		setIcon(creator, "icon", club.owner.logo);

		setBtnEvent(grid.GetChild (3), "btn_share", () => {
			mShare.SetActive(true);
			mShare.GetComponent<Share>().club_id = club.id;
		});

		Transform auto = grid.GetChild(4);

		setToggleEvent (auto, "auto_start", null);
		setToggle (auto, "auto_start", club.auto_start);

		auto.GetComponentInChildren<Collider>().enabled = mAdmin;

		Debug.Log ("enabled=" + mAdmin);

		if (mAdmin) {
			setToggleEvent (auto, "auto_start", val => {
				Debug.Log("audo_start changed, val=" + val);
				setAutoStart (val);
			});
		}

		setText(bottom, "create_time", "创建于" + Utils.formatTime(club.create_time));
		setBtnEvent(bottom, "btn_exit", () => {
			onBtnExit(mClubID);
		});

		grid.GetComponent<UIGrid> ().Reposition ();
		grid.GetComponentInParent<UIScrollView> ().ResetPosition ();
	}

	void onBtnExit(int club_id) {
		GameAlert.Show ("确定退出俱乐部吗？", () => {
			doExit(club_id);
		}, true);
	}

	void doExit(int club_id) {
		NetMgr.GetInstance ().request_apis ("leave_or_delete_club", "club_id", club_id, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString());
			if (ret.errcode != 0) {
				Debug.Log("leave_or_delete_club fail");
				return;
			}

			back();
		});
	}

	void save(JsonObject ob) {
		NetMgr nm = NetMgr.GetInstance();

		nm.request_apis("set_club", ob, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString());
			if (ret.errcode != 0) {
				Debug.Log("set_club fail: " + ret.errmsg);
				return;
			}

			refresh();
		});
	}

	void setAutoStart(bool value) {
		JsonObject ob = new JsonObject();
		ob["id"] = mClubID;
		ob["auto_start"] = value;

		save (ob);
	}

	void setDesc(string desc) {
		JsonObject ob = new JsonObject();
		ob["id"] = mClubID;
		ob["desc"] = desc;

		save (ob);
	}

	void saveIcon(string path) {
		JsonObject ob = new JsonObject();
		ob["id"] = mClubID;

		byte[] bytes = File.ReadAllBytes (path);
		string base64 = Convert.ToBase64String (bytes);
		ob["logo"] = base64;

		save(ob);
	}

	public void onBtnIcon() {
		Debug.Log("onBtnIcon");

		if (!mAdmin)
			return;

		AnysdkMgr.pick ((ret, path) => {
			if (0 != ret)
				return;

			Debug.Log("after pick " + path);
			saveIcon(path);
		});
	}

	public void onBtnDescEdit() {
		UILabel desc = mDescItem.Find ("desc").GetComponent<UILabel> ();

		setInput (mDescItem, "input", desc.text);
		setActive (mDescItem, "desc", false);
		setActive (mDescItem, "btn_edit", false);
		setActive (mDescItem, "input", true);
	}

	public void onDescSubmit() {
		UIInput input = mDescItem.GetComponentInChildren<UIInput>();
		string desc = input.value;

		setDesc(desc);
		setText(mDescItem, "desc", desc);

		setActive (mDescItem, "desc", true);
		setActive (mDescItem, "btn_edit", true);
		setActive (mDescItem, "input", false);
	}
}


