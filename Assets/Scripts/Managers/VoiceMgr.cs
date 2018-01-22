
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;

/*
	public static VoiceMgr _instance = null;
	public static VoiceMgr GetInstance() { return Instance; }
	string path;

	static AudioClip mClip = null;
	int SAMPLE_RATE = 8000;

	public static VoiceMgr Instance {
		get {
			if (_instance == null) {
				GameObject obj = new GameObject("VoiceMgr");
				_instance = obj.AddComponent<VoiceMgr>();
				DontDestroyOnLoad(obj);
			}

			return _instance;
		}
	}

	public void startRecord() {
		Debug.Log("start record");
		Microphone.End(null);
		mClip = Microphone.Start(null, false, 15, SAMPLE_RATE);
	}

	public byte[] stopRecord(out int time) {
		Debug.Log("stop record");

		float len = 0;
		int lastPos = Microphone.GetPosition(null);
		if (Microphone.IsRecording (null)) {
			len = (float)lastPos / SAMPLE_RATE;
		} else {
			len = 15.0f;
		}

		Microphone.End(null);

		if (len < 1.0f) {
			time = 0;
			return null;
		}

		int rescaleFactor = 32767;
		int channels = mClip.channels;
		float[] samples = new float[mClip.samples * mClip.channels];

		Debug.Log("channels=" + channels);

		mClip.GetData(samples, 0);
		byte[] data = new byte[samples.Length * 2];

		for (int i = 0; i < samples.Length; i++) {
			short temshort = (short)(samples[i] * rescaleFactor);
			byte[] temdata = BitConverter.GetBytes(temshort);
			temdata.CopyTo (data, i * 2);
		}

		time = (int)(len * 1000);
		return data;
*/
/*
		float[] _samples = new float[data.Length / 2];
		for (int i = 0; i < _samples.Length; i++) {
			short tmp = BitConverter.ToInt16(data, i * 2);
			_samples[i] = (float)tmp / rescaleFactor;
		}

		AudioClip clip = AudioClip.Create("wav", _samples.Length, channels, SAMPLE_RATE, false);
		clip.SetData(_samples, 0);

		AudioSource.PlayClipAtPoint(clip, Vector3.zero);
		return null;
	}
*/

public class VoiceMgr {
	static VoiceMgr mInstance = null;
	bool inited = false;

	string _voiceMediaPath = null;
	string recordSDK = "com.dinosaur.voicesdk.VoiceRecorder";
	string playSDK = "com.dinosaur.voicesdk.VoicePlayer";

	#if UNITY_IPHONE
	[DllImport("__Internal")]
	private static extern void prepareRecordIOS(string path);

	[DllImport("__Internal")]
	private static extern void finishRecordIOS();

	[DllImport("__Internal")]
	private static extern void cancelRecordIOS();

	[DllImport("__Internal")]
	private static extern void playIOS(string file);

	[DllImport("__Internal")]
	private static extern void stopPlayIOS();

	[DllImport("__Internal")]
	private static extern void setStorageDirIOS(string dir);
	#endif

	public static VoiceMgr GetInstance () {
		if (mInstance == null)
			mInstance = new VoiceMgr ();

		return mInstance;
	}

	public VoiceMgr () {}

	public void Init() {
		if (inited)
			return;

		string path = Application.persistentDataPath + "/voicemsgs/";

		if (!Directory.Exists(path))
			Directory.CreateDirectory (path);

		_voiceMediaPath = path;
		setStorageDir(path);

		inited = true;
	}

	void setStorageDir(string path) {
		if (!isNative())
			return;

		if (isAndroid ()) {
			AndroidJavaClass recorder = new AndroidJavaClass(recordSDK);
			recorder.CallStatic("setStorageDir", path);
		} else if (isIOS()) {
			#if UNITY_IPHONE
			setStorageDirIOS(path);
			#endif
		}
	}

	bool isAndroid() {
		return Application.platform == RuntimePlatform.Android;
	}

	bool isIOS() {
		return Application.platform == RuntimePlatform.IPhonePlayer;
	}

	bool isNative() {
		return isAndroid() || isIOS();
	}

	public void prepare(string filename) {
		if (!isNative())
			return;

		AudioManager.pauseAll();
		clearCache(filename);

		if (isAndroid()) {
			AndroidJavaClass recorder = new AndroidJavaClass(recordSDK);
			recorder.CallStatic("prepare", filename);
		} else if (isIOS()) {
			#if UNITY_IPHONE
			prepareRecordIOS(filename);
			#endif
		}
	}

	void clearCache(string filename) {
		if (!isNative())
			return;

		string url = _voiceMediaPath + filename;
		if (File.Exists(url))
			File.Delete(url);

		if (File.Exists(url + ".wav"))
			File.Delete(url + ".wav");
	}

	public void release() {
		if (!isNative ())
			return;

		AudioManager.resumeAll();
		if (isAndroid ()) {
			AndroidJavaClass recorder = new AndroidJavaClass(recordSDK);
			recorder.CallStatic("release");
		} else if (isIOS ()) {
			#if UNITY_IPHONE
			Debug.Log ("before finishRecordIOS");
			finishRecordIOS();
			#endif
		}
	}

	public void cancel() {
		if (!isNative())
			return;

		AudioManager.resumeAll();
		if (isAndroid ()) {
			AndroidJavaClass recorder = new AndroidJavaClass(recordSDK);
			recorder.CallStatic("cancel");
		} else if (isIOS ()) {
			#if UNITY_IPHONE
			cancelRecordIOS();
			#endif
		}
	}

	public void writeVoice(string filename, string voice) {
		if (!isNative())
			return;

		if (voice != null && voice.Length > 0) {
			string url = _voiceMediaPath + filename;
			byte[] data = Convert.FromBase64String(voice);
			clearCache(filename);
			File.WriteAllBytes(url, data);
		}
	}

	public void play(string filename) {
		if (!isNative())
			return;

		AudioManager.pauseAll();

		if (isAndroid ()) {
			AndroidJavaClass player = new AndroidJavaClass(playSDK);
			player.CallStatic("play", filename);
		} else if (isIOS ()) {
			#if UNITY_IPHONE
			playIOS(filename);
			#endif
		}
	}

	public void stop() {
		if (!isNative())
			return;

		AudioManager.resumeAll();

		if (isAndroid ()) {
			AndroidJavaClass player = new AndroidJavaClass(playSDK);
			player.CallStatic("stop");
		} else if (isIOS ()) {
			#if UNITY_IPHONE
			stopPlayIOS();
			#endif
		}
	}

	public string getVoiceData(string filename) {
		if (isNative()) {
			string url = _voiceMediaPath + filename;

			if (!File.Exists (url))
				return "";

			byte[] data = File.ReadAllBytes (url);
			if (data != null)
				return Convert.ToBase64String (data);
		}

		return "";
	}
}

