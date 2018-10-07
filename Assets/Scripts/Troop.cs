using HexMapTerrain;
using HexMapTools;
using System.Collections.Generic;
using UnityEngine;

public enum TroopColor { White = 0, Blue, Red, Purple, Orange, Yellow, Brown, Green }

public class Troop : MonoBehaviour {

    //Comments should describe "Why" | "What" should be clear from the code

    //game variables
    public int movement; //keep public
    public int basePower; //keep public
    public float attackDistance; //keep public

    public int actionPower; //make private

    //location and movement (look into making private)
    public Vector3 currentPos; //keep
    public Vector3 newPos; //keep
    public TroopColor color = TroopColor.White; //keep
    private Vector3 direction = new Vector3(0,0,0);

    //Support
    public List<Troop> supportedByTroops = new List<Troop>(); //privatize both of these
    public Troop supportingTroop;

    //Action Turn
    public HexCoordinates coords; //keep this
    public List<Cell> conflictingCells = new List<Cell>();
    public List<Troop> conflictingTroops = new List<Troop>();
    public List<Cell> cellPath = new List<Cell>(); //look into redundancy
    public List<Cell> finalCellPath = new List<Cell>(); //localize this
    public List<HexCoordinates> coordPath; //look into redundancy
    public List<Vector3> vectorPath; //look into redundancy
    public List<Vector3> supportingPositions; //look into making private

    //references
    private HexControls hexControls;
    private HexCalculator hexCalculator;
    private Animator animator;
    private PlayBack playBack;
    public GameObject movementArrow;
    private PlayerColors playerColors;
    private GameManager gameManager;

    //action move animation logic
    public bool firstPass = true;
    private int moveCounter = 0;
    private Vector3 point;
    private float newPosPoint; 
    private float transformPoint;
    private bool isAnimating = false;

    private void OnDrawGizmos() {
        if (color == TroopColor.White)
            return;

        if (color == TroopColor.Red)
            Gizmos.color = Color.red;
        else if (color == TroopColor.Blue)
            Gizmos.color = Color.blue;
        else if (color == TroopColor.Purple)
            Gizmos.color = Color.magenta;
        else if (color == TroopColor.Orange)
            Gizmos.color = new Color(1f, 0.456129f, 0.07111073f);
        else if (color == TroopColor.Yellow)
            Gizmos.color = Color.yellow;
        else if (color == TroopColor.Brown)
            Gizmos.color = Color.grey;
        else if (color == TroopColor.Green)
            Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position, 0.433f);
    }

    void OnMouseDown() {
        if (hexControls.selectedTroop == this) {
            hexControls.DeselectCell();
        } else {
            hexControls.SelectTroop(this);
        }
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
        playBack = FindObjectOfType<PlayBack>();
        actionPower = basePower;
        playerColors = FindObjectOfType<PlayerColors>();
        gameManager = FindObjectOfType<GameManager>();

        if (color == TroopColor.Blue)
            color = playerColors.playerOne;
        else if (color == TroopColor.Red)
            color = playerColors.playerTwo;

        if (color == TroopColor.Blue)
            animator.SetInteger("Color", 1);
        else if (color == TroopColor.Red)
            animator.SetInteger("Color", 2);
        else if (color == TroopColor.Purple)
            animator.SetInteger("Color", 3);
        else if (color == TroopColor.Orange)
            animator.SetInteger("Color", 4);
        else if (color == TroopColor.Yellow)
            animator.SetInteger("Color", 5);
        else if (color == TroopColor.Brown)
            animator.SetInteger("Color", 6);
        else if (color == TroopColor.Green)
            animator.SetInteger("Color", 7);
    }
    void Update() {
        coords = hexCalculator.HexFromPosition(transform.position);
        AnimateTroops();
    }

    private void AnimateTroops() {
        bool moving = true;
        if (coordPath.Count > 0 && isAnimating) {
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
                    vectorPath.Clear();
                    TroopMoved();//if on review mode, update the path and call again
                }
            }
        } 
    }

    public void ActionMove() {
        if (!(coordPath.Count > 0)) {
            moveCounter = 0;
            TroopMoved();
        } else {
            point = hexCalculator.HexToPosition(coordPath[moveCounter]);
            direction = (point - transform.position) * Time.deltaTime * coordPath.Count;
            newPosPoint = (point.x * direction.x);
            isAnimating = true;
        }
    }

    private void TroopMoved() { // calls a TroopMoved function in gamemanager which counts all the moved troops and passes when all troops have moved
        isAnimating = false;
        gameManager.TroopFinishedAnimating();
        if (gameManager.turnNum % 4 == 0) {
            transform.SetParent(hexControls.GetCell(coords).transform);
            transform.position = newPos;
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
        if (ally.transform.position != ally.currentPos) { return; }
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
        ally.supportingPositions.Add(ally.transform.position);
        ally.supportingPositions.Add(transform.position);
        ally.GetComponent<LineRenderer>().SetPosition(0, ally.transform.position);
        ally.GetComponent<LineRenderer>().SetPosition(1, transform.position);
        
        actionPower += ally.basePower;
    }

    public void PlanningMove() {
        
        foreach (Troop troop in supportedByTroops) {
            troop.PlayBackReset();
            troop.supportingTroop = null;
        }
        actionPower = basePower;
        supportedByTroops.Clear();

    }

    public void PrepareAction() {
        playBack.AddTroopValues(this, vectorPath, supportingTroop,supportingPositions);
        hexControls.FindConflicts(this);
        if (conflictingCells.Count > 0) {
            CutSupport();
        }
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
                                    transform.position = hexControls.graveyard.transform.position;
                                } else {
                                    conflictSolved = true;
                                    Cell retreatCell = hexControls.GetRetreatPath(this, conflictingCells[x]);
                                    if (retreatCell == conflictingCells[x]) { // Troop could not find a cell to retreat to
                                        transform.position = hexControls.graveyard.transform.position;
                                        finalCellPath.Add(hexControls.graveyard);
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

    public void HandleAction() { 

        conflictingCells.Clear();

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
        supportingPositions.Clear();
        currentPos = newPos;
    }

    public void DrawReviewArrows(TroopValues troopValues) {
        if (troopValues.path.Count > 1) {
            GetComponent<LineRenderer>().startColor = new Color(.3191082f, 0.7802383f, 0.9528302f);
            GetComponent<LineRenderer>().endColor = new Color(.3191082f, 0.7802383f, 0.9528302f);
            GetComponent<LineRenderer>().positionCount = troopValues.path.Count;
            for (int i = 0; i < troopValues.path.Count; i++) {
                GetComponent<LineRenderer>().SetPosition(i, troopValues.path[i]);
            }
        }
        if (troopValues.supportingTroop) {
            GetComponent<LineRenderer>().SetPosition(0, troopValues.supportingPositions[0]);
            GetComponent<LineRenderer>().SetPosition(1, troopValues.supportingPositions[1]);
        }
    }

    public void DrawCorrectionArrows(List<Vector3> correctionPath) {
        if (correctionPath.Count > 1) {
            GetComponent<LineRenderer>().startColor = new Color(0.9339623f, 0.2732842f, 0.03083839f);
            GetComponent<LineRenderer>().endColor = new Color(0.9339623f, 0.2732842f, 0.03083839f);
            GetComponent<LineRenderer>().positionCount = correctionPath.Count;
             for (int i = 0; i < correctionPath.Count; i++) {
                GetComponent<LineRenderer>().SetPosition(i, correctionPath[i]);
            }
        }

    }

    public void AnimateValues(List<Vector3> animationPath) {
        firstPass = true;
        for (int i = 0; i < animationPath.Count; i++) {  
            if (!(i == 0 && animationPath[0] == transform.position))
                coordPath.Add(hexCalculator.HexFromPosition(animationPath[i]));
        }
        ActionMove();
    }

    public void PlayBackReset() {
        coordPath.Clear();
        direction = new Vector3(0,0,0);
        GetComponent<LineRenderer>().startColor = new Color(0.2588235f, 0.7254902f, 0.3738212f);
        GetComponent<LineRenderer>().endColor = new Color(0.2588235f, 0.7254902f, 0.3738212f);
        for (int i = 0; i < GetComponent<LineRenderer>().positionCount; i++) {
            GetComponent<LineRenderer>().SetPosition(i, new Vector3(0, 0, 0));
        }
    }

    public void ClearArrows() {
        GetComponent<LineRenderer>().startColor = new Color(0.2588235f, 0.7254902f, 0.3738212f);
        GetComponent<LineRenderer>().endColor = new Color(0.2588235f, 0.7254902f, 0.3738212f);
        for (int i = 0; i < GetComponent<LineRenderer>().positionCount; i++) {
            GetComponent<LineRenderer>().SetPosition(i, new Vector3(0, 0, 0));
        }
    }

}


