
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Utils : MonoBehaviour {
	public static Utils _instance = null;
	public static Utils GetInstance() { return Instance; }
	string path;

	public static Utils Instance {
		get {
			if (_instance == null) {
				GameObject obj = new GameObject("Utils");
				_instance = obj.AddComponent<Utils>();
				DontDestroyOnLoad(obj);
			}

			return _instance;
		}
	}

	public static void onClick(Transform ob, Action cb) {
		if (ob == null)
			return;

		List<EventDelegate> onclick = ob.GetComponent<UIButton>().onClick;

		onclick.Clear();
		onclick.Add(new EventDelegate(()=>{
			cb.Invoke();
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
		Utils ut = GetInstance();

		ut._setTimeout(cb, seconds);
	}
}
