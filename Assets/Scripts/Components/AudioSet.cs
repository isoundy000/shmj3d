using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSet : MonoBehaviour {
	public UISlider musicGame;
	public UISlider musicBg;

	void Start() {
		AudioManager am = AudioManager.GetInstance();

		musicGame.value = am.getSFXVolume ();
		musicBg.value = am.getBGMVolume ();
	}

	public void onMusicGameChanged() {
		AudioManager am = AudioManager.GetInstance();
		am.setSFXVolume(musicGame.value);
	}

	public void onMusicBgChanged() {
		AudioManager am = AudioManager.GetInstance();
		am.setBGMVolume(musicBg.value);
	}

	public void onBtnClose() {
		gameObject.SetActive(false);
	}
}
