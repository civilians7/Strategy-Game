using UnityEngine;
using HexMapTools;
using System.Collections.Generic;

namespace HexMapTerrain
{

    //public enum CellColor { White = 0, Blue, Red, Purple, Orange, Yellow, Brown, Green }

    [RequireComponent(typeof(Animator))]
    public class Cell : MonoBehaviour
    {
        [SerializeField]
        //private CellColor color = CellColor.White;
        private bool isHighlighted = false;
        private bool isSelected = false;
        private Animator animator;
        private HexContainer<Cell> cells;

        private HexCoordinates[] neighborCoords;
        private List<Cell> neighborCells = new List<Cell>();
        public Color color;
        public float alpha = .5f;
        
        private void Start() {
            cells = new HexContainer<Cell>(GetComponentInParent<HexGrid>());
            cells.FillWithChildren();
            //GetComponent<SpriteRenderer>().color = color;
            animator = GetComponent<Animator>();
            neighborCoords = HexUtility.GetNeighbours(GetComponentInParent<HexGrid>().HexCalculator.HexFromPosition(transform.position));
            if (neighborCoords.Length > 0) {
                foreach (HexCoordinates coord in neighborCoords) {
                    neighborCells.Add(cells[coord]);
                }
            }
        }

        private void Update() {
            if (neighborCoords.Length > 0) {
                SetColor();
            }
        }

        private void SetColor() {
            Dictionary<Color, int> cellColors = new Dictionary<Color, int>();
            Color newColor = new Color(1,1,1,alpha);
            int largestAmount = 0;
            if (GetComponentInChildren<Troop>()) {
                color = GetComponentInChildren<Troop>().GetComponent<SpriteRenderer>().color;
                color.a = alpha;
            } else {
                foreach (Cell cell in neighborCells) {
                    if (cell) {
                        if (!cellColors.ContainsKey(cell.color)) {
                            cellColors.Add(cell.color, 1);
                        } else {
                            cellColors[cell.color]++;
                        }
                    }
                }
                foreach (KeyValuePair<Color, int> entry in cellColors) {
                    if (largestAmount < entry.Value && entry.Key != new Color(1,1,1,alpha)) {
                        largestAmount = entry.Value;
                        newColor = entry.Key;
                    }
                }
                foreach (KeyValuePair<Color, int> entry in cellColors) {
                    if (largestAmount == entry.Value && newColor != entry.Key) {
                        newColor = Color.white;
                    }
                }
                color = newColor;
            }
            color.a = alpha;

            GetComponent<SpriteRenderer>().color = color;
        }

        public bool IsHighlighted
        {
            get { return isHighlighted; }
            set
            {
                if (isHighlighted == value)
                    return;

                isHighlighted = value;
                animator.SetBool("IsHighlighted", isHighlighted);
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (isSelected == value)
                    return;

                isSelected = value;
                animator.SetBool("IsSelected", isSelected);
            }
        }

        public HexCoordinates Coords
        {
            get;
            private set;
        }

        public void Init(HexCoordinates coords)
        {
            Coords = coords;
        }
    }



}
