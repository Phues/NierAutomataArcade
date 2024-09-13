using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

public class PlayerController : MonoBehaviour, IDamageable
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;
    public float fireRate = 0.5f;
    public long bulletCount = 19;

    [SerializeField] private int health = 3;
    [SerializeField] private ParticleSystem movementEffect;
    [SerializeField] private GameObject hitEffect, dieEffect;
    
    [SerializeField] private GameObject[] lowerRightTriangle; // the three meshes that make up our player
    [SerializeField] private GameObject[] lowerLeftTriangle; // the three meshes that make up our player
    [SerializeField] private GameObject[] upperTriangle; // the three meshes that make up our player
    [SerializeField] private GameObject sphere;
    public GameObject gameOverPanel;
    
    [SerializeField] private AudioClip shootSFX, hitSFX, dieSFX;

    private int componentIndex = 0;

    [Range(0, 10)] [SerializeField] private int occurAfterVelocity;
    [Range(0, 0.2f)] [SerializeField] private float effectFormationPeriod;

    private float fireTimer; // Timer to track the time between shots
    private float counter;


    public GameObject bullet;
    private GameObject shooter;
    
    private AudioSource _audioSource;

    private Rigidbody rb;
    private Vector3 bulletPos;
    
    public Transform timerUI, bulletUI; // Reference to the timer UI element
    private TextMeshProUGUI bulletText;
    public Vector3 offset = new Vector3(0f, 1f, 0f);
    public long timerValue = 33333L; // Starting timer value
    

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        shooter = transform.Find("Shooter").gameObject;
        _audioSource = GetComponent<AudioSource>();
        bulletText = bulletUI.GetComponent<TextMeshProUGUI>();
        bulletText.text = bulletCount.ToString();
    }

    private void Update()
    {
        // Check if enough time has passed to allow another shot
        if (fireTimer <= 0f)
        {
            // Check for input to fire the gun
            if (Input.GetButton("Fire1") && bulletCount > 0)
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
        //Timer();
    }

    private void ShootBullet()
    {
        bulletCount--;
        bulletText.text = bulletCount.ToString();
        bulletPos = shooter.transform.position;
        Quaternion bulletRotation =
            transform.rotation;
        bulletRotation = Quaternion.Euler(bulletRotation.eulerAngles.x, bulletRotation.eulerAngles.y+90, bulletRotation.eulerAngles.z+90);
        _audioSource.PlayOneShot(shootSFX);
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
        _audioSource.PlayOneShot(hitSFX);
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

        HitAnimation();
                
        if (health <= 0)
        {
            sphere.SetActive(false);
            Instantiate(dieEffect, transform.position, quaternion.identity);
            gameOverPanel.SetActive(true);
            _audioSource.PlayOneShot(dieSFX);
            gameOverPanel.SetActive(true);
            Destroy(this.gameObject, dieSFX.length);
            componentIndex = 0;
        }
    }

    public void HitAnimation()
    {
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
    }

    public void Timer()
    {
        // Update timer position to match player position
        if (timerUI != null)
        {
            timerUI.position = Camera.main.WorldToScreenPoint(transform.position)+offset;
        }
        else
        {
            Debug.Log("Timer UI is not set in the inspector");
        }

        // Update timer logic
        if (timerValue > 0)
        {
            timerValue -= (long)(Time.deltaTime * 1000); // Subtract milliseconds
            // Update timer UI text or image with current timer value
            if (timerUI != null)
            {
                // Convert milliseconds to minutes, seconds, and milliseconds
                long totalMilliseconds = timerValue;
                float totalSeconds = totalMilliseconds / 1000;
                int minutes = Mathf.FloorToInt(totalSeconds / 60);
                long seconds = Mathf.FloorToInt(totalSeconds % 60);
                long milliseconds = (long)(totalMilliseconds % 1000);

                // Format the timer display
                string timerText = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);

                // Update the text component of the timer UI
                timerUI.GetComponent<TextMeshProUGUI>().text = timerText;
            }
        }
        else
        {
            gameOverPanel.SetActive(true);
            //freeze the game
            Time.timeScale = 0;
        }
    }
}
