using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource Source;
    public int position = 0;
    public const int SAMPLE_RATE = 44100;
    public const float FREQUENCY = 440;
    
    void Start()
    {
        // Source = GetComponent<AudioSource>();
    }

    public void Play(float[] samples, int channels, int freq)
    {
        AudioClip clip = AudioClip.Create("", samples.Length, channels, freq, false);
        clip.SetData(samples, 0);
        Source.clip = clip;
        if (Source.isPlaying == false)
        {
            Source.Play();
        }
    }
}
