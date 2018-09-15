using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMapTerrain;
using HexMapTools;

public enum CellColor { White = 0, Blue, Red, Purple, Orange, Yellow, Brown, Green }

public class Troop : MonoBehaviour {

    //Comments should describe "Why" | "What" should be clear from the code

    //game variables
    public int movement; //keep public
    public int basePower; //keep public
    public float attackDistance; //keep public

    private int actionPower; //make private

    //location and movement
    public Vector3 currentPos;
    public Vector3 newPos;
    public CellColor color = CellColor.Blue;
    public Vector3 direction = new Vector3(0,0,0);

    //Support
    public List<Troop> supportedByTroops = new List<Troop>(); //privatize both of these
    public Troop supportingTroop;

    //Action Turn
    public HexCoordinates coords;
    public List<Cell> conflictingCells = new List<Cell>();
    public List<Troop> conflictingTroops = new List<Troop>();
    public List<Cell> cellPath = new List<Cell>();
    public List<Cell> finalCellPath = new List<Cell>();
    public List<HexCoordinates> coordPath;
    public List<HexCoordinates> reviewAnimation;

    //class references
    private HexControls hexControls;
    private GameManager gameManager; //not used
    private HexCalculator hexCalculator;

    //action move animation logic
    public bool firstPass = true;
    private bool moving = true;
    private int moveCounter = 0;
    private Vector3 point;
    private float newPosPoint; 
    private float transformPoint;

    // Use this for initialization
    void Start() {
        HexGrid hexGrid = FindObjectOfType<HexGrid>().GetComponent<HexGrid>();
        currentPos = transform.position;
        newPos = currentPos;
        supportedByTroops.Add(this);
        hexControls = FindObjectOfType<HexControls>();
        gameManager = FindObjectOfType<GameManager>();
        hexCalculator = hexGrid.HexCalculator;

    }
    void Update() {
        coords = hexCalculator.HexFromPosition(transform.position);
        if (coordPath.Count > 0) {
            transformPoint = (transform.position.x * direction.x);
            transform.Translate(direction);

            if (firstPass) {
                moving = newPosPoint > transformPoint;
                firstPass = false;
            }
            if (moving != (newPosPoint > transformPoint)) {
                direction = new Vector3(0, 0, 0);
                if (coordPath.Count > moveCounter + 1) {
                    moveCounter++;
                    ActionMove();
                } else {
                    moveCounter = 0;
                    coordPath.Clear();
                    cellPath.Clear();
                    transform.position = newPos;
                    hexControls.TroopMoved(this);
                }
            } 
        }

    }

    public void ActionMove() {
        if (!(coordPath.Count > 0)) {
            moveCounter = 0;
            hexControls.TroopMoved(this);
        } else {
            point = hexCalculator.HexToPosition(coordPath[moveCounter]);
            direction = (point - transform.position) * Time.deltaTime * coordPath.Count;
            newPosPoint = (point.x * direction.x);
        }
    }

    public void Support(Troop troop) {
        HexCoordinates[] neighbors = HexUtility.GetNeighbours(coords);
        if (troop.color != color) { return; }
        foreach (HexCoordinates neighbor in neighbors) {
            if (neighbor == troop.coords) {
                SupportedBy(troop);
                return;
            }
        }
        
    }

    void SupportedBy(Troop ally) {

        foreach (Troop troop in supportedByTroops) {
            if (ally == troop || ally.supportingTroop != null) {
                return;
            }
        }
        foreach (Troop troop in ally.supportedByTroops) {
            if (this == troop) {
                return;
            }
        }
        ally.supportingTroop = this;
        supportedByTroops.Add(ally);

        ally.GetComponent<LineRenderer>().SetPosition(0, ally.transform.position);
        ally.GetComponent<LineRenderer>().SetPosition(1, transform.position);
        
        actionPower += ally.basePower;
    }

    public void Move() {
        
        foreach (Troop troop in supportedByTroops) {
            troop.GetComponent<LineRenderer>().SetPosition(0, new Vector3(0, 0, 0));
            troop.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, 0));
            troop.supportingTroop = null;
        }
        actionPower = basePower;
        supportedByTroops.Clear();

    }

    public void DestroyTroop() {
        Destroy(gameObject);
    }

    void OnMouseDown() {
        if (hexControls.selectedTroop == this) {
            hexControls.DeselectCell();
        } else {
            hexControls.SelectTroop(this);
        }
    }

    public bool ActionTurn() {
        hexControls.FindConflicts(this);
        if (conflictingCells.Count > 0) {
            CutSupport();
        }
        return ResolveConflicts();
    }

    public void CutSupport() { //Move to Troop since it deals with troop properties
        if (supportingTroop) {
            supportingTroop.supportedByTroops.Remove(this);
            supportingTroop.actionPower -= basePower;
            supportingTroop = null;
        }
    }

    public bool ResolveConflicts() {
        bool conflictSolved = false;
        bool pathStopped = false;
        for (int i = 0; i < cellPath.Count; i++) { // loop through animation path and check for the conflict cells
            bool emptySpace = true;
            if (!pathStopped) {
                for (int x = 0; x < conflictingCells.Count; x++) {
                    if (cellPath[i] == conflictingCells[x]) {
                        emptySpace = false;
                        Debug.Log(conflictingCells[x] == GetComponentInParent<Cell>()); // special condition: conflict is handled at troops currentpos
                        if (conflictingCells[x] == GetComponentInParent<Cell>()) { // Troop is attacked by enemy and is forced to retreat
                            if (conflictingTroops[x].actionPower > actionPower) {
                                Debug.Log(color + " retreating");
                                if (GetComponent<HQ>()) { //Handle End Game Condition
                                    Debug.LogWarning("Game Over! " + conflictingTroops[x].color + " wins!");
                                } else {
                                    Cell retreatCell = hexControls.GetRetreatPath(this, conflictingCells[x]);
                                    if (retreatCell == conflictingCells[x]) { // Troop could not find a cell to retreat to
                                        print(color + " being destroyed");
                                        Destroy(this);
                                    } else {
                                        finalCellPath.Add(retreatCell);
                                    }
                                }
                            }
                        } else if (conflictingTroops[x].actionPower < actionPower) { //  Troop attacks enemy troop and moves into the space
                            finalCellPath.Add(conflictingCells[x]);
                            conflictSolved = true;
                        } else if (conflictingTroops[x].actionPower >= actionPower) { //if conflict troop is equal to or greater than -> stop looping or set pathStopped bool to true and ignore rest
                            pathStopped = true;
                        }
                    }
                }
                if (emptySpace && cellPath[i] != GetComponentInParent<Cell>()) {
                    conflictSolved = true;
                    finalCellPath.Add(cellPath[i]);
                }
            }
        }
        if (conflictSolved) { //troop successfully moved with conflicts
            List<Cell> newCell = new List<Cell>();
            newCell.Add(finalCellPath[finalCellPath.Count - 1]);
            conflictingCells.Clear();
            hexControls.SetPath(this, newCell);
        }
        return conflictSolved;

    }

    public void HandleAction() { //Troop because it deals only with troop properties, loop is unnecesary

        conflictingCells.Clear();

        reviewAnimation.Clear();
        //foreach (HexCoordinates path in coordPath) {
        //    reviewAnimation.Add(path);
        //}
        hexControls.SetPath(this, finalCellPath);
        if (finalCellPath.Count > 0) {
            newPos = finalCellPath[finalCellPath.Count - 1].transform.position;
        } else {
            newPos = transform.position;
        }
        ActionMove();
        finalCellPath.Clear();
        actionPower = basePower;
        supportingTroop = null;
        supportedByTroops.Clear();
        currentPos = newPos;
    }

}


