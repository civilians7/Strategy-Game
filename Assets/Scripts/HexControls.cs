using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HexMapTools;

//priority is dealing with the hexes and movement of troops

namespace HexMapTerrain
{


    [RequireComponent(typeof(HexGrid))]
    public class HexControls : MonoBehaviour {

        public Animator cameraAnimator;
        public bool planningMode;

        private HexCalculator hexCalculator;
        private PathFinder pathFinder;
        private HexContainer<Cell> cells;


        private HexCoordinates selectedCoords;
        private List<HexCoordinates> possibleMoves;

        private TroopColor player;
        private GameManager gameManager;
        public Troop selectedTroop;
        public Cell graveyard;


        private void Start() {
            HexGrid hexGrid = GetComponent<HexGrid>();

            hexCalculator = hexGrid.HexCalculator;
            pathFinder = FindObjectOfType<PathFinder>();
            gameManager = FindObjectOfType<GameManager>();
            possibleMoves = new List<HexCoordinates>();

            cells = new HexContainer<Cell>(hexGrid);
            cells.FillWithChildren();
            // SetUpMap();
        }


        private void Update() {



            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                HexCoordinates mouseCoords = hexCalculator.HexFromPosition(mouse);

                if (player == TroopColor.White)
                    return;

                //Move or select cell
                if (possibleMoves.Contains(mouseCoords) && selectedTroop && mouseCoords != hexCalculator.HexFromPosition(selectedTroop.transform.position)) {
                    Move(mouseCoords);

                }


            }
        }

        public void SelectTroop(Troop troop) {
            Vector3 cellPos = troop.GetComponentInParent<Cell>().transform.position;
            HexCoordinates cellCoords = hexCalculator.HexFromPosition(cellPos);
            if (player == TroopColor.White)
                return;
            if (gameManager.playBackMode) {
                //Add Review Mode Functionality later
            } else { //Move or select cell
                if (!selectedTroop) {
                    SelectCell(cellCoords);
                }
            }

        }

        private void Move(HexCoordinates coords) {
            Troop thisTroop = cells[selectedCoords].GetComponentInChildren<Troop>();
            foreach (Troop troop in gameManager.troopArray) {

                if (cells[coords].transform.position == troop.transform.position && troop.color == thisTroop.color) {
                    troop.Support(thisTroop);
                    DeselectCell();
                    return;
                } 
            }

            thisTroop.PlanningMove();
            thisTroop.transform.position = cells[coords].transform.position;

            DeselectCell();

        }

        public void HandleWin(TroopColor winner) //Call from GameManager when game is over
        {
            player = TroopColor.White;
            cameraAnimator.SetInteger("Player", (int)winner);

        }


        //Change the player and the background
        public void ChangePlayer(TroopColor color) {

            player = color;

            cameraAnimator.SetInteger("Player", (int)player);

        }


        List<HexCoordinates> GetPossibleMoves(HexCoordinates coords, int movement) {
            HexPathFinder pathing = new HexPathFinder(HexCost);
            List<HexCoordinates> path;
            List<HexCoordinates> moves = new List<HexCoordinates>();

            var newCoords = HexUtility.GetInRange(coords, movement);

            foreach (var c in newCoords) {
                Cell cell = cells[c];

                if (cell != null) {
                    pathing.FindPath(coords, c, out path);
                    if (!(pathFinder.CalculatePathCost(path) > cells[coords].GetComponentInChildren<Troop>().movement - 1)) {
                        moves.Add(c);
                    }
                }
            }


            return moves;
        }

        //Turn off all highlighted cells
        public void DeselectCell() {
            foreach (var move in possibleMoves) {
                cells[move].IsHighlighted = false;
            }
            possibleMoves.Clear();
            selectedTroop = null;
        }

        //Select cell and highlight possible moves
        private void SelectCell(HexCoordinates coords) {
            DeselectCell();

            Cell cell = cells[coords];
            //if active player isn't owner, return
            if (cell == null || !(cell.GetComponentInChildren<Troop>()) || cell.GetComponentInChildren<Troop>().color != player)
                return;

            selectedCoords = coords;
            selectedTroop = cells[selectedCoords].GetComponentInChildren<Troop>();
            possibleMoves = GetPossibleMoves(coords, cells[selectedCoords].GetComponentInChildren<Troop>().movement);
            if (planningMode == true) {

                Troop troop = cell.GetComponentInChildren<Troop>();
                troop.GetComponent<LineRenderer>().SetPosition(0, new Vector3(0, 0, 0));
                troop.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, 0, 0));

                troop.CutSupport();

            }
            //Highlight all possible moves
            foreach (HexCoordinates move in possibleMoves) {
                cells[move].IsHighlighted = true;
            }

        }

        public void SetUpMap() { //add snap functionality to troops
            foreach (Troop troop in gameManager.troopArray) {
                troop.transform.SetParent(cells[troop.coords].transform);
            }
        }

        public float HexCost(HexCoordinates a, HexCoordinates b) {
            Cell cell = cells[b];

            if (cell == null)
                return float.PositiveInfinity;
            return 1;
        }

        public void FindPath(Troop troop) { // Keep here because it deals with Cell Pathfinding
            troop.coordPath.Clear();
            troop.coordPath = pathFinder.FindPath(troop.currentPos, troop.newPos);
            if (cells[hexCalculator.HexFromPosition(troop.newPos)].GetComponentInChildren<Troop>() && troop != cells[hexCalculator.HexFromPosition(troop.newPos)].GetComponentInChildren<Troop>()) {
                troop.coordPath.Add(hexCalculator.HexFromPosition(troop.newPos));
            }
            troop.coordPath.Insert(0, hexCalculator.HexFromPosition(troop.currentPos));

            foreach (HexCoordinates hexCoord in troop.coordPath) {
                troop.cellPath.Add(cells[hexCoord]);
                troop.vectorPath.Add(hexCalculator.HexToPosition(hexCoord));
            }

        }

        public void FindConflicts(Troop thisTroop) {// pass in cell list instead?
            if (thisTroop.GetComponentInParent<Cell>() == graveyard) { return; }
            foreach (Troop thatTroop in gameManager.troopArray) {
                if (thisTroop != thatTroop) {
                    foreach (HexCoordinates thisPath in thisTroop.coordPath) {
                        foreach (HexCoordinates thatPath in thatTroop.coordPath) {
                            if ((thisTroop.transform.position == thatTroop.transform.position || thisPath == thatPath)) {
                                    thisTroop.conflictingCells.Add(cells[thisPath]);
                                    thisTroop.conflictingTroops.Add(thatTroop);
                            }
                        }
                    }
                }
            }
        }

        public bool CheckCellConflicts(Cell cell) {
            bool cellConflict = false;
            foreach (Troop troop in gameManager.troopArray) {
                foreach(Cell otherCell in troop.cellPath) {
                    if (cell == otherCell)
                        cellConflict = true;
                }
            }
            return cellConflict;
        }

        public Cell GetRetreatPath(Troop troop, Cell cell) { 
            HexCoordinates coords = hexCalculator.HexFromPosition(cell.transform.position);
            var newCoords = HexUtility.GetInRange(coords, 1);
            Dictionary<int, Vector3> retreatPos = new Dictionary<int, Vector3>();
            foreach (var c in newCoords) {
                if (!CheckCellConflicts(cells[c])) {
                    if (!cells[c].GetComponentInChildren<Troop>() && troop.color == gameManager.playerOneColor && c.Y == coords.Y && c.X < coords.X) {
                        retreatPos.Add(1, hexCalculator.HexToPosition(c));
                    } else if (!cells[c].GetComponentInChildren<Troop>() && troop.color == gameManager.playerTwoColor && c.Y == coords.Y && c.X > coords.X) {
                        retreatPos.Add(2, hexCalculator.HexToPosition(c));
                    } else if (!cells[c].GetComponentInChildren<Troop>() && troop.color == gameManager.playerOneColor && c.X < coords.X) {
                        retreatPos.Add(3, hexCalculator.HexToPosition(c));
                    } else if (!cells[c].GetComponentInChildren<Troop>() && troop.color == gameManager.playerTwoColor && c.X > coords.X) {
                        retreatPos.Add(4, hexCalculator.HexToPosition(c));
                    }
                }
            }
            if (retreatPos.ContainsKey(1)) {
                troop.newPos = retreatPos[1];
                return(cells[hexCalculator.HexFromPosition(retreatPos[1])]);
            } else if (retreatPos.ContainsKey(2)) {
                troop.newPos = retreatPos[2];
                return(cells[hexCalculator.HexFromPosition(retreatPos[2])]);;
            } else if (retreatPos.ContainsKey(3)) {
                troop.newPos = retreatPos[3];
                return(cells[hexCalculator.HexFromPosition(retreatPos[3])]);
            } else if (retreatPos.ContainsKey(4)) {
                troop.newPos = retreatPos[4];
                return(cells[hexCalculator.HexFromPosition(retreatPos[4])]);
            } else {
                return (cell);
            }
        }

        public void SetPath(Troop troop, List<Cell> path) {
            troop.coordPath.Clear();
            troop.cellPath.Clear();
            troop.vectorPath.Clear();
            foreach (Cell cell in path) {
                HexCoordinates hexCoord = hexCalculator.HexFromPosition(cell.transform.position);
                troop.coordPath.Add(hexCoord);
                troop.cellPath.Add(cell);
                troop.vectorPath.Add(cell.transform.position);
            }
        }

        public Cell GetCell(HexCoordinates cellCoords) {
            return cells[cellCoords];
        }
    }
}
