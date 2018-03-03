

using UnityEngine;
using System;
using System.Collections;
//using GameProtocol;

public class DHM_HandAnimationCtr : MonoBehaviour {

    public Transform _handright_point = null;
    private GameObject _currentObj = null;
    private Transform _RecyleHandCardMgrPos = null;
    private Transform _HandCardPlacePos = null;
    public delegate void MoveHandDelegate();
    public event MoveHandDelegate moveHandEvent;

    public delegate void ChaPaiDelegate();
    public event ChaPaiDelegate chaPaiEvent;

    public delegate void ChaPaiEndDelegate(DHM_HandAnimationCtr hand);
    public event ChaPaiEndDelegate chaPaiEndEvent;

    public delegate void MoPaiDelegate();
    public event MoPaiDelegate moPaiEvent;

    public delegate void HuPaiDelegate(GameObject go);
    public event HuPaiDelegate huPaiEvent;
    //被桌牌注册
    public delegate void ChuPaiEndDelegate();
    public event ChuPaiEndDelegate chuPaiEndEvent;
        
	bool playing = false;

    public int id = -1;
    public bool isMoNiMoPai = false;
    public void PlayChaPaiAnimation(GameObject obj)
    {
        _currentObj = obj;
        _currentObj.transform.SetParent(_handright_point);
        _currentObj.transform.localPosition = Vector3.zero;
        _currentObj.transform.localRotation = Quaternion.Euler(-90, 0, 0);

		playing = true;

    }
    public void MoveHand()
    {
        if(moveHandEvent!=null)
        {
            moveHandEvent();
        }
    }
    public void FinishChaPaiAnimation()
    {
        _currentObj = null;
        if (chaPaiEndEvent != null)
            chaPaiEndEvent(this);
    }
    public void SetMoNiMoPai(bool isMoni)
    {
        isMoNiMoPai = isMoni;
    }
		
    public void PlayChuPaiAnimation(HandCardItem item)
    {
		_currentObj = item.getObj();
		id = item.getId();
        _currentObj.transform.SetParent(_handright_point);
        _currentObj.transform.localPosition = Vector3.zero;
        _currentObj.transform.localRotation = Quaternion.Euler(90, 0, 0);

    }

    public void FinishChuPaiAnimation()
    {
        if (isMoNiMoPai)
            Debug.LogWarning("DHM_HandAnimationCtr+模拟出牌的ID：" + id);
        else
        {
            Debug.LogWarning("DHM_HandAnimationCtr+出的牌的ID：" + id);
        }
        //if (_RecyleHandCardMgrPos==null)
        //{
        //    _currentObj.transform.SetParent(null);
        //}
        //else
        //{
        //    if(_currentObj==null)
        //    {
        //        Debug.LogError(":"+id);
        //        //Time.timeScale = 0;
        //    }
            //_currentObj.transform.SetParent(_RecyleHandCardMgrPos);
            if (chuPaiEndEvent != null)
                chuPaiEndEvent();//回调出牌结束方法
        //}
        _currentObj = null;
        if(chaPaiEvent!=null)
        {
            chaPaiEvent();
        }
/* fuck build
        if(id!=-1 && !isMoNiMoPai)
        {
            NetManager.m_Instance.SendMessage(Protocol.TYPE_FIGHT, ((int)MainViewMgr.m_Instance.m_MySeat) + 1, FightProtocol.PUT_CREQ, id);
        }
 */
        Debug.Log("RemoveSelf 2");
        ResourcesMgr.mInstance.RemoveGameObject(this.gameObject);
		playing = false;

    }
    public void Set_RecyleHandCardMgrPos(GameObject tran)
    {
        _RecyleHandCardMgrPos = tran.transform;
    }
    public void Set_HandCardPlacePos(GameObject tran)
    {
        _HandCardPlacePos = tran.transform;
    }

    public void HuPaiEvent(GameObject go)
    {
        if (huPaiEvent != null)
            huPaiEvent(go);
        AnimationClip clip = this.gameObject.GetComponent<Animation>().GetClip("hupai");
        //Destroy(this.gameObject, clip.length);
    }

	public void Stop() {
		if (playing) {
			Animation anim = GetComponent<Animation>();
			anim.Stop();
			RemoveSelf();
		}
	}

    public void RemoveSelf()
    {
        //Destroy(this.gameObject);
        //this.GetComponent<Animation>()
        Debug.Log("RemoveSelf");
        ResourcesMgr.mInstance.RemoveGameObject(this.gameObject);
		playing = false;
    }

    private void OnDisable()
    {
		if (moPaiEvent != null) {
			Delegate[] dels = moPaiEvent.GetInvocationList();
			foreach (Delegate d in dels)
				moPaiEvent -= d as MoPaiDelegate;
		}

		if (moveHandEvent != null) {
			Delegate[] dels = moveHandEvent.GetInvocationList();
			foreach (Delegate d in dels)
				moveHandEvent -= d as MoveHandDelegate;
		}

		if (chaPaiEndEvent != null) {
			Delegate[] dels = chaPaiEndEvent.GetInvocationList();
			foreach (Delegate d in dels)
				chaPaiEndEvent -= d as ChaPaiEndDelegate;
		}

		if (chaPaiEvent != null) {
			Delegate[] dels = chaPaiEvent.GetInvocationList();
			foreach (Delegate d in dels)
				chaPaiEvent -= d as ChaPaiDelegate;
		}

		if (huPaiEvent != null) {
			Delegate[] dels = huPaiEvent.GetInvocationList();
			foreach (Delegate d in dels)
				huPaiEvent -= d as HuPaiDelegate;
		}

		if (chuPaiEndEvent != null) {
			Delegate[] dels = chuPaiEndEvent.GetInvocationList();
			foreach (Delegate d in dels)
				chuPaiEndEvent -= d as ChuPaiEndDelegate;
		}

		moPaiEvent = null;
		moveHandEvent = null;
		chaPaiEndEvent = null;
		chaPaiEvent = null;
		huPaiEvent = null;
		chuPaiEndEvent = null;

		Debug.Log("OnDisable");
    }
}
