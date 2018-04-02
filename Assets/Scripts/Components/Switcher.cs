
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switcher : MonoBehaviour {

	public string prefix;
	public int defaultAngle = 0;

	List<Texture> pointers;

	void Start () {
		pointers = new List<Texture>();

		string[] mats = new string[]{ prefix + "_east", prefix + "_south", prefix + "_west", prefix + "_north", prefix };

		foreach (string mat in mats)
			pointers.Add(Resources.Load ("Materials/" + mat) as Texture);

		RoomMgr rm = RoomMgr.GetInstance();
		int local = rm.getLocalIndex (0);
		int angle = (((5 - local) % 4) * 90 + defaultAngle) % 360;

		transform.rotation = Quaternion.Euler(-90, angle, 0);
	}

	public void switchTo(int seat) {
		Debug.Log ("SwitchTo " + seat);

		int id = 4;

		if (seat < 4) {
			RoomMgr rm = RoomMgr.GetInstance ();
			int locale = rm.getLocalIndex (0);
			int local = rm.getLocalIndex (seat);
			id = (4 + local - locale) % 4;
		}

		GetComponent<Renderer>().materials[0].mainTexture = pointers[id];
	}
}



