﻿
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
    private float offSetX = -0.0340f;
    [SerializeField]
    private float offSetZ = 0.0460f;

	void Start () {
	
	}

    public void ChuPai(int id)
    {
        HandCardItem item = new HandCardItem();
        item._id = id;
        GameObject obj = (GameObject)Instantiate(_handCardPrefab);
        item._obj = RuleManager.UVoffSetWithReturn(id, obj);
    }

    public void ChuPai(HandCardItem item, bool isMoNi) {
        item._obj.layer = LayerMask.NameToLayer("ZhuoPai");
		// TODO
        //RuleManager.m_instance.ResetHandCardColor(item._obj);
        Debug.Log("[" + seatindex + "]_RecyleHandCardList" + _RecyleHandCardList.Count);
        _RecyleHandCardList.Add(item);
		Debug.Log("[" + seatindex + "]_RecyleHandCardList" + _RecyleHandCardList.Count);
        PlayChuPaiAnimation(isMoNi);

        AudioManager.Instance.PlayHandCardAudio(item._id);
    }

    public GameObject GetChuPaiWay() {
		ResourcesMgr rm = ResourcesMgr.GetInstance ();

        int way = Random.Range(1, 3);
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

    public GameObject GetHandCard(int id) {
        GameObject obj = null;
        for (int i = _RecyleHandCardList.Count - 1; i >= 0 ;i--)
        {
            if (_RecyleHandCardList[i]._id == id) {
                obj = _RecyleHandCardList[i]._obj;
                _RecyleHandCardList.RemoveAt(i);
            }
        }

        return obj;
    }

    public void PlayChuPaiAnimation(bool isMoNi)
    {
		int index = _RecyleHandCardList.Count - 1;

		Debug.LogWarning("[" + seatindex + "]DHM_RecyleHandCardManager+模拟出牌的ID：" + _RecyleHandCardList[index]._id);

        GameObject hand = GetChuPaiWay();
        if (ChuPaiCallBackEvent != null)
            ChuPaiCallBackEvent(hand);

        int row = index / 6;
        int col = index % 6;
        hand.transform.rotation = this.transform.rotation;
        hand.transform.Translate(offSetX * col, 0, offSetZ * row);
        DHM_HandAnimationCtr handCtr = hand.GetComponent<DHM_HandAnimationCtr>();
        handCtr.SetMoNiMoPai(isMoNi);

        handCtr.PlayChuPaiAnimation(_RecyleHandCardList[index]);
        handCtr.chuPaiEndEvent += ChuPaiEndEventHandle;

        GameManager.m_instance.m_ProState = this.transform.parent.GetComponent<DHM_CardManager>();
    }

    public void ChuPaiEndEventHandle()
    {
		Debug.Log ("[" + seatindex + "]ChuPaiEndEventHandle");
        GameObject obj = _RecyleHandCardList[_RecyleHandCardList.Count - 1]._obj;

		if (obj != null) {
			obj.transform.SetParent (this.transform);

			obj.GetComponent<HandCard> ().resetColor ();
		}
    }

    public void DeleteCard()
    {
        int id = _RecyleHandCardList.Count - 1;
        DestroyImmediate(_RecyleHandCardList[id]._obj);
        _RecyleHandCardList.RemoveAt(id);
    }

    public void ResetInfo()
    {
        Transform[] trans = this.GetComponentsInChildren<Transform>();
        for (int i = trans.Length - 1; i >= 0; i--) {
            if (trans[i] != this.transform)
                DestroyImmediate(trans[i].gameObject);
        }

        _RecyleHandCardList.Clear();
    }

	public void sync() {
		List<int> folds = RoomMgr.GetInstance ().seats [seatindex].folds;

		ResetInfo ();

		for (int i = 0; i < folds.Count; i++) {
			int id = folds [i];
			//GameObject ob = (GameObject)Instantiate (_handCardPrefab);
			GameObject ob = Instantiate(_handCardPrefab, new Vector3(0, 0, 0), Quaternion.identity, this.transform) as GameObject;
			HandCardItem item = new HandCardItem(id, ob);

			int row = i / 6;
			int col = i % 6;

			//ob.transform.SetParent (this.transform);
			ob.transform.localRotation = Quaternion.Euler (new Vector3 (-90, 0, 0));
			ob.transform.localPosition = new Vector3(offSetX * col, 0, offSetZ * row + 0.0098f);
			//ob.transform.Translate(offSetX * col, 0, offSetZ * row);

			_RecyleHandCardList.Add (item);
		}
	}
}
