using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMapTerrain;

//center of the archetecture, handles comunication between other classes and handles most of the game engine logic

public class GameManager : MonoBehaviour {

    public int turnNum; //keep
    public int turnsPerSeason = 1; //keep
    public Troop[] troopArray; //keep
    public bool playBackMode = false;
    private PlayBack playBack;
    private TurnDisplay turnDisplay;
    private SeasonDisplay seasonDisplay;
    private PlayBackDisplay[] playBackDisplay;
    private HexControls hexControls;
    private ReviewButton reviewButton;
    private int roundNum;

    // Use this for initialization
    void Start() {
        turnDisplay = FindObjectOfType<TurnDisplay>();
        seasonDisplay = FindObjectOfType<SeasonDisplay>();
        playBackDisplay = FindObjectsOfType<PlayBackDisplay>();
        hexControls = FindObjectOfType<HexControls>();
        reviewButton = FindObjectOfType<ReviewButton>();
        playBack = FindObjectOfType<PlayBack>();
        reviewButton.Disable();
        foreach (PlayBackDisplay display in playBackDisplay) {
            display.Disable();
        }
    }

    void Update() {
        troopArray = FindObjectsOfType<Troop>();
    }

    public void EndTurn() { //rename and clean up code
        turnNum++;
        troopArray = FindObjectsOfType<Troop>();
        hexControls.planningMode = false;
        hexControls.DeselectCell();

        if (turnNum%4 == 0) {
            roundNum++;
            playBack.CreateNewSeason();
            bool conflictSolved = false;
            seasonDisplay.SeasonCounter(roundNum);
            turnDisplay.SetText("Next Season");
            reviewButton.Disable();
            turnDisplay.Disable();
            seasonDisplay.Disable();
            hexControls.ChangePlayer(TroopColor.White);
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
                if (i == 5) { Debug.LogWarning("Action Turn Looped 5 times"); }
            } while (conflictSolved && i < 5);
            foreach (Troop troop in troopArray) {
                troop.HandleAction();
            }
            Invoke("EnableDisplays", 1);

        } else if (turnNum%4 == 1) {
            if (roundNum > 0) {reviewButton.Enable(); }
            turnDisplay.SetText("Finish Turn");
            hexControls.ChangePlayer(TroopColor.Blue);
        } else if (turnNum%4 == 2) {
            hexControls.ChangePlayer(TroopColor.Red);

        } else if (turnNum%4 == 3) {
            turnDisplay.SetText("Ready?");
            hexControls.ChangePlayer(TroopColor.White);
        }

        foreach (Troop thisTroop in troopArray) {
            thisTroop.GetComponent<LineRenderer>().SetPosition(0, new Vector3(0, 0, 0));
            thisTroop.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, 0));
            thisTroop.firstPass = true;
            if (turnNum%4 != 0 && thisTroop.currentPos != thisTroop.transform.position) {
                thisTroop.newPos = thisTroop.transform.position;
                thisTroop.transform.position = thisTroop.currentPos;

                if (turnNum == 0) {
                    thisTroop.newPos = thisTroop.currentPos;
                }
            }
        }
        hexControls.planningMode = true;
    }

    private void EnableDisplays() {
        turnDisplay.Enable();
        seasonDisplay.Enable();
    }

    public void TogglePlaybackMode() {
        FindObjectOfType<SeasonSlider>().EnterReviewMode();
        playBackMode = !playBackMode;
        if (playBackMode) { //turn on playback
            turnDisplay.Disable();
            foreach (PlayBackDisplay display in playBackDisplay) {
                display.Enable();
            }
        } else { // turn off playback
            turnDisplay.Enable();
            foreach(Troop troop in troopArray) {
                troop.ResetArrows();
                troop.transform.position = troop.currentPos;
            }
            foreach (PlayBackDisplay display in playBackDisplay) {
                display.Disable();
            }
        }
    }
}
