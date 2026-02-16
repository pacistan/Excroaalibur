using UnityEngine;

public class GSwordVFX : MonoBehaviour
{
    public GCrown GCrown;
    public ParticleSystem[] ring;
    public float IncreaseSizeFloat;
    private Vector3 baseScale = Vector3.one;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        GHudManager.Instance.playMenu.OnUpdateCrownUI += OnUpdateCrownDamage;
    }

    private void OnDisable()
    {
        GHudManager.Instance.playMenu.OnUpdateCrownUI -= OnUpdateCrownDamage;
    }

    // Update is called once per frame

    void OnUpdateCrownDamage(int damage)
    {
        Vector3 IncreaseSize = new Vector3(IncreaseSizeFloat, IncreaseSizeFloat, IncreaseSizeFloat);
        transform.localScale = baseScale + (damage - 1) * IncreaseSize;
       // print(transform.localScale);
        if (damage == 1)
        {
            transform.localScale = baseScale;
            for (int i = 0; i < ring.Length; i++)
            {
                ring[i].gameObject.SetActive(false);
            }
        }
        ring[Mathf.Min(damage - 1, ring.Length - 1)].gameObject.SetActive(true);
       
    }
}
