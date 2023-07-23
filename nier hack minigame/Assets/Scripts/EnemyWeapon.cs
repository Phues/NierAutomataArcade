using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] private float fireRate;
    [SerializeField] private GameObject _orange, _purple;
    
    [SerializeField] private Transform player;


    private ParticleSystem _particleSystem, _purpleParticleSystem, _orangeParticleSystem;
    
    private bool fireCoolDown;


    private void Start()
    {
        _purpleParticleSystem = _purple.GetComponent<ParticleSystem>();
        _orangeParticleSystem = _orange.GetComponent<ParticleSystem>();
        _particleSystem = _orangeParticleSystem;
    }
    

    private void Update()
    {
        Fire();
    }

    public void Fire()
    {
        if (fireCoolDown) return;
        if (player != null)
        {
            // Calculate the direction from the current position to the target position
            Vector3 direction = player.position - transform.position;
            direction.y = 0f; // Set the y-component to zero to restrict rotation to the y-axis

            // Rotate the object to look towards the target in the y-axis
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        
        _particleSystem.Emit(1);
        fireCoolDown = true;
        StartCoroutine(CoolDown());
    }

    private IEnumerator CoolDown()
    {
        yield return new WaitForSeconds(fireRate);
        fireCoolDown = false;
        if ( _particleSystem == _orangeParticleSystem) 
        {
            _particleSystem = _purpleParticleSystem;
        }
        else 
        {
            _particleSystem = _orangeParticleSystem;
        }
    }
}
