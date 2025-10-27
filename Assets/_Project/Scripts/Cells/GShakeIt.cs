using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

public class GShakeIt : MonoBehaviour
{
    public float speed;
    public float amount;
    bool isShake;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isShake)
        {
            Vector3 newPos = transform.localPosition;
            newPos.y = Random.Range(0, amount) * Time.deltaTime*speed;
            /*newPos.x = Random.Range(0, amount)*Time.deltaTime;
            newPos.z = Random.Range(0, amount) * Time.deltaTime;*/

            transform.localPosition = newPos;
        }
        
    }
    public void StartShake()
    {
        isShake = true;
    }
    public void StopShake()
    {
        isShake=false;
        transform.localPosition= new Vector3(0,0,0);
    }
}
