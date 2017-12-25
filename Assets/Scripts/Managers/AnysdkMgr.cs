using UnityEngine;
using System.Collections;

public class AnysdkMgr {

	public static void setPortait() {
		Screen.SetResolution (1080, 1920, false);
	}

	public static void setLandscape() {
		Screen.SetResolution (1920, 1080, false);
	}
}
