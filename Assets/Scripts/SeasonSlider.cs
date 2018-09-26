using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeasonSlider : MonoBehaviour {

    private Slider seasonSlider;
    private PlayBack playBack;

    // Use this for initialization
    void Start() {
        seasonSlider = GetComponent<Slider>();
        playBack = FindObjectOfType<PlayBack>();
        seasonSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    public void ValueChangeCheck() {
        playBack.SeasonSelector(seasonSlider.value);
    }

    public void EnterReviewMode() {
        seasonSlider.value = seasonSlider.maxValue;
    }
}
