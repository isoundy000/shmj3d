
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AssetBundles;

public class ABMgr : MonoBehaviour {

	//public string sceneName
	const string HOT_UPDATE_URL = "http://ip2.queda88.com:9000/hotupdate";

	Loader ldr = null;

	public static ABMgr mInstance = null;
	public static ABMgr GetInstance() { return mInstance; }

	static List<string> mAbs = new List<string>();

	void Awake() {
		mInstance = this;

		ldr = GameObject.Find("UI Root").GetComponent<Loader>();
	}

	IEnumerator Start() {
		string path = Application.persistentDataPath + "/hot";
		AssetBundleManifest mb = null;

		//#if !UNITY_EDITOR
		if (File.Exists(path)) {
			AssetBundleCreateRequest bundle = AssetBundle.LoadFromFileAsync (path);
			yield return bundle;

			if (bundle != null && bundle.assetBundle != null) {
				mb = bundle.assetBundle.LoadAsset ("AssetBundleManifest") as AssetBundleManifest;
				bundle.assetBundle.Unload (false);
			}
		}

		string spath = Application.streamingAssetsPath + "/iOS/iOS";
		AssetBundleManifest mc = null;

		if (File.Exists (spath)) {
			AssetBundleCreateRequest bundle = AssetBundle.LoadFromFileAsync (spath);
			yield return bundle;

			if (bundle != null && bundle.assetBundle != null) {
				mc = bundle.assetBundle.LoadAsset ("AssetBundleManifest") as AssetBundleManifest;
				bundle.assetBundle.Unload (false);
			}
		}
		//#endif

		yield return StartCoroutine(Initialize());

		//#if !UNITY_EDITOR
		yield return StartCoroutine(AssetBundleManager.CheckAB(mb, mc, progress => {
			ldr.setProgress(progress);
		}));

		yield return StartCoroutine(updateManifest());
		//#endif

		LoadingScene.LoadNewScene ("01.login");
	}

	IEnumerator updateManifest() {
		WWW bd = new WWW (HOT_UPDATE_URL + "/iOS/iOS");

		yield return bd;

		if (bd.bytes == null)
			yield break;

		var path = Application.persistentDataPath + "/iOS";

		if (File.Exists (path))
			File.Delete (path);

		File.WriteAllBytes (path, bd.bytes);

		bd.Dispose();
		yield break;
	}

	public static void saveABs(string[] abs) {
		mAbs = new List<string> (abs);
	}

	public static bool luaExist(string name) {
		#if !UNITY_EDITOR
		return mAbs.Contains(name);
		#else
		return false;
		#endif
	}

	void InitializeSourceURL()
	{
		#if ENABLE_IOS_ON_DEMAND_RESOURCES
		if (UnityEngine.iOS.OnDemandResources.enabled)
		{
			AssetBundleManager.SetSourceAssetBundleURL("odr://");
			return;
		}
		#endif
		#if DEVELOPMENT_BUILD || UNITY_EDITOR
		//AssetBundleManager.SetDevelopmentAssetBundleServer();
		AssetBundleManager.SetSourceAssetBundleURL(HOT_UPDATE_URL);
		return;
		#else
		//AssetBundleManager.SetSourceAssetBundleURL(Application.dataPath + "/");
		AssetBundleManager.SetSourceAssetBundleURL(HOT_UPDATE_URL);
		return;
		#endif
	}

	protected IEnumerator Initialize()
	{
		DontDestroyOnLoad(gameObject);

		InitializeSourceURL();

		var request = AssetBundleManager.Initialize();

		if (request != null)
			yield return StartCoroutine(request);
	}

	protected IEnumerator InitializeLevelAsync(string levelName, bool isAdditive)
	{
		float startTime = Time.realtimeSinceStartup;

		AssetBundleLoadOperation request = AssetBundleManager.LoadLevelAsync("ab_scenes_" + levelName, levelName, isAdditive);
		if (request == null)
			yield break;

		yield return StartCoroutine(request);

		float elapsedTime = Time.realtimeSinceStartup - startTime;
		Debug.Log("Finished loading scene " + levelName + " in " + elapsedTime + " seconds");
	}

	public IEnumerator LoadLevelAsync(string name) {
		Debug.Log ("start load: " + name);

		AssetBundleLoadOperation request = AssetBundleManager.LoadLevelAsync("ab_scenes_" + name, name, false);
		if (request == null)
			yield break;

		yield return StartCoroutine (request);
	}

	public void LoadLevel(string name) {
		StartCoroutine (LoadLevelAsync(name));
	}
}
