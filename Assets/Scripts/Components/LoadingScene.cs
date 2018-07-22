using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour {

	public static string LoadingName;

	public UIProgressBar progress;

	UILabel percent;

	AsyncOperation async;

	void Start() {
		AnysdkMgr.setPortait ();

		percent = progress.transform.Find ("percent").GetComponent<UILabel> ();

		StartCoroutine ("BeginLoading");
	}

	#if true
	void Update () {
		progress.value = async.progress;
		if (percent != null)
			percent.text = string.Format ("{0:P0}", async.progress);
	}

	IEnumerator BeginLoading() {
		async = SceneManager.LoadSceneAsync(LoadingName);

		yield return async;
	}

	public static void LoadNewScene(string name) {
		LoadingName = name;

		SceneManager.LoadScene ("99.loading");
	}

	#else

	IEnumerator BeginLoading() {
		yield return ABMgr.GetInstance().LoadLevelAsync (LoadingName);
	}

	public static void LoadNewScene(string name) {
		LoadingName = name;

		Debug.Log ("LoadNewScene: " + name);
		ABMgr.GetInstance().LoadLevel("99.loading");
	}

	#endif
}
