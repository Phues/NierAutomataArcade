using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    
    public int maxHP = 5; // Maximum hit points of the enemy
    public int currentHP; // Current hit points of the enemy

    public Transform target; // The target position to move towards
    
    public float speed = 5;
    public bool canMove = true;

    public NavMeshAgent agent;
    public GameObject hitEffect, dieEffect;
    public Animator anim;
    
    public AudioSource audioSource;
    public AudioClip hitSound, dieSound;

    public virtual void TakeDamage(int damage)
    {
        audioSource.PlayOneShot(hitSound);
        currentHP -= damage;
        
        HitAnimation();
        //Debug.Log(currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Quaternion rotation = Quaternion.Euler(-90,0,0);
        Instantiate(dieEffect, gameObject.transform.position, rotation);
        Destroy(gameObject);
    }

    public virtual void HitAnimation()
    {
        GameObject hitObject = Instantiate(hitEffect, transform.position, quaternion.identity);
        // Set the parent of the spawned object
        hitObject.transform.parent = this.gameObject.transform;
        Transform childTransform = hitObject.transform.Find("Hit_Ripples");
        
        if (childTransform != null)
        {
            // Set the child GameObject to active.
            childTransform.gameObject.SetActive(true);
        }
        anim.SetTrigger("hit");
    }

    public virtual void Move()
    {
        if (target != null)
        {
            agent.SetDestination(target.position);
            agent.speed = this.speed;
        }
    }

    void Update()
    {
        if (canMove)
        {
            Move();
        }
    }

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentHP = maxHP;
    }
}
