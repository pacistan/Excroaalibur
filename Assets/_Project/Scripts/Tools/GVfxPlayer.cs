using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;


public class GVfxPlayer : MonoBehaviour
{
    [SerializeField] 
    private List<ParticleSystem> particles;
    
    public void Play()
    {
        if (particles.Count <= 0) return;
        foreach (var particle in particles)
        {
            if (particle == null) continue;
            particle.Play();
        }
    }

    public void Stop()
    {
        if (particles.Count <= 0) return;
        foreach (var particle in particles)
        {
            if (particle == null) continue;

            particle.Clear();
            particle.Stop();
            
        }
    }
}
