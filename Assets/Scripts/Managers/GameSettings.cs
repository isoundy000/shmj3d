
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings.asset")]
public class GameSettings : ScriptableObject
{
	public static string NAME = "GameSettings";

	public static string PATH = "Assets/Resources/";

	public static string EXT = ".asset";

	public bool DEBUG;

	public bool development;

	public bool forceLoadFromWWW = true;

	public bool loadConfig;

	public string version;

	public string platform;

	public string configUrl = string.Empty;

	public string slotUrl = string.Empty;

	public string wsUrl = string.Empty;

	public string cdnUrl = string.Empty;

	public string appUrl = string.Empty;

	public string loginUrl = string.Empty;

	public string des_key = string.Empty;

	public string md5_key = string.Empty;

	public string username = string.Empty;

	public string token = string.Empty;

	public string nextScene = string.Empty;

	public string appid = string.Empty;

	public string appname = string.Empty;

	public string gateUrl = string.Empty;
	public int gatePort = 0;

	public string httpUrl = string.Empty;

	static GameSettings _instance;

	public static GameSettings Instance {
		get {
			if (GameSettings._instance == null)
				GameSettings._instance = Resources.Load<GameSettings>(GameSettings.NAME);

			return GameSettings._instance;
		}
	}

	public void OnValidate()
	{
		if (Application.isEditor)
		{
/*
			PlayerPrefs.SetString(AAGameConstant.SLOT_URL, GameSettings.Instance.slotUrl);
			PlayerPrefs.SetString(AAGameConstant.WS_URL, GameSettings.Instance.wsUrl);
			PlayerPrefs.SetString(AAGameConstant.CDN_URL, GameSettings.Instance.cdnUrl);
			PlayerPrefs.SetString(AAGameConstant.LOGIN_URL, GameSettings.Instance.loginUrl);
			PlayerPrefs.SetString(AAGameConstant.APP_URL, GameSettings.Instance.appUrl);
			PlayerPrefs.SetString(AAGameConstant.PLATFORM, GameSettings.Instance.platform);
			if (!string.IsNullOrEmpty(GameSettings.Instance.username))
			{
				PlayerPrefs.SetString(AAGameConstant.USER_NAME, GameSettings.Instance.username);
			}
			else
			{
				PlayerPrefs.DeleteKey(AAGameConstant.USER_NAME);
			}
			if (!string.IsNullOrEmpty(AAGameConstant.TOKEN))
			{
				PlayerPrefs.SetString(AAGameConstant.TOKEN, GameSettings.Instance.token);
			}
			else
			{
				PlayerPrefs.DeleteKey(AAGameConstant.TOKEN);
			}
*/
		}
	}
}
