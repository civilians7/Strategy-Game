using System.Collections;
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
        transform.SetParent(FindObjectOfType<GameManager>().transform);
    }

    public void Enable() {
        transform.SetParent(FindObjectOfType<LevelCanvas>().transform);
    }
}
