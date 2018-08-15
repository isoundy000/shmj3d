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

		var chupai = transform.Find ("chupai");
		if (chupai != null) {
			var single = chupai.Find ("single").GetComponent<UIToggle>();
			var db = chupai.Find ("double").GetComponent<UIToggle>();

			if (!PlayerPrefs.HasKey ("chupai_method"))
				PlayerPrefs.SetInt("chupai_method", 0);
			
			int method = PlayerPrefs.GetInt ("chupai_method");

			single.value = method == 0;
			db.value = method == 1;

			PUtils.setToggleEvent (chupai, "single", onSetChupai);
			PUtils.setToggleEvent (chupai, "double", onSetChupai);
		}
	}

	public void onBtnClose() {
		gameObject.SetActive(false);
	}

	void onSetChupai(bool val) {
		var chupai = transform.Find ("chupai");

		if (chupai != null) {
			var single = chupai.Find ("single").GetComponent<UIToggle>();

			PlayerPrefs.SetInt("chupai_method", single.value ? 0 : 1);
		}
	}
}
