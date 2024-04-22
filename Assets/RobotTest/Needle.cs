using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle : MonoBehaviour
{
    public ParticleSystem needleParticles;
    public bool test;
    
    void Start()
    {
        
    }

    void Update()
    {
        if (test)
            DestroyNeedle();
    }

    void DestroyNeedle()
    {
        //play sound?

        //play particle effect
        Instantiate(needleParticles, transform.position, transform.rotation);

        //destroy needle object
        Destroy(this.gameObject);
    }
}
