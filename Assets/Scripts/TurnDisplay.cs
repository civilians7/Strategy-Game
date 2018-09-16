﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnDisplay : MonoBehaviour {

    Text text;

	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
        text.text = "Start";
	}
	
    public void SetText(string newText) {
        text.text = newText;
    }

    public void Disable() {
        text.color = new Color(text.color.r,text.color.g,text.color.b,0f);
        enabled = false;
    }

    public void Enable() {
        text.color = new Color(text.color.r,text.color.g,text.color.b,1f);
        enabled = true;
    }
}
