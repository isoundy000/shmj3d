
using System;
using UnityEngine;
using System.Collections.Generic;
using SimpleJson;

[Serializable]
public class ListRecords {
	public int errcode;
	public string errmsg;
	public List<TransferRecords> data;
}

[Serializable]
public class TransferRecords {
	public int count;
	public string created_at;
	public int target;
}

public class TransferList : ListBase {

	public void enter(int uid) {
		refresh(uid);
		show();
	}

	void refresh(int uid) {
		var http = Http.GetInstance ();
		var args = new JsonObject ();

		args.Add ("uid", uid);

		http.Post ("/dealer/list_transfer_records", args, text => {
			var ret = JsonUtility.FromJson<ListRecords> (text);
			if (ret.errcode != 0) {
				Debug.Log("list_transfer_records errcode=" + ret.errcode);
				return;
			}

			showRecords(ret.data);
		}, err => {
		
		});
	}

	void showRecords(List<TransferRecords> records) {
		for (int i = 0; i < records.Count; i++) {
			TransferRecords record = records[i];
			Transform item = getItem(i);

			setText(item, "desc", record.count + "颗钻石");
			setText(item, "id", "ID:" + record.target);
			setIcon(item, "bghead/icon", record.target);
			setText (item, "time", record.created_at);
		}

		updateItems(records.Count);
	}
}


