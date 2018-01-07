using UnityEngine;
using System.Collections;

public class ClubDetail : ListBase {

	int mClubID = 0;
	bool mAdmin = false;
	public GameObject mShare = null;

	void Awake() {
		base.Awake();

		GameMgr gm = GameMgr.GetInstance();

		gm.AddHandler ("club_message_notify", data => {
			int id = (int)data;

			if (id == mClubID)
				updateMessageCnt();
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

	void updateMessageCnt() {
		NetMgr.GetInstance ().request_apis ("get_club_message_cnt", "club_id", mClubID, data => {
			GetClubMessageCnt ret = JsonUtility.FromJson<GetClubMessageCnt> (data.ToString ());
			if (ret.errcode != 0) {
				Debug.Log("get_club_message_cnt fail");
				return;
			}

			int cnt = ret.data.cnt;

			setActive(transform, "me/btn_mail/msg_num", cnt > 0);
			if (cnt > 0)
				setText(transform, "me/btn_mail/msg_num/tile", "" + cnt);
		});
	}

	void showClub(ClubDetailInfo club) {
		Transform me = transform.FindChild("me");
		Transform bottom = transform.FindChild("bottom");

		setActive (transform, "top/btn_edit", mAdmin);
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
		// TODO

		Transform grid = transform.FindChild("items/grid_ign");
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
		setText(grid.GetChild (3), "id", "" + club.id);
		setText(creator, "name", club.owner.name);
		setIcon(creator, "icon", club.owner.logo);

		setBtnEvent(grid.GetChild (3), "btn_share", () => {
			mShare.SetActive(true);
			mShare.GetComponent<Share>().club_id = club.id;
		});

		setText(bottom, "create_time", "创建于" + Utils.formatTime(club.create_time));
		setBtnEvent(bottom, "btn_exit", () => {
			onBtnExit(mClubID);
		});
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
}


