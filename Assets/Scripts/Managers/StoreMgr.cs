
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;
using SimpleJson;

[Serializable]
public class QueryOrderReply {
	public int errcode;
	public int currency;
	public int quantity;
}

public enum QueryOrderErrcode {
	ORDER_SUCCESS = 3100,
	ORDER_NOT_PAY,
	ORDER_ALREADY_GOT,
	ORDER_NOT_EXIST
}

public class StoreMgr : MonoBehaviour {

	static StoreMgr mInstance = null;
	public static StoreMgr GetInstance() { return mInstance; }

	static bool iapInited = false;
	static string receiptPath;

	static bool buying = false;

	#if UNITY_IPHONE
	[DllImport("__Internal")]
	private static extern void initIAP(string products, string receipts);

	[DllImport("__Internal")]
	private static extern void buyIAP(string type);
	#endif

	void Awake() {
		mInstance = this;
	}

	public static bool InitIAP (string ids) {
		#if UNITY_IPHONE
		Debug.Log("InitIAP");
		if (ids == null || ids.Length == 0)
			return false;

		if (iapInited)
			return true;

		string receipts = Application.persistentDataPath + "/receipts/";

		if (!Directory.Exists (receipts))
			Directory.CreateDirectory (receipts);

		initIAP(ids, receipts);

		iapInited = true;
		#endif
		return true;
	}

	public static void pay(GoodsInfo info) {

		if (buying) {
			Debug.Log ("buying");
			return;
		}

		buying = true;

		WaitMgr.Show("交易进行中，请等待...");

		#if UNITY_IPHONE
		buyIAP(info.product);
		#endif
	}

	public void onBuyIAPFail(string code) {
		Debug.Log ("onBuyIAPFail: " + code);

		buying = false;
		WaitMgr.Hide();

		GameAlert.Show ("购买失败:" + code);
	}

	public void onBuyIAPResp(string receipt) {
		var file = Application.persistentDataPath + "/receipts/" + receipt;

		buying = false;

		if (!File.Exists (file)) {
			Debug.Log ("receipt not exist");
			return;
		}

		string content = File.ReadAllText (file);
		var args = new JsonObject();
		var token = NetMgr.GetInstance ().getToken ();

		args ["token"] = token;
		args["receipt"] = content;

		var http = Http.GetInstance ();

		http.Post("/pay_iap/query_order", args, data => {
			Debug.Log("ret from pay_iap");

			QueryOrderReply ret = JsonUtility.FromJson<QueryOrderReply> (data.ToString());

			if (ret.errcode == (int)QueryOrderErrcode.ORDER_SUCCESS) {
				GameAlert.Show("购买成功");

				GameMgr.GetInstance().get_coins();

				File.Delete(file);
			} else {
				GameAlert.Show("购买失败:" + ret.errcode);
			}

			WaitMgr.Hide();
		}, err => {
			WaitMgr.Hide();
			GameAlert.Show("购买通讯失败");
		});
	}
}


