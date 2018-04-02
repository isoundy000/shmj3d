
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

[System.Serializable]
public class HandCardItem {
    int _id;
    GameObject _obj;
	HandCard _hc;
	bool _ting = false;
	Action _cb = null;

	public HandCardItem(int id, GameObject ob) {
		_id = id;
		_obj = ob;

		if (id < 11 || id > 58)
			Debug.LogError("handcard id error: " + id);

		if (ob == null)
			Debug.LogError("handcard ob null");
		else
			_hc = ob.GetComponent<HandCard>();
	}

	public bool checkId(int id) {
		return id == _id;
	}

	public bool checkObj(GameObject ob) {
		if (_obj == null || ob == null)
			return false;

		return ob.Equals(_obj);
	}

	public bool checkObj(HandCardItem item) {
		if (item == null || !item.valid ())
			return false;

		return item.getObj ().Equals (_obj);
	}

	public bool interactable() {
		return _hc != null && _hc.getInteractable();
	}

	public void setInteractable(bool enable, bool setcolor = true) {
		if (_hc != null)
			_hc.setInteractable(enable, setcolor);
	}

	public bool valid() {
		return _obj != null;
	}

	public void destroy() {
		if (_obj != null)
			MonoBehaviour.DestroyImmediate(_obj);

		_obj = null;
	}

	public int getId() {
		return _id;
	}

	public GameObject getObj() {
		return _obj;
	}

	public void setLayer(string name) {
		if (_obj != null)
			_obj.layer = LayerMask.NameToLayer(name);
	}

	public string getLayer() {
		if (_obj != null)
			return LayerMask.LayerToName(_obj.layer);

		return "";
	}

	public void choosed(bool enable = true) {
		if (_hc != null) {
			if (enable)
				_hc.choosed ();
			else
				resetColor();
		}
	}

	public void setTing(bool ting = true) {
		_ting = ting;

		if (ting)
			_hc.ting();
		else
			_hc.resetColor();
	}

	public void resetColor() {
		if (_ting)
			_hc.ting();
		else
			_hc.resetColor();
	}

	public void setCB(Action cb) {
		_cb = cb;
	}

	public void invoke() {
		if (_cb != null)
			_cb();
	}
}

public class DHM_HandCardManager : MonoBehaviour {
    public List<HandCardItem> _handCardList = new List<HandCardItem>();//手牌数组
	private List<int> idArray = new List<int>();           //ID数组
    public float offSetX = 0.035f;          //每张手牌x轴的偏移量

	public GameObject currentObj = null;    //当前点击的手牌
	bool moved = false;
	bool draging = false;
	Vector3 screenPosition;
	Vector3 oldPosition;
	Vector3 offset;

    public GameObject _handCardPrefab = null;//手牌预设体
    public Transform _HandCardPlace = null; //手牌放置父节点
    private int newIndex = -1;                   //摸牌要插入的下标
    private int oldIndex = -1;                   //打出去的牌的下标

    public HandCardItem _MoHand = null;    //摸牌
    [SerializeField]
    private Transform _MoHandPos;           //摸牌的位置

    string strChaPaiHand = "ChaPaiHand";    //插牌动画的名字前缀

	public Transform _flowerPlace = null;

    public enum PlayerType
    {
        East,
        West,
        South,
        North,
        None
    }
    
    public string tagValue = string.Empty;           //当前玩家的tag值
    public PlayerType _currentType = PlayerType.None;//当前玩家类型
	public int seatindex;

    public LayerMask m_handCard_layer;//手牌的层
    public Camera camera_3D;//3D相机，不渲染自身的手牌和插排动画
    public Camera camera_2D;//2D相机，只渲染手牌

    public bool isPeng = false;
    public delegate void ChuPaiDelegate(HandCardItem item, bool isMoNi);
    public event ChuPaiDelegate chuPaiEvent;

    public GameObject _huPaiHand = null;
    public GameObject _huEffect = null;
    
    //每次碰杠胡，出生点左移,只能移动三次，当下一局开始时，需要回到初始位置
    private int m_pengOrGangMoveCount = 0;
    private Vector3 m_HandCardPlace_StartPos;
    private Vector3 m_HandCardMgr_StartPos;

    Transform huPaiSpawn;

	int INVALID_ID = 11;

    [SerializeField]
    private Material m_shouMaterial;

    public void ResetInfo() {
        m_pengOrGangMoveCount = 0;
        _HandCardPlace.position = m_HandCardPlace_StartPos;
        _HandCardPlace.localRotation = Quaternion.identity;
		_MoHandPos.localRotation = Quaternion.identity;
        this.transform.position = m_HandCardMgr_StartPos;
        isPeng = false;
        newIndex = -1;
        oldIndex = -1;
        idArray.Clear();
        _handCardList.Clear();

        Transform[] trans = _HandCardPlace.GetComponentsInChildren<Transform>();
		for (int i = trans.Length - 1; i >= 0; i--) {
            if (trans[i] != _HandCardPlace)
                DestroyImmediate(trans[i].gameObject);
        }

		Transform[] _trans = _flowerPlace.GetComponentsInChildren<Transform>();
		for (int i = _trans.Length - 1; i >= 0; i--) {
			if (_trans[i] != _flowerPlace)
				DestroyImmediate(_trans[i].gameObject);
		}

		if (_MoHand != null && _MoHand.valid())
			_MoHand.destroy();
		
        _MoHand = null;
        currentObj = null;
        
        if (huPaiSpawn != null) {
            Transform[] tranArray = huPaiSpawn.GetComponentsInChildren<Transform>();
            for (int i = tranArray.Length - 1; i >= 0; i--) {
                if (tranArray[i] != huPaiSpawn)
                    DestroyImmediate(tranArray[i].gameObject);
            }
        }
    }

    void Awake() {
        m_handCard_layer = LayerMask.NameToLayer("HandCard");
    }

    void Start () {
        _HandCardPlace =  transform.parent.Find("HandCardPlace").transform;
        InitTagValue();
        camera_3D = Camera.main;
        camera_2D = camera_3D.transform.Find("Camera").GetComponent<Camera>();
        m_HandCardPlace_StartPos = _HandCardPlace.position;
        m_HandCardMgr_StartPos = transform.position;
        huPaiSpawn = transform.parent.Find("HuPaiSpwan");
    }

	bool isMyself() {
		return RoomMgr.GetInstance ().seatindex == seatindex;
	}

    void InitTagValue() {
        tagValue = string.Empty;
        switch (_currentType) {
			case PlayerType.East:
				tagValue = "East";
                break;
            case PlayerType.West:
                tagValue = "West";
                break;
            case PlayerType.South:
                tagValue = "South";
                break;
            case PlayerType.North:
                tagValue = "North";
                break;
        }
    }

	HandCardItem GetHandCardItemByObj(GameObject obj) {
		if (obj == null)
			return null;

		HandCardItem item = null;

		for (int i = 0; i < _handCardList.Count; i++) {
			item = _handCardList[i];

			if (item != null && item.checkObj(obj))
				return item;
		}

		item = _MoHand;
		if (item != null && item.checkObj (obj))
			return item;

		return null;
	}

	HandCardItem GetHandCardItemById(int id) {
		HandCardItem item = null;

		item = _MoHand;
		if (item != null && item.checkId (id))
			return item;

		for (int i = 0; i < _handCardList.Count; i++) {
			item = _handCardList[i];

			if (item != null && item.checkId (id))
				return item;
		}

		return null;
	}

	HandCardItem GetRandomHandCardItem(int id) {
		int cnt = _handCardList.Count;
		System.Random rd = new System.Random ();

		int start = _MoHand != null ? -1 : 0;
		int off = rd.Next (start, cnt);

		HandCardItem item = null;

		if (-1 == off) {
			item = _MoHand;
		} else {
			item = _handCardList[off];
		}

		GameObject obj = ResourcesMgr.GetInstance().LoadMJ(id);
		obj.layer = m_handCard_layer;
		obj.gameObject.tag = tagValue;
		obj.transform.SetParent(_HandCardPlace);
		HandCardItem card = new HandCardItem(id, obj);

		obj.transform.localPosition = item.getObj ().transform.localPosition;
		obj.transform.localRotation = item.getObj ().transform.localRotation;

		if (-1 == off) {
			_MoHand = card;
		} else {
			_handCardList[off] = card;
		}

		item.destroy();
		return card;
	}

	bool isHoldsValid() {
		return RoomMgr.GetInstance().seats[seatindex].isHoldsValid();
	}

	int GetOldIndex() {
		HandCardItem item = null;

		item = _MoHand;
		if (item != null && item.getLayer () == "ZhuoPai")
			return -1;

		for (int i = 0; i < _handCardList.Count; i++) {
			item = _handCardList[i];

			if (item != null && item.getLayer() == "ZhuoPai")
				return i;
		}

		return -2;
	}

	void onMJClicked(GameObject ob) {
		InteractMgr im = InteractMgr.GetInstance();
		HandCardItem item = GetHandCardItemByObj(ob);

		if (item == null)
			return;
		
		if (item.interactable()) {
			im.onMJClicked (item);
			currentObj = ob;
		}
	}

    void Update () {
		ReplayMgr replay = ReplayMgr.GetInstance();
		if (replay.isReplay() || !isMyself())
			return;
		
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = camera_2D.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, m_handCard_layer)) {
				GameObject ob = hit.collider.gameObject;

				if (ob.CompareTag (tagValue)) {
					onMJClicked (ob);

					Vector3 pos = ob.transform.position;
					moved = false;
					oldPosition = pos;
					screenPosition = camera_2D.WorldToScreenPoint(pos);
					offset = pos - camera_2D.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z));

					draging = true;
				}
            }
        }

		if (currentObj == null)
			return;

		Vector3 currentPos = currentObj.transform.position;

		if (Input.GetMouseButtonUp(0)) {
			Vector3 off = currentPos - oldPosition;

			if (moved) {
				if (Math.Abs(off.x) > 0.017f || Math.Abs(off.y) > 0.025f)
					onMJClicked(currentObj);
				else
					currentObj.transform.position = oldPosition;
			}

			draging = false;
		} else if (draging) {
			Vector3 currentScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPosition.z);
			Vector3 currentPosition = camera_2D.ScreenToWorldPoint(currentScreenSpace) + offset;
				
			if (currentPosition != currentPos) {
				moved = true;

				Vector3 off = currentPosition - oldPosition;

				if (Math.Abs(off.x) > 0.017f || Math.Abs(off.y) > 0.025f)
					currentObj.transform.position = currentPosition + new Vector3(0, 0.04f, 0);
				else
					currentObj.transform.position = currentPosition;
			}
		}
    }

    public void SetLayer(LayerMask layer) {
        m_handCard_layer = layer;
    }

	public void RemoveMoHandCard(int id) {
		HandCardItem item = _MoHand;

		if (item != null)
			item.destroy();

		_MoHand = null;
	}

	public void ChuPai(int pai, Action cb) {
		GameObject ob = currentObj;
		HandCardItem item = null;

		int id = pai % 100;
		bool ting = pai > 100;

		if (ob != null) {
			item = GetHandCardItemByObj(ob);

			if (item != null && item.checkId (id)) {
				item.setTing(ting);

				item.setCB(cb);
				chuPaiEvent (item, true);
				currentObj = null;
				return;
			} else if (item == null) {
				Debug.LogError ("id not found");
			} else {
				Debug.LogError ("id wrong");
			}
		}

		currentObj = null;

		bool valid = isHoldsValid();

		if (valid)
			item = GetHandCardItemById (id);
		else
			item = GetRandomHandCardItem (id);

		if (item != null) {
			item.setTing(ting);

			item.setCB(cb);
			chuPaiEvent(item, true);
			return;
		}

		Debug.LogError ("chupai not found: " + id);
    }

	public int GetRandomIndex() {

		int cnt = _handCardList.Count;
		System.Random rd = new System.Random ();

		return rd.Next (0, cnt + 1);
	}

	public int GetIndexByItem(HandCardItem targetItem) {
        int index = -1;
        int key = int.MaxValue;

		HandCardItem item = null;
		int tid = targetItem.getId();

        for (int i = 0; i < _handCardList.Count;i++) {
			item = _handCardList[i];

			int id = item.getId();

            if (tid == id) {
                index = i;
                break;
            } else if (tid < id && key > id) {
                index = i;
                key = id;
            }
        }

		if (index == -1)
            index = _handCardList.Count;

        return index;
    }

    public void chapai() {
		oldIndex = GetOldIndex();
		if (oldIndex == -2) {
			Debug.LogError("oldindex error");
			return;
		}

		HandCardItem item = null;

		if (oldIndex != -1)
			item = _handCardList[oldIndex];
		else
			item = _MoHand;

        if (isPeng) {   //碰牌以后，直接打牌，不需要摸牌，也不能插牌
			if (oldIndex != -1) {
				_handCardList.RemoveAt (oldIndex);
			} else {
                _MoHand = null;
            }
            
            isPeng = false;
            UpdateHandCard();

			item.invoke();
        } else if (oldIndex != -1 && _MoHand != null) {       //如果需要插牌，则执行插牌
			newIndex = isHoldsValid() ? GetIndexByItem(_MoHand) : GetRandomIndex();
            if (newIndex > oldIndex)
                newIndex--;

            if (newIndex == oldIndex && newIndex == 13)
                newIndex--;

			item.invoke();
			ChaPai(newIndex, _MoHand.getObj());
        } else if (oldIndex == -1) {
			_MoHand = null;

			item.invoke();
        } else {
			_handCardList.RemoveAt(oldIndex);
			UpdateHandCard();

			item.invoke();
        }
    }

    public void MoveHandCard() {
		HandCardItem item = null;

        if (newIndex == oldIndex) {

        } else if (newIndex < oldIndex) {
            //newIndex~oldIndex，数组中后移，手牌上右移
            GameObject obj = new GameObject("tempParent");
            obj.transform.SetParent(_HandCardPlace);//获取父节点的方式有问题
            obj.transform.localPosition = Vector3.zero;
            obj.transform.rotation = _HandCardPlace.rotation;
            for (int i = oldIndex - 1; i >= newIndex; i--) {
				item = _handCardList[i];
				if (item.getLayer () == "ZhuoPai") {
					Debug.LogError ("move handcard zhuopai");
					continue;
				}

				_handCardList[i + 1] = item;
				item.getObj().transform.SetParent(obj.transform);
            }
            
            obj.transform.Translate(-offSetX, 0, 0);
            Transform[] tran = obj.GetComponentsInChildren<Transform>();
            for (int j = 0; j < tran.Length; j++)
            {
                if (!tran[j].gameObject.Equals(obj.gameObject))
                {
                    tran[j].transform.SetParent(_HandCardPlace);//获取父节点的方式有问题
                }
            }
            
            Destroy(obj);
        }
        else if (newIndex > oldIndex)
        {

            //oldIndex~list.count:数组前移，牌桌上的手牌左移，item入数组尾部
            GameObject obj = new GameObject("tempParent");
            obj.transform.SetParent(_HandCardPlace);//获取父节点的方式有问题
            obj.transform.localPosition = Vector3.zero;
            obj.transform.rotation = _HandCardPlace.rotation;
            for (int i = oldIndex; i < newIndex; i++)
            {
				item = _handCardList[i + 1];
				if (item.getLayer () == "ZhuoPai") {
					Debug.LogError ("move handcard2 zhuopai");
					continue;
				}

				_handCardList[i] = item;
				item.getObj().transform.SetParent(obj.transform);
            }
            obj.transform.Translate(offSetX, 0, 0);
            Transform[] tran = obj.GetComponentsInChildren<Transform>();
            for (int j = 0; j < tran.Length; j++)
            {
                if (!tran[j].gameObject.Equals(obj.gameObject))
                {
                    tran[j].transform.SetParent(_HandCardPlace);//获取父节点的方式有问题
                }
            }
            
            Destroy(obj);
        }
        
        _handCardList[newIndex] = _MoHand;
    }
   
    public void ChaPai(int needIndex, GameObject obj) {
        //创建手，设置手的位置，即，确定要插入的位置
        //拿起摸到的牌
        //将摸到的牌放在手上
        //将手牌左移或者右移
		Debug.Log("[" + seatindex + "]插牌下标：" + newIndex);
        int handIndex = 13 - (_handCardList.Count-needIndex)+1;
        string name = strChaPaiHand + handIndex.ToString();
        GameObject hand = ResourcesMgr.mInstance.InstantiateGameObjectWithType(name, ResourceType.Hand);
/*
        if (m_handCard_layer==(LayerMask.NameToLayer("Self")))
        {
            foreach (var tran in hand.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                tran.material = ResourcesMgr.mInstance.M_transparent;
            }
        }
        else
        {
            foreach (var tran in hand.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                tran.material = m_shouMaterial;
            }
        }
*/

        hand.transform.rotation = _MoHandPos.rotation;
        hand.transform.position = _MoHandPos.TransformPoint(-0.011f, -0.0229f, 0.257f);

        DHM_HandAnimationCtr handAniCtr = hand.GetComponent<DHM_HandAnimationCtr>();
        handAniCtr.PlayChaPaiAnimation(obj);
        handAniCtr.moveHandEvent += MoveHandCard;
        handAniCtr.chaPaiEndEvent += ChaPaiEndEventHandle;
        //handAniCtr.Set_HandCardPlacePos(_HandCardPlace.gameObject);
    }

    void ChaPaiEndEventHandle(DHM_HandAnimationCtr hand)
    {
        hand.moveHandEvent -= MoveHandCard;
        hand.chaPaiEndEvent -= ChaPaiEndEventHandle;
        float x = _handCardList.Count / 2.0f - newIndex;

		Transform tm = _MoHand.getObj().transform;

		tm.SetParent(_HandCardPlace);
        tm.localPosition = Vector3.zero;
        tm.localRotation = Quaternion.Euler(Vector3.zero);
        tm.Translate(offSetX * x, 0, 0);

        _MoHand = null;
        ResourcesMgr.mInstance.RemoveGameObject(hand.gameObject);
    }

	public void Chi(int id) {

		if (m_pengOrGangMoveCount < 3) {
			m_pengOrGangMoveCount++;
			_HandCardPlace.transform.Translate(-1.5f * offSetX, 0, 0);
			transform.Translate(-1.5f * offSetX, 0, 0);
		}

		int pai = id % 100;
		int type = id / 100;
		int begin = pai - type;
		List<int> arr = new List<int> ();

		if (isHoldsValid()) {
			for (int i = 0; i < 3; i++) {
				if (begin + i != pai)
					arr.Add (begin + i);
			}
		} else {
			arr.Add(INVALID_ID);
			arr.Add(INVALID_ID);
		}

		isPeng = true;
		RemoveGameObj(arr[0], 1);
		RemoveGameObj(arr[1], 1);
	}

    public void Peng(int id) {

        if (m_pengOrGangMoveCount < 3) {
            m_pengOrGangMoveCount++;
            _HandCardPlace.transform.Translate(-1.5f * offSetX, 0, 0);
            transform.Translate(-1.5f * offSetX, 0, 0);
        }

		int pai = id % 100;

		if (!isHoldsValid())
			pai = INVALID_ID;

        isPeng = true;
        RemoveGameObj(pai, 2);
    }

	void MotoCardList() {

		if (_MoHand != null) {
			GameObject ob = _MoHand.getObj();
			ob.transform.SetParent (_HandCardPlace);
			_handCardList.Add (_MoHand);
			_MoHand = null;

			_handCardList.Sort ((a, b) => a.getId () - b.getId ());

			UpdateHandCard ();
		}
	}

	public void Gang(int id, int type) {

		if (type <= 2 && m_pengOrGangMoveCount < 3) {
			m_pengOrGangMoveCount++;
			_HandCardPlace.transform.Translate(-1.5f * offSetX, 0, 0);
			transform.Translate(-1.5f * offSetX, 0, 0);
		}

		int pai = id % 100;

		if (!isHoldsValid())
			pai = INVALID_ID;

		if (1 == type) {
			RemoveGameObj (pai, 3);
		} else if (2 == type) {
			RemoveGameObj (pai, 4);

			MotoCardList ();
		} else if (3 == type) {
			RemoveGameObj (pai, 1);

			MotoCardList ();
		}
	}

    public void RemoveGameObj(int id, int Number) {
		id = id % 100;

        int count = 0;
		HandCardItem item = null;

        for (int i = _handCardList.Count - 1; i >= 0; i--) {
			item = _handCardList[i];
			if (item.checkId(id)) {
                count++;
				item.destroy();
                _handCardList.RemoveAt(i);
                if (count == Number) {
                    UpdateHandCard();
                    return;
                }
            }
        }

		item = _MoHand;

		if (item != null && item.checkId(id)) {
			count++;
			item.destroy();
			_MoHand = null;
		} else {
			Debug.LogError ("mohand id != " + id.ToString());
		}

		if (count != Number)
			Debug.LogError ("count < number!!!!");

		UpdateHandCard();
    }

	public void Ting(bool silent = false) {
		bool replay = ReplayMgr.GetInstance ().isReplay ();
		Transform tm = _HandCardPlace.transform;

		if (!silent)
			AudioManager.GetInstance().PlayEffectAudio("ting2");

		if (!isMyself() && !replay) {
			tm.Translate (0, 0.0225f, 0);
			tm.Rotate (90, 0, 0);
		}
	}

	void UpdateHandCard(bool silent = false) {
		if (!silent)
			AudioManager.GetInstance().PlayEffectAudio("sort");

		HandCardItem item = null;
        for (int i = 0; i < _handCardList.Count; i++) {
            float x =_handCardList.Count / 2.0f -i ;

			item = _handCardList[i];

			if (item.getLayer () == "ZhuoPai") {
				Debug.LogError("UpdateHandCard zhuopai");
				continue;
			}

			Transform tm = item.getObj().transform;
            tm.localPosition = Vector3.zero;
			tm.localRotation = Quaternion.Euler (Vector3.zero);
            tm.Translate(offSetX * x, 0, 0);
        }

		bool replay = ReplayMgr.GetInstance().isReplay();

		_MoHandPos.localPosition = !isMyself() && replay ? new Vector3(0, 0, 0.04f) : Vector3.zero;
        _MoHandPos.Translate(-(_handCardList.Count/2.0f +0.5f)*offSetX, 0, 0);
    }

    public void SetMoHandCard(int id, GameObject go = null)
    {
		if (id == -1)
			id = INVALID_ID;

		if (go == null)
			go = ResourcesMgr.GetInstance ().LoadMJ (id);

		if (_MoHand != null)
			Debug.LogError ("[" + seatindex + "]SetMoHandCard error!!!!!!!!!");

		_MoHand = new HandCardItem(id, go);
		GameObject obj = _MoHand.getObj();
        obj.layer = m_handCard_layer;
        obj.tag = tagValue;
		obj.transform.SetParent(_MoHandPos);
        obj.transform.rotation = _MoHandPos.rotation;
        obj.transform.position = _MoHandPos.TransformPoint(0.0731f*offSetX, 0, 0);
    }

	void hideMopai() {
		HandCardItem item = _MoHand;
		if (item != null && item.valid())
			item.destroy();

		_MoHand = null;
	}

	public void sync() {
	    RoomMgr rm = RoomMgr.GetInstance();
		ResetInfo();

		SeatInfo seat = rm.seats[seatindex];

		bool valid = seat.isHoldsValid();

		List<int> holds = new List<int>(seat.holds);
		int cnt = seat.getHoldsLen();
		int mopai = 0;

		if (!valid) {
			for (int i = 0; i < cnt; i++)
				holds.Add(INVALID_ID);
		}

		if (cnt % 3 == 2) {
			mopai = holds[cnt - 1];
			holds.RemoveAt (cnt - 1);
		}

		bool replay = ReplayMgr.GetInstance().isReplay();
		if (!isMyself() && replay) {
			_HandCardPlace.transform.Translate(0, 0, 0.04f);
			_HandCardPlace.transform.Rotate(-90, 0, 0);

			_MoHandPos.transform.Rotate(-90, 0, 0);
		}

		holds.Sort((a, b)=> a-b);
		idArray = holds;

		for (int i = 0; i < holds.Count; i++) {
			int id = holds[i];
			GameObject obj = ResourcesMgr.GetInstance().LoadMJ(id);

			obj.layer = m_handCard_layer;
			obj.gameObject.tag = tagValue;
			obj.transform.SetParent(_HandCardPlace);
			obj.transform.Rotate(90, 0, 0);
			HandCardItem item = new HandCardItem(id, obj);
			_handCardList.Add(item);
		}

		UpdateHandCard(true);

		if (seat.tingpai)
			Ting(true);

		showFlowers();

		cnt = rm.seats[seatindex].getCPGCnt();
		m_pengOrGangMoveCount = cnt < 3 ? cnt : 3;
		if (cnt > 0) {
			_HandCardPlace.transform.Translate(-1.5f * cnt * offSetX, 0, 0);
			transform.Translate(-1.5f * cnt * offSetX, 0, 0);
		}

		if (mopai > 0)
			SetMoHandCard(mopai);
	}

	public void FaPai()
	{
		StartCoroutine(_Fapai());
	}

	public void Dance() {
		Sequence seq = DOTween.Sequence();

		float stick = 0.4f;
		float off = 0.05f;
		float duration = 0.1f;

		List<HandCardItem> items = new List<HandCardItem>(_handCardList);

		if (_MoHand != null)
			items.Add(_MoHand);

		stick *= duration;

		for(int i = 0; i < items.Count; i++) {
			Transform tm = items[i].getObj().transform;

			seq.Insert(i * stick, tm.DOLocalMoveY(off, duration).SetEase(Ease.Linear));
			seq.Insert(i * stick, tm.DOLocalMoveY(0, duration).SetEase(Ease.Linear).SetDelay(duration));
		}

		seq.OnComplete (() => {
			RoomMgr rm = RoomMgr.GetInstance();
			if (seatindex == rm.seatindex)
				InteractMgr.GetInstance().checkChuPai(rm.isMyTurn());
		});

		seq.Play();
	}

	public IEnumerator _Fapai() {
		RoomMgr rm = RoomMgr.GetInstance();
		ResetInfo();

		SeatInfo seat = rm.seats[seatindex];
		bool valid = seat.isHoldsValid();

		List<int> holds = new List<int>(seat.holds);
		int cnt = seat.getHoldsLen();
		int mopai = 0;

		if (!valid) {
			for (int i = 0; i < cnt; i++)
				holds.Add(INVALID_ID);
		}

		if (cnt % 3 == 2) {
			mopai = holds[cnt - 1];
			holds.RemoveAt (cnt - 1);
		}

		bool replay = ReplayMgr.GetInstance().isReplay();
		if (!isMyself() && replay) {
			_HandCardPlace.transform.Translate(0, 0, 0.04f);
			_HandCardPlace.transform.Rotate(-90, 0, 0);

			_MoHandPos.transform.Rotate(-90, 0, 0);
		}

		holds.Sort((a, b)=> a-b);
		idArray = holds;

		for (int i = 0; i < holds.Count; i++) {
			int id = holds[i];
			float x = holds.Count / 2.0f - i;
			GameObject obj = ResourcesMgr.GetInstance().LoadMJ(id);
			Transform tm = obj.transform;

			obj.layer = m_handCard_layer;
			obj.gameObject.tag = tagValue;
			tm.SetParent(_HandCardPlace);

			HandCardItem item = new HandCardItem(id, obj);

			item.setInteractable(false, false);
			_handCardList.Add(item);

			tm.localPosition = Vector3.zero;
			tm.localRotation = Quaternion.Euler(Vector3.zero);
			tm.Translate(offSetX * x, 0, 0);

			yield return new WaitForSeconds(0.05f);
		}

		_MoHandPos.localPosition = !isMyself() && replay ? new Vector3(0, 0, 0.04f) : Vector3.zero;
		_MoHandPos.Translate(-(holds.Count/2.0f +0.5f)*offSetX, 0, 0);

		if (mopai > 0) {
			SetMoHandCard (mopai);
			_MoHand.setInteractable(false, false);
		}

		Dance();
	}

    // 注册插牌事件，当出牌动画执行完毕自动调用
    public void ChuPaiCallBackEventHandle(GameObject go)
    {
        DHM_HandAnimationCtr handCtr = go.GetComponent<DHM_HandAnimationCtr>();
        handCtr.chaPaiEvent += chapai;
    }

	public void HuPai(HuPushInfo info, Action cb)
    {
        StartCoroutine(_HuPai(info, cb));
    }

	IEnumerator _HuPai(HuPushInfo info, Action cb)
    {
		int layer = LayerMask.NameToLayer("ZhuoPai");
		List<int> holds = info.holds;

		holds.Sort ((a, b) => a - b);

		for (int i = 0; i < _handCardList.Count && i < holds.Count; i++) {
			HandCardItem item = _handCardList[i];

			int id = holds[i];
			GameObject obj = ResourcesMgr.GetInstance().LoadMJ(id);
			HandCardItem card = new HandCardItem (id, obj);
			obj.layer = layer;
			obj.tag = tagValue;
			obj.transform.SetParent (_HandCardPlace);

			obj.transform.localRotation = item.getObj().transform.localRotation;
			obj.transform.localPosition = item.getObj().transform.localPosition;

			_handCardList[i] = card;
			item.destroy();
		}

		if (_MoHand != null)
			_MoHand.destroy();
		
		if (true) {
			int id = info.hupai;
			GameObject obj = ResourcesMgr.GetInstance ().LoadMJ (id);
			_MoHand = new HandCardItem (id, obj);
			obj.layer = layer;
			obj.tag = tagValue;
			obj.transform.SetParent (_MoHandPos);
			obj.transform.rotation = _MoHandPos.rotation;
			obj.transform.position = _MoHandPos.TransformPoint (0.0731f * offSetX, 0, 0);
		}

		bool anim = true;
		bool replay = ReplayMgr.GetInstance().isReplay();
		if (isMyself ()) {
			_HandCardPlace.transform.Translate (0, 0.0225f, 0);
			_HandCardPlace.transform.Rotate (90, 0, 0);

			_MoHandPos.transform.Translate(0, 0, 0.04f);
			_MoHandPos.transform.Rotate (-90, 0, 0);
		} else {
			if (replay) {
				anim = false;
			} else {
				_MoHandPos.transform.Translate(0, 0, 0.04f);
				_MoHandPos.transform.Rotate (-90, 0, 0);
			}
		}

/*
		if (huPaiSpawn == null)
            huPaiSpawn = this.transform.parent.Find("HuPaiSpwan");

        GameObject effectObj = Instantiate(_huEffect);
        effectObj.SetActive(true);
        effectObj.transform.position = huPaiSpawn.position;

		GameObject huCard = ResourcesMgr.GetInstance ().LoadMJ (id);
        huCard.transform.rotation = huPaiSpawn.rotation;
        huCard.transform.position = huPaiSpawn.position;
        huCard.transform.SetParent(huPaiSpawn);
*/

		if (anim) {
			Transform huHandSpawn = this.transform.parent.Find ("HandSpawn");
			GameObject huHand = ResourcesMgr.mInstance.InstantiateGameObjectWithType ("HupaiHand", ResourceType.Hand);
			huHand.transform.rotation = huHandSpawn.rotation;
			huHand.transform.position = huHandSpawn.position;
			huHand.GetComponent<DHM_HandAnimationCtr> ().huPaiEvent += HuPaiEventHandle;
		}

        yield return new WaitForSeconds(4.0f);
        cb();
    }

    public void HuPaiEventHandle(GameObject go)
    {
		_HandCardPlace.transform.Translate(0, 0.04f, 0.0225f);
        _HandCardPlace.transform.Rotate(-180, 0, 0);
    }

	public void UpdateFlowers() {
		MainViewMgr mm = MainViewMgr.GetInstance();
		mm.updateFlowers(seatindex);

		showFlowers();
	}

	public void AddFlower(int id, Action cb) {
		StartCoroutine(_AddFlower(id, cb));
	}

	public IEnumerator _AddFlower(int id, Action cb) {
		SetMoHandCard(id);
		AudioManager.GetInstance().PlayEffectAudio("buhua");

		yield return new WaitForSeconds(0.5f);

		hideMopai ();

		cb();

		MainViewMgr mm = MainViewMgr.GetInstance();

		mm.updateFlowers(seatindex);
		mm.showAction(seatindex, "add_flower", id);

		showFlowers();
	}

	void showFlowers() {
		RoomMgr rm = RoomMgr.GetInstance ();
		List<int> flowers = rm.seats[seatindex].flowers;
		int cnt = flowers.Count;
		int childs = _flowerPlace.childCount;

		for (int i = 0; i < childs; i++) {
			Transform tm = _flowerPlace.GetChild(i);

			tm.localPosition = new Vector3(offSetX * (cnt - i), 0, 0);
		}

		for (int i = childs; i < cnt; i++) {
			int id = flowers[i];
			GameObject obj = ResourcesMgr.GetInstance().LoadMJ(id);

			obj.layer = LayerMask.NameToLayer("ZhuoPai");
			obj.gameObject.tag = tagValue;

			Transform tm = obj.transform;

			tm.SetParent(_flowerPlace);
			tm.localRotation = Quaternion.Euler(-90, 0, 0);
			tm.localPosition = new Vector3(offSetX * (cnt - i), 0, 0);
			tm.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		}
	}
}


