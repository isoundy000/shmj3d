
using UnityEngine;
using System.Collections;
using SimpleJson;

public class Feedback : ListBase {

	public UIInput mContent = null;

	void reset() {
		mContent.value = "";
	}

	public void enter() {
		reset();
		show();
	}

	public void onBtnSubmit() {
		string content = mContent.value;

		if (content == "") {
			GameAlert.Show("请填写反馈内容");
			return;
		}

		JsonObject ob = new JsonObject();
		ob["content"] = content;
		ob["qq"] = "123";
		ob["phone"] = "12345678901";

		NetMgr.GetInstance ().request_apis("feedback", ob, data => {
			NormalReturn ret = JsonUtility.FromJson<NormalReturn> (data.ToString());
			if (ret.errcode != 0) {
				GameAlert.Show("提交失败, 请检查网络");
				return;
			}

			GameAlert.Show("感谢您的反馈，我们会尽快处理!", () => {
				if (this != null)
					back();
			});
		});
	}
}
