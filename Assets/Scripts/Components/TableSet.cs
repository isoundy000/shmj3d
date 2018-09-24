
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableSet : MonoBehaviour {

	void setTable(int id) {
		var table = GameObject.Find("paizhuo.obj");
		var paizhuo = table.GetComponent<MJTable> ();

		paizhuo.change (id);
	}

	public void setTable0() {
		setTable(0);
	}

	public void setTable1() {
		setTable(1);
	}
}
