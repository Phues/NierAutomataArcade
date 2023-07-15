using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{

    private ParticleSystem _particleSystem;

    private List<ParticleCollisionEvent> _particleCollisionEvents;

    [SerializeField] private string playerTag;
    [SerializeField] private float damage;
    
    // Start is called before the first frame update
    void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _particleCollisionEvents = new List<ParticleCollisionEvent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnParticleCollision(GameObject other)
    {
        ParticlePhysicsExtensions.GetCollisionEvents(_particleSystem, other, _particleCollisionEvents);

        for (int i = 0; i < _particleCollisionEvents.Count; i++)
        {
            var collider = _particleCollisionEvents[i].colliderComponent;
            if (collider.CompareTag(playerTag))
            {
                other.GetComponent<PlayerController>().TakeDamage(1);
            }
        }
    }
}
