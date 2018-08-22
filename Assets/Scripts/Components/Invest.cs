using UnityEngine;
using System.Collections;
using SimpleJson;

public class Invest : ListBase {

	void Start() {
		var body = transform.Find ("body");

		UIInput name = body.Find ("inputName").GetComponent<UIInput> ();
		UIInput phone = body.Find ("inputPhone").GetComponent<UIInput> ();

		PUtils.setBtnEvent (body, "BtnSubmit", () => {
			if (string.IsNullOrEmpty(name.value)) {
				GameAlert.Show("名字不能为空");
				return;
			}

			if (string.IsNullOrEmpty(phone.value)) {
				GameAlert.Show("电话不能为空");
				return;
			}

			var data = new JsonObject();

			data.Add("name", name.value);
			data.Add("phone", phone.value);
			data.Add("wechat", GameMgr.getUserMgr().account);

			NetMgr.GetInstance().request_apis("invest", data, ret => {
				if (this != null)
					back();
			});
		});
	}
}
