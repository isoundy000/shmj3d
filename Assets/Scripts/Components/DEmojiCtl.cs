
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEmojiCtl : MonoBehaviour {

	public string file = null;

	public void playAudio() {
		if (file != null && file.Length > 0)
		AudioManager.GetInstance().PlayEffectAudio(file);
	}
}


