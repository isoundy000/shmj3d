
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HandCardItem {
    int _id;
    GameObject _obj;
	HandCard _hc;
	bool _ting = false;

	public int _index = -1;

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

	public void setInteractable(bool enable) {
		if (_hc != null)
			_hc.setInteractable(enable);
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

		Debug.Log ("setTing");

		if (ting)
			_hc.ting();
		else
			_hc.resetColor();
	}

	public void resetColor() {
		if (_ting)
			setTing();
		else
			_hc.resetColor();
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

    public bool isState = false;//是否是当前玩家回合
    public LayerMask m_handCard_layer;//手牌的层
    public Camera camera_3D;//3D相机，不渲染自身的手牌和插排动画
    public Camera camera_2D;//2D相机，只渲染手牌

    public bool isPeng = false;
    public delegate void ChuPaiDelegate(HandCardItem item, bool isMoNi);
    public event ChuPaiDelegate chuPaiEvent;

    public GameObject _huPaiHand = null;
    public GameObject _huEffect = null;
    
    //2017-0317-添加，每次碰杠胡，出生点左移,只能移动三次，当下一局开始时，需要回到初始位置
    private int m_pengOrGangMoveCount = 0;
    private Vector3 m_HandCardPlace_StartPos;
    private Vector3 m_HandCardMgr_StartPos;

    Transform huPaiSpawn;

    [SerializeField]
    private Material m_shouMaterial;

    public bool IsState {
        get { return isState; }
        set { isState = value; }
    }

    public void ResetInfo() {
        m_pengOrGangMoveCount = 0;
        _HandCardPlace.position = m_HandCardPlace_StartPos;
        _HandCardPlace.localRotation = Quaternion.identity;
		_MoHandPos.localRotation = Quaternion.identity;
        this.transform.position = m_HandCardMgr_StartPos;
        IsState = false;
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

		//m_MoHand_StartPos = _MoHandPos.
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

			if (item != null && item.checkObj(obj)) {
				item._index = i;
				return item;
			}
		}

		item = _MoHand;
		if (item != null && item.checkObj (obj)) {
			item._index = -1;
			return item;
		}

		return null;
	}

	HandCardItem GetHandCardItemById(int id) {
		HandCardItem item = null;

		item = _MoHand;
		if (item != null && item.checkId (id)) {
			item._index = -1;
			return item;
		}

		for (int i = 0; i < _handCardList.Count; i++) {
			item = _handCardList[i];

			if (item != null && item.checkId (id)) {
				item._index = i;
				return item;
			}
		}

		return null;
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

		ReplayMgr replay = ReplayMgr.GetInstance();
		if (replay.isReplay ())
			return;

		InteractMgr im = InteractMgr.GetInstance();
		HandCardItem item = GetHandCardItemByObj(ob);

		if (item == null)
			return;
		
		if (item.interactable()) {
			Debug.Log ("onMJClicked");
			im.onMJClicked (item);
			currentObj = ob;
		}
	}

    void Update () {
		if (!isMyself ())
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

		if (Input.GetMouseButtonUp (0)) {
			Vector3 off = currentObj.transform.position - oldPosition;

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
				
			if (currentPosition != currentObj.transform.position) {
				moved = true;
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

    public void MoNiChuPai(int pai) {
		GameObject ob = currentObj;
		HandCardItem item = null;

		int id = pai % 100;
		bool ting = pai > 100;

		Debug.Log ("[" + seatindex + "]模拟出牌" + id);

		if (ob != null) {
			item = GetHandCardItemByObj(ob);

			if (item != null && item.checkId (id)) {
				oldIndex = item._index;

				if (ting)
					item.setTing(true);

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

		item = GetHandCardItemById(id);

		if (item != null) {
			oldIndex = item._index;

			if (ting)
				item.setTing(true);
			
			chuPaiEvent(item, true);
			return;
		}

		Debug.LogError ("monichupai not found: " + id);
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
		Debug.Log("[" + seatindex + "]chapai");

		// recaculate oldIndex
		oldIndex = GetOldIndex();
		if (oldIndex == -2) {
			Debug.LogError("oldindex error");
			return;
		}

        if (isPeng) {   //碰牌以后，直接打牌，不需要摸牌，也不能插牌
            if (oldIndex != -1)
                _handCardList.RemoveAt(oldIndex);
            else {
                _MoHand = null;
            }
            
            isPeng = false;
            UpdateHandCard();
            
            GameManager.m_instance.islock = false;
        } else if (oldIndex != -1 && _MoHand != null) {       //如果需要插牌，则执行插牌
            newIndex = GetIndexByItem(_MoHand);
            if (newIndex > oldIndex)
                newIndex--;

            if (newIndex == oldIndex && newIndex == 13)
                newIndex--;

			GameManager.m_instance.islock = false;
			ChaPai(newIndex, _MoHand.getObj());
        } else if (oldIndex == -1) {
			_MoHand = null;

            GameManager.m_instance.islock = false;
			Debug.LogWarning("[" + seatindex + "]打出莫的牌：" + GameManager.m_instance.islock);
        } else {
			_handCardList.RemoveAt(oldIndex);
			UpdateHandCard();

            GameManager.m_instance.islock = false;
			Debug.LogWarning("[" + seatindex + "]默认打开开关：" + GameManager.m_instance.islock);
        }
    }

    // 移动手牌
    public void MoveHandCard() {
		Debug.Log ("[" + seatindex + "]MoveHandCard");

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
        if(handAniCtr==null)
        {
            //hand.SendMessage("PlayChaPaiAnimation", obj, SendMessageOptions.RequireReceiver);
        }
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

		if (_MoHand.getLayer () == "ZhuoPai") {
			Debug.LogError("chapai error");
		}

		tm.SetParent(_HandCardPlace);
        tm.localPosition = Vector3.zero;
        tm.localRotation = Quaternion.Euler(Vector3.zero);
        tm.Translate(offSetX * x, 0, 0);

        _MoHand = null;
        ResourcesMgr.mInstance.RemoveGameObject(hand.gameObject);

		//GameManager.m_instance.islock = false;
    }

	public void Chi(int id) {
		Debug.Log("[" + seatindex + "]Chi" + id);
		if (m_pengOrGangMoveCount < 3) {
			m_pengOrGangMoveCount++;
			_HandCardPlace.transform.Translate(-1.5f * offSetX, 0, 0);
			transform.Translate(-1.5f * offSetX, 0, 0);
		}

		int pai = id % 100;
		int type = id / 100;
		int begin = pai - type;
		List<int> arr = new List<int> ();

		for (int i = 0; i < 3; i++) {
			if (begin + i != pai)
				arr.Add (begin + i);
		}

		isPeng = true;
		RemoveGameObj(arr[0], 1);
		RemoveGameObj(arr[1], 1);
	}

    public void Peng(int id) {
		Debug.Log("[" + seatindex + "]Peng" + id);
        if (m_pengOrGangMoveCount < 3) {
            m_pengOrGangMoveCount++;
            _HandCardPlace.transform.Translate(-1.5f * offSetX, 0, 0);
            transform.Translate(-1.5f * offSetX, 0, 0);
        }

		int pai = id % 100;

        isPeng = true;
        RemoveGameObj(pai, 2);
    }

	void MotoCardList() {
		Debug.Log("[" + seatindex + "]MotoCardList");
		if (_MoHand != null) {
			GameObject ob = _MoHand.getObj();
			ob.transform.SetParent (_HandCardPlace);
			_handCardList.Add (_MoHand);
			_MoHand = null;

			_handCardList.Sort ((a, b) => {
				return a.getId() - b.getId();
			});

			UpdateHandCard ();
		}
	}

	public void Gang(int id, int type) {
		Debug.Log("[" + seatindex + "]Gang id=" + id + "type=" + type);

		if (type <= 2 && m_pengOrGangMoveCount < 3) {
			m_pengOrGangMoveCount++;
			_HandCardPlace.transform.Translate(-1.5f * offSetX, 0, 0);
			transform.Translate(-1.5f * offSetX, 0, 0);
		}

		int pai = id % 100;

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

	public void UpdateHandCard(bool silent = false) {
		if (!silent)
			AudioManager.GetInstance().PlayEffectAudio("sort");

		HandCardItem item = null;
        for (int i = 0; i < _handCardList.Count; i++)
        {
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

    public void SetMoHandCard(int id, GameObject go=null)
    {
		Debug.LogWarning("[" + seatindex + "]摸牌" + id);
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

        GameManager.m_instance.islock = false;
    }

	void showMopai(int id) {
		GameObject ob = ResourcesMgr.GetInstance ().LoadMJ (id);
		_MoHand = new HandCardItem(id, ob);
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

		List<int> holds = new List<int>(seat.holds);
		int cnt = holds.Count;
		int mopai = 0;

		if (cnt % 3 == 2) {
			mopai = holds[cnt - 1];
			holds.RemoveAt (cnt - 1);
			Debug.Log ("mopai=" + mopai);
		}

		bool replay = ReplayMgr.GetInstance().isReplay();
		if (!isMyself() && replay) {
			_HandCardPlace.transform.Translate(0, 0, 0.04f);
			_HandCardPlace.transform.Rotate(-90, 0, 0);

			_MoHandPos.transform.Rotate(-90, 0, 0);
		}

		//SortList(holds); 
		holds.Sort((a, b)=> a-b);
		idArray = holds;

		for (int i = 0; i < idArray.Count; i++) {
			int id = idArray [i];
			GameObject obj = ResourcesMgr.GetInstance ().LoadMJ (id);

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
			SetMoHandCard (mopai);
	}

	#if UNIT_TEST

    public void FaPai()
    {
		_handCardList.Clear();

		Debug.Log ("FaPai");
		Debug.Log (idArray.Count);

		for (int i = 0; i < idArray.Count; i++)
        {
            GameObject obj = Instantiate(_handCardPrefab);

            obj.layer = m_handCard_layer;
            obj.gameObject.tag = tagValue;
            obj.transform.SetParent(_HandCardPlace);
            obj.transform.Rotate(90, 0, 0);
            HandCardItem item = new HandCardItem();
            item._obj = obj;
            _handCardList.Add(item);
        }
        UpdateHandCard();
        TestUV();
		Debug.Log ("FaPai done");
    }

    // 问题：如何获取 id 数组
    public void TestUV()
    {
        //string str = "1,21,3,4,13,21,16,17,18,21,23,24,25";
        //ParseString(str);
        TestUVOffSet(idArray);
        HideHandCard();

		Debug.Log ("active");
        StartCoroutine(ActiveHandCard());

    }

    // 解析字符串，得到id数组
    public void ParseString(string str)
    {
        string[] array = str.Split(',');
        //idArray = new int[array.Length];
        for (int i = 0; i < array.Length; i++)
        {
            idArray[i] = int.Parse(array[i]);
        }
    }

    public void SetIDArray(List<int> idList)
    {
		int cnt = idList.Count;
		if (cnt % 3 == 2)
			cnt--;

		idArray = new List<int>();

		for (int i = 0; i < cnt; i++)
			idArray.Add (idList [i]);
    }

    // 根据下标返回ID
    public int GetIdFromArrayAtIndex(int index)
    {
        if (idArray.Count != 0 && index < idArray.Count)
        {
            return idArray[index];
        }
        return -1;
    }
    public void TestUVOffSet(List<int> IdArray)
    {
		Debug.Log ("TestUVOffSet len=" + IdArray.Count);
        for (int i = 0; i < IdArray.Count; i++)
        {
            UVoffSet(IdArray[i], _handCardList[i]._obj);
            _handCardList[i]._id = idArray[i];
        }
    }
    void UVOffSet()
    {
        for (int i = 0; i < idArray.Count; i++)
        {
            UVoffSet(idArray[i], _handCardList[i]._obj);
            _handCardList[i]._id = idArray[i];
        }
    }
    /// <summary>
    /// 根据ID，给指定的牌进行贴图的UV偏移
    /// 1-9 ：  万
    /// 11-19： 条
    /// 21-29： 筒
    /// </summary>
    /// <param name="handCardId"></param>
    /// <param name="handCard"></param>
    public void UVoffSet(int handCardId, GameObject handCard)
    {
        UVoffSetWithReturn(handCardId, handCard);
    }
    public static GameObject UVoffSetWithReturn(int handCardId, GameObject handCard)
    {
		handCardId -= 10;

        int UVy = handCardId / 10;
        int UVx = handCardId % 10;
        if (UVy == 0)
        {
            UVy = 1;
        }
        else if (UVy == 1)
        {
            UVy = 0;
        }
        handCard.GetComponent<Renderer>().materials[1].mainTextureOffset = new Vector2((UVx - 1) * 0.1068f, -UVy * 0.168f);
        return handCard;
    }

    #region  对ID数组进行排序
    /// <summary>
    ///将手牌的ID排序，更新手牌UV偏移
    /// </summary>
    /// <param name="array"></param>
    /// <returns></returns>
    IEnumerator SortHandCard()
    {
        yield return new WaitForSeconds(0.2f);
        SortList(idArray);
        TestUVOffSet(idArray);
        yield break;
    }
    /// <summary>
    /// 将将指定数组排序
    /// </summary>
    /// <param name="array"></param>
    public void SortList(List<int> array)
    {
        Quick_Sort(array, 0, array.Count - 1);
    }
    /// <summary>
    /// 将ID排序
    /// </summary>
    public void SortList()
    {
        Quick_Sort(idArray, 0, idArray.Count - 1);
    }
    void Quick_Sort(List<int> array, int first, int last)
    {
        if (first < last)
        {
            int key = array[first];
            int low = first;
            int hight = last;
            while (low < hight)
            {
                while (low < hight && array[hight] >= key)
                {
                    hight--;
                }
                while (low < hight && array[low] <= key)
                {
                    low++;
                }
                int temp = array[low];
                array[low] = array[hight];
                array[hight] = temp;
            }
            array[first] = array[low];
            array[low] = key;
            Quick_Sort(array, first, low - 1);
            Quick_Sort(array, low + 1, last);
        }
    }
    #endregion

    // 隐藏手牌
    public void HideHandCard()
    {
		Debug.Log ("hide");
        for (int i = 0; i < _handCardList.Count; i++)
        {
            _handCardList[i]._obj.SetActive(false);
        }
    }

    // 激活手牌，四张一组激活
    public IEnumerator ActiveHandCard()
    {
        int count = 0;
        GameObject obj = new GameObject();
        _HandCardPlace.transform.Rotate(90,0,0);
        obj.transform.SetParent(_HandCardPlace);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.rotation = _HandCardPlace.rotation;
        for (int i = 0; i < _handCardList.Count; i++)
        {
            _handCardList[i]._obj.SetActive(true);
            _handCardList[i]._obj.transform.SetParent(obj.transform);
            if ((++count) % 4 == 0)
            {
                yield return StartCoroutine(RotateTo(obj.transform, new Vector3(-90, 0, 0)));
                Transform[] tran = obj.GetComponentsInChildren<Transform>();
                for (int j = 0; j < tran.Length; j++)
                {
                    if (!tran[j].gameObject.Equals(obj.gameObject))
                        tran[j].transform.SetParent(_HandCardPlace);

                }
                yield return new WaitForSeconds(0.5f);
                obj.transform.Rotate(90, 0, 0);
            }
            else if ((i+1) % 13 == 0)
            {

                yield return StartCoroutine(RotateTo(obj.transform, new Vector3(-90, 0, 0)));
                _handCardList[i]._obj.transform.SetParent(_HandCardPlace);
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForFixedUpdate();
        }

		for (int i = 0; i < _handCardList.Count; i++) {
			_handCardList [i]._obj.transform.SetParent (_HandCardPlace);
		}

        Destroy(obj.gameObject);
        foreach (var item in _handCardList)
        {
            item._obj.transform.Rotate(90, 0, 0);
        }
        yield return StartCoroutine(SortHandCard());
        _HandCardPlace.Rotate(-90, 0, 0);

		Debug.Log ("active done!");

        yield break;
    }

    // 旋转到指定的角度
    IEnumerator RotateTo(Transform target, Vector3 targetDirection)
    {
        Vector3 temp = target.TransformDirection(Vector3.forward).normalized;
        while (true)
        {
            if (Vector3.Dot(temp, target.TransformDirection(Vector3.forward).normalized) <0.01f)//<= Mathf.Cos(Mathf.PI * 90 / 180))
            {
                target.localRotation = Quaternion.Euler(-90,0,0);
                yield break;
            }
            target.Rotate(Vector3.Lerp(Vector3.zero, targetDirection, Time.deltaTime*5));
            yield return new WaitForFixedUpdate();
        }
    }

#endif

    // 注册插牌事件，当出牌动画执行完毕自动调用
    public void ChuPaiCallBackEventHandle(GameObject go)
    {
        DHM_HandAnimationCtr handCtr = go.GetComponent<DHM_HandAnimationCtr>();
        handCtr.chaPaiEvent += chapai;
    }

    public void HuPai(int id)
    {
        Debug.Log("胡牌" + this.name);

		int layer = LayerMask.NameToLayer("ZhuoPai");

        for (int i = 0; i < _handCardList.Count; i++)
			_handCardList[i].getObj().layer = layer;

		if (_MoHand == null) {
			GameObject obj = ResourcesMgr.GetInstance ().LoadMJ (id);
			_MoHand = new HandCardItem (id, obj);
			obj.layer = layer;
			obj.tag = tagValue;
			obj.transform.SetParent (_MoHandPos);
			obj.transform.rotation = _MoHandPos.rotation;
			obj.transform.position = _MoHandPos.TransformPoint (0.0731f * offSetX, 0, 0);
		} else
			_MoHand.setLayer("ZhuoPai");

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

	public void AddFlower(int id) {
		StartCoroutine(_AddFlower(id));
	}

	public IEnumerator _AddFlower(int id) {
		showMopai(id);
		AudioManager.GetInstance().PlayEffectAudio("buhua");

		yield return new WaitForSeconds(0.5f);

		hideMopai ();

		GameManager.GetInstance().islock = false;

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


