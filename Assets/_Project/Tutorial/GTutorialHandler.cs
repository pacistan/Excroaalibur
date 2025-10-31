using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* Use this for Displaying Tutorial Information to the Player */
public class GTutorialHandler : MonoBehaviour
{
    [Serializable]
    public struct STutorialInfo
    {
        /** GameObject to activate */
        [SerializeField]
        public GameObject CanvasObject;
        
        /** Tutorial Text Object to Activate at End of timer */
        [SerializeField]
        public TextMeshProUGUI TextObject;
        
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
        GTurnBaseManager.Instance.OnUnregisterController += HandleUnregisterController;
    }

    void HandleUnregisterController(GController controller)
    {
        if (controller is GPlayerController) return;

        
        GGameManager.Instance._currentTutorialSceneToLoadIndex++;
        GGameManager.Instance.LoadScene();
    }

    void HandlePrePlayerTurn(int TurnCount)
    {
        STutorialInfo? info = GetTutorialInfoAtWave(TurnCount);
        if (!info.HasValue) return;
            
        info.Value.CanvasObject.gameObject.SetActive(true);
        StartCoroutine(WaitAndHideInfo(info.Value));
    }
    
    IEnumerator WaitAndHideInfo(STutorialInfo info)
    {

        GGameManager.Instance.playerController.enabled = false; 
        yield return new WaitForSeconds(info.stopTime);
        
        info.TextObject.gameObject.SetActive(false);
        GGameManager.Instance.playerController.enabled = true;
        
        yield return new WaitUntil(() => Input.anyKeyDown);
        
        info.CanvasObject.SetActive(false);
    }
}
