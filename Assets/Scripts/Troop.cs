using HexMapTerrain;
using HexMapTools;
using System.Collections.Generic;
using UnityEngine;

public enum TroopColor { White = 0, Blue, Red, Purple, Orange, Yellow, Brown, Green }

public class Troop : MonoBehaviour {

    //Comments should describe "Why" | "What" should be clear from the code

    //game variables
    public int movement; //keep public move to units
    public int basePower; //keep public move to units
    public float attackDistance; //keep public move to units
    public int player;

    public int actionPower; //make private keep

    //location and movement (look into making private)
    public Vector3 currentPos; //keep move to units
    public Vector3 newPos; //keep move to units
    public TroopColor color = TroopColor.White; //keep moove to units
    private Vector3 direction = new Vector3(0,0,0); // move to units

    //Support
    public List<Troop> supportedByTroops = new List<Troop>(); //privatize both of these keep
    public Troop supportingTroop;//keep

    //Action Turn
    public HexCoordinates coords; //keep this keep hex stuff
    public Cell conflictingCell;
    public Troop conflictingTroop;
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
    private GameManager gameManager;

    //action move animation logic
    public bool firstPass = true;
    private int moveCounter = 1;
    private Vector3 nextAnimationPos;
    private float newPosPoint; 
    private float transformPoint;
    private bool isAnimating = false;
    public bool continueAction;

    private void OnDrawGizmos() {
        if (color == TroopColor.White)
            return;

        if (color == TroopColor.Red) //possibly move to Units
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

    void OnMouseDown() { //move to units
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
        gameManager = FindObjectOfType<GameManager>();
        if (FindObjectOfType<PlayerColors>()) {
            if (player == 1)
                color = FindObjectOfType<PlayerColors>().playerOne;
            else if (player == 2)
                color = FindObjectOfType<PlayerColors>().playerTwo;
        } 

        animator.SetInteger("Color", (int)color);

    }
    void Update() {
        coords = hexCalculator.HexFromPosition(transform.position);
        AnimateTroops();
    }

    private void AnimateTroops() {
        bool moving = true;
        if (coordPath.Count > 1 && isAnimating) {
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
                    moveCounter = 1;
                    TroopMoved();//if on review mode, update the path and call again
                }
            }
        } 
    }

    public void ActionMove() {
        if (!(coordPath.Count > 1)) {
            moveCounter = 1;
            TroopMoved();
        } else {
            nextAnimationPos = vectorPath[moveCounter];
            direction = (nextAnimationPos - transform.position) * Time.deltaTime * coordPath.Count;
            newPosPoint = (nextAnimationPos.x * direction.x);
            isAnimating = true;
        }
    }

    private void TroopMoved() { // calls a TroopMoved function in gamemanager which counts all the moved troops and passes when all troops have moved
        isAnimating = false;
        if (gameManager.turnNum % 4 == 0) {
            transform.SetParent(hexControls.GetCell(coords).transform);
            gameManager.CheckGameOver();
            transform.position = newPos;
        } 
        gameManager.TroopFinishedAnimating();
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
        CheckCutSupport();
    }

    public void CheckCutSupport() { //Checks if troop looses its support
        if (!supportingTroop) { return; }
        bool keepSupport = true;
        if (conflictingTroop) {
            keepSupport = supportingTroop.conflictingTroop == conflictingTroop;
            if (!keepSupport)
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

    private void OnCollisionEnter2D(Collision2D collision) {
        Debug.Log("Collision");
    }

    public bool ResolveConflicts(int i) { // all troops check the ability to move one space
        pathAltered = false;
        if (coordPath.Count - 1 < i)
            i = coordPath.Count-1;
        newPos = vectorPath[i];
        bool conflictSolved = false;
        bool pathStopped = false;
        if (cellPath.Count - 1 > i && cellPath.Count > 2 ) {
            gameManager.continueAction = true;
            continueAction = true;
        }
        //do checks for single value in path
            bool emptySpace = true;
        if (!pathStopped && conflictingTroop) {
            //Debug.Log(cellPath.Count);
            //if (conflictingCell == null && i < cellPath.Count-1 && i > 0 && cellPath[i] != GetComponentInParent<Cell>()) {
            //    finalCellPath.Add(cellPath[i]);
            //}
            if (cellPath[i] == conflictingCell || cellPath[i] ==  cellPath[cellPath.Count-1]) {
                emptySpace = false;
                if (conflictingCell == GetComponentInParent<Cell>()) { // Troop is attacked by enemy and is forced to retreat
                    pathAltered = true;
                    if (conflictingTroop.actionPower > actionPower && conflictingTroop.color != color) {
                        conflictSolved = true;
                        if (GetComponent<HQ>()) { //Handle End Game Condition
                            transform.position = hexControls.graveyard.transform.position;
                            if (gameManager.gameOver) {
                                gameManager.tieBreaker = true;
                            } else {
                                gameManager.gameOver = true;
                                gameManager.winner = conflictingTroop.color;
                            }
                        } else {
                            conflictSolved = true;
                            Cell retreatCell = hexControls.GetRetreatPath(this, conflictingCell);
                            if (retreatCell == conflictingCell) { // Troop could not find a cell to retreat to
                                transform.position = hexControls.graveyard.transform.position;
                                finalCellPath.Add(GetComponentInParent<Cell>());
                                finalCellPath.Add(hexControls.graveyard);
                            } else {
                                finalCellPath.Add(GetComponentInParent<Cell>());
                                finalCellPath.Add(retreatCell);
                            }
                        }
                    }
                } else if (conflictingTroop.actionPower < actionPower && conflictingTroop.color != color) { //  Troop attacks enemy troop and moves into the space
                    if (!finalCellPath.Contains(conflictingCell)) { //This is just a temporary hack, look into problem more later (when target is destroyed it loops all the way to the end)
                        finalCellPath.Add(conflictingCell);
                        conflictSolved = true;
                        pathStopped = true;
                    }
                } else if (conflictingTroop.actionPower >= actionPower) { // Troop is unable to move
                    //if (i == 1) { ResolveConflicts(i - 1); }
                    newPos = vectorPath[i-1];
                    pathAltered = true;
                    pathStopped = true;
                    gameManager.continueAction = false;
                    continueAction = false;
                } else if (conflictingTroop.actionPower < actionPower && conflictingTroop.color == color) {
                    //if (i == 1) { ResolveConflicts(i - 1); }
                    pathAltered = true;
                    newPos = vectorPath[i - 1];
                    pathStopped = true;
                    gameManager.continueAction = false;
                    continueAction = false;
                }
            }


            if (emptySpace && cellPath.Count > 1 &&  cellPath[i] != GetComponentInParent<Cell>()) {
                conflictSolved = true;
                finalCellPath.Add(cellPath[i]);
            }

        }


       
        return conflictSolved;

    }
    bool pathAltered = false;
    public void HandleAction() {

        if (pathAltered) {
            hexControls.SetPath(this, finalCellPath);
        }

        firstPass = true;
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


