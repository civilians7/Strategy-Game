using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMapTools;

public class PlayBack : MonoBehaviour {

    private GameManager gameManager;
	// Use this for initialization
	void Start () {
        gameManager = GetComponent<GameManager>();
	}

    public void Review() {
        foreach (Troop troop in gameManager.troopArray) {
            foreach (HexCoordinates path in troop.reviewAnimation) {
                troop.coordPath.Add(path);
            }
            troop.ActionMove();
        }
    }
}


