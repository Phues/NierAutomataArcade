using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableShield : MonoBehaviour
{
    
    public List<GameObject> enemies = new List<GameObject>();
    private AudioSource audioSource;
    public AudioClip shieldDownSfx;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //check if all enemies are dead and deactivate the shield
        foreach (var enemy in enemies)
        {
            if (enemy == null)
            {
                //remove from the list
                enemies.Remove(enemy);
            }
        }

        if (enemies.Count == 0)
        {
            audioSource.PlayOneShot(shieldDownSfx);
            gameObject.SetActive(false);
        }
    }
}
