using SWNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Text;

namespace Domino42
{
    public enum Trump
    {
        NELO = -2,
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

    public class Game : MonoBehaviour
    {
        //Rules
        protected static int marksToWin = 3;
        public int MarksToWin { get { return marksToWin; } }
        protected static bool isNelO = true;
        public bool IsNelO { get { return isNelO; } }
        protected static bool isForceBid = true;
        public bool IsForceBid { get { return isForceBid; } }

        public Sprite[] dominoFaces;
        public GameObject dominoPrefab;
        [SerializeField]
        protected DominoAnimator dominoAnimator;
        public List<Player> players = new List<Player>(4);

        public readonly List<string> dominoes = new List<string> { "0_0", "1_0", "1_1", "2_0", "2_1", "2_2", "3_0", "3_1", "3_2", "3_3", "4_0", "4_1", "4_2", "4_3", "4_4", "5_0", "5_1", "5_2", "5_3", "5_4", "5_5", "6_0", "6_1", "6_2", "6_3", "6_4", "6_5", "6_6" };
        
        public List<byte> deck;
        public List<byte> discardPile = new List<byte>();
        public bool? IsDealing { get; protected set; } = null;
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
        public Text MarkText;

        public WinLoseMenu winLoseMenu;

        public List<Text> PlayerNames = new List<Text>();

        public Text MessageText;

        public enum GameState
        {
            Idle,
            Shuffle,
            Deal,
            Bid,
            Trump,
            Play,
            RoundWinner,
            SetWinner,
            Win,
            Lose
        };

        protected GameState gameState = GameState.Idle;
        public GameState CurrGameState { get { return gameState; } }

        protected byte[] encryptionKey;
        byte[] safeData;
        
        protected void Awake()
        {
            var gameOptions = FindObjectOfType<GameOptions>();

            if (gameOptions != null)
            {
                marksToWin = gameOptions.marks;
                isNelO = gameOptions.isNelO;
                isForceBid = gameOptions.isForceBid;

                Destroy(gameOptions);
            }

            for (int i = 0; i < 4; i++)
            {
                players[i].name = PlayerNames[i].text;
            }

            MessageText.text = string.Empty;

            //MarkText.text = $"{marksToWin}M";
        }

        // Start is called before the first frame update
        protected void Start()
        {
            Debug.Log($"isNelO : {(isNelO ? "true" : "false")}");

            MarkText.text = $"{marksToWin}M";

            gameState = GameState.Shuffle;
            
            GameFlow();
        }

        // Update is called once per frame
        protected void Update()
        {
            switch (gameState)
            {
                case GameState.Deal:
                    {
                        if (IsDealing == false)
                        {
                            gameState = GameState.Bid;

                            //GameFlow();
                        }
                        break;
                    }
                case GameState.Bid:
                    {
                        if (CurrentPlayerTurn >= 0 && players[CurrentPlayerTurn].BidAmount != null)
                        {
                            if (players.Exists(player => player.BidAmount == null))
                            {
                                GameFlow();
                            }
                            else
                            {
                                if (!isForceBid && players.All(p => p.BidAmount == -1))
                                {
                                    ResetSet();
                                    gameState = GameState.Shuffle;
                                    GameFlow();
                                }
                                else
                                {
                                    gameState = GameState.Trump;
                                    GameFlow();
                                }
                            }
                        }
                        break;
                    }
                case GameState.Trump:
                    {
                        if (players[CurrentPlayerTurn].Trump != null)
                        {
                            InitialPlayerTurn = CurrentPlayerTurn;

                            gameState = GameState.Play;
                            GameFlow();
                        }
                        break;
                    }
                case GameState.Play:
                    {
                        if (players[CurrentPlayerTurn].TurnComplete)
                        {
                            if (players.Exists(player => player.TurnComplete == false))
                            {
                                // Hand not finished
                                GameFlow();
                            }
                            else
                            {
                                gameState = GameState.RoundWinner;
                                GameFlow();
                            }
                        }
                        break;
                    }
                case GameState.RoundWinner:
                    {
                        if (RoundComplete)
                        {
                            gameState = GameState.SetWinner;
                            GameFlow();
                        }
                        break;
                    }
                case GameState.SetWinner:
                    {
                        if (SetComplete != null)
                        {
                            if (SetComplete == true)
                            {
                                if (SetScoreUs >= marksToWin)
                                {
                                    gameState = GameState.Win;
                                    GameFlow();
                                }
                                else if (SetScoreThem >= marksToWin)
                                {
                                    gameState = GameState.Lose;
                                    GameFlow();
                                }
                                else
                                {
                                    ResetSet();

                                    gameState = GameState.Shuffle;
                                    GameFlow();
                                }
                            }
                            else
                            {
                                ResetRound();

                                gameState = GameState.Play;
                                GameFlow();
                            }
                        }
                        break;
                    }
                case GameState.Win:
                    {
                        if (winLoseMenu.Next != null)
                        {
                            if (winLoseMenu.Next == "RESTART")
                            {
                                ResetMatch();

                                gameState = GameState.Shuffle;
                                GameFlow();
                            }
                        }
                        break;
                    }
                case GameState.Lose:
                    {
                        if (winLoseMenu.Next != null)
                        {
                            if (winLoseMenu.Next == "RESTART")
                            {
                                ResetMatch();

                                gameState = GameState.Shuffle;
                                GameFlow();
                            }
                        }
                        break;
                    }
            }
        }

        public virtual void GameFlow()
        {
            switch(gameState)
            {
                case GameState.Idle:
                    {
                        Debug.Log("IDLE");
                        break;
                    }
                case GameState.Shuffle:
                    {
                        Debug.Log("SHUFFLE");
                        Shuffle();
                        break;
                    }
                case GameState.Deal:
                    {
                        Debug.Log("DEAL");
                        //IsDealing = true;
                        Deal();
                        break;
                    }
                case GameState.Bid:
                    {
                        Debug.Log("BID");
                        Bid();
                        break;
                    }
                case GameState.Trump:
                    {
                        Debug.Log("TRUMP");
                        SetTrump();
                        break;
                    }
                case GameState.Play:
                    {
                        Debug.Log("PLAY");
                        SetPlay();
                        break;
                    }
                case GameState.RoundWinner:
                    {
                        Debug.Log("ROUND WINNER");
                        RoundWinner();
                        break;
                    }
                case GameState.SetWinner:
                    {
                        Debug.Log("SET WINNER");
                        SetWinner();
                        break;
                    }
                case GameState.Win:
                    {
                        Debug.Log("WIN");

                        winLoseMenu.WinLoseStart("You Won!!!");

                        break;
                    }
                case GameState.Lose:
                    {
                        Debug.Log("LOSE");

                        winLoseMenu.WinLoseStart("You Lost...");

                        break;
                    }
            }
        }

        // Shuffle
        protected virtual void Shuffle()
        {
            deck = GenerateDeck();
            Shuffle(deck);

            gameState = GameState.Deal;
            GameFlow();
        }

        public List<byte> GenerateDeck()
        {
            List<byte> newDeck = new List<byte>();

            for (int i = 0; i < dominoes.Count; i++)
            {
                newDeck.Add((byte)i);
            }
            
            return newDeck;
        }

        protected void Shuffle<T>(List<T> list)
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
        protected virtual void Deal()
        {
            IsDealing = true;

            //StartCoroutine(DominoDeal());
            DominoDeal();
            DominoDealDealer();

            IsDealing = false;
        }
        
        protected void DominoDeal()
        {
            IsDealing = true;

            float yOffset = 1.0f;
            float xOffset = 1.0f;
            int i = players.FindIndex(p => p.IsDealer);

            foreach (byte domino in deck)
            {
                //yield return new WaitForSeconds(0.03f);

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
                    new Vector3(0, 0, players[index].transform.position.z),
                    Quaternion.Euler(0, 0, 0),
                    players[index].transform
                );
                newDomino.name = dominoes[domino];
                newDomino.GetComponent<Selectable>().faceUp = true;

                dominoAnimator.AddDominoAnimation(
                    newDomino,
                    new Vector2(
                        players[index].transform.position.x + (xOffsetCalc * direction),
                        players[index].transform.position.y + (yOffsetCalc * direction)),
                    Quaternion.Euler(0, 0, 90 * index));

                i++;

                discardPile.Add(domino);
                players[index].Hand.Add(domino);

                if (discardPile.Count >= 21) break;
            }

            foreach (byte domino in discardPile)
            {
                if (deck.Contains(domino))
                {
                    deck.Remove(domino);
                }
            }
            discardPile.Clear();

            //StartCoroutine(DominoDealDealer());
        }
        
        protected void DominoDealDealer()
        {
            float yOffset = 1.0f;
            float xOffset = 1.0f;

            int index = players.FindIndex(player => player.IsDealer);

            float xOffsetCalc = players[index].transform.childCount * xOffset;
            float yOffsetCalc = 0f;
            int direction = 1;

            foreach (byte domino in deck)
            {
                //yield return new WaitForSeconds(0.03f);

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
                    new Vector3(0, 0, players[index].transform.position.z),
                    Quaternion.Euler(0, 0, 0),
                    players[index].transform
                );
                newDomino.name = dominoes[domino];
                newDomino.GetComponent<Selectable>().faceUp = true;

                dominoAnimator.AddDominoAnimation(
                    newDomino,
                    new Vector2(
                        players[index].transform.position.x + (xOffsetCalc * direction),
                        players[index].transform.position.y + (yOffsetCalc * direction)),
                    Quaternion.Euler(0, 0, 90 * index));

                discardPile.Add(domino);
                players[index].Hand.Add(domino);
            }

            foreach (byte domino in discardPile)
            {
                if (deck.Contains(domino))
                {
                    deck.Remove(domino);
                }
            }
            discardPile.Clear();

            IsDealing = false;
        }

        // Bid
        protected virtual void Bid()
        {
            Debug.Log("Base - Bid");

            int dealerIndex = players.FindIndex(player => player.IsDealer);
            CurrentPlayerTurn = -1;

            for (int i = (dealerIndex + 1); i < ((dealerIndex + 1) + 4); i++)
            {
                if (players[i % 4].BidComplete == false)
                {
                    CurrentPlayerTurn = i % 4;
                    break;
                }
            }

            if (CurrentPlayerTurn == -1)
            {
                // why? go to the next section
                gameState = GameState.Trump;
                GameFlow();
            }
            else if (!players[CurrentPlayerTurn].IsAI)
            {
                // Player bid
                MessageText.text = string.Empty;

                bidMenu.BidStart();
            }
            else
            {
                // AI bid
                MessageText.text = $"{players[CurrentPlayerTurn].name} is bidding...";

                BidEnumerator(CurrentPlayerTurn);
            }
        }

        protected void BidEnumerator(int playerIndex)
        {
            Debug.Log("Base - BidEnumerator");

            StartCoroutine(Bid_AI(playerIndex));
        }

        IEnumerator Bid_AI(int playerIndex)
        {
            Debug.Log("Base - Bid_AI");

            yield return new WaitForSeconds(2f);

            System.Random bidRandom = new System.Random();

            int bidAmmount = -1;

            // Force Bid option
            bool forceBid = false;
            if (players[CurrentPlayerTurn].IsDealer && IsForceBid)
            {
                if (players.Any(p => p.BidAmount != -1 && p.Id != players[CurrentPlayerTurn].Id))
                {
                    // continue...
                }
                else
                {
                    forceBid = true;
                }
            }

            if (bidRandom.Next(0, 10) < 4 || forceBid)
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
            players[playerIndex].BidComplete = true;
            playerBidTexts[playerIndex].text = players[playerIndex].BidAmount.ToString();

            yield return new WaitForSeconds(2f);
        }

        public virtual void BidEnd(int amount)
        {
            if (gameState == GameState.Bid && CurrentPlayerTurn == 0)
            {
                players[CurrentPlayerTurn].BidAmount = amount;
                players[CurrentPlayerTurn].BidComplete = true;
                playerBidTexts[CurrentPlayerTurn].text = amount.ToString();
            }
        }

        // Trump 
        protected virtual void SetTrump()
        {
            int maxBid = -100; // as to not interfere with pass (-1)
            CurrentPlayerTurn = -1;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].BidAmount > maxBid)
                {
                    maxBid = players[i].BidAmount.Value;
                    CurrentPlayerTurn = i;
                }
            }

            CurrentBidAmount = maxBid;
            WhoBid = CurrentPlayerTurn;

            if (CurrentPlayerTurn == -1)
            {
                // why? go to the next section
                gameState = GameState.Play;
                GameFlow();
            }
            else if (!players[CurrentPlayerTurn].IsAI)
            {
                // Player trump
                MessageText.text = string.Empty;

                trumpMenu.TrumpStart();
            }
            else
            {
                // AI trump
                MessageText.text = $"{players[CurrentPlayerTurn].name} is selecting trump...";

                TrumpEnumerator(CurrentPlayerTurn);
            }
        }

        protected void TrumpEnumerator(int playerIndex)
        {
            StartCoroutine(Trump_AI(playerIndex));
        }

        IEnumerator Trump_AI(int playerIndex)
        {
            yield return new WaitForSeconds(2f);

            List<string> dominoNums = new List<string>();
            players[playerIndex].Hand.ForEach(domino =>
            {
                var dominoSplit = dominoes[domino].Split('_');
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

        public virtual void TrumpEnd(int trump)
        {
            if (gameState == GameState.Trump && CurrentPlayerTurn == 0)
            {
                players[CurrentPlayerTurn].Trump = (Trump)trump;
                Trump = (Trump)trump;

                trumpText.text = Trump.ToString();
            }
        }

        // Play
        protected virtual void SetPlay()
        {
            CurrentPlayerTurn = -1;
            for (int i = InitialPlayerTurn; i < (InitialPlayerTurn + 4); i++)
            {
                if (players[i % 4].TurnComplete == false)
                {
                    CurrentPlayerTurn = i % 4;
                    break;
                }
            }

            int partnerIndex = (WhoBid + 2) % 4;

            if (CurrentPlayerTurn == -1)
            {
                //determine Game winner
                gameState = GameState.RoundWinner;
                GameFlow();
            }
            else if (isNelO && Trump == Trump.NELO && CurrentPlayerTurn == partnerIndex)
            {
                StartCoroutine(PlayNeloSkipTurn(CurrentPlayerTurn));
            }
            else if (!players[CurrentPlayerTurn].IsAI)
            {
                // Player bid
                MessageText.text = string.Empty;

                //_domino42.bidMenu.BidStart();
            }
            else
            {
                // AI bid
                MessageText.text = $"{players[CurrentPlayerTurn].name}'s turn...";

                PlayEnumerator(CurrentPlayerTurn);
            }
        }

        protected void PlayEnumerator(int playerIndex)
        {
            StartCoroutine(Play_AI(playerIndex));
        }

        IEnumerator Play_AI(int playerIndex)
        {
            yield return new WaitForSeconds(2f);

            int trumpInt = (int)Trump;
            string trumpStr = trumpInt.ToString();
            byte? selectedDomino = null;

            IEnumerable<byte> trumpAvailable = players[playerIndex].Hand.Where(domino => dominoes[domino].Contains(trumpStr));
            if (trumpAvailable.Count() <= 0)
            {
                Selectable initSelectedDomino = players[InitialPlayerTurn].SelectedDomino;
                if (initSelectedDomino.name != null)
                {
                    IEnumerable<byte> tempTrumpAvailable = players[playerIndex].Hand.Where(domino => dominoes[domino].Contains(initSelectedDomino.high.ToString()));

                    trumpAvailable = tempTrumpAvailable;
                }
            }

            if (trumpAvailable.Count() > 0)
            {
                selectedDomino = trumpAvailable.Aggregate((i1, i2) =>
                {
                    int left1 = 0;
                    int right1 = 0;
                    var dominoSplit1 = dominoes[i1].Split('_');
                    int.TryParse(dominoSplit1[0], out left1);
                    int.TryParse(dominoSplit1[1], out right1);
                    int sum1 = left1 + right1;

                    int left2 = 0;
                    int right2 = 0;
                    var dominoSplit2 = dominoes[i2].Split('_');
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
                    var dominoSplit1 = dominoes[i1].Split('_');
                    int.TryParse(dominoSplit1[0], out left1);
                    int.TryParse(dominoSplit1[1], out right1);
                    int sum1 = left1 + right1;

                    int left2 = 0;
                    int right2 = 0;
                    var dominoSplit2 = dominoes[i2].Split('_');
                    int.TryParse(dominoSplit2[0], out left2);
                    int.TryParse(dominoSplit2[1], out right2);
                    int sum2 = left2 + right2;

                    return sum1 > sum2 ? i1 : i2;
                });
            }

            SelectDominoFromHand(playerIndex, selectedDomino.Value);

            yield return new WaitForSeconds(2f);
        }

        protected IEnumerator PlayNeloSkipTurn(int playerIndex)
        {
            MessageText.text = $"{players[CurrentPlayerTurn].name}'s turn is skipped...";

            yield return new WaitForSeconds(2f);

            players[playerIndex].TurnComplete = true;
            players[playerIndex].SelectedDomino = null;
        }

        public virtual void SelectDominoFromHand(int playerIndex, byte selectedDomino)
        {
            // Verify selected domino is in hand...?

            var selectedDominoGameObject = players[playerIndex].transform.Find(dominoes[selectedDomino]).gameObject;
            var selectableDomino = selectedDominoGameObject.GetComponent<Selectable>();

            Vector3 initPos = selectedDominoGameObject.transform.position;
            Quaternion initRot = selectedDominoGameObject.transform.rotation;

            // Remove domino from hand
            Destroy(selectedDominoGameObject);
            players[playerIndex].Hand.Remove(selectedDomino);

            // Move domino to play area
            GameObject newDomino = Instantiate(
                dominoPrefab,
                initPos,
                initRot,
                playerSpots[playerIndex].transform
            );
            newDomino.name = dominoes[selectedDomino];
            newDomino.GetComponent<Selectable>().faceUp = true;

            dominoAnimator.AddDominoAnimation(
                newDomino,
                new Vector2(
                    playerSpots[playerIndex].transform.position.x,
                    playerSpots[playerIndex].transform.position.y),
                Quaternion.Euler(0, 0, 90),
                false);

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

            if (players.Exists(p => p.SelectedDomino?.name?.Contains(((int)Trump).ToString()) ?? false))
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

                MessageText.text = $"We won the round...";
            }
            else
            {
                // Them
                RoundScoreThem += GetGameScore() + 1;
                RoundThemText.text = $"Them: {RoundScoreThem}";

                MessageText.text = $"They won the round...";
            }

            InitialPlayerTurn = playerWinnerIndex;

            RoundComplete = true;
        }

        int GetPlayerHandWinner(int trump)
        {
            int playerIndex = -1;
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].SelectedDomino?.name?.Contains((trump).ToString()) ?? false)
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
                if (((player.SelectedDomino?.sum ?? 0) % 5) == 0)
                {
                    gameScore += player.SelectedDomino?.sum ?? 0;
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
                if (!string.IsNullOrWhiteSpace(players[i]?.SelectedDomino?.name))
                {
                    var childGameObj = playerSpots[i].gameObject.transform.Find(players[i].SelectedDomino.name).gameObject;

                    Destroy(childGameObj);

                    discardPile.Add((byte)dominoes.FindIndex(d => d == childGameObj.name));
                }
            }
        }

        public void ResetSet()
        {
            ResetRound();

            IsDealing = null;

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
                players[i].BidComplete = false;

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
                if (isNelO && Trump == Trump.NELO)
                {
                    if (RoundScoreUs > 0)
                    {
                        SetScoreThem += 1;
                        SetThemText.text = $"Them: {SetScoreThem}";
                        SetComplete = true;

                        MessageText.text = $"They won the set";
                    }
                    else if (players[InitialPlayerTurn].Hand.Count == 0)
                    {
                        SetScoreUs += 1;
                        SetUsText.text = $"Us: {SetScoreUs}";
                        SetComplete = true;

                        MessageText.text = $"We won the set";
                    }
                }
                else
                {
                    if (RoundScoreUs >= CurrentBidAmount)
                    {
                        SetScoreUs += 1;
                        SetUsText.text = $"Us: {SetScoreUs}";
                        SetComplete = true;

                        MessageText.text = $"We won the set";
                    }
                    else if (RoundScoreThem > (42 - CurrentBidAmount))
                    {
                        SetScoreThem += 1;
                        SetThemText.text = $"Them: {SetScoreThem}";
                        SetComplete = true;

                        MessageText.text = $"They won the set";
                    }
                }
            }
            else
            {
                if (isNelO && Trump == Trump.NELO)
                {
                    if (RoundScoreThem > 0)
                    {
                        SetScoreUs += 1;
                        SetUsText.text = $"Us: {SetScoreUs}";
                        SetComplete = true;

                        MessageText.text = $"We won the set";
                    }
                    else if (players[InitialPlayerTurn].Hand.Count == 0)
                    {
                        SetScoreThem += 1;
                        SetThemText.text = $"Them: {SetScoreThem}";
                        SetComplete = true;

                        MessageText.text = $"They won the set";
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
            }

            SetComplete = SetComplete == null ? false : SetComplete;
        }

        //****************** Encryption Data *********************//
        
        protected void CalculateKey(string roomId)
        {
            string roomIdSubString = roomId.Substring(0, 16);
            encryptionKey = Encoding.UTF8.GetBytes(roomIdSubString);
        }

        protected void Encrypt()
        {
            Debug.Log("ENCRYPT");
            SWNetworkMessage message = new SWNetworkMessage();
            
            for (int i = 0; i < players.Count; i++)
            {
                message.PushUTF8ShortString(players[i].Id);

                message.Push((byte)players[i].Hand.Count);
                message.PushByteArray(players[i].Hand.ToArray());

                message.Push(players[i].IsDealer);

                message.Push(players[i].IsAI);

                message.PushUTF8ShortString(players[i].BidAmount == null ? string.Empty : players[i].BidAmount.ToString());

                message.Push(players[i].BidComplete);

                message.PushUTF8ShortString(players[i].Trump.HasValue ? ((int)players[i].Trump).ToString() : string.Empty);

                message.Push(players[i].TurnComplete);
                
                message.PushUTF8ShortString(players[i].SelectedDomino?.name ?? string.Empty);

                message.Push(players[i].IsActive);

                message.PushUTF8ShortString(playerBidTexts[i].text);
            }
            

            message.Push((byte)deck.Count);
            message.PushByteArray(deck.ToArray());

            message.Push((byte)discardPile.Count);
            message.PushByteArray(discardPile.ToArray());
            

            message.Push((int)gameState);

            message.Push(IsDealing == null ? -1 : IsDealing.Value ? 1 : 0);

            message.PushUTF8ShortString(CurrentBidAmount.ToString());

            message.PushUTF8ShortString(WhoBid == -1 ? string.Empty : players[WhoBid].Id);

            message.PushUTF8ShortString(trumpText.text);

            message.Push((int)Trump);

            message.PushUTF8ShortString(InitialPlayerTurn == -1 ? string.Empty : players[InitialPlayerTurn].Id);

            message.PushUTF8ShortString(CurrentPlayerTurn == -1 ? string.Empty : players[CurrentPlayerTurn].Id);

            message.Push(RoundComplete);

            message.Push(SetComplete == null ? -1 : SetComplete.Value ? 1 : 0);

            //message.Push(RoundScoreUs);

            //message.Push(RoundScoreThem);

            //message.Push(SetScoreUs);

            //message.Push(SetScoreThem);

            //message.Push(marksToWin);

            //message.Push(isNelO);

            //message.Push(isForceBid);

            safeData = AES.EncryptAES128(message.ToArray(), encryptionKey);
        }

        protected void Decrypt()
        {
            Debug.Log("DECRYPT");
            byte[] byteArray = AES.DecryptAES128(safeData, encryptionKey);

            SWNetworkMessage message = new SWNetworkMessage(byteArray);

            for (int i = 0; i < players.Count; i++)
            {
                string id = message.PopUTF8ShortString();

                int playerIndex = players.FindIndex(p => p.Id == id);
                
                byte handCount = message.PopByte();
                players[playerIndex].Hand = message.PopByteArray(handCount).ToList();
                
                players[playerIndex].IsDealer = message.PopBool();
                
                players[playerIndex].IsAI = message.PopBool();

                string bidAmountStr = message.PopUTF8ShortString();
                players[playerIndex].BidAmount = string.IsNullOrWhiteSpace(bidAmountStr) ? (int?)null : int.Parse(bidAmountStr);
                
                players[playerIndex].BidComplete = message.PopBool();

                string playerTrumpStr = message.PopUTF8ShortString();
                players[playerIndex].Trump = string.IsNullOrWhiteSpace(playerTrumpStr) ? (Trump?)null : (Trump)int.Parse(playerTrumpStr);
                
                players[playerIndex].TurnComplete = message.PopBool();

                string selectedDominoName = message.PopUTF8ShortString();

                if (!string.IsNullOrWhiteSpace(selectedDominoName))
                {
                    var selectedDominoTransform = players[playerIndex].transform.Find(selectedDominoName);
                    if (selectedDominoTransform == null)
                    {
                        playerSpots[playerIndex].transform.Find(selectedDominoName);
                    }

                    if (selectedDominoTransform != null)
                    {
                        var selectedDominoGameObject = selectedDominoTransform.gameObject;
                        var selectableDomino = selectedDominoGameObject.GetComponent<Selectable>();
                        players[playerIndex].SelectedDomino = selectableDomino;
                    }
                }

                players[playerIndex].IsActive = message.PopBool();
                
                playerBidTexts[playerIndex].text = message.PopUTF8ShortString();
            }

            
            byte deckCount = message.PopByte();
            deck = message.PopByteArray(deckCount).ToList();
            
            byte discardPileCount = message.PopByte();
            discardPile = message.PopByteArray(discardPileCount).ToList();

            
            gameState = (GameState)message.PopUInt32();
            
            int isDealingInt = message.PopInt32();
            IsDealing = isDealingInt == -1 ? (bool?)null : isDealingInt == 1 ? true : false;
            
            string currentBidAmountStr = message.PopUTF8ShortString();
            CurrentBidAmount = string.IsNullOrWhiteSpace(currentBidAmountStr) ? (int?)null : int.Parse(currentBidAmountStr);

            string whoBidId = message.PopUTF8ShortString();
            WhoBid = players.FindIndex(p => p.Id == whoBidId);
            
            trumpText.text = message.PopUTF8ShortString();
            
            Trump = (Trump)message.PopInt32();
            
            string initialPlayerTurnId = message.PopUTF8ShortString();
            InitialPlayerTurn = players.FindIndex(p => p.Id == initialPlayerTurnId);
            
            string currentPlayerTurnId = message.PopUTF8ShortString();
            CurrentPlayerTurn = players.FindIndex(p => p.Id == currentPlayerTurnId);
            
            RoundComplete = message.PopBool();
            
            int setCompleteInt = message.PopInt32();
            SetComplete = setCompleteInt == -1 ? (bool?)null : setCompleteInt == 1 ? true : false;

            //RoundScoreUs = message.PopInt32();
            //RoundUsText.text = $"Us: {RoundScoreUs}";

            //RoundScoreThem = message.PopInt32();
            //RoundThemText.text = $"Them: {RoundScoreThem}";

            //SetScoreUs = message.PopInt32();
            //SetUsText.text = $"Us: {SetScoreUs}";

            //SetScoreThem = message.PopInt32();
            //SetThemText.text = $"Them: {SetScoreThem}";

            //marksToWin = message.PopInt32();
            //MarkText.text = $"{marksToWin}M";

            //isNelO = message.PopBool();

            //isForceBid = message.PopBool();
        }

        public EncryptedData EncryptedData()
        {
            EncryptedData encryptedData = new EncryptedData();
            encryptedData.data = safeData;

            return encryptedData;
        }

        public void ApplyEncrptedData(EncryptedData encryptedData)
        {
            if (encryptedData == null)
            {
                return;
            }

            safeData = encryptedData.data;
        }

        //****************** Animator Event *********************//

        public virtual void AllAnimationsFinished()
        {
            GameFlow();
        }
    }
}