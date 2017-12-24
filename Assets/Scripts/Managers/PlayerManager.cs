
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
		switch (seatindex) {
		case 0:
			return m_EastPlayer;
		case 1:
			return m_SouthPlayer;
		case 2:
			return m_WestPlayer;
		case 3:
			return m_NorthPlayer;
		default:
			return null;
		}
	}
}
