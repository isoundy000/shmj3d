
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DHM_RecyleHandCardManager : MonoBehaviour {
    public List<HandCardItem> _RecyleHandCardList = new List<HandCardItem>();
    public GameObject _chuPaiHand1 = null;
    public GameObject _chuPaiHand2 = null;
    public GameObject _handCardPrefab = null;

    public delegate void ChuPaiCallBackDelegate(GameObject go);
    public event ChuPaiCallBackDelegate ChuPaiCallBackEvent;

	public int seatindex;

    [SerializeField]
    private float offSetX = -0.035f;
    [SerializeField]
    private float offSetZ = 0.0485f;

	static GameObject focus = null;

	void Awake() {
		if (focus == null)
			focus = GameObject.Find("focus");

		int numofseats = RoomMgr.GetInstance().numOfSeats();
		if (2 == numofseats)
			transform.Translate(0 - offSetX * 3, 0, 0);
	}

	void showFocus(GameObject mj) {
		Transform tm = focus.transform;
		TweenPosition tp = tm.GetComponent<TweenPosition>();

		tp.enabled = false;

		tm.SetParent(mj.transform);
		//tm.localScale = new Vector3 (0.6f, 0.6f, 0.6f);
		//tm.localPosition = new Vector3 (0, 0.25f, 0.25f);
		tm.localRotation = Quaternion.Euler (0, 0, 0);
		focus.SetActive(true);


		tp.enabled = false;

		tp.from = tm.localPosition;
		tp.to = new Vector3 (0, 0.25f, 0.25f);
		tp.style = UITweener.Style.Once;
		tp.duration = 0.3f;
		tp.AddOnFinished(new EventDelegate(() => {
			tp.onFinished.Clear();

			tm.localPosition = new Vector3 (0, 0.25f, 0.25f);

			tp.style = UITweener.Style.PingPong;
			tp.from = tm.localPosition;
			tp.to = tm.localPosition + new Vector3(0, 0, 0.2f);
			tp.duration = 1.0f;
			tp.Play();

			TweenRotation tr = tm.GetComponent<TweenRotation>();
			tr.from = tm.localEulerAngles;
			tr.to = tm.localEulerAngles + new Vector3(0, 0, 360);
			tr.Play();
		}));

		tp.Play();
	}


	public void hideFocus() {
		
		Transform tm = focus.transform;

		TweenPosition tp = tm.GetComponent<TweenPosition>();
		TweenRotation tr = tm.GetComponent<TweenRotation>();

		tp.enabled = false;
		tr.enabled = false;

		tm.SetParent(GameObject.Find("mjtable").transform);
		tm.localPosition = new Vector3 (0, 0.04f, 0);
		tm.localRotation = Quaternion.Euler (-90, 0, 0);
	}

	void detachFocus() {
		Transform tm = focus.transform;
		Transform mj = tm.parent;

		TweenPosition tp = tm.GetComponent<TweenPosition>();
		TweenRotation tr = tm.GetComponent<TweenRotation>();

		tp.enabled = false;
		tr.enabled = false;

		tm.localPosition = new Vector3 (0, 0.25f, 0.25f);

		Transform table = GameObject.Find ("mjtable").transform;

		tm.SetParent(table);

		tp.from = tm.localPosition;
		tp.to = tm.localPosition + new Vector3 (0, 0.02f, 0);
		tp.style = UITweener.Style.PingPong;
		tp.duration = 1.0f;
		tp.Play();

/*
		string path = "Prefab/Meishu/bomb";
		Transform ob = Instantiate(Resources.Load (path) as GameObject).transform;
		ob.SetParent(mj);
		ob.localPosition = new Vector3 (0, 0.25f, 0);
		ob.SetParent(table);
		ob.localRotation = Quaternion.Euler (-90, 0, 0);
		ob.GetComponent<ParticleSystem>().Play();
*/
	}

    public void ChuPai(HandCardItem item, bool isMoNi) {
		item.setLayer("ZhuoPai");
        _RecyleHandCardList.Add(item);
        PlayChuPaiAnimation(isMoNi);

		AudioManager.GetInstance().PlayHandCardAudio(item.getId());
    }

    public GameObject GetChuPaiWay() {
		ResourcesMgr rm = ResourcesMgr.GetInstance ();

        int way = Random.Range(1, 3);

		way = 2; // TODO
        if (way == 1)
        {
            _chuPaiHand1 = rm.InstantiateGameObjectWithType("ChuPaiHand1", ResourceType.Hand);
            _chuPaiHand1.transform.position = this.transform.TransformPoint(-0.0809f, -0.0141f, 0.4405f);
            return _chuPaiHand1;
        }
        else 
        {
            _chuPaiHand2 = rm.InstantiateGameObjectWithType("ChuPaiHand2", ResourceType.Hand);
            _chuPaiHand2.transform.position = this.transform.TransformPoint(-0.1367f, -0.0131f, 0.505f);
            return _chuPaiHand2; 
        }
    }

    public void PlayChuPaiAnimation(bool isMoNi)
    {
		int index = _RecyleHandCardList.Count - 1;

		Debug.LogWarning("[" + seatindex + "]DHM_RecyleHandCardManager+模拟出牌的ID：" + _RecyleHandCardList[index].getId());

        GameObject hand = GetChuPaiWay();
        if (ChuPaiCallBackEvent != null)
            ChuPaiCallBackEvent(hand);

		int row = getRow(index);
		int col = getCol(index);
        hand.transform.rotation = transform.rotation;
        hand.transform.Translate(offSetX * col, 0, offSetZ * row);
        DHM_HandAnimationCtr handCtr = hand.GetComponent<DHM_HandAnimationCtr>();
        handCtr.SetMoNiMoPai(isMoNi);

        handCtr.PlayChuPaiAnimation(_RecyleHandCardList[index]);
        handCtr.chuPaiEndEvent += ChuPaiEndEventHandle;

        GameManager.m_instance.m_ProState = transform.parent.GetComponent<DHM_CardManager>();
    }

    public void ChuPaiEndEventHandle()
    {
		Debug.Log ("[" + seatindex + "]ChuPaiEndEventHandle");

		if (_RecyleHandCardList.Count == 0)
			return;

		HandCardItem item = _RecyleHandCardList[_RecyleHandCardList.Count - 1];
		GameObject obj = item.getObj();

		if (obj != null) {			
			obj.transform.SetParent (this.transform);

			item.resetColor();
			showFocus(obj);
		}
    }

    public void DeleteCard()
    {
		if (_RecyleHandCardList.Count == 0)
			return;

        int id = _RecyleHandCardList.Count - 1;
		HandCardItem item = _RecyleHandCardList[id];

		detachFocus ();
        
		_RecyleHandCardList.RemoveAt(id);
		item.destroy();
    }

    public void ResetInfo()
    {
		hideFocus ();

        Transform[] trans = this.GetComponentsInChildren<Transform>();
        for (int i = trans.Length - 1; i >= 0; i--) {
            if (trans[i] != this.transform)
                DestroyImmediate(trans[i].gameObject);
        }

        _RecyleHandCardList.Clear();
    }

	int getRow(int id) {
		int nseats = RoomMgr.GetInstance().numOfSeats();

		if (nseats > 2) {
			if (id < 6)
				return 0;
			else
				return 1 + (id - 6) / 10;
		} else {
			return id / 12;
		}
	}

	int getCol(int id) {
		int nseats = RoomMgr.GetInstance().numOfSeats();
		if (nseats > 2) {
			if (id < 6)
				return id;
			else
				return (id - 6) % 10;
		} else {
			return id % 12;
		}
	}

	public void sync() {
		List<int> folds = RoomMgr.GetInstance ().seats [seatindex].folds;

		ResetInfo ();

		for (int i = 0; i < folds.Count; i++) {
			int id = folds [i] % 100;
			bool ting = folds [i] > 100;
			GameObject ob = ResourcesMgr.GetInstance().LoadMJ(id);
			HandCardItem item = new HandCardItem(id, ob);

			int row = getRow(i);
			int col = getCol(i);

			ob.layer = LayerMask.NameToLayer("ZhuoPai");
			ob.transform.SetParent (this.transform);
			ob.transform.localRotation = Quaternion.Euler(new Vector3 (-90, 0, 0));
			ob.transform.localPosition = new Vector3(offSetX * col, 0, offSetZ * row + 0.0098f);
			//ob.transform.Translate(offSetX * col, 0, offSetZ * row);

			if (ting)
				item.setTing(true);

			_RecyleHandCardList.Add(item);
		}
	}

	public void highlight(int id, bool enable) {
		foreach (HandCardItem item in _RecyleHandCardList) {
			if (item.checkId(id))
				item.choosed(enable);
		}
	}
}
