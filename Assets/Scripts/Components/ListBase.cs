
using System;
using UnityEngine;
using System.Collections.Generic;

public class ListBase : MonoBehaviour {
	Transform mGrid = null;
	Transform mTemp = null;
	UITweener tweener = null;
	protected bool mShow = false;
	protected string listPath = "items/grid";
	public event Action UpdateEvents = null;

	protected void Awake() {
		mGrid = transform.Find (listPath);
		if (mGrid != null) {
			mTemp = mGrid.GetChild(0);
			mTemp.parent = null;
		}

		tweener = transform.GetComponent<UITweener> ();

		Transform btnBack = transform.Find ("top/BtnBack");
		Utils.onClick (btnBack, () => {
			back();
		});
	}

	protected void back(bool update = true) {
		mShow = false;
		if (tweener != null)
			tweener.PlayReverse();

		if (update && UpdateEvents != null)
			UpdateEvents.Invoke();

		UpdateEvents = null;
		onBack();
	}

	protected void show() {
		gameObject.SetActive(true);
		if (tweener != null)
			tweener.PlayForward ();

		mShow = true;
	}

	protected virtual void onBack() {}

	protected Transform getItem(int id) {
		if (mGrid.childCount > id)
			return mGrid.GetChild(id);

		GameObject ob = Instantiate(mTemp.gameObject, mGrid) as GameObject;
		return ob.transform;
	}

	protected void shrinkContent(int num) {
		while (mGrid.childCount > num)
			DestroyImmediate(mGrid.GetChild(mGrid.childCount - 1).gameObject);
	}

	protected void updateItems(int count) {
		shrinkContent(count);

		UIGrid grid = mGrid.GetComponent<UIGrid>();
		if (grid != null)
			grid.Reposition();

		UITable table = mGrid.GetComponent<UITable>();
		if (table != null)
			table.Reposition();
	}

	Transform getChild(Transform item, string child) {
		if (item == null)
			return null;
		
		return item.Find (child);
	}

	protected void setText(Transform item, string child, string text) {
		Transform lbl = getChild (item, child);
		if (lbl != null)
			lbl.GetComponent<UILabel>().text = text;
	}

	protected void setIcon(Transform item, string child, int uid) {
		Transform icon = getChild (item, child);
		if (icon != null)
			icon.GetComponent<IconLoader>().setUserID (uid);
	}

	protected void setIcon(Transform item, string child, string url) {
		if (url == null || url.Length == 0)
			return;

		Transform icon = getChild (item, child);
		if (icon != null)
			ImageLoader.GetInstance().LoadImage(url, icon.GetComponent<UITexture>());
	}

	protected void setBtnEvent(Transform item, string child, Action cb) {
		Transform btn = child == null ? item : getChild (item, child);
		if (btn != null)
			Utils.onClick(btn, cb);
	}

	protected void setActive(Transform item, string child, bool enable) {
		Transform ob = child == null ? item : getChild (item, child);
		if (ob != null)
			ob.gameObject.SetActive(enable);
	}

	protected void setInput(Transform item, string child, string text) {
		Transform input = getChild (item, child);
		if (input != null) {
			UIInput ob = input.GetComponent<UIInput>();
			ob.Start();
			ob.Set(text);
		}
	}

	protected string getInput(Transform item, string child) {
		Transform input = getChild (item, child);
		if (input != null)
			return input.GetComponent<UIInput>().value;

		return "";
	}

	protected T getPage<T>(string page) {
		return GameObject.Find(page).GetComponent<T>();
	}

	public void enter() {
		show();
	}
}
