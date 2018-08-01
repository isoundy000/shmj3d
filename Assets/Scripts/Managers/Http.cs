
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJson;

public class Http : MonoBehaviour {
	static string URL = "http://ip2.queda88.com:9001";

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

		www.Dispose ();
	}

	public void Post(string path, JsonObject args, Action<string> success, Action<string> failure, bool useToken = true, float timeout = 10f, string extraUrl = null) {
		if (extraUrl == null)
			extraUrl = URL;

		string url = extraUrl + path;

		if (args == null)
			args = new JsonObject ();

		var token = NetMgr.GetInstance ().getToken ();

		Debug.Log ("token: " + token);

		if (useToken)
			args.Add("token", token);

		byte[] bytes = System.Text.Encoding.Default.GetBytes (args.ToString());

		Debug.Log ("bytes: " + System.Text.Encoding.Default.GetString(bytes));

		StartCoroutine (doPost(url, bytes, success, failure, timeout));
	}

	IEnumerator doPost(string url, byte[] bytes, Action<string> success, Action<string> failure, float timeout = 10f) {

		var header = new Dictionary<string, string> ();
		header.Add("Content-Type", "application/json");

		float tm = 0;
		bool failed = false;
		WWW www = new WWW (url, bytes, header);
		yield return www;

		while (!www.isDone) {
			if (tm < timeout) {
				yield return new WaitForFixedUpdate ();
				tm += Time.unscaledDeltaTime;
			} else {
				failed = true;
				break;
			}
		}

		if (failed) {
			failure ("连接超时");
		} else {
			UnityEngine.Debug.Log(www.text);
			if (success != null) {
				try {
					success(www.text);
				} catch (Exception ex) {
					UnityEngine.Debug.LogException(ex);
				}
			}
		}

		www.Dispose ();
	}

	public void POST(string url, Action<string, Dictionary<string, string>> success, Action<string> failure, Dictionary<string, string> param = null, bool useToken = true, float timeout = 10f)
	{
		StartCoroutine(_POST(url, success, failure, param, useToken, timeout));
	}

	private IEnumerator _POST(string url, Action<string, Dictionary<string, string>> success, Action<string> failure, Dictionary<string, string> param = null, bool useToken = true, float timeout = 10f)
	{
		var form = new WWWForm ();

		if (param != null) {
			foreach (var p in param)
				form.AddField (p.Key, p.Value);
		}

		var headers = form.headers;

		if (useToken) {
			var token = NetMgr.GetInstance ().getToken ();
			if (!string.IsNullOrEmpty (token)) {
				headers ["token"] = token;
				form.AddField ("token", token);
			}
		}

		if (url.StartsWith ("/"))
			url = URL + url;

		float tm = 0;
		bool failed = false;
		var req = new WWW (url, form.data, headers);

		Debug.Log ("data: " + form.data.ToString());

		while (!req.isDone) {
			if (tm < timeout) {
				yield return new WaitForFixedUpdate ();
				tm += Time.unscaledDeltaTime;
			} else {
				failed = true;
				break;
			}
		}

		if (failed) {
			failure ("连接超时");
		} else {
			UnityEngine.Debug.Log(req.text);
			if (success != null) {
				try {
					success(req.text, req.responseHeaders);
				} catch (Exception ex) {
					UnityEngine.Debug.LogException(ex);
/*
					if (onError != null)
						onError(req.text);
*/
				}
			}
		}

		req.Dispose ();
	}
}
