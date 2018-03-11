
using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {
    [Header("东家管理")]
    public GameObject m_EastPlayer = null;
    [Header("南家管理")]
	public GameObject m_SouthPlayer = null;
    [Header("西家管理")]
	public GameObject m_WestPlayer = null;
    [Header("北家管理")]
	public GameObject m_NorthPlayer = null;

    public static PlayerManager m_instance = null;
    
	void Awake() {
        m_instance = this;
    }
    
	public static PlayerManager GetInstance() {
		return m_instance;
	}

	public DHM_CardManager getCardManager(int seatindex) {
		string[] mgrs = new string[]{ "EastPlayer", "SouthPlayer", "WestPlayer", "NorthPlayer" };
/*
		for (int i = 0; i < mgrs.Length; i++) {
			DHM_CardManager cm = mgrs[i].GetComponent<DHM_CardManager>();
			if (cm != null && cm.seatindex == seatindex)
				return cm;
		}

		return null;
*/
		RoomMgr rm = RoomMgr.GetInstance();
		int local = rm.getLocalIndex(seatindex);

		return GameObject.Find(mgrs[local]).GetComponent<DHM_CardManager>();
	}

	public DHM_CardManager[] getCardManagers() {
		RoomMgr rm = RoomMgr.GetInstance();

		string[] mgrs = new string[]{ "EastPlayer", "SouthPlayer", "WestPlayer", "NorthPlayer" };
		int nseats = rm.info.numofseats;

		DHM_CardManager[] cms = new DHM_CardManager[nseats];

		for (int i = 0; i < nseats; i++)
			cms[i] = GameObject.Find(mgrs[rm.getLocalIndex(i)]).GetComponent<DHM_CardManager>();

		return cms;
	}

	public DHM_CardManager getSelfCardManager() {
		GameObject east = GameObject.Find ("EastPlayer");

		return east.GetComponent<DHM_CardManager>();
	}
}
