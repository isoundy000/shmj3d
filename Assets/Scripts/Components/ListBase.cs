
using System;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class ListBase : MonoBehaviour {
	Transform mGrid = null;
	Transform mTemp = null;
	public bool mShow = false;
	public string listPath = "items/grid";
	public event Action UpdateEvents = null;

	public void Awake() {
		layout();

		mGrid = transform.Find (listPath);
		if (mGrid != null) {
			mTemp = mGrid.GetChild(0);
			mTemp.parent = null;
		}

		Transform btnBack = transform.Find ("top/BtnBack");
		PUtils.onClick (btnBack, () => {
			back();
		});
	}

	void layout() {
		Transform bg = transform.Find("bg_portait");

		if (bg != null) {
			UISprite sp = bg.GetComponent<UISprite> ();
			Transform root = GameObject.Find("UI Root").transform;
			sp.topAnchor.Set(root, 1, 0);
			sp.bottomAnchor.Set(root, 0, 0);
		}
	}

	public void back(bool update = true) {
		mShow = false;

		Sequence seq = DOTween.Sequence();

		seq.Insert(0, transform.DOLocalMoveX(-270, 0.1f).SetEase(Ease.Linear));
		seq.InsertCallback (0.1f, () => {
			transform.localPosition = new Vector2(-1100, 0);
		});

		seq.Play();

		if (update && UpdateEvents != null)
			UpdateEvents.Invoke();

		UpdateEvents = null;
		onBack();
	}

	public void show(Action cb = null) {
		gameObject.SetActive (true);

		transform.localPosition = new Vector3 (-270, 0, 0);

		Sequence seq = DOTween.Sequence ();

		seq.Insert (0, transform.DOLocalMoveX (0, 0.1f).SetEase (Ease.Linear));

		if (cb != null) {
			seq.InsertCallback (0.1f, () => {
				cb.Invoke ();
			});
		}

		seq.Play ();

		mShow = true;
	}

	public virtual void onBack() {}

	public Transform getItem(int id) {
		if (mGrid.childCount > id)
			return mGrid.GetChild(id);

		GameObject ob = Instantiate(mTemp.gameObject, mGrid) as GameObject;
		return ob.transform;
	}

	public void shrinkContent(int num) {
		while (mGrid.childCount > num)
			DestroyImmediate(mGrid.GetChild(mGrid.childCount - 1).gameObject);
	}

	public void updateItems(int count) {
		shrinkContent(count);

		UIGrid grid = mGrid.GetComponent<UIGrid>();
		if (grid != null)
			grid.Reposition();

		UITable table = mGrid.GetComponent<UITable>();
		if (table != null)
			table.Reposition();

		var scroll = mGrid.GetComponentInParent<UIScrollView> ();
		if (scroll != null)
			scroll.ResetPosition();
	}

	public void Reposition(string grid = null) {
		Transform ob = grid == null ? mGrid : transform.Find (grid);
		Reposition (ob);
	}

	public void Reposition(Transform ob) {
		UIGrid gd = ob.GetComponent<UIGrid>();
		if (gd != null)
			gd.Reposition();

		UITable table = ob.GetComponent<UITable>();
		if (table != null)
			table.Reposition();

		ob.GetComponentInParent<UIScrollView>().ResetPosition();
	}

	public Transform getChild(Transform item, string child) {
		if (item == null)
			return null;
		
		return item.Find (child);
	}

	public void setText(Transform item, string child, string text) {
		Transform lbl = getChild (item, child);
		if (lbl != null)
			lbl.GetComponent<UILabel>().text = text;
	}

	public void setIcon(Transform item, string child, int uid) {
		Transform icon = getChild (item, child);
		if (icon != null)
			icon.GetComponent<IconLoader>().setUserID (uid);
	}

	public void setIcon(Transform item, string child, string url) {
		Transform icon = getChild (item, child);
		UITexture texture = icon.GetComponent<UITexture>();

		Debug.Log ("setIcon, url=" + url);

		if (url == null || url.Length == 0) {
			texture.mainTexture = null;
			return;
		}

		if (icon != null)
			ImageLoader.GetInstance().LoadImage(url, icon.GetComponent<UITexture>());
	}

	public void setBtnEvent(Transform item, string child, Action cb) {
		Transform btn = child == null ? item : getChild (item, child);
		if (btn != null)
			PUtils.onClick(btn, cb);
	}

	public void setActive(Transform item, string child, bool enable) {
		Transform ob = child == null ? item : getChild (item, child);
		if (ob != null)
			ob.gameObject.SetActive(enable);
	}

	public void setInput(Transform item, string child, string text) {
		Transform input = getChild (item, child);
		if (input != null) {
			UIInput ob = input.GetComponent<UIInput>();
			ob.Start();
			ob.Set(text);
		}
	}

	public string getInput(Transform item, string child) {
		Transform input = getChild (item, child);
		if (input != null)
			return input.GetComponent<UIInput>().value;

		return "";
	}

	public T getPage<T>(string page) {
		return GameObject.Find(page).GetComponent<T>();
	}

	public void enter() {
		show();
	}

	public void setToggle(Transform item, string child, bool value) {
		Transform ob = child == null ? item : getChild (item, child);
		if (ob != null) {
			UIToggle tg = ob.GetComponent<UIToggle>();
			tg.value = value;
		}
	}

	public void setToggleEvent(Transform item, string child, Action<bool> cb) {
		PUtils.setToggleEvent (item, child, cb);
	}

	public void setSliderEvent(Transform item, string child, Action<float> cb) {
		Transform ob = child == null ? item : getChild (item, child);

		if (ob != null) {
			UISlider slider = ob.GetComponent<UISlider>();
			var onChange = slider.onChange;

			onChange.Clear();

			if (cb != null) {
				onChange.Add(new EventDelegate(() => {
					cb.Invoke(slider.value);
				}));
			}
		}
	}
}
