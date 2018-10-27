using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverDisplay : MonoBehaviour {

    public void Disable() {
        transform.SetParent(FindObjectOfType<GameManager>().transform);
    }

    public void Enable() {
        transform.SetParent(FindObjectOfType<LevelCanvas>().transform);
    }
}
