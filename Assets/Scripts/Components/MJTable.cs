
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MJTable : MonoBehaviour {

	public Texture2D[] textures;

	void Start() {
		int id = PlayerPrefs.GetInt("table_id", 0);
	
		if (id != 0)
			change (id);
	}

	public bool change(int id) {
		int cnt = textures.Length;
		if (id < 0 || id >= cnt)
			return false;

		var render = gameObject.GetComponent<MeshRenderer>();
		render.materials[0].mainTexture = textures[id];

		PlayerPrefs.SetInt ("table_id", id);
		return true;
	}
}
