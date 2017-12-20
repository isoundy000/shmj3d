using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour {

	public static string LoadingName;
	public UISlider slider;
	public UILabel label;
	AsyncOperation async;

	void Start() {
		StartCoroutine ("BeginLoading");
	}

	void Update () {
		slider.value = async.progress;
		label.text = (slider.value * 100).ToString(".00") + "%";
	}

	IEnumerator BeginLoading() {
		async = SceneManager.LoadSceneAsync (LoadingName);

		yield return async;
	}

	public static void LoadNewScene(string name) {
		LoadingName = name;

		SceneManager.LoadScene ("99.loading");
	}
}
