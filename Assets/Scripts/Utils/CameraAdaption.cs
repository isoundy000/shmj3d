
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAdaption : MonoBehaviour {

	void Start () 
	{
#if false 
		int ManualWidth = 1920;
		int ManualHeight = 1080;
		int manualHeight;

		if (Convert.ToSingle(Screen.height) / Screen.width > Convert.ToSingle(ManualHeight) / ManualWidth)
		{
			manualHeight = Mathf.RoundToInt(Convert.ToSingle(ManualWidth) / Screen.width * Screen.height);
		}
		else
		{
			manualHeight = ManualHeight;
		}

		Camera camera = GetComponent<Camera>();       
		float scale = Convert.ToSingle(manualHeight*1.0f / ManualHeight);   
		camera.fieldOfView *= scale;
#else
		float aspect = Screen.width * 1.0f / Screen.height;
		Camera camera = GetComponent<Camera>();       

		camera.orthographicSize = 1920 / aspect / 200;
#endif
	}
}
