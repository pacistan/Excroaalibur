using UnityEngine;

public class GPlayVFX : MonoBehaviour
{
    public ParticleSystem particle;

    public void PlayParticle()
    {
        particle.Play();
    }
}
