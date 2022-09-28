using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    [SerializeField] List<AudioData> sfxList;
    [SerializeField] AudioSource musicPlayer;
    [SerializeField] AudioSource sfxPlayer;
    [SerializeField] float fadeDuration = 0.75f;

    AudioClip currentMusic;

    public static AudioManager Instance { get; private set; }

    float originalMusicVolume;
    Dictionary<AudioId, AudioData> sfxLookup;

    private void Awake() 
    {
        Instance = this;   
    }

    private void Start() 
    {
        originalMusicVolume = musicPlayer.volume;  

        sfxLookup = sfxList.ToDictionary(x => x.id); 
    }

    public void PlaySfx(AudioClip clip, bool pauseMusic = false)
    {
        if (clip == null)  return;

        if (pauseMusic)
        {
            musicPlayer.Pause();
            StartCoroutine(UnpauseMusic(clip.length));
        }

        sfxPlayer.PlayOneShot(clip);
    }

    public void PlaySfx(AudioId audioId, bool pauseMusic=false) 
    {
        // if audio id doesn't exist
        if (!sfxLookup.ContainsKey(audioId))  return;

        // play audio if it exists
        var audioData = sfxLookup[audioId];
        PlaySfx(audioData.clip, pauseMusic);  
    }

    public void PlayMusic(AudioClip clip, bool loop=true, bool fade=false) 
    {
        if (clip == null || clip == currentMusic)  return;

        currentMusic = clip;
        StartCoroutine(PlayMusicAsync(clip, loop, fade));
    }

    private IEnumerator PlayMusicAsync(AudioClip clip, bool loop, bool fade)
    {
        // fade music volume
        if (fade) 
        {
            yield return musicPlayer.DOFade(0, fadeDuration).WaitForCompletion();   
        }

        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();

        // fade back to original volume
        if (fade)
        {
            yield return musicPlayer.DOFade(originalMusicVolume, fadeDuration).WaitForCompletion();
        }
    }

    private IEnumerator UnpauseMusic(float delay) 
    {
        yield return new WaitForSeconds(delay);

        musicPlayer.volume = 0;
        musicPlayer.UnPause();
        musicPlayer.DOFade(originalMusicVolume, fadeDuration);    
    }
}

public enum AudioId{ UISelect, Hit, Faint, ExpGain, ItemObtained, PokemonObtained }

[System.Serializable]
public class AudioData
{
    public AudioId id;
    public AudioClip clip;
}
