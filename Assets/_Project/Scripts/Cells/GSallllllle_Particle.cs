using Unity.VisualScripting;
using UnityEngine;

public class GSallllllle_Particle: MonoBehaviour
{
   public ParticleSystem _particle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseDown()
    {
        _particle.gameObject.SetActive(true);
        _particle.Play();
    }
}
