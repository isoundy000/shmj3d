using UnityEngine;
using System.Collections;

public class Mahjong2D : MonoBehaviour {

	UISprite tile = null;
	UISprite board = null;
	int _depth = 10;
	float _scale = 1.0f;

	void Awake() {
		tile = transform.Find("tile").GetComponent<UISprite>();
		board = GetComponent<UISprite> ();

		//board.spriteName = "south_meld_board";
		setDepth (_depth);
		setScale (_scale);
	}

	public void setID(int id) {
		if (id == 0) {
			tile.spriteName = null;
			//board.spriteName = "south_meld_cover_board";
			return;
		}

		tile.spriteName = "" + id;

		UISpriteData sp = tile.GetAtlasSprite();
		tile.width = sp.width;
		tile.height = sp.height;
	}

	public void setBoard(string name) {
		board.spriteName = name;
	}

	public void setDepth(int depth) {
		board.depth = depth;
		tile.depth = depth + 1;

		_depth = depth;
	}

	public void setScale(float scale) {
		board.transform.localScale = new Vector3 (scale, scale, 1.0f);

		_scale = scale;
	}
}
