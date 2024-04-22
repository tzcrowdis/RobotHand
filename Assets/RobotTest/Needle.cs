using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Needle : MonoBehaviour
{
    public ParticleSystem needleParticles;
    public bool test;

    public bool locked;

    Rigidbody rb;
    
    void Start()
    {
        locked = false;
        
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {

    }

    public void DestroyNeedle()
    {
        //play sound?

        //play particle effect
        Instantiate(needleParticles, transform.position, transform.rotation);

        //destroy needle object
        Destroy(this.gameObject);
    }

    public void LockMotion(GameObject parent)
    {
        //set parent
        transform.SetParent(parent.transform);

        //freeze rigidbody
        Destroy(rb);

        locked = true;
    }

    public void UnlockMotion()
    {
        //remove parent
        transform.SetParent(null);

        //unfreeze rigidbody
        if (rb == null)
            gameObject.AddComponent<Rigidbody>();

        locked = false;
    }
}
