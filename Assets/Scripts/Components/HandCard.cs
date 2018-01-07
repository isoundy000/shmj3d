
using UnityEngine;
using System.Collections;

public class HandCard : MonoBehaviour {

	bool interactable = true;

	public void setID(int id) {
		id = id % 100;

		if (id >= 51 && id <= 52)
			id -= 3;
		else if (id >= 53)
			id -= 2;

		id -= 10;

		int UVy = id / 10;
		int UVx = id % 10;
		if (UVy == 0)
			UVy = 1;
		else if (UVy == 1)
			UVy = 0;

		gameObject.GetComponent<Renderer>().materials[1].mainTextureOffset = new Vector2((UVx - 1) * 0.1068f, -UVy * 0.168f);
	}

	public void setColor(Color cl) {
		gameObject.GetComponent<Renderer>().materials[1].color = cl;
	}

	public void resetColor() {
		setColor(Color.white);
	}

	public void setInteractable(bool status) {

		if (status) {
			setColor (Color.white);
		} else {
			setColor (Color.gray);
			//setColor (new Color(0.715f, 0.715f, 0.715f));
		}

		interactable = status;
	}

	public bool getInteractable() {
		return interactable;
	}
}
