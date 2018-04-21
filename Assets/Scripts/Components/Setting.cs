using UnityEngine;
using System.Collections;

public class Setting : ListBase {

	public void enter() {
		show ();

		Transform grid = transform.Find("items/grid_ign");

		grid.GetComponent<UIGrid> ().Reposition ();
		grid.GetComponentInParent<UIScrollView> ().ResetPosition ();
	}
}
