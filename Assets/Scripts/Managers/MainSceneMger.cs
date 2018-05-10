/************************************************************************
*	@brief		：Main场景管理类.
*		
*	@author		：李经纬
*	@copyright	：时光科技   2017
*	
*	@date		：2017年2月28日11:00:00
************************************************************************/
using UnityEngine;
using System.Collections;
//using GameProtocol;
//using GameProtocol.Model;

public class MainSceneMger : MonoBehaviour {
    /// <summary>
    /// 游戏主场景的主题，初始化为默认主题
    /// </summary>
    public enum MainSceneType
    {
        Default = 0,
        Else = 1
    }

    public static MainSceneMger m_Instance = null;

    public Transform m_MainCamera;      //当前场景的主摄像机
    Transform Camera2D;
    //public Transform _light;
    string tableName;               //当前主题桌子模型的名字
    string ditanName;

    private GameObject remainCardCount;//剩余牌数

    private void Awake()
    {
        m_Instance = this;
    }

    // Use this for initialization
    void Start () {
		AudioManager.GetInstance().PlayBackgrounfAudio("backmusic1");
		MainSceneInit(0, RoomMgr.GetInstance().seatindex);
    }

    public void MainSceneInit(MainSceneType type = 0, int seat = 0)
    {
/*
		int angle = ((seat + 1) % 4) * 90;

		pointer.transform.rotation = Quaternion.Euler(new Vector3(-90, angle, 0));
		

        Camera2D = m_MainCamera.Find("Camera").transform;
        switch (seat)
        {
            case 0:
                //当前玩家为玩家东，初始化摄像机位置
                //m_MainCamera.position = new Vector3(0.006f,1.05f,1.206f);
                //m_MainCamera.eulerAngles = new Vector3 (43.15805f, -179.9955f, 0f );
               

                m_MainCamera.position = new Vector3(0.006f, 1.058f, 1.206f);
                m_MainCamera.rotation = Quaternion.Euler(new Vector3(42.83002f, 180f, 0));

                Camera2D.localPosition = new Vector3(0.023f, -0.253f, 0.892f);
                Camera2D.localRotation = Quaternion.Euler(new Vector3(-25.0f, 0, 0));
				_light.Rotate(0, 0, 0);
                //remainCardCount.transform.Rotate(0, 0, 0);
                Debug.Log("玩家东摄像机初始化");
                break;
            case 1:
                //当前玩家为玩家南，初始化摄像机位置
                //m_MainCamera.position = new Vector3(-1.159f, 1.26f, 0);
                //m_MainCamera.eulerAngles = new Vector3(49f, 90f, 0.3f);
               

                m_MainCamera.position = new Vector3(-1.152f, 0.987f, -0.009f);
                m_MainCamera.rotation = Quaternion.Euler(new Vector3(42.02f, 90f, 0));

                Camera2D.localPosition = new Vector3(0.0427f, -0.2194f, 0.8769f);
                Camera2D.localRotation = Quaternion.Euler(new Vector3(-25.0f, 0, 0));
                _light.Rotate(0, 90, 0);
                //remainCardCount.transform.Rotate(0, -90, 0);
                Debug.Log("玩家南摄像机初始化");
                break;
            case 2:
                //当前玩家为玩家西，初始化摄像机位置
                //m_MainCamera.position = new Vector3(0, 1.26f, -1.16f);
                //m_MainCamera.eulerAngles = new Vector3(49f, 0.2f, 0);

               

                //m_MainCamera.position = new Vector3(0, 0.994f, -1.169f);
                //m_MainCamera.rotation = Quaternion.Euler(new Vector3(42.02005f, 0.3f, 0.3600001f));
                m_MainCamera.position = new Vector3(0.003f, 1.023f, -1.162f);
                m_MainCamera.rotation = Quaternion.Euler(new Vector3(42.83001f, 0f, 0.1300032f));

                Camera2D.localPosition = new Vector3(0.015f, -0.228f, 0.896f);
                Camera2D.localRotation = Quaternion.Euler(new Vector3(-25.0f, 0, 0));
                _light.Rotate(0, 180, 0);
               // remainCardCount.transform.Rotate(0, -180, 0);
                Debug.Log("玩家西摄像机初始化");
                break;
            case 3:
                //当前玩家为玩家北，初始化摄像机位置
                //m_MainCamera.position = new Vector3(1.163f, 1.26f, 0);
                //m_MainCamera.eulerAngles = new Vector3(49f, -90, 0f);
                //
                //m_MainCamera.position = new Vector3(1.128f, 1.005f, -0.00800f);
                m_MainCamera.position = new Vector3(1.154f, 1.023f, -0.00800f);
                m_MainCamera.rotation = Quaternion.Euler(new Vector3(42.83001f, -90, 0.1300032f));

                Camera2D.localPosition = new Vector3(0.043f, -0.228f, 0.9f);
                Camera2D.localRotation = Quaternion.Euler(new Vector3(-25f, 0, 0));
                _light.Rotate(0, -90, 0);
                //remainCardCount.transform.Rotate(0, 90, 0);
                Debug.Log("玩家北摄像机初始化");
                break;
            default:
                Debug.Log("当前座位出错！");
                break;
        }
*/
/*		todo
        switch(type)
        {
            //默认主题
            case 0:
                tableName = "mahjongTable_001";
                ResourcesMgr.mInstance.InstantiateGameObject(tableName);
                ditanName = "ditan";
                GameObject obj = ResourcesMgr.mInstance.InstantiateGameObject(ditanName,ResourceType.DiTan);
                obj.transform.localScale = new Vector3(16.4f, 19.48f, 10.78249f);
                Debug.Log("场景初始化");
                break;
            default:
                Debug.Log("没有当前主题类型！");
                break;
        }
*/
    }

    public string ReturnTableName()
    {
        return tableName;
    }
}
