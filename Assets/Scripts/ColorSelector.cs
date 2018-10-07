using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ColorSelector : MonoBehaviour {

private SetTroopColor[] troops;
    public GameObject colorTaken;
    public GameObject startButton;
    public Slider mainSlider;
    private TroopColor troopColor;
    private Color textColor;
    private PlayerColors playerColors;

	// Use this for initialization

	void Start () {
        troops = FindObjectsOfType<SetTroopColor>();
        mainSlider.onValueChanged.AddListener(delegate { ValueChangedCheck(); });
        playerColors = FindObjectOfType<PlayerColors>();
        troopColor = TroopColor.Blue;
        mainSlider.value = 1;
        ChooseColor();
	}

    public void ValueChangedCheck() { // or on scene loaded
        ChooseColor();
    }

    private void ChooseColor() {
        foreach (SetTroopColor troop in troops) {
            troop.SetColor((int)mainSlider.value);
        }
        if (mainSlider.value == 1)
            troopColor = TroopColor.Blue;
        if (mainSlider.value == 2)
            troopColor = TroopColor.Red;
        if (mainSlider.value == 3)
            troopColor = TroopColor.Purple;
        if (mainSlider.value == 4)
            troopColor = TroopColor.Orange;
        if (mainSlider.value == 5)
            troopColor = TroopColor.Yellow;
        if (mainSlider.value == 6)
            troopColor = TroopColor.Brown;
        if (mainSlider.value == 7)
            troopColor = TroopColor.Green;
        if (SceneManager.GetActiveScene().name.Equals("01c Color Selector P2") && troopColor == playerColors.playerOne) {
            colorTaken.GetComponent<Text>().color = new Color(1, 1, 1, 1);
            startButton.transform.SetParent(FindObjectOfType<PlayerColors>().transform);
        } else if (SceneManager.GetActiveScene().name.Equals("01c Color Selector P2")) {
            colorTaken.GetComponent<Text>().color = new Color(1, 1, 1, 0);
            startButton.transform.SetParent(FindObjectOfType<LevelCanvas>().transform);
        }
        if (SceneManager.GetActiveScene().name.Equals("01c Color Selector P1")) {
            playerColors.playerOne = troopColor;
        } else {
            playerColors.playerTwo = troopColor;
        }
    }
}
