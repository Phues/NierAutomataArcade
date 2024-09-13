using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallEnemy : Enemy
{
    private MeshRenderer meshRenderer;
    private Collider _collider;
    public void Start()
    {
        base.Start();   
        anim = GetComponent<Animator>();
        meshRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<Collider>();
    }
    
    
    
    public override void Move()
    {
        
    }

    public override void Die()
    {
        Quaternion rotation = Quaternion.Euler(-90,0,0);
        Instantiate(dieEffect, gameObject.transform.position, rotation);
        audioSource.PlayOneShot(dieSound);
        meshRenderer.enabled = false;
        _collider.enabled = false;
        Destroy(gameObject, 1f);
    }

    public override void HitAnimation()
    {
        
        anim.SetTrigger("hit");
    }
}
