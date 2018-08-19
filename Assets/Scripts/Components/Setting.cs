using UnityEngine;
using System.Collections;

public class Setting : ListBase {

	void Start() {
		PUtils.setBtnEvent (transform, "bottom/BtnLogout", () => {
			NetMgr.GetInstance().logout();
		});
	}

	public void enter() {
		show ();

		Transform grid = transform.Find("items/grid_ign");

		PUtils.setText(grid, "version/version", "V" + GameSettings.Instance.version);

		grid.GetComponent<UIGrid> ().Reposition ();
		grid.GetComponentInParent<UIScrollView> ().ResetPosition ();
	}
}
