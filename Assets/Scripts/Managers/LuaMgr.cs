
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using AssetBundles;

[Hotfix]
public class LuaMgr : MonoBehaviour {

	public static LuaMgr mInstance = null;
	public static LuaMgr GetInstance() { return mInstance; }

	public static LuaEnv luaenv = null;

	float lastGCTime = 0;
	const float GCInterval = 1;

	void Awake() {
		mInstance = this;

		luaenv = new LuaEnv();
	}

	void Start () {
		//yield return StartCoroutine(hotfix("the_bugs.lua"));
	}

	IEnumerator hotfix(string file) {
		string name = "ab_hotfix_" + file;

		var request = AssetBundleManager.LoadAssetAsync(name, file, typeof(TextAsset));
		if (request == null)
			yield break;

		yield return StartCoroutine(request);

		var script = request.GetAsset<TextAsset>().text;

		luaenv.DoString(script);
	}
		
	void Update () {
		var now = Time.time;
		if (now - lastGCTime > GCInterval) {
			if (luaenv != null)
				luaenv.Tick();
			
			lastGCTime = now;
		}
	}

	void OnDestroy() {
		luaenv = null;
	}

	public static void loadLua(string file, Action<string> cb) {
		mInstance.StartCoroutine(mInstance._loadLua(file, cb));
	}

	IEnumerator _loadLua(string file, Action<string> cb) {
		file = (file + ".lua").ToLower();
		file = file.Replace("(clone)", "");

		bool ab = ABMgr.luaExist(file);

		string name = "ab_lua_" + file;
		string ret = "";

		if (ab) {
			var request = AssetBundleManager.LoadAssetAsync (name, file, typeof(TextAsset));
			if (request == null) {
				TextAsset asset = Resources.Load ("Lua/" + file) as TextAsset;
				if (asset != null) {
					cb (asset.text);
					yield break;
				}

				cb (ret);
				yield break;
			}

			yield return StartCoroutine (request);

			TextAsset ta = request.GetAsset<TextAsset> ();

			if (ta != null)
				ret = ta.text;
		} else {
			TextAsset asset = Resources.Load("Lua/" + file) as TextAsset;
			if (asset != null) {
				cb (asset.text);
				yield break;
			}
		}

		cb (ret);
		yield break;
	}
}


