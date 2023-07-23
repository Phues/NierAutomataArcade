using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private GameObject bulletExpolsion;

    private Vector3 initialDirection;
    private Rigidbody rb;

    private void Start()
    {
        // Store the initial forward direction of the bullet
        initialDirection = transform.up;
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rb.velocity = initialDirection * moveSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SphereEnemy"))
        {
            // The collided GameObject has the "enemy" tag
            // Perform actions specific to hitting an enemy
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(1);
                // Destroy the bullet after colliding with an object
                Destroy(gameObject);
            }
        }
        else if (other.gameObject.CompareTag("Walls"))
        {
            GameObject newObject = Instantiate(bulletExpolsion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
