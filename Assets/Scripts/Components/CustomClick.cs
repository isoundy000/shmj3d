

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomClick : MonoBehaviour
{
	public string audio;
	public GameObject particle;

	void playVoice() {
		AudioManager.GetInstance ().PlayEffectAudio(audio);
	}

	void Update ()
	{
		if (Input.GetMouseButtonDown (0)) {

			Vector3 pos = UICamera.currentCamera.ScreenToWorldPoint (new Vector3 (Input.mousePosition.x, Input.mousePosition.y, -1));
			GameObject particle = Instantiate (Resources.Load ("Prefab/UI/ParticleClick") as GameObject);
			Transform tm = particle.transform;
		
			tm.SetParent (transform);
			tm.position = pos;
			tm.localScale = Vector3.one;

			playVoice();
		}
	}
}
