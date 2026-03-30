using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines.Interpolators;

public class GChangeColorOnHit : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public float fadeDuration;
    Material[] mats;

    private void Start()
    {
        if (skinnedMeshRenderer != null)
        {
            mats = new Material[skinnedMeshRenderer.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = skinnedMeshRenderer.materials[i];
            }
        }      
    }

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
    public void GetStun()
    {
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].SetColor("_Color",Color.red);
        }   
    }
    public void EndStun()
    {
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].SetColor("_Color", Color.white);
        }
    }
    IEnumerator Fade()
    {
        float elapsedTime = 0f;
        Color color = Color.white;
        while (elapsedTime < fadeDuration/2)
        {
            elapsedTime += Time.deltaTime;

            color = Color.Lerp(color, Color.red, elapsedTime);              
            for ( int i = 0; i < mats.Length; i++)
            {
                mats[i].SetColor("_Color",color);
            }
            yield return null;
        }
        while (elapsedTime > fadeDuration/2 && elapsedTime<fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            color = Color.Lerp(color, Color.white, elapsedTime);
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetColor("_Color", color);
            }
            yield return null;
        }
    }
}
