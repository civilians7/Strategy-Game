using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMapTools;
using UnityEngine.UI;

public class PlayBack : MonoBehaviour {

    private List<Dictionary<Troop,List<TroopValues>>> seasonsList = new List<Dictionary<Troop,List<TroopValues>>>(); // SeasonsList contains SeasonsReview contains troopReview contains troopValues
    private int selectedSeason;
    private Slider seasonSlider;
    private GameManager gameManager;
    private SeasonDisplay seasonDisplay;
    public bool fullPlayBack = false;
	// Use this for initialization
	void Start () {
        seasonSlider = FindObjectOfType<SeasonSlider>().GetComponent<Slider>(); 
        gameManager = FindObjectOfType<GameManager>();
        seasonDisplay = FindObjectOfType<SeasonDisplay>();
	}

    public void CreateNewSeason() {
        Dictionary<Troop, List<TroopValues>> seasonReview = new Dictionary<Troop, List<TroopValues>>();
        foreach (Troop troop in gameManager.troopArray) {
            List<TroopValues> troopReview = new List<TroopValues>();
            seasonReview.Add(troop, troopReview);
        }
        seasonsList.Add(seasonReview);

        seasonSlider.maxValue = seasonsList.Count-1;
    }

    public void AddTroopValues(Troop troop, List<Vector3> vectorPath, Troop supportingTroop, List<Vector3> supportingPositions) {
        Dictionary<Troop,List<TroopValues>> seasonReview = seasonsList[seasonsList.Count - 1];
        List<Vector3> troopPath = new List<Vector3>();
        List<Vector3> troopSupportingPos = new List<Vector3>();
        foreach (Vector3 position in vectorPath) {
            troopPath.Add(position);
        }
        foreach (Vector3 position in supportingPositions) {
            troopSupportingPos.Add(position);
        }
        TroopValues troopValues = new TroopValues (troopPath,supportingTroop,troopSupportingPos);
        seasonReview[troop].Add(troopValues);
    }

    public void SeasonSelector(float season) { //Just set positions
        selectedSeason = (int)season;
        seasonDisplay.SeasonCounter(selectedSeason);
        
        foreach (Troop troop in gameManager.troopArray) {
            troop.PlayBackReset();
            troop.transform.position = (seasonsList[selectedSeason][troop][0].path[0]); //passing troopReview to each respective troop
            troop.DrawReviewArrows(seasonsList[selectedSeason][troop][0]);
        }
    }

    private int maxTroopValues = -1;
    private int troopValueIteration = 0;

    public void AnimatePlayBack() {

        maxTroopValues = seasonsList[selectedSeason][gameManager.troopArray[0]].Count;

        foreach (Troop troop in gameManager.troopArray) {
            List<Vector3> correctionPath = new List<Vector3>();
            bool pathCorrected = false;
            List<TroopValues> troopValues = seasonsList[selectedSeason][troop];
            correctionPath.Clear();
            if (troopValueIteration < maxTroopValues - 1) {
                List<Vector3> troopPath = seasonsList[selectedSeason][troop][troopValueIteration].path;
                Vector3 thisPathEnd = troopValues[troopValueIteration].path[troopValues[troopValueIteration].path.Count - 1];
                Vector3 nextPathEnd = troopValues[troopValueIteration + 1].path[troopValues[troopValueIteration + 1].path.Count - 1];

                if (thisPathEnd != nextPathEnd) {//this works only if path is corrected from starting pos
                    pathCorrected = true;
                    correctionPath.Add(troopPath[0]);
                    correctionPath.Add(nextPathEnd);
                    troop.ClearArrows();
                    troop.DrawCorrectionArrows(correctionPath);
                }

            }
            if (pathCorrected) {
                troop.AnimateValues(correctionPath);
            } else {
                troop.AnimateValues(troopValues[0].path);
            }
        }

        if (seasonsList.Count - 1 == selectedSeason) {
            troopValueIteration = maxTroopValues-1;
            fullPlayBack = false;
        }

    }

    public void FullPlay() {
        fullPlayBack = !fullPlayBack;
        if (!fullPlayBack) { return; }
        AnimatePlayBack();
    }

    public void StepBack() { //for every troop construct a full game single list path that removes repeats and smoothly animates back and forth
        maxTroopValues = seasonsList[selectedSeason][gameManager.troopArray[0]].Count;
        fullPlayBack = false;
        if (troopValueIteration > 0) {
            troopValueIteration=0;

            foreach (Troop troop in gameManager.troopArray) {
                List<TroopValues> troopValues = seasonsList[selectedSeason][troop];
                List<Vector3> correctionPath = new List<Vector3>();
                List<Vector3> startingPosition = new List<Vector3>();
                startingPosition.Clear();
                correctionPath.Clear();
                bool pathCorrected = false;
                if (troopValueIteration < maxTroopValues - 1) {
                    List<Vector3> troopPath = seasonsList[selectedSeason][troop][troopValueIteration].path;
                    Vector3 thisPathEnd = troopValues[troopValueIteration].path[troopValues[troopValueIteration].path.Count - 1];
                    Vector3 nextPathEnd = troopValues[troopValueIteration + 1].path[troopValues[troopValueIteration + 1].path.Count - 1];
                    startingPosition.Add(troopPath[0]);
                    if (thisPathEnd != nextPathEnd) {//this works only if path is corrected from starting pos
                        pathCorrected = true;
                        correctionPath.Add(troopPath[0]);
                        correctionPath.Add(nextPathEnd);
                        troop.ClearArrows();
                        troop.DrawCorrectionArrows(correctionPath);
                    }

                }
                
                List<Vector3> reversePath = new List<Vector3>();
                for (int i = troopValues[troopValueIteration].path.Count - 1; i >= 0; i--) {
                    if (!(troopValues[troopValueIteration].path[i] == transform.position))
                        reversePath.Add(troopValues[troopValueIteration].path[i]);
                }
                
                if (pathCorrected) {
                    troop.AnimateValues(startingPosition);
                } else {
                    troop.AnimateValues(reversePath);
                }

            }

        } else if (selectedSeason > 0) {
            seasonSlider.value--;
            troopValueIteration = maxTroopValues;
        }

    }


    public void StepForward() {
        maxTroopValues = seasonsList[selectedSeason][gameManager.troopArray[0]].Count;
        fullPlayBack = false;
        if (troopValueIteration < maxTroopValues) {
            maxTroopValues = seasonsList[selectedSeason][gameManager.troopArray[0]].Count;

            foreach (Troop troop in gameManager.troopArray) {
                List<TroopValues> troopValues = seasonsList[selectedSeason][troop];
                List<Vector3> correctionPath = new List<Vector3>();
                List<Vector3> startingPosition = new List<Vector3>();
                startingPosition.Clear();
                correctionPath.Clear();
                bool pathCorrected = false;
                if (troopValueIteration < maxTroopValues - 1) {
                    List<Vector3> troopPath = seasonsList[selectedSeason][troop][troopValueIteration].path;
                    Vector3 thisPathEnd = troopValues[troopValueIteration].path[troopValues[troopValueIteration].path.Count - 1];
                    Vector3 nextPathEnd = troopValues[troopValueIteration + 1].path[troopValues[troopValueIteration + 1].path.Count - 1];
                    startingPosition.Add(troopPath[0]);
                    if (thisPathEnd != nextPathEnd) {//this works only if path is corrected from starting pos
                        pathCorrected = true;
                        correctionPath.Add(troopPath[0]);
                        correctionPath.Add(nextPathEnd);
                        troop.ClearArrows();
                        troop.DrawCorrectionArrows(correctionPath);
                    } else {
                        troopValueIteration++;
                    }

                }
                if (pathCorrected) {
                    troop.AnimateValues(startingPosition);
                } else {
                    troop.AnimateValues(troopValues[troopValueIteration].path);
                }

            }
                troopValueIteration++;
        
        } else {
            if (seasonsList.Count - 1 > selectedSeason) {
                troopValueIteration = 0;
                seasonSlider.value++;
            } 
        }
    }
}

public class TroopValues {
    public List<Vector3> path = new List<Vector3>();
    public Troop supportingTroop;
    public List<Vector3> supportingPositions = new List<Vector3>();

    public TroopValues(List<Vector3> path, Troop troop, List<Vector3> supportingPositions) {
        this.path = path;
        this.supportingTroop = troop;
        this.supportingPositions = supportingPositions;
    }
}


