using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMapTerrain;
using UnityEngine.UI;

//center of the archetecture, handles comunication between other classes and handles most of the game engine logic

public class GameManager : MonoBehaviour {

    public int turnNum; //keep
    public int turnsPerSeason = 1; //keep
    public Troop[] troopArray; //keep
    public bool playBackMode = false;
    private PlayBack playBack;
    private TurnDisplay turnDisplay;
    private SeasonDisplay seasonDisplay;
    private PlayBackDisplay playBackDisplay;
    private GameOverDisplay gameOverDisplay;
    private HexControls hexControls;
    private ReviewButton reviewButton;
    private PlayerColors playerColors;
    private SeasonSlider seasonSlider;
    private LevelManager levelManager;
    private int roundNum;
    public TroopColor playerOneColor = TroopColor.Blue;
    public TroopColor playerTwoColor = TroopColor.Red;
    public TroopColor winner;
    public bool tieBreaker = false;
    public bool gameOver = false;
    public Text[] PlayerScores = new Text[2];
    public bool continueAction = false;
    public int itteration;

    // Use this for initialization
    void Start() {
        turnDisplay = FindObjectOfType<TurnDisplay>();
        seasonDisplay = FindObjectOfType<SeasonDisplay>();
        playBackDisplay = FindObjectOfType<PlayBackDisplay>();
        gameOverDisplay = FindObjectOfType<GameOverDisplay>();
        hexControls = FindObjectOfType<HexControls>();
        reviewButton = FindObjectOfType<ReviewButton>();
        playBack = FindObjectOfType<PlayBack>();
        seasonSlider = FindObjectOfType<SeasonSlider>();
        levelManager = FindObjectOfType<LevelManager>();
        playerColors = FindObjectOfType<PlayerColors>();

        if (playerColors) {
            playerOneColor = playerColors.playerOne;
            playerTwoColor = playerColors.playerTwo;
        }
        
        playBackDisplay.Disable();
        reviewButton.Disable();
        gameOverDisplay.GetComponent<Text>().text = "";
        
    }

    void Update() {
        troopArray = FindObjectsOfType<Troop>();
    }

    public void EndTurn() { //rename and clean up /refactor / comment code
        if (gameOver) {
            levelManager.LoadLevel("01a Start");
            return;
        }
        turnNum++;
        troopArray = FindObjectsOfType<Troop>();
        hexControls.planningMode = false;
        hexControls.DeselectCell();

        if (turnNum % 4 == 0) {
            roundNum++;
            playBack.CreateNewSeason();
            seasonDisplay.SeasonCounter(roundNum);
            turnDisplay.SetText("Next Season");
            reviewButton.Disable();
            turnDisplay.Disable();
            seasonDisplay.Disable();
            hexControls.ChangePlayer(TroopColor.White);
            itteration = 1;
            StartActionPhase(itteration);

        } else if (turnNum % 4 == 1) {
            if (roundNum > 0) { reviewButton.Enable(); }
            turnDisplay.SetText("Finish Turn");
            hexControls.ChangePlayer(playerOneColor);
        } else if (turnNum % 4 == 2) {
            hexControls.ChangePlayer(playerTwoColor);

        } else if (turnNum % 4 == 3) {
            turnDisplay.SetText("Ready?");
            hexControls.ChangePlayer(TroopColor.White);
        }

        foreach (Troop thisTroop in troopArray) {
            thisTroop.GetComponent<LineRenderer>().SetPosition(0, new Vector3(0, 0, 0));
            thisTroop.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, 0));
            thisTroop.firstPass = true;
            if (turnNum % 4 != 0 && thisTroop.currentPos != thisTroop.transform.position) {
                thisTroop.newPos = thisTroop.transform.position;
                thisTroop.transform.position = thisTroop.currentPos;

                if (turnNum == 0) {
                    thisTroop.newPos = thisTroop.currentPos;
                }
            }
        }
        hexControls.planningMode = true;
    }

    private void StartActionPhase(int i) {
        foreach (Troop troop in troopArray) {
            if (i == 1)
                hexControls.FindPath(troop);
        }
        foreach (Troop troop in troopArray) {
            hexControls.FindConflicts(troop, i);
        }
        foreach (Troop troop in troopArray) {
            troop.PrepareAction();
        }
        foreach (Troop troop in troopArray) {
            if (troop.ResolveConflicts(i))
            troop.conflictingTroop = null;
        }
        ActionItterationComplete();
    }

    public void ActionItterationComplete() {
        if (continueAction) {
            continueAction = false;
            itteration++;
            if (itteration < 6) {
                StartActionPhase(itteration);//
            } else {
                Debug.LogWarning("Action Phase itterated 6 times");
                EnableDisplays();
            }
        } else {
            //Animate Troops
            foreach (Troop troop in troopArray) {
                troop.HandleAction();
            }
        }
    }

    int numOfTroopsFinished = 0;
    public void TroopFinishedAnimating() { // troop finished animating action
        int numOfTroops = troopArray.Length;
        numOfTroopsFinished++;
        if (numOfTroopsFinished == numOfTroops) {
             numOfTroopsFinished =  0;
            if (turnNum % 4 == 0) {
                foreach (Troop troop in troopArray) {
                    troop.coordPath.Clear();
                    troop.cellPath.Clear();
                    troop.vectorPath.Clear();
                }
                EnableDisplays();
                CalculatePowerScores();
            } else if (playBack.fullPlayBack) {
                seasonSlider.GetComponent<Slider>().value++;
                playBack.AnimatePlayBack();
            }
        }
    }

    private void EnableDisplays() {
        turnDisplay.Enable();
        seasonDisplay.Enable();
    }

    public void TogglePlaybackMode() {
        playBackMode = !playBackMode;
        if (playBackMode) { //turn on playback
            FindObjectOfType<SeasonSlider>().EnterReviewMode();
            gameOverDisplay.Disable();
            turnDisplay.Disable();
            playBackDisplay.Enable();
            if (roundNum == 1)
                seasonSlider.Disable();
        } else { // turn off playback
            gameOverDisplay.Enable();
            turnDisplay.Enable();
            foreach (Troop troop in troopArray) {
                troop.PlayBackReset();
                troop.transform.position = troop.currentPos;
            }
            playBackDisplay.Disable();
        }
    }

    void CalculatePowerScores() {
        int[] powerInEnemy = new int[2]; //make this variable later (count of players)
        foreach (Troop troop in troopArray) {
            if (troop.player != troop.GetComponentInParent<Cell>().playerTerritory) { //do this after each season
                powerInEnemy[troop.player - 1] += troop.basePower;
            }
        }
        PlayerScores[0].text = powerInEnemy[0].ToString();
        PlayerScores[1].text = powerInEnemy[1].ToString();
    }

    public void CheckGameOver() {
        if (gameOver) {
            if (tieBreaker) {
                HandleTieBreaker();
            } else {
                HandleGameOver();
            }
        }
    }

    private void HandleTieBreaker() { //check which player has more power in enemy territory, that player wins
        int[] powerInEnemy = new int[2]; //make this variable later (count of players)
        foreach (Troop troop in troopArray) {
            if (troop.player != troop.GetComponentInParent<Cell>().playerTerritory) { //do this after each season
                powerInEnemy[troop.player - 1] += troop.basePower;
            }
        }
        if (powerInEnemy[0] > powerInEnemy[1]) {
            winner = playerOneColor;
        } else {
            winner = playerTwoColor;
        }
        HandleGameOver();
    }

    private void HandleGameOver() {
        gameOverDisplay.GetComponent<Text>().text = winner + " Wins!";
        turnDisplay.SetText("Exit to Menu");
        hexControls.HandleWin(winner);
        reviewButton.Enable();
    }

}
