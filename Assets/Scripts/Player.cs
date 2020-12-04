using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public List<string> Hand = new List<string>();
    public bool IsDealer = false;
    public int Id;
    public int? BidAmount;
    public bool BidComplete { get { return BidAmount != null; } }
    public Trump? Trump;
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
