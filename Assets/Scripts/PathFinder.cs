using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMapTools;

namespace HexMapTerrain {


    [RequireComponent(typeof(HexGrid))]
    public class PathFinder : MonoBehaviour {


        private HexCalculator hexCalculator;
        private HexContainer<Cell> cells;
        private GameManager gameManager;

        private void Start() {
            HexGrid hexGrid = GetComponent<HexGrid>();
            hexCalculator = hexGrid.HexCalculator;
            gameManager = FindObjectOfType<GameManager>();

            cells = new HexContainer<Cell>(hexGrid);
            cells.FillWithChildren();
        }

        public float HexCost(HexCoordinates a, HexCoordinates b) { 

            Cell cell = cells[b];

            foreach (Troop troop in gameManager.troopArray)
                if ((cell == null || cell.transform.position == troop.transform.position))
                    return float.PositiveInfinity;

            return 1;
        }

        public float CalculatePathCost(List<HexCoordinates> path) { 
            float cost = 0;

            for (int i = 1; i < path.Count; ++i) {
                cost += HexCost(path[i - 1], path[i]);
            }

            return cost;
        }

        public List<HexCoordinates> FindPath(Vector3 pos1, Vector3 pos2) { //refactor to pathfinder
            HexPathFinder pathFinder = new HexPathFinder(HexCost);
            List<HexCoordinates> path;

            HexCoordinates startPos = hexCalculator.HexFromPosition(pos1);
            HexCoordinates endPos = hexCalculator.HexFromPosition(pos2);
            pathFinder.FindPath(startPos, endPos, out path);
            if (gameManager.turnNum == 0) {
                foreach (HexCoordinates pathpoint in path) {
                }
            }
            return path;
        }
    }
}
