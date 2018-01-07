
using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class UserInfo {
	public int code;
	public string name;
	public string headimgurl;
	public int sex;
	public string room_tag;
}

public class UserInfoMgr : MonoBehaviour {
	public static UserInfoMgr _instance = null;
	public static UserInfoMgr GetInstance() { return Instance; }

	Dictionary<int, UserInfo> infoMap;

	public static UserInfoMgr Instance {
		get {
			if (_instance == null) {
				GameObject obj = new GameObject("UserInfoMgr");
				_instance = obj.AddComponent<UserInfoMgr>();
				DontDestroyOnLoad(obj);
			}

			return _instance;
		}
	}

	void Awake() {
		infoMap = new Dictionary<int, UserInfo> ();
	}

	public void getBaseInfo(int uid, Action<UserInfo> cb, bool force = false) {
		if (infoMap.ContainsKey (uid) && !force) {
			cb (infoMap [uid]);
			return;
		}

		NetMgr nm = NetMgr.GetInstance ();

		nm.request_apis ("query_base_info", "uid", uid, ret => {
			UserInfo info = JsonUtility.FromJson<UserInfo>(ret.ToString());

			if (info.code != 0) {
				cb(null);
				return;
			}

			infoMap[uid] = info;
			cb(info);
		});
	}
}
