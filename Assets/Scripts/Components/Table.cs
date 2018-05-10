using UnityEngine;
using System.Collections;

public class Table : MonoBehaviour {

	void Awake() {
		AnysdkMgr.setLandscape ();

		StartCoroutine(LoadAssets());
	}

	IEnumerator LoadAssets() {
		string[] ab3d = new string[]{ "EastPlayer", "SouthPlayer", "WestPlayer", "NorthPlayer", "light" };
		foreach (var ab in ab3d)
			yield return StartCoroutine(PUtils.LoadAsset(null, ab));

		string[] ab2d = new string[]{ "MainView" };
		foreach (var ab in ab2d)
			yield return StartCoroutine(PUtils.LoadAsset(transform, ab));
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
