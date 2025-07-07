using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

	public static MusicManager instance;
	AudioSource audioSource;
	public AudioClip[] musicClips;

	public bool isGameOver = false;

	void Awake ()
	{
#pragma warning disable 0618
        if (GameObject.FindObjectsOfType<MusicManager> ().Length > 1) {
#pragma warning restore 0618
            Destroy(this.gameObject);
			return;
		}
		if (!instance)
			instance = this;
		audioSource = GetComponent<AudioSource> ();
	}

	public void PlayMusic (int index)
	{
		audioSource.clip = musicClips [index]; 
		audioSource.Play ();
	}

	void OnEnable()
	{
		PlayMusic (Random.Range(0,musicClips.Length));
	}

	public void Reset()
	{
		isGameOver = false;
		audioSource.pitch = 1;
	}

	void Update()
	{
		if (isGameOver) {
			if (audioSource.pitch > 0.4f) {
				audioSource.pitch -= 0.5f * Time.deltaTime;
			} else {
				isGameOver = false;
			}
		}
	}
}
