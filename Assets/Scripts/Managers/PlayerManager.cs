
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
		Debug.Log ("PlayerManager awake");
    }
    
	public static PlayerManager GetInstance() {
		return m_instance;
	}

	public DHM_CardManager getCardManager(int seatindex) {
		//GameObject[] mgrs = new GameObject[]{ m_EastPlayer, m_SouthPlayer, m_WestPlayer, m_NorthPlayer };

		GameObject east = GameObject.Find ("EastPlayer");
		GameObject south = GameObject.Find("SouthPlayer");
		GameObject west = GameObject.Find("WestPlayer");
		GameObject north = GameObject.Find("NorthPlayer");

		GameObject[] mgrs = new GameObject[]{ east, south, west, north };

		for (int i = 0; i < mgrs.Length; i++) {
			DHM_CardManager cm = mgrs[i].GetComponent<DHM_CardManager>();
			if (cm != null && cm.seatindex == seatindex)
				return cm;
		}

		return null;
	}
}
