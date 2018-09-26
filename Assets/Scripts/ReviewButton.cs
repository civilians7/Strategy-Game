using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviewButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Disable() {
        transform.SetParent(FindObjectOfType<GameManager>().transform);
    }

    public void Enable() {
        transform.SetParent(FindObjectOfType<LevelCanvas>().transform);
    }
}
