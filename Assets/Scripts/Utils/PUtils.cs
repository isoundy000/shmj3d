
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;


public class PUtils : MonoBehaviour {
	public static PUtils _instance = null;
	public static PUtils GetInstance() { return Instance; }
	string path;

	public static PUtils Instance {
		get {
			if (_instance == null) {
				GameObject obj = new GameObject("PUtils");
				_instance = obj.AddComponent<PUtils>();
				DontDestroyOnLoad(obj);
			}

			return _instance;
		}
	}

	public static Transform getChild(Transform item, string child) {
		if (item == null)
			return null;

		return item.Find (child);
	}
		
	public static void setText(Transform item, string child, string text) {
		Transform lbl = getChild (item, child);
		if (lbl != null)
			lbl.GetComponent<UILabel>().text = text;
	}

	public static void setIcon(Transform item, string child, int uid) {
		Transform icon = getChild (item, child);
		if (icon != null)
			icon.GetComponent<IconLoader>().setUserID (uid);
	}

	public static void setIcon(Transform item, string child, string url) {
		Transform icon = getChild (item, child);
		UITexture texture = icon.GetComponent<UITexture>();

		if (url == null || url.Length == 0) {
			texture.mainTexture = null;
			return;
		}

		if (icon != null)
			ImageLoader.GetInstance().LoadImage(url, icon.GetComponent<UITexture>());
	}

	public static void setBtnEvent(Transform item, string child, Action cb) {
		Transform btn = child == null ? item : getChild (item, child);
		if (btn != null)
			onClick(btn, cb);
	}

	public static void setBtnInteractable(Transform item, string child, bool enable) {
		Transform tm = child == null ? item : getChild (item, child);
		if (tm != null) {
			var btn = tm.GetComponent<UIButton>();

			if (btn != null)
				btn.enabled = enable;
		}
	}

	public static void setActive(Transform item, string child, bool enable) {
		Transform ob = child == null ? item : getChild (item, child);
		if (ob != null)
			ob.gameObject.SetActive(enable);
	}

	public static void setInput(Transform item, string child, string text) {
		Transform input = getChild (item, child);
		if (input != null) {
			UIInput ob = input.GetComponent<UIInput>();
			ob.Start();
			ob.Set(text);
		}
	}

	public static string getInput(Transform item, string child) {
		Transform input = getChild (item, child);
		if (input != null)
			return input.GetComponent<UIInput>().value;

		return "";
	}

	public static T getPage<T>(string page) {
		return GameObject.Find(page).GetComponent<T>();
	}

	public static void setToggle(Transform item, string child, bool value) {
		Transform ob = child == null ? item : getChild (item, child);
		if (ob != null) {
			UIToggle tg = ob.GetComponent<UIToggle>();
			tg.value = value;
		}
	}

	public static void setToggleEvent(Transform item, string child, Action<bool> cb) {
		Transform ob = child == null ? item : getChild (item, child);

		if (ob != null) {
			UIToggle tg = ob.GetComponent<UIToggle>();
			List<EventDelegate> onChange = tg.onChange;

			onChange.Clear();

			onChange.Add (new EventDelegate (() => {
				if (cb != null) cb.Invoke(tg.value);

				if (tg.group == 0 || tg.value)
					AudioManager.PlayButtonClicked();
			}));
		}
	}

	public static void setSliderEvent(Transform item, string child, Action<float> cb) {
		Transform ob = child == null ? item : getChild (item, child);

		if (ob != null) {
			UISlider slider = ob.GetComponent<UISlider>();
			var onChange = slider.onChange;

			onChange.Clear();

			if (cb != null) {
				onChange.Add(new EventDelegate(() => {
					cb.Invoke(slider.value);
				}));
			}
		}
	}

	public static void onClick(Transform ob, Action cb) {
		if (ob == null)
			return;

		List<EventDelegate> onclick = ob.GetComponent<UIButton>().onClick;

		onclick.Clear();
		onclick.Add(new EventDelegate(()=>{
			cb.Invoke();

			AudioManager.PlayButtonClicked();
		}));
	}


	public static void onClick(GameObject ob, Action cb) {
		onClick(ob.transform, cb);
	}

	public static string formatTime(int secs, string format = "yyyy/MM/dd HH:mm:ss") {
		DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime (new DateTime (1970, 1, 1));
		DateTime dt = startTime.AddSeconds(secs);

		return dt.ToString(format);
	}

	public static int getSeconds() {
		return (int)((DateTime.Now.Ticks - DateTime.Parse("1970-01-01").Ticks) / 10000000);
	}

	public static long getMilliSeconds() {
		return (long)((DateTime.Now.Ticks - DateTime.Parse("1970-01-01").Ticks) / 10000);
	}

	public static Dictionary<string, string> parseQuery(string query) {
		Dictionary<string, string> ret = new Dictionary<string, string> ();

		if (query.Length == 0)
			return ret;

		string[] arr =  query.Split ('&');

		foreach (string x in arr) {
			int off = x.IndexOf ('=');

			if (off > 0)
				ret.Add (x.Substring(0, off), x.Substring(off + 1));
		}

		return ret;
	}

	void _setTimeout(Action cb, float seconds) {
		StartCoroutine(timeoutCB(cb, seconds));
	}

	IEnumerator timeoutCB(Action cb, float seconds) {
		yield return new WaitForSeconds(seconds);

		cb();
	}

	public static void setTimeout(Action cb, float seconds) {
		PUtils ut = GetInstance();

		ut._setTimeout(cb, seconds);
	}

	public static IEnumerator LoadAsset(Transform parent, string name) {
		string asset = "ab_ui_prefabs_" + name.ToLower();

		var request = AssetBundleManager.LoadAssetAsync(asset, name, typeof(GameObject));
		if (request == null)
			yield break;

		yield return GetInstance().StartCoroutine(request);

		GameObject prefab = request.GetAsset<GameObject>();

		if (prefab != null) {
			var ob = GameObject.Instantiate(prefab, parent) as GameObject;
			ob.name = name;
		}

		yield break;
	}

	public static void layout(Transform tm, string child) {
		Transform bg = tm.Find(child);

		if (bg != null) {
			UISprite sp = bg.GetComponent<UISprite> ();
			Transform root = GameObject.Find("UI Root").transform;
			sp.topAnchor.Set(root, 1, 0);
			sp.bottomAnchor.Set(root, 0, 0);
		}
	}

	public static void EnterPage(string page, int club_id) {
		GameObject ob = GameObject.Find (page);

		if (ob == null)
			return;

		if (page == "PClubDetail")
			ob.GetComponent<ClubDetail> ().enter (club_id, true);
		else if (page == "PSetMember")
			ob.GetComponent<SetMember> ().enter (club_id);
		else if (page == "PClubMessage")
			ob.GetComponent<ClubMessage> ().enter (club_id);
		else if (page == "PClubHistory")
			ob.GetComponent<ClubHistory> ().enter (club_id);
		else if (page == "PCreateRoom")
			ob.GetComponent<CreateRoom> ().enter (club_id);
	}

	public static void EnterEditRoom(ClubRoomInfo room, Action cb) {
		GameObject ob = GameObject.Find ("PEditRoom");

		EditRoom er = ob.GetComponent<EditRoom> ();
		if (er != null) {
			er.UpdateEvents += cb;
			er.enter (room);
		}
	}

	public static void activeChildren(Transform parent, bool active = true) {
		for (int i = 0; i < parent.childCount; i++)
			parent.GetChild (i).gameObject.SetActive (active);
	}

	public static void activeChildren(GameObject parent, bool active = true) {
		activeChildren (parent.transform, active);
	}

	public static string subString(string str, int max) {
		if (str.Length >= max)
			return str.Substring (0, max);
		else
			return str;
	}
}



