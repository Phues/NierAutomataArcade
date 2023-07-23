using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    
    public int maxHP = 5; // Maximum hit points of the enemy
    private int currentHP; // Current hit points of the enemy

    public Transform target; // The target position to move towards
    
    public float speed = 5;

    public NavMeshAgent agent;
    public GameObject hitEffect, dieEffect;
    public Animator anim;

    public virtual void TakeDamage(int damage)
    {
        currentHP -= damage;
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
        
        Debug.Log(currentHP);

        if (currentHP <= 0)
        {
            Quaternion rotation = Quaternion.Euler(-90,0,0);
            Instantiate(dieEffect, gameObject.transform.position, rotation);
            Destroy(gameObject);
        }
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
        Move();
    }

    private void Start()
    {
        currentHP = maxHP;
    }
}
