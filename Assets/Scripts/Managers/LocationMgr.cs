
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LocationError 
{ 
	ERROR_NONE, //没有错误
	ERROR_NOT_ENABLED, //GPS未启用 
	ERROR_TIMEOUT, //请求超时 
	ERROR_FAILED, //请求失败

}

public class LocationInfo 
{ 
	/// 
	/// Geographical device location latitude. 纬度 
	/// 
	public float latitude; 
	/// 
	/// Geographical device location longitude 经度 
	/// 
	public float longitude; 
	/// 
	/// Geographical device location altitude 
	/// 
	public float altitude; 
	/// 
	/// Horizontal accuracy of the location. 
	/// 
	public float horizontalAccuracy; 
	/// 
	/// Vertical accuracy of the location. 
	/// 
	public float verticalAccuracy; 
	/// 
	/// Timestamp (in seconds since 1970) when location was last time updated 
	/// 
	public double timestamp;

	public string error = null;
	public LocationError errcode = LocationError.ERROR_NONE;

	public LocationInfo() {
		latitude = 0;
		longitude = 0;
		altitude = 0;
		horizontalAccuracy = 0;
		verticalAccuracy = 0;
		timestamp = 0;
	}

	public bool valid() {
		return errcode == LocationError.ERROR_NONE && latitude > 0;
	}
}

public class LocationMgr : MonoBehaviour {

	bool enable = true;
	static LocationMgr mInstance = null;

	LocationInfo mInfo = new LocationInfo();

	float lastcheck = 0;
	int checkTimeout = 60;

	public static LocationMgr GetInstance () {
		return mInstance;
	}
		
	void Awake() {
		mInstance = this;
	}

	void Start() {
		lastcheck = Time.time;

#if UNITY_EDITOR
		enable = false;
		return;
#endif

		if (enable)
			GetGPS();
	}

	public void GetGPS(Action<LocationInfo> cb = null, bool force = false) {
		if (!force && mInfo.valid()) {
			if (cb != null)
				cb (mInfo);

			return;
		}

		StartCoroutine (StartGPS(cb));
	}

	IEnumerator StartGPS (Action<LocationInfo> cb) {
		LocationInfo info = new LocationInfo();

		lastcheck = Time.time;

		//Debug.Log ("StartGPS");

		if (!Input.location.isEnabledByUser) {
			//Debug.Log("用户未开启GPS");
			info.errcode = LocationError.ERROR_NOT_ENABLED;
			info.error = "用户未开启GPS";
			mInfo = info;
			if (cb != null)
				cb (info);
			
			yield break;
		}

		Input.location.Start();

		int maxWait = 20;

		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
			yield return new WaitForSeconds (1);
			maxWait--;
		}

		if (maxWait < 1) {
			//Debug.Log ("获取GPS信息超时");
			Input.location.Stop();
			info.errcode = LocationError.ERROR_TIMEOUT;
			info.error = "获取GPS信息超时";
			mInfo = info;
			if (cb != null)
				cb (info);
			
			yield break;
		}

		if (Input.location.status == LocationServiceStatus.Failed) {
			//Debug.Log ("获取GPS信息失败");
			Input.location.Stop();
			info.errcode = LocationError.ERROR_FAILED;
			info.error = "获取GPS信息失败";
			mInfo = info;
			if (cb != null)
				cb (info);
			
			yield break;
		}

		info.latitude = Input.location.lastData.latitude;
		info.longitude = Input.location.lastData.longitude;
		info.altitude = Input.location.lastData.altitude;
		info.horizontalAccuracy = Input.location.lastData.horizontalAccuracy;
		info.verticalAccuracy = Input.location.lastData.verticalAccuracy;
		info.timestamp = Input.location.lastData.timestamp;

		Input.location.Stop();
		mInfo = info;

		//Debug.Log ("lat=" + info.latitude + " lon=" + info.longitude);

		if (cb != null)
			cb(info);
	}

	void Update() {
		if (!enable)
			return;

		float now = Time.time;

		if (/*mInfo.errcode != LocationError.ERROR_NOT_ENABLED && */checkTimeout > 0 && now - lastcheck > checkTimeout)
			GetGPS(null, true);
	}

	public LocationInfo Get() {
#if UNITY_EDITOR
		var info = new LocationInfo();
		//"lat":30.50836,"lon":114.3359
		info.latitude = 30.50836f;
		info.longitude = 114.3369f;
		return info;
#endif
		return mInfo;
	}
}



