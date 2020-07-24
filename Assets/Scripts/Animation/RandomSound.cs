using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSound : MonoBehaviour
{
    public AudioSource audiosource;
    public List<AudioClip> audioClips;
    private IEnumerator changeSounds;

    private void Start()
    {
        if (!audiosource) 
            audiosource = GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        changeSounds = ChangeSounds();
    }
    private void OnDisable()
    {
        StopCoroutine(changeSounds);
    }

    private IEnumerator ChangeSounds()
    {
        if(audiosource)
        {
            AudioClip clip;
            while(true)
            {
                clip = audioClips[Random.Range(0, audioClips.Count)];
                yield return new WaitForSeconds(clip.length);
            }
        }
    }
}
