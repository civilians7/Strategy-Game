using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMapTerrain;
using HexMapTools;
using UnityEditor;

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
    public CellColor color = CellColor.White;
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

    //references
    private HexControls hexControls;
    private HexCalculator hexCalculator;
    private Animator animator;

    //action move animation logic
    public bool firstPass = true;
    private bool moving = true;
    private int moveCounter = 0;
    private Vector3 point;
    private float newPosPoint; 
    private float transformPoint;


    private void OnDrawGizmos()
        {
            if (color == CellColor.White)
                return;

            if (color == CellColor.Red)
                Gizmos.color = Color.red;
            else if(color == CellColor.Blue)
                Gizmos.color = Color.blue;
            else if(color == CellColor.Purple)
                Gizmos.color = Color.magenta;
            else if(color == CellColor.Orange)
                Gizmos.color = new Color(1f,0.456129f,0.07111073f);
            else if(color == CellColor.Yellow)
                Gizmos.color = Color.yellow;
            else if(color == CellColor.Brown)
                Gizmos.color = Color.grey;
            else if(color == CellColor.Green)
                Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(transform.position, 0.433f);
        }

    // Use this for initialization
    void Start() {
        HexGrid hexGrid = FindObjectOfType<HexGrid>().GetComponent<HexGrid>();
        currentPos = transform.position;
        newPos = currentPos;
        supportedByTroops.Add(this);
        hexControls = FindObjectOfType<HexControls>();
        animator = GetComponent<Animator>();
        hexCalculator = hexGrid.HexCalculator;
        actionPower = basePower;

        if (color == CellColor.Blue) 
            animator.SetInteger("Color", 1);
        else if (color == CellColor.Red) 
            animator.SetInteger("Color", 2);
        else if (color == CellColor.Purple) 
            animator.SetInteger("Color", 3);
        else if (color == CellColor.Orange) 
            animator.SetInteger("Color", 4);
        else if (color == CellColor.Yellow) 
            animator.SetInteger("Color", 5);
        else if (color == CellColor.Brown) 
            animator.SetInteger("Color", 6);
        else if (color == CellColor.Green) 
            animator.SetInteger("Color", 7);
        
    }
    void Update() {
        coords = hexCalculator.HexFromPosition(transform.position);
        AnimateTroops();

    }

    private void AnimateTroops() {
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
                        if (conflictingCells[x] == GetComponentInParent<Cell>()) { // Troop is attacked by enemy and is forced to retreat
                            if (conflictingTroops[x].actionPower > actionPower) {                               
                                if (GetComponent<HQ>()) { //Handle End Game Condition
                                    Debug.LogWarning("Game Over! " + conflictingTroops[x].color + " wins!");
                                    Destroy(gameObject);
                                } else {
                                    conflictSolved = true;
                                    Cell retreatCell = hexControls.GetRetreatPath(this, conflictingCells[x]);
                                    if (retreatCell == conflictingCells[x]) { // Troop could not find a cell to retreat to
                                        Destroy(gameObject);
                                        return true;
                                    } else {
                                        finalCellPath.Add(retreatCell);
                                    }
                                }
                            }
                        } else if (conflictingTroops[x].actionPower < actionPower) { //  Troop attacks enemy troop and moves into the space
                            if (!finalCellPath.Contains(conflictingCells[x])) { //This is just a temporary hack, look into problem more later (when target is destroyed it loops all the way to the end)
                                finalCellPath.Add(conflictingCells[x]);
                                conflictSolved = true;
                            }
                        } else if (conflictingTroops[x].actionPower >= actionPower) { // Troop is unable to move
                            pathStopped = true;
                        }
                    }
                }

                if (emptySpace && cellPath.Count > 1 && cellPath[i] != GetComponentInParent<Cell>()) {
                    conflictSolved = true;
                    finalCellPath.Add(cellPath[i]);
                }
            }
        }
        if (conflictSolved) { //troop successfully moved with conflicts
            List<Cell> newCell = new List<Cell>();
            newCell.Add(finalCellPath[finalCellPath.Count - 1]);
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


