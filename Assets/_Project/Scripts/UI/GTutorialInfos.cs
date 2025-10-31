using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Use this for Displaying Tutorial Information to the Player */
public class GTutorialInfos : MonoBehaviour
{
    [Serializable]
    public struct STutorialInfo
    {
        /** GameObject to activate */
        [SerializeField]
        public GameObject CanvasObject;
        
        /** Time the Game is Stopped to Show the Info */
        [SerializeField]
        public int stopTime;
        
        /** Wave at which to Show the Info */
        [SerializeField]
        public int showAtWave;
    }
    
    [SerializeField, Tooltip("List of Tutorial Infos to Show to the Player")]
    private List<STutorialInfo> tutorialInfos = new List<STutorialInfo>();
    
    
    /** Get the Tutorial Info at a Specific Wave */
    public STutorialInfo? GetTutorialInfoAtWave(int wave)
    {
        for (int i = 0; i < tutorialInfos.Count; i++)
        {
            if (tutorialInfos[i].showAtWave == wave)
                return tutorialInfos[i];
        }
        return null;
    }

    void Awake()
    {
        GTurnBaseManager.Instance.OnPrePlayerTurn += HandlePrePlayerTurn;
    }

    void HandlePrePlayerTurn(int TurnCount)
    {
        STutorialInfo? info = GetTutorialInfoAtWave(TurnCount);
        if (!info.HasValue) return;
            
        info.Value.CanvasObject.gameObject.SetActive(true);
        StartCoroutine(WaitAndHideInfo(info.Value.CanvasObject, info.Value.stopTime));
    }
    
    IEnumerator WaitAndHideInfo(GameObject infoPicture, int waitTime)
    {
        GGameManager.Instance._playerController.enabled = false; 
        yield return new WaitForSeconds(waitTime);
        
        GGameManager.Instance._playerController.enabled = true;
        yield return new WaitUntil(() => Input.anyKeyDown);
        
        infoPicture.gameObject.SetActive(false);
    }
}
