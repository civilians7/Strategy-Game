using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour {

    public AudioClip[] levelMusicChangeArray;

    private AudioSource audioSource;

	// Use this for initialization
	void Awake () {
        DontDestroyOnLoad(gameObject);
	}

    void Start() {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update () {
		
	}

    //void OnLevelWasLoaded(int level) {
    //    AudioClip thisLevelMusic = levelMusicChangeArray[level];
    //    Debug.Log("Playing clip: " + thisLevelMusic);

    //    if (thisLevelMusic && audioSource.clip != thisLevelMusic) { //If there is music attached
    //        audioSource.clip = thisLevelMusic;
    //        audioSource.loop = true;
    //        audioSource.Play();
    //    }
    //}

    public void SetVolume(float volume) {
        audioSource.volume = volume;
    }
}
