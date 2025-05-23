using UnityEngine.Audio;
using System;
using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{

	public static AudioManager instance;

	public AudioMixerGroup mixerGroup;

	public Sound[] sounds;


	void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.loop = s.loop;

			s.source.outputAudioMixerGroup = mixerGroup;
		}
	}


    private void Start()
    {
		//Play("BGM");
    }

    public void Play(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
		s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

		s.source.Play();
	}

	public bool CheckIfPlaying(string sound)
    {
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return false;
		}
		return s.source.isPlaying;
	}

	public void Stop(string sound)
    {
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.source.Stop();
	}

	public void StopAllTogglableFurnitureSoundEffects(GameObject levelObj)
    {
		InteractableObject[] furnitureScriptList = levelObj.GetComponentsInChildren<InteractableObject>();
		foreach (InteractableObject script in furnitureScriptList)
        {
			if (script.toggleOnSoundName.Length > 0)
			{
				AudioManager.instance.Stop(script.toggleOnSoundName);
			}
			if (script.toggleOffSoundName.Length > 0)
			{
				AudioManager.instance.Stop(script.toggleOffSoundName);
			}
		}
	}

	public void Pause(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.source.Pause();
	}

	public void SetVolume(string sound, float volume)
    {
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}


		s.source.volume = volume * s.volume;
	}

	public void FadeVolume(string sound, float targetVolume, float duration)
    {
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		DOTween.To(() => s.source.volume, x => s.source.volume = x, (targetVolume * s.volume), duration);
	}


	// <---------------------------------- PLAYING SPECIFIC SOUND EFFECTS ---------------------------------- > //

	public void PlayButtonClickSFX()
    {
		//Plays button SFX
		AudioManager.instance.Play("Button Click");
	}



}
