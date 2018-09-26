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

    public void SeasonSelector(float season) {
        selectedSeason = (int)season;
        seasonDisplay.SeasonCounter(selectedSeason);
        
        foreach (Troop troop in gameManager.troopArray) {
            troop.ResetArrows();
            troop.SeasonPlayBack(seasonsList[selectedSeason][troop]); //passing troopReview to each respective troop
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


