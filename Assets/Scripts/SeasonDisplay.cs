using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeasonDisplay : MonoBehaviour {

    public int startYear = 2010; //starting year is 2010
    private Text seasonText; 
    private int currentYear;
    private Dictionary<int, string> seasons = new Dictionary<int, string>() { { 0, "Winter" }, { 1, "Spring" },{ 2, "Summer" },{ 3, "Autumn" } };
    private string currentSeason = "Winter"; //starting season is winter

	// Use this for initialization
	void Start () {
        seasonText = GetComponent<Text>();
		currentYear = startYear; // 
        seasonText.text = currentSeason + " " + currentYear;
	}

    public void SeasonCounter(int roundNum) { //Display by what number turn it is // Year is start year + (int)turnNum/4| Season is turnNum%4
        currentYear = startYear + roundNum/4;
        currentSeason = seasons[roundNum%4];
        seasonText.text = currentSeason + " " + currentYear;
    }


    public void Disable() {
        transform.SetParent(FindObjectOfType<GameManager>().transform);
    }

    public void Enable() {
        transform.SetParent(FindObjectOfType<LevelCanvas>().transform);
    }

}
