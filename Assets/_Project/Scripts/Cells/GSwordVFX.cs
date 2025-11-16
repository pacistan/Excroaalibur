using UnityEngine;

public class GSwordVFX : MonoBehaviour
{
    public GCrown GCrown;
    public ParticleSystem[] ring;
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
        if (damage == 1)
        {
            for (int i = 0; i < ring.Length; i++)
            {
                ring[i].gameObject.SetActive(false);
            }
        }
        ring[Mathf.Min(damage - 1, ring.Length - 1)].gameObject.SetActive(true);
    }
}
