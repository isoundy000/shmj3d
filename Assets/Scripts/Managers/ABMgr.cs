
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AssetBundles;
//using ICSharpCode.SharpZipLib.Checksums;  
//using ICSharpCode.SharpZipLib.Zip; 

public class ABMgr : MonoBehaviour {

	//public string sceneName;
	const string HOT_UPDATE_URL = "http://ip.queda88.com:9000/hot_ios";
	Loader ldr = null;

	public static ABMgr mInstance = null;
	public static ABMgr GetInstance() { return mInstance; }

	static List<string> mAbs = new List<string>();

	void Awake() {
		mInstance = this;

		ldr = GameObject.Find("UI Root").GetComponent<Loader>();
	}

	IEnumerator Start() {
		string path = Application.persistentDataPath + "/iOS";
		AssetBundleManifest mb = null;

		#if !UNITY_EDITOR
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
		#endif

		yield return StartCoroutine(Initialize());

		#if !UNITY_EDITOR
		yield return StartCoroutine(AssetBundleManager.CheckAB(mb, mc, progress => {
			ldr.setProgress(progress);
		}));

		yield return StartCoroutine(updateManifest());
		#endif

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

	// Initialize the downloading URL.
	// eg. Development server / iOS ODR / web URL
	void InitializeSourceURL()
	{
		// If ODR is available and enabled, then use it and let Xcode handle download requests.
		#if ENABLE_IOS_ON_DEMAND_RESOURCES
		if (UnityEngine.iOS.OnDemandResources.enabled)
		{
		AssetBundleManager.SetSourceAssetBundleURL("odr://");
		return;
		}
		#endif
		#if DEVELOPMENT_BUILD || UNITY_EDITOR
		// With this code, when in-editor or using a development builds: Always use the AssetBundle Server
		// (This is very dependent on the production workflow of the project.
		//      Another approach would be to make this configurable in the standalone player.)
		AssetBundleManager.SetDevelopmentAssetBundleServer();
		//AssetBundleManager.SetSourceAssetBundleURL(HOT_UPDATE_URL);
		return;
		#else
		// Use the following code if AssetBundles are embedded in the project for example via StreamingAssets folder etc:
		//AssetBundleManager.SetSourceAssetBundleURL(Application.dataPath + "/");
		// Or customize the URL based on your deployment or configuration
		AssetBundleManager.SetSourceAssetBundleURL(HOT_UPDATE_URL);
		return;
		#endif
	}

	// Initialize the downloading url and AssetBundleManifest object.
	protected IEnumerator Initialize()
	{
		// Don't destroy the game object as we base on it to run the loading script.
		DontDestroyOnLoad(gameObject);

		InitializeSourceURL();

		// Initialize AssetBundleManifest which loads the AssetBundleManifest object.
		var request = AssetBundleManager.Initialize();

		if (request != null)
			yield return StartCoroutine(request);
	}

	protected IEnumerator InitializeLevelAsync(string levelName, bool isAdditive)
	{
		// This is simply to get the elapsed time for this phase of AssetLoading.
		float startTime = Time.realtimeSinceStartup;

		// Load level from assetBundle.
		AssetBundleLoadOperation request = AssetBundleManager.LoadLevelAsync("ab_scenes_" + levelName, levelName, isAdditive);
		if (request == null)
			yield break;

		yield return StartCoroutine(request);

		// Calculate and display the elapsed time.
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

/*
	static void UnzipFile(ZipEntry zip, ZipInputStream zipInStream, string dirPath)  
	{  
		try  
		{  
			//文件名不为空    
			if (!string.IsNullOrEmpty(zip.Name))  
			{  
				string filePath = dirPath;  
				filePath += ("/" + zip.Name);  


				//如果是一个新的文件路径　这里需要创建这个文件路径    
				if (IsDirectory(filePath))  
				{  
					Debug.Log("Create  file paht " + filePath);  
					if (!Directory.Exists(filePath))  
					{  
						Directory.CreateDirectory(filePath);  
					}  
				}  
				else  
				{  
					FileStream fs = null;  
					//当前文件夹下有该文件  删掉  重新创建    
					if (File.Exists(filePath))  
					{  
						File.Delete(filePath);  
					}  
					fs = File.Create(filePath);  
					int size = 2048;  
					byte[] data = new byte[2048];  
					//每次读取2MB  直到把这个内容读完    
					while (true)  
					{  
						size = zipInStream.Read(data, 0, data.Length);  
						//小于0， 也就读完了当前的流    
						if (size > 0)  
						{  
							fs.Write(data, 0, size);  
						}  
						else  
						{  
							break;  
						}  
					}  
					fs.Close();  
				}  
			}  
		}  
		catch (Exception e)  
		{  
			throw new Exception();  
		}  
	}  

	static bool IsDirectory(string path)  
	{  
		if (path[path.Length - 1] == '/')
			return true;

		return false;  
	}  

	public IEnumerator Unzip(string file, string outpath)
	{  
		int doneCount = 0;
	 	int indicatorStep = 1;

		Debug.Log("zip file is:" + file);  
		Debug.Log("outpath is:" + outpath);  

		ZipEntry zip = null;  
		ZipInputStream zipInStream = null;  
		zipInStream = new ZipInputStream(File.OpenRead(file));

		while ((zip = zipInStream.GetNextEntry()) != null)  
		{  
			Debug.Log("name is:" + zip.Name + " zipStream " + zipInStream);  
			UnzipFile(zip, zipInStream, outpath);
			doneCount++;
			if (doneCount % indicatorStep == 0)  
				yield return new WaitForEndOfFrame();  
		}

		try  
		{  
			zipInStream.Close();  
		}  
		catch (Exception ex)  
		{  
			Debug.Log("UnZip Error");  
			throw ex;  
		}
	}
*/
}
