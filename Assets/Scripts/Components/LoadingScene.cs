using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour {

	public static string LoadingName;

	public UITexture progress;

	AsyncOperation async;

	void Start() {
		AnysdkMgr.setPortait ();
		StartCoroutine ("BeginLoading");
	}
	#if true
	void Update () {
		progress.fillAmount = async.progress;
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
