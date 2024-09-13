using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ContinuousFireSFX : MonoBehaviour
{
    private AudioSource _audioSource;
    [SerializeField] private float waitRate, waitDuration;

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponentInParent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(MuteFireSfx());
    }
    private IEnumerator MuteFireSfx()
    {
        while (true)
        {
            //mute every fireRate seconds
            _audioSource.mute = false;
            yield return new WaitForSeconds(waitRate);
            _audioSource.mute = true;
        }
    }
}
