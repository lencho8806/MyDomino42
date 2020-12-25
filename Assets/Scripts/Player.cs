using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Domino42
{
    public class Player : MonoBehaviour
    {
        public List<byte> Hand = new List<byte>();
        public bool IsDealer = false;
        public bool IsAI = true;
        public string Id;
        public int? BidAmount;
        public bool BidComplete { get { return BidAmount != null; } }
        public Domino42.Trump? Trump;
        public bool TurnComplete = false;
        public Selectable SelectedDomino;
        public bool IsActive = false;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}