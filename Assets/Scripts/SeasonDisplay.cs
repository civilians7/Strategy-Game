using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeasonDisplay : MonoBehaviour {

    public int startYear = 2010; 

    private Text seasonText; 
    private int currentYear; 
    private int currentSeasonNum = 0; 
    private string currentSeason = "Winter"; 
    Text text;

	// Use this for initialization
	void Start () {
        seasonText = GetComponent<Text>();
		currentYear = startYear;
        seasonText.text = currentSeason + " " + currentYear;
        text = GetComponent<Text>();
	}

    public void SeasonCounter() { //refactor to season display
        if (currentSeasonNum == 3) {
            currentSeasonNum = 0;
        } else {
            currentSeasonNum++;
        }

        if (currentSeasonNum == 0) {
            currentSeason = "Winter";
            currentYear++;
        } else if (currentSeasonNum == 1) {
            currentSeason = "Spring";
        } else if (currentSeasonNum == 2) {
            currentSeason = "Summer";
        } else if (currentSeasonNum == 3) {
            currentSeason = "Autumn";
        }

        seasonText.text = currentSeason + " " + currentYear;
    }

    public void Disable() {
        text.color = new Color(text.color.r,text.color.g,text.color.b,0f);
        enabled = false;
    }

    public void Enable() {
        text.color = new Color(text.color.r,text.color.g,text.color.b,1f);
        enabled = true;
    }

}
