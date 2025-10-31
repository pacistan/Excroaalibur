using UnityEngine;
using UnityEngine.UI;

/* Use this for Diplaying Tutorial Information to the Player */
public class GTutorialInfos : MonoBehaviour
{
    struct STutorialInfo
    {
        /** Picture to Show in the UI */
        [SerializeField]
        public Image infosPicture;
        
        /** Time the Game is Stopped to Show the Info */
        [SerializeField]
        public int stopTime;
        
        /** Wave at which to Show the Info */
        [SerializeField]
        public int showAtWave;
    }
    
    
    
    
    
}
