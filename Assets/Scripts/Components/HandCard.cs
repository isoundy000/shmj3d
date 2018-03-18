
using UnityEngine;
using System.Collections;

public class HandCard : MonoBehaviour {

	bool interactable = true;

	Color origin;
/*
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
*/

	void Awake() {
		origin = gameObject.GetComponent<Renderer>().materials[0].color;
	}

	public void setColor(Color cl) {
		gameObject.GetComponent<Renderer>().materials[0].color = cl;
	}

	public void resetColor() {
		setColor(origin);
	}

	public void choosed() {
		setColor(new Color(1.0f, 0.89f, 0.34f));
	}

	public void ting() {
		setColor(new Color(0.96f, 0.445f, 0.012f));
	}

	public void setInteractable(bool status, bool setcolor = true) {

		if (setcolor) {
			if (status)
				setColor(origin);
			else
				setColor(Color.gray);
		}

		interactable = status;
	}

	public bool getInteractable() {
		return interactable;
	}
}
