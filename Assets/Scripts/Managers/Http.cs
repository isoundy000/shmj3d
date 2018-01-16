
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJson;

public class Http : MonoBehaviour {
	static string URL = "http://ip.queda88.com:9001";

	static Http mInstance = null;

	void Awake() {
		mInstance = this;
	}

	public static Http GetInstance() {
		return mInstance;
	}

	public void Get(string path, Dictionary<string, object> args, Action<JsonObject> cb, Action<string> err, string extraUrl = null) {
		string parameters = "";

		if (extraUrl == null)
			extraUrl = URL;

		if (args.Count > 0) {
			bool first = true;

			parameters += "?";
			foreach (KeyValuePair<string, object> arg in args) {
				if (first)
					first = false;
				else
					parameters += "&";

				parameters += arg.Key + "=" + arg.Value.ToString();
			}
		}

		string url = extraUrl + path + parameters;
		Debug.Log ("http get: " + url);
		StartCoroutine (doGet(url, cb, err));
	}

	IEnumerator doGet(string url, Action<JsonObject> cb, Action<string> err) {
		WWW www = new WWW (url);
		yield return www;

		if (www.error != null) {
			err.Invoke(www.error);
		} else {
			JsonObject ob = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(www.text);
			cb.Invoke(ob);
		}
	}

	public void Post(string path, Dictionary<string, object> args, Action<JsonObject> cb, Action<string> err, string extraUrl = null) {
		if (extraUrl == null)
			extraUrl = URL;

		WWWForm form = new WWWForm ();

		foreach (KeyValuePair<string, object> arg in args) {
			form.AddField(arg.Key, arg.Value.ToString());
		}

		string url = extraUrl + path;
		Debug.Log ("http post: " + url);
		StartCoroutine (doPost(url, form, cb, err));
	}

	IEnumerator doPost(string url, WWWForm form, Action<JsonObject> cb, Action<string> err) {
		WWW www = new WWW (url, form);
		yield return www;

		if (www.error != null) {
			err.Invoke(www.error);
		} else {
			JsonObject ob = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(www.text);
			cb.Invoke(ob);
		}
	}
}
