using System.Collections.Generic;
using UnityEngine;


public class GVfxPlayer : MonoBehaviour
{
    [SerializeField] 
    private List<ParticleSystem> particles;
    
    public void Play()
    {
        foreach (var particle in particles)
        {
            particle.Play();
        }
    }

    public void Stop()
    {
        foreach (var particle in particles)
        {
            particle.Stop();
        }
    }
}
