using UnityEngine;
using System.Collections;

public class Discover : MonoBehaviour {

	public void onBtnCreate() {
		GameObject ob = GameObject.Find("PCreateRoom");
		ob.GetComponent<CreateRoom>().enter();
	}

	public void onBtnJoin() {
		GameObject ob = GameObject.Find("PJoinRoom");
		ob.GetComponent<JoinRoom>().enter();
	}
}
