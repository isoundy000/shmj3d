﻿using UnityEngine;
using System.Collections;

public class PlayDicAnimationClick : MonoBehaviour {

	private UILabel Numlabel1;
	private UILabel Numlabel2;

    private int _number1 = 0;
	private int _number2 = 0;

    public void SetSaiZiNumber(int[] numbers)
    {
        _number1 = numbers[0];
        _number2 = numbers[1];
    }
	
	public void PlayFinshed(){
/*   fuck build
		fsm.SendEvent ("DeletePaiDuo");
        ShowRemainCard.instance.isShowCardCount = true;
        AudioManager.Instance.PlayEffectAudio("fa");
        PlayerManager.m_instance.m_EastPlayer.FaPai();
        PlayerManager.m_instance.m_SouthPlayer.FaPai();
        PlayerManager.m_instance.m_WestPlayer.FaPai();
        PlayerManager.m_instance.m_NorthPlayer.FaPai();
        ResourcesMgr.mInstance.RemoveGameObject(this.gameObject);
*/
		ResourcesMgr.mInstance.RemoveGameObject(this.gameObject);
	}

	public void PlayAnimation555(){
        CheckShaizi(_number1.ToString(), _number2.ToString());
	}

	void CheckShaizi(string num1,string num2){
		switch (num1) {
		case "1":
			PlayAnimation1 ("one");
			break;
		case "2":
			PlayAnimation1 ("two");
			break;
		case "3":
			PlayAnimation1 ("three");
			break;
		case "4":
			PlayAnimation1 ("four");
			break;
		case "5":
			PlayAnimation1 ("five");
			break;
		case "6":
			PlayAnimation1 ("six");
			break;
		default:
			break;
		}

		switch (num2) {
		case "1":
			PlayAnimation2 ("one");
			break;
		case "2":
			PlayAnimation2 ("two");
			break;
		case "3":
			PlayAnimation2 ("three");
			break;
		case "4":
			PlayAnimation2 ("four");
			break;
		case "5":
			PlayAnimation2 ("five");
			break;
		case "6":
			PlayAnimation2 ("six");
			break;
		default:
			break;
		}
	}

	void PlayAnimation1(string num1){
		GameObject shaizi1 = GameObject.Find ("001shaizi_ani");
		if (shaizi1 != null)
			shaizi1.GetComponent<Animation>().Play(num1);
	}

	void PlayAnimation2(string num2)
	{
		GameObject shaizi2 = GameObject.Find ("002shaizi_ani");
		if (shaizi2 != null)
			shaizi2.GetComponent<Animation>().Play(num2);
	}
}
