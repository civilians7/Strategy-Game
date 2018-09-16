using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMapTools;
using UnityEngine.UI;

public class PlayBack : MonoBehaviour {

    Text text;

    private GameManager gameManager;
	// Use this for initialization
	void Start () {
        gameManager = GetComponent<GameManager>();
        text = GetComponent<Text>();
	}

    public void Review() {
        foreach (Troop troop in gameManager.troopArray) {
            foreach (HexCoordinates path in troop.reviewAnimation) {
                troop.coordPath.Add(path);
            }
            troop.ActionMove();
        }
    }

    public void SetText(string turn) {
        text.text = turn;
    }
}


