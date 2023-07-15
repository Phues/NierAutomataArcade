using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;
    public float fireRate = 0.5f;

    [SerializeField] private int health = 3;

    private float fireTimer; // Timer to track the time between shots


    public GameObject bullet;
    private GameObject shooter;

    private Rigidbody rb;
    private Vector3 bulletPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        shooter = transform.Find("Shooter").gameObject;
    }

    private void Update()
    {
        // Check if enough time has passed to allow another shot
        if (fireTimer <= 0f)
        {
            // Check for input to fire the gun
            if (Input.GetButton("Fire1"))
            {
                // Shoot beam
                ShootBullet();
                // Reset the timer to the fire rate
                fireTimer = fireRate;
            }
        }
        else
        {
            // Reduce the timer
            fireTimer -= Time.deltaTime;
        }
    }

    private void ShootBullet()
    {
        bulletPos = shooter.transform.position;
        Quaternion bulletRotation =
            transform.rotation;
        bulletRotation = Quaternion.Euler(bulletRotation.eulerAngles.x, bulletRotation.eulerAngles.y+90, bulletRotation.eulerAngles.z+90);
        
        Instantiate(bullet, bulletPos, bulletRotation);
    }

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = transform.TransformDirection(new Vector3(moveHorizontal, 0f, moveVertical)) * moveSpeed;
        rb.velocity = movement;
        
        RotatePlayerWithMouse();
    }
    
    private void RotatePlayerWithMouse()
    {
        float mouseX = Input.GetAxis("Mouse X");

        transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.fixedDeltaTime);
    }

    

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
