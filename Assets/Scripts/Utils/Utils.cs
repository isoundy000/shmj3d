
using System;
using UnityEngine;
using System.Collections.Generic;

public class Utils : MonoBehaviour {
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
}
