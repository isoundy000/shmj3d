using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSet : ListBase {
	public UISlider musicGame;
	public UISlider musicBg;

	void Start() {
		AudioManager am = AudioManager.GetInstance();

		musicGame.value = am.getSFXVolume ();
		musicBg.value = am.getBGMVolume ();

		setSliderEvent (musicGame.transform, null, value => {
			am.setSFXVolume(value);
		});

		setSliderEvent (musicBg.transform, null, value => {
			am.setBGMVolume(value);
		});
	}
/*
	public void onMusicGameChanged() {
		AudioManager am = AudioManager.GetInstance();
		am.setSFXVolume(musicGame.value);
	}

	public void onMusicBgChanged() {
		AudioManager am = AudioManager.GetInstance();
		am.setBGMVolume(musicBg.value);
	}
*/
	public void onBtnClose() {
		gameObject.SetActive(false);
	}
}
