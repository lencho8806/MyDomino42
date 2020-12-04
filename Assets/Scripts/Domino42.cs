using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum Trump
{
    FOLLOW_ME = -1,
    ZERO,
    ONE,
    TWO,
    THREE,
    FOUR,
    FIVE,
    SIX,
    DOUBLES
}

public class Domino42 : MonoBehaviour
{
    public Sprite[] dominoFaces;
    public GameObject dominoPrefab;
    public List<Player> players = new List<Player>(4);

    public static string[] dominoes = new string[] { "0_0" ,"1_0" ,"1_1" ,"2_0" ,"2_1" ,"2_2" ,"3_0" ,"3_1" ,"3_2" ,"3_3" ,"4_0" ,"4_1" ,"4_2" ,"4_3" ,"4_4" ,"5_0" ,"5_1" ,"5_2" ,"5_3" ,"5_4" ,"5_5" ,"6_0" ,"6_1" ,"6_2" ,"6_3" ,"6_4" ,"6_5" ,"6_6" };

    public List<string> deck;
    public List<string> discardPile = new List<string>();
    public bool? IsDealing { get; private set; } = null;
    public int? CurrentBidAmount;
    public int WhoBid = -1;

    public BidMenu bidMenu;
    public List<Text> playerBidTexts = new List<Text>(4);

    public TrumpMenu trumpMenu;
    public Text trumpText;

    public Trump Trump;
    public int InitialPlayerTurn = -1;
    public int CurrentPlayerTurn = -1;
    public bool RoundComplete = false;
    public bool? SetComplete = null;

    public List<GameObject> playerSpots = new List<GameObject>();

    public int RoundScoreUs { get; private set; } = 0;
    public Text RoundUsText;
    public int RoundScoreThem { get; private set; } = 0;
    public Text RoundThemText;
    public int SetScoreUs { get; private set; } = 0;
    public Text SetUsText;
    public int SetScoreThem { get; private set; } = 0;
    public Text SetThemText;

    public WinLoseMenu winLoseMenu;

    // Start is called before the first frame update
    void Start()
    {
        // Make sure user input script for game is disabled
        //GetComponent<UserInput>().enabled = false;

        // Generate deck; shuffle; deal
        //StartCoroutine(PlayDominoes());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Shuffle
    public void Shuffle()
    {
        deck = GenerateDeck();
        Shuffle(deck);
    }
    
    public static List<string> GenerateDeck()
    {
        List<string> newDeck = new List<string>();

        newDeck.AddRange(dominoes);

        return newDeck;
    }

    void Shuffle<T>(List<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;

        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }

    // Deal
    public void Deal()
    {
        StartCoroutine(DominoDeal());
    }

    IEnumerator DealEnumerator()
    {
        IsDealing = true;

        yield return StartCoroutine(DominoDeal());

        IsDealing = false;
    }

    IEnumerator DominoDeal()
    {
        IsDealing = true;

        float yOffset = 1.0f;
        float xOffset = 1.0f;
        int i = 0;

        foreach(string domino in deck)
        {
            yield return new WaitForSeconds(0.03f);

            int index = i % 4;
            if (players[index].IsDealer)
            {
                i++;
                index = i % 4;
            }

            float xOffsetCalc = players[index].transform.childCount * xOffset;
            float yOffsetCalc = 0f;
            int direction = 1;

            switch (index)
            {
                case 0:
                    //initialized values
                    break;
                case 1:
                    xOffsetCalc = 0;
                    yOffsetCalc = players[index].transform.childCount * yOffset;
                    direction = -1;
                    break;
                case 2:
                    xOffsetCalc = players[index].transform.childCount * xOffset;
                    yOffsetCalc = 0;
                    direction = -1;
                    break;
                case 3:
                    xOffsetCalc = 0;
                    yOffsetCalc = players[index].transform.childCount * yOffset;
                    direction = 1;
                    break;
            }
            
            GameObject newDomino = Instantiate(
                dominoPrefab, 
                new Vector3(
                    players[index].transform.position.x + (xOffsetCalc * direction), 
                    players[index].transform.position.y + (yOffsetCalc * direction), 
                    players[index].transform.position.z), 
                Quaternion.Euler(0, 0, 90 * index), 
                players[index].transform
            );
            newDomino.name = domino;
            newDomino.GetComponent<Selectable>().faceUp = true;
            
            i++;

            discardPile.Add(domino);
            players[index].Hand.Add(domino);

            if (discardPile.Count >= 21) break;
        }

        foreach (string domino in discardPile)
        {
            if (deck.Contains(domino))
            {
                deck.Remove(domino);
            }
        }
        discardPile.Clear();

        StartCoroutine(DominoDealDealer());
    }

    IEnumerator DominoDealDealer()
    {
        float yOffset = 1.0f;
        float xOffset = 1.0f;

        int index = players.FindIndex(player => player.IsDealer);

        float xOffsetCalc = players[index].transform.childCount * xOffset;
        float yOffsetCalc = 0f;
        int direction = 1;
        
        foreach (string domino in deck)
        {
            yield return new WaitForSeconds(0.03f);

            switch (index)
            {
                case 0:
                    xOffsetCalc = players[index].transform.childCount * xOffset;
                    yOffsetCalc = 0f;
                    direction = 1;
                    break;
                case 1:
                    xOffsetCalc = 0;
                    yOffsetCalc = players[index].transform.childCount * yOffset;
                    direction = -1;
                    break;
                case 2:
                    xOffsetCalc = players[index].transform.childCount * xOffset;
                    yOffsetCalc = 0;
                    direction = -1;
                    break;
                case 3:
                    xOffsetCalc = 0;
                    yOffsetCalc = players[index].transform.childCount * yOffset;
                    direction = 1;
                    break;
            }

            GameObject newDomino = Instantiate(
                dominoPrefab,
                new Vector3(
                    players[index].transform.position.x + (xOffsetCalc * direction),
                    players[index].transform.position.y + (yOffsetCalc * direction),
                    players[index].transform.position.z),
                Quaternion.Euler(0, 0, 90 * index),
                players[index].transform
            );
            newDomino.name = domino;
            newDomino.GetComponent<Selectable>().faceUp = true;
            
            discardPile.Add(domino);
            players[index].Hand.Add(domino);
        }

        foreach (string domino in discardPile)
        {
            if (deck.Contains(domino))
            {
                deck.Remove(domino);
            }
        }
        discardPile.Clear();

        IsDealing = false;
    }

    // Bid - AI
    public void Bid(int playerIndex)
    {
        BidEnumerator(playerIndex);
    }

    void BidEnumerator(int playerIndex)
    {
        StartCoroutine(Bid_AI(playerIndex));
    }

    IEnumerator Bid_AI(int playerIndex)
    {
        yield return new WaitForSeconds(2f);
        
        System.Random bidRandom = new System.Random();

        int bidAmmount = -1;

        if (bidRandom.Next(0, 10) < 4)
        {
            int? maxBid = players.Max(player => player.BidAmount);

            if (maxBid.HasValue)
            {
                if (maxBid == -1)
                {
                    bidAmmount = 30;
                }
                else
                {
                    bidAmmount = maxBid.Value + 1;
                }
            }
            else
            {
                bidAmmount = 30;
            }
        }

        players[playerIndex].BidAmount = bidAmmount;
        playerBidTexts[playerIndex].text = players[playerIndex].BidAmount.ToString();

        yield return new WaitForSeconds(2f);
    }

    // Trump - AI
    public void SetTrump(int playerIndex)
    {
        TrumpEnumerator(playerIndex);
    }

    void TrumpEnumerator(int playerIndex)
    {
        StartCoroutine(Trump_AI(playerIndex));
    }

    IEnumerator Trump_AI(int playerIndex)
    {
        yield return new WaitForSeconds(2f);

        List<string> dominoNums = new List<string>();
        players[playerIndex].Hand.ForEach(domino =>
        {
            var dominoSplit = domino.Split('_');
            dominoNums.Add(dominoSplit[0]);
            dominoNums.Add(dominoSplit[1]);
        });

        var dominoGroups = dominoNums.GroupBy(num => num)
            .Select(x => new
            {
                Value = x.Key,
                Count = x.Count()
            })
            .OrderByDescending(x => x.Count).ToList();

        int defaultTrump = -1;
        int.TryParse(dominoGroups[0].Value, out defaultTrump);
        
        players[playerIndex].Trump = (Trump)defaultTrump;

        Trump = (Trump)defaultTrump;

        trumpText.text = Trump.ToString();

        yield return new WaitForSeconds(2f);
    }

    // Play - AI
    public void SetPlay(int playerIndex)
    {
        PlayEnumerator(playerIndex);
    }

    void PlayEnumerator(int playerIndex)
    {
        StartCoroutine(Play_AI(playerIndex));
    }

    IEnumerator Play_AI(int playerIndex)
    {
        yield return new WaitForSeconds(2f);

        int trumpInt = (int)Trump;
        string trumpStr = trumpInt.ToString();
        string selectedDomino = null;

        IEnumerable<string> trumpAvailable = players[playerIndex].Hand.Where(domino => domino.Contains(trumpStr));
        if (trumpAvailable.Count() <= 0)
        {
            Selectable initSelectedDomino = players[InitialPlayerTurn].SelectedDomino;
            if (initSelectedDomino.name != null)
            {
                IEnumerable<string> tempTrumpAvailable = players[playerIndex].Hand.Where(domino => domino.Contains(initSelectedDomino.high.ToString()));

                trumpAvailable = tempTrumpAvailable;
            }
        }
        
        if (trumpAvailable.Count() > 0)
        {
            selectedDomino = trumpAvailable.Aggregate((i1, i2) =>
            {
                int left1 = 0;
                int right1 = 0;
                var dominoSplit1 = i1.Split('_');
                int.TryParse(dominoSplit1[0], out left1);
                int.TryParse(dominoSplit1[1], out right1);
                int sum1 = left1 + right1;

                int left2 = 0;
                int right2 = 0;
                var dominoSplit2 = i2.Split('_');
                int.TryParse(dominoSplit2[0], out left2);
                int.TryParse(dominoSplit2[1], out right2);
                int sum2 = left2 + right2;

                if (left1 == right1)
                    return i1;
                else if (left2 == right2)
                    return i2;
                else
                    return sum1 > sum2 ? i1 : i2;
            });
        }

        if (selectedDomino == null)
        {
            // ELSE select hightest domino if no trump available
            selectedDomino = players[playerIndex].Hand.Aggregate((i1, i2) =>
            {
                int left1 = 0;
                int right1 = 0;
                var dominoSplit1 = i1.Split('_');
                int.TryParse(dominoSplit1[0], out left1);
                int.TryParse(dominoSplit1[1], out right1);
                int sum1 = left1 + right1;

                int left2 = 0;
                int right2 = 0;
                var dominoSplit2 = i2.Split('_');
                int.TryParse(dominoSplit2[0], out left2);
                int.TryParse(dominoSplit2[1], out right2);
                int sum2 = left2 + right2;

                return sum1 > sum2 ? i1 : i2;
            });
        }

        SelectDominoFromHand(playerIndex, selectedDomino);

        yield return new WaitForSeconds(2f);
    }

    public void SelectDominoFromHand(int playerIndex, string selectedDomino)
    {
        // Verify selected domino is in hand...?

        var selectedDominoGameObject = players[playerIndex].transform.Find(selectedDomino).gameObject;
        var selectableDomino = selectedDominoGameObject.GetComponent<Selectable>();

        // Remove domino from hand
        Destroy(selectedDominoGameObject);
        players[playerIndex].Hand.Remove(selectedDomino);

        // Move domino to play area
        GameObject newDomino = Instantiate(
            dominoPrefab,
            playerSpots[playerIndex].transform.position,
            Quaternion.Euler(0, 0, 90),
            playerSpots[playerIndex].transform
        );
        newDomino.name = selectedDomino;
        newDomino.GetComponent<Selectable>().faceUp = true;

        players[playerIndex].TurnComplete = true;
        players[playerIndex].SelectedDomino = selectableDomino;
    }

    // Round Winner
    public void RoundWinner()
    {
        RoundWinnerEnumerator();
    }

    void RoundWinnerEnumerator()
    {
        StartCoroutine(DetermineRoundWinner());
    }

    IEnumerator DetermineRoundWinner()
    {
        RoundComplete = false;

        yield return new WaitForSeconds(2f);

        List<int> playersWithTrump = new List<int>();
        int playerWinnerIndex = -1;

        if (players.Exists(p => p.SelectedDomino.name.Contains(((int)Trump).ToString())))
        {
            playerWinnerIndex = GetPlayerHandWinner((int)Trump);
        }
        else
        {
            int tempTrump = players[InitialPlayerTurn].SelectedDomino.high;

            playerWinnerIndex = GetPlayerHandWinner(tempTrump);
        }

        if (playerWinnerIndex % 2 == 0)
        {
            // Us
            RoundScoreUs += GetGameScore() + 1;
            RoundUsText.text = $"Us: {RoundScoreUs}";
        }
        else
        {
            // Them
            RoundScoreThem += GetGameScore() + 1;
            RoundThemText.text = $"Them: {RoundScoreThem}";
        }

        InitialPlayerTurn = playerWinnerIndex;

        RoundComplete = true;
    }

    int GetPlayerHandWinner(int trump)
    {
        int playerIndex = -1;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].SelectedDomino.name.Contains((trump).ToString()))
            {
                if (playerIndex == -1)
                {
                    playerIndex = i;

                    if (players[i].SelectedDomino.isDoubles) break;
                }
                else
                {
                    if (players[i].SelectedDomino.isDoubles)
                    {
                        playerIndex = i;
                        break;
                    }
                    else if (players[i].SelectedDomino.sum > players[playerIndex].SelectedDomino.sum)
                    {
                        playerIndex = i;
                    }
                }
            }
        }

        return playerIndex;
    }

    int GetGameScore()
    {
        int gameScore = 0;

        players.ForEach(player =>
        {
            if (player.SelectedDomino.sum % 5 == 0)
            {
                gameScore += player.SelectedDomino.sum;
            }
        });

        return gameScore;
    }

    public void ResetRound()
    {
        players.ForEach(player =>
        {
            player.TurnComplete = false;
        });

        RoundComplete = false;
        SetComplete = null;

        for (int i = 0; i < playerSpots.Count; i++)
        {
            var childGameObj = playerSpots[i].gameObject.transform.Find(players[i].SelectedDomino.name).gameObject;
            Destroy(childGameObj);

            discardPile.Add(childGameObj.name);
        }
    }

    public void ResetSet()
    {
        ResetRound();

        int dealerIndex = 0;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].Hand.Clear();
            foreach (Transform child in players[i].transform)
            {
                Destroy(child.gameObject);
            }
            players[i].transform.DetachChildren();

            if (players[i].IsDealer)
            {
                dealerIndex = i;
                players[i].IsDealer = false;
            }

            players[i].BidAmount = null;

            players[i].Trump = null;
        }

        discardPile.Clear();

        players[(dealerIndex + 1) % 4].IsDealer = true;

        RoundScoreUs = 0;
        RoundUsText.text = $"Us: {RoundScoreUs}";
        RoundScoreThem = 0;
        RoundThemText.text = $"Them: {RoundScoreThem}";

        playerBidTexts.ForEach(bidText =>
        {
            bidText.text = "0";
        });

        trumpText.text = "Trump";
    }

    public void ResetMatch()
    {
        ResetRound();

        ResetSet();

        SetScoreUs = 0;
        SetUsText.text = $"Us: {SetScoreUs}";
        SetScoreThem = 0;
        SetThemText.text = $"Them: {SetScoreThem}";
    }

    // Set Winner
    public void SetWinner()
    {
        SetWinnerEnumerator();
    }

    void SetWinnerEnumerator()
    {
        StartCoroutine(DetermineSetWinner());
    }

    IEnumerator DetermineSetWinner()
    {
        SetComplete = null;

        yield return new WaitForSeconds(2f);
        
        if (WhoBid % 2 == 0)
        {
            if (RoundScoreUs >= CurrentBidAmount)
            {
                SetScoreUs += 1;
                SetUsText.text = $"Us: {SetScoreUs}";
                SetComplete = true;
            }
            else if (RoundScoreThem > (42 - CurrentBidAmount))
            {
                SetScoreThem += 1;
                SetThemText.text = $"Them: {SetScoreThem}";
                SetComplete = true;
            }
        }
        else
        {
            if (RoundScoreThem >= CurrentBidAmount)
            {
                SetScoreThem += 1;
                SetThemText.text = $"Them: {SetScoreThem}";
                SetComplete = true;
            }
            else if (RoundScoreUs > (42 - CurrentBidAmount))
            {
                SetScoreUs += 1;
                SetUsText.text = $"Us: {SetScoreUs}";
                SetComplete = true;
            }
        }

        SetComplete = SetComplete == null ? false : SetComplete;
    }
}
