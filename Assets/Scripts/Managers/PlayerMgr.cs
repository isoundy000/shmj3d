
using UnityEngine;
using System.Collections;

public class UserModel {
	public int UserID;
	public string UserName;
	public int UserCoin;
	public int UserDiamond;
	public int UserIconNum;
};

public class PlayerMgr : MonoBehaviour {

    public static PlayerMgr mInstance = null;

    #region 玩家数据接口
    [System.NonSerialized]
    private int m_UserID;              //用户ID

    [System.NonSerialized]
    private int m_PlayerCoin;          //金币

    [System.NonSerialized]
    private int m_PlayerDiamond;       //钻石

    [System.NonSerialized]
    private string m_PlayerName;       //用户名

    [System.NonSerialized]
    private int m_IconNum;             //玩家头像ID

    #endregion


    #region 数据操作接口
    public int UserID
    {
        get { return m_UserID; }
        set { m_UserID = value; }
    }

    public string PlayerName
    {
        get { return m_PlayerName; }
        set { m_PlayerName = value; }
    }

    public int PlayerCoin
    {
        get { return m_PlayerCoin; }
        set { m_PlayerCoin = value; }
    }

    public int PlayerDiamond
    {
        get { return m_PlayerDiamond; }
        set { m_PlayerDiamond = value; }
    }

    public int IconNum
    {
        get { return m_IconNum; }
        set { m_IconNum = value; }
    }

    public void InitPlayerInfo(UserModel userModel)
    {
        if (userModel.UserID == -1)
        {
            Debug.Log("ID不存在");
        }
        UserID = userModel.UserID;
        PlayerName = userModel.UserName;
        PlayerCoin = userModel.UserCoin;
        PlayerDiamond = userModel.UserDiamond;
        IconNum = userModel.UserIconNum;

		// todo
        //CreateRoomViewMgr.m_Instance._Init();
    }
    #endregion

    public static PlayerMgr GetInstance() {
        return mInstance;
    }

    private void Awake() {
        mInstance = this;
    }

    public void ResetInfo()
    {

    }
}

