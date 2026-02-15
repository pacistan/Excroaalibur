using System.Collections;
using UnityEngine;
using UnityEngine.Splines.Interpolators;

public class GChangeColorOnHit : MonoBehaviour
{
    
    public float fadeDuration;

    /*private void Update()
    {
        if (Input.GetKey(KeyCode.S))
        {
            GetHit();
        }
    }*/
    public void GetHit()
    {
        StartCoroutine(Fade());
    }
    IEnumerator Fade()
    {
        Material mat = GetComponent<SkinnedMeshRenderer>().material;
        float elapsedTime = 0f;
        Color color = Color.white;
        while (elapsedTime < fadeDuration/2)
        {
            elapsedTime += Time.deltaTime;

            color = Color.Lerp(color, Color.red, elapsedTime);
            mat.SetColor("_Color",color);
            yield return null;
        }
        while (elapsedTime > fadeDuration/2 && elapsedTime<fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            color = Color.Lerp(color, Color.white, elapsedTime);
            mat.SetColor("_Color", color);
            yield return null;
        }
    }
}
