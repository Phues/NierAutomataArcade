using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    
    public int maxHP = 5; // Maximum hit points of the enemy
    private int currentHP; // Current hit points of the enemy

    public Transform target; // The target position to move towards
    
    public float speed = 5;

    public NavMeshAgent agent;

    public virtual void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log(currentHP);

        if (currentHP <= 0)
        {
            // Enemy defeated, perform necessary actions (e.g., play death animation, destroy object)
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
