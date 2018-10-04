using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerColors : MonoBehaviour {

    public TroopColor playerOne;
    public TroopColor playerTwo;

	// Use this for initialization

    void Awake () {
        DontDestroyOnLoad(gameObject);
	}


}
