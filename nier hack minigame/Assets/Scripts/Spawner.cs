using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    
    public GameObject[] nextWave;
    private Collider _collider;
    
    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider>();
    }
    
    

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            foreach (var enemy in nextWave)
            {
                enemy.SetActive(true);
            }
        }
    }
}
