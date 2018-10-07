using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTroopColor : MonoBehaviour {
    private Animator animator;
	// Use this for initialization
	void Awake () {
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetColor(int colorNum) {
        animator.SetInteger("Color", colorNum);
    }

}
