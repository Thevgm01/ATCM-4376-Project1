using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioHelper
{
    public static AudioSource PlayRandomClip2DFromArray(AudioClip[] clips, float volume)
    {
        return PlayClip2D(clips[UnityEngine.Random.Range(0, clips.Length)], volume);
    }
    public static AudioSource PlayClip2D(AudioClip clip, float volume) { return PlayClip2D(clip, volume, true); }
    public static AudioSource PlayClip2D(AudioClip clip, float volume, bool destroyWhenDone)
    {
        GameObject audioObject = new GameObject("Audio2D");
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();
        if(destroyWhenDone) Object.Destroy(audioObject, clip.length);
        return audioSource;
    }
}
