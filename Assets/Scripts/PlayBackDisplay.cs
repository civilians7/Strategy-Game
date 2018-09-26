﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayBackDisplay : MonoBehaviour {


	// Use this for initialization
	void Start () {
	}
	
    public void Disable() {
        transform.SetParent(FindObjectOfType<GameManager>().transform);
    }

    public void Enable() {
        transform.SetParent(FindObjectOfType<LevelCanvas>().transform);
    }
}
