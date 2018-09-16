using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMapTerrain;

//center of the archetecture, handles comunication between other classes and handles most of the game engine logic

public class GameManager : MonoBehaviour {

    public int turnNum = 0; //keep
    public int turnsPerSeason = 1; //keep
    public string startingSeason = "Winter"; //eventually refactor to seperate season class
    public Troop[] troopArray; //keep
    private TurnDisplay turnDisplay;
    private SeasonDisplay seasonDisplay;
    private PlayBackDisplay playBackDisplay;
    private HexControls hexControls;

	// Use this for initialization
	void Start () {
        turnDisplay = FindObjectOfType<TurnDisplay>();
        seasonDisplay = FindObjectOfType<SeasonDisplay>();
        playBackDisplay = FindObjectOfType<PlayBackDisplay>();
        hexControls = FindObjectOfType<HexControls>();
        turnNum = 4;
        playBackDisplay.Disable();
	}

    void Update() {
        troopArray = FindObjectsOfType<Troop>();
    }

    public void EndTurn() { //rename and clean up code
        troopArray = FindObjectsOfType<Troop>();
        hexControls.planningMode = false;
        if (turnNum == 3) {
            turnNum = 0;
        } else if (turnNum == 4) {
            turnNum = 1;
        } else { 
            turnNum += 1;
        }

        if (turnNum == 0) {
            bool conflictSolved = false;
            playBackDisplay.Disable();
            seasonDisplay.SeasonCounter();
            turnDisplay.SetText("Next Season");
            hexControls.ChangePlayer(CellColor.White);
            int i = 0;
            do {
                conflictSolved = false;
                foreach (Troop troop in troopArray) {
                    if (i == 0) {
                        hexControls.FindPath(troop);
                    }
                }
                foreach (Troop troop in troopArray) {
                    if (troop.ActionTurn()) {
                        conflictSolved = true;
                    }
                    troop.conflictingCells.Clear();
                    troop.conflictingTroops.Clear();
                }
                i++;
            } while (conflictSolved && i < 5);
            foreach (Troop troop in troopArray) {
                troop.HandleAction();
            }
        }else if (turnNum == 1) {
            playBackDisplay.Enable();
            turnDisplay.SetText("Finish Turn");
            hexControls.ChangePlayer(CellColor.Blue);
        } else if (turnNum == 2) {
            hexControls.ChangePlayer(CellColor.Red);
           
        } else if (turnNum == 3) {
            turnDisplay.SetText("Ready?");
            hexControls.ChangePlayer(CellColor.White);
        }

        foreach (Troop thisTroop in troopArray) {
            thisTroop.GetComponent<LineRenderer>().SetPosition(0, new Vector3(0, 0, 0));
            thisTroop.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, 0));
            thisTroop.firstPass = true;
            if (turnNum != 0 && thisTroop.currentPos != thisTroop.transform.position) {
                thisTroop.newPos = thisTroop.transform.position;
                thisTroop.transform.position = thisTroop.currentPos;

                if (turnNum == 0) {
                    thisTroop.newPos = thisTroop.currentPos;
                }
            }
        }
        hexControls.planningMode = true;
    }
}
