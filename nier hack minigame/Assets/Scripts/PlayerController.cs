using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

public class PlayerController : MonoBehaviour, IDamageable
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;
    public float fireRate = 0.5f;

    [SerializeField] private int health = 3;
    [SerializeField] private ParticleSystem movementEffect;
    [SerializeField] private GameObject hitEffect;
    [SerializeField] private GameObject[] lowerRightTriangle; // the three meshes that make up our player
    [SerializeField] private GameObject[] lowerLeftTriangle; // the three meshes that make up our player
    [SerializeField] private GameObject[] upperTriangle; // the three meshes that make up our player

    private int componentIndex = 0;

    [Range(0, 10)] [SerializeField] private int occurAfterVelocity;
    [Range(0, 0.2f)] [SerializeField] private float effectFormationPeriod;

    private float fireTimer; // Timer to track the time between shots
    private float counter;


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
        Move();
    }

    private void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = transform.TransformDirection(new Vector3(moveHorizontal, 0f, moveVertical)) * moveSpeed;
        rb.velocity = movement;
        PlayWalkingParticles();
        
        RotatePlayerWithMouse();
    }

    private void PlayWalkingParticles()
    {
        counter += Time.deltaTime;

        if (Mathf.Abs(rb.velocity.x) > occurAfterVelocity)
        {
            if (counter > effectFormationPeriod)
            {
                movementEffect.Play();
                counter = 0;
            }
        }
    }

    private void RotatePlayerWithMouse()
    {
        float mouseX = Input.GetAxis("Mouse X");

        transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.fixedDeltaTime);
    }

    

    public void TakeDamage(int amount)
    {
        health -= amount;

        if (componentIndex == 0)
        {
            foreach (var t in lowerLeftTriangle)
            {
                t.SetActive(false);
            }

            componentIndex++;
        } 
        else if (componentIndex == 1)
        {
            foreach (var t in lowerRightTriangle)
            {
                t.SetActive(false);
            }

            componentIndex++;
        }
        else
        {
            foreach (var t in upperTriangle)
            {
                t.SetActive(false);
            }
        }

        GameObject hitObject = Instantiate(hitEffect, transform.position, quaternion.identity);
        // Set the parent of the spawned object
        hitObject.transform.parent = this.gameObject.transform;
        // Iterate through all the child objects of the parent
        for (int i = 0; i < hitObject.transform.childCount; i++)
        {
            // Get the child GameObject at index i
            GameObject child = hitObject.transform.GetChild(i).gameObject;

            // Set the child GameObject active
            child.SetActive(true);
        }
                
        if (health <= 0)
        {
            Destroy(this.gameObject);
            componentIndex = 0;
        }
    }
}
