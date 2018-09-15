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

        private void Start()
        {
            animator = GetComponent<Animator>();

        }

        public void Init(HexCoordinates coords)
        {
            Coords = coords;
        }
    }



}
