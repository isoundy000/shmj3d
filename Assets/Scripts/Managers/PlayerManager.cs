
using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {
    [Header("东家管理")]
    public DHM_CardManager m_EastPlayer = null;
    [Header("南家管理")]
    public DHM_CardManager m_SouthPlayer = null;
    [Header("西家管理")]
    public DHM_CardManager m_WestPlayer = null;
    [Header("北家管理")]
    public DHM_CardManager m_NorthPlayer = null;
    public static PlayerManager m_instance = null;
    
	void Awake() {
        m_instance = this;
    }
    
	public static PlayerManager GetInstance() {
		return m_instance;
	}

	public DHM_CardManager getCardManager(int seatindex) {
		DHM_CardManager[] mgrs = new DHM_CardManager[]{ m_EastPlayer, m_SouthPlayer, m_WestPlayer, m_NorthPlayer };

		for (int i = 0; i < mgrs.Length; i++) {
			if (mgrs [i].seatindex == seatindex)
				return mgrs [i];
		}

		return null;
	}
}
