using SWNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Domino42
{
    [Serializable]
    public class EncryptedData
    {
        public byte[] data;
    }
    
    public class MultiplayerGame : Game
    {
        NetCode netCode;
        int hostPlayerIndex = -1;

        protected new void Awake()
        {
            base.Awake();
            Debug.Log("MultiplayerGame -> Awake");
            
            netCode = FindObjectOfType<NetCode>();

            NetworkClient.Lobby.GetPlayersInRoom((successful, reply, error) =>
            {
                if (successful)
                {
                    foreach (SWPlayer swPlayer in reply.players)
                    {
                        string playerName = swPlayer.GetCustomDataString();
                        string playerId = swPlayer.id;

                        if (playerId.Equals(NetworkClient.Instance.PlayerId))
                        {
                            players[0].Id = playerId;
                            players[0].name = playerName;
                            players[0].IsAI = false;
                            PlayerNames[0].text = playerName;
                        }
                        else
                        {
                            players[2].Id = playerId;
                            players[2].name = playerName;
                            players[2].IsAI = false;
                            PlayerNames[2].text = playerName;
                        }
                    }
                    
                    CalculateKey(NetworkClient.Lobby.RoomId);
                    Encrypt();
                    netCode.EnableRoomPropertyAgent();
                }
                else
                {
                    Debug.Log("Failed to get players in room.");
                }

            });
        }

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("Multiplayer Game Start");

            //MarkText.text = $"{marksToWin}M";

            if (NetworkClient.Instance.IsHost)
            {
                players[0].IsDealer = true;
            }

            var gameOptions = FindObjectOfType<GameOptions>();

            if (NetworkClient.Lobby.IsOwner)
            {
                if (gameOptions != null)
                {
                    marksToWin = gameOptions.marks;
                    isNelO = gameOptions.isNelO;
                    isForceBid = gameOptions.isForceBid;
                }

                netCode.NotifyOtherPlayersGameOptions(marksToWin, isNelO, isForceBid);
            }

            if (gameOptions != null)
            {
                Destroy(gameOptions);
            }
        }

        private void OnDestroy()
        {
            netCode.LeaveRoom();
        }

        // Update is called once per frame
        protected new void Update()
        {
            if (NetworkClient.Instance.IsHost)
            {
                switch (gameState)
                {
                    case GameState.Deal:
                        {
                            if (IsDealing == false)
                            {
                                gameState = GameState.Bid;

                                Encrypt();
                                netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first
                            }
                            break;
                        }
                    case GameState.Bid:
                        {
                            if (CurrentPlayerTurn >= 0 && players[CurrentPlayerTurn].BidComplete)
                            {
                                if (players.Exists(player => !player.BidComplete))
                                {
                                    CurrentPlayerTurn = -1;
                                    //GameFlow();
                                    Encrypt();
                                    netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first

                                    netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
                                }
                                else
                                {
                                    gameState = GameState.Trump;
                                    //GameFlow();
                                    Encrypt();
                                    netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first

                                    netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
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
                                //GameFlow();
                                Encrypt();
                                netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first

                                netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
                            }
                            break;
                        }
                    case GameState.Play:
                        {
                            
                            break;
                        }
                    case GameState.RoundWinner:
                        {
                            if (RoundComplete)
                            {
                                gameState = GameState.SetWinner;
                                //GameFlow();
                                Encrypt();
                                netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first

                                netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
                            }
                            break;
                        }
                    case GameState.SetWinner:
                        {
                            if (SetComplete != null)
                            {
                                Debug.Log("SetComplete != null");
                                if (SetComplete == true)
                                {
                                    Debug.Log("SetComplete == true");
                                    if (SetScoreUs >= marksToWin)
                                    {
                                        gameState = GameState.Win;
                                        //GameFlow();
                                        Encrypt();
                                        netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first

                                        netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
                                    }
                                    else if (SetScoreThem >= marksToWin)
                                    {
                                        gameState = GameState.Lose;
                                        //GameFlow();
                                        Encrypt();
                                        netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first

                                        netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
                                    }
                                    else
                                    {
                                        Debug.Log("Reset Set");
                                        //ResetSet();
                                        netCode.NotifyOtherPlayerResetSet();

                                        gameState = GameState.Idle; // just wait until notifications are complete
                                    }
                                }
                                else
                                {
                                    Debug.Log("SetComplete == false");

                                    Debug.Log("Reset Round");
                                    //ResetRound();
                                    netCode.NotifyOtherPlayerResetRound();

                                    gameState = GameState.Idle; // just wait until notifications are complete
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
                                    //ResetMatch();
                                    netCode.NotifyOtherPlayerResetMatch();
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
                                    //ResetMatch();
                                    netCode.NotifyOtherPlayerResetMatch();
                                }
                            }
                            break;
                        }
                }
            }
        }

        //****************** Game Flow *********************//

        // Shuffle
        protected override void Shuffle()
        {
            MessageText.text = "Shuffling...";

            if (NetworkClient.Instance.IsHost)
            {
                deck = GenerateDeck();
                Shuffle(deck);

                gameState = GameState.Deal;

                Encrypt();
                netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first
                //GameFlow();
                netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
            }

            MessageText.text = string.Empty;
        }

        // Deal
        protected override void Deal()
        {
            IsDealing = true;
            
            DominoDeal();
            DominoDealDealer();

            IsDealing = false;
        }

        // Bid
        protected override void Bid()
        {
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
                if (NetworkClient.Instance.IsHost)
                {
                    // why? go to the next section
                    gameState = GameState.Trump;
                    //GameFlow();
                    Encrypt();
                    netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first

                    netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
                }
            }
            else if (!players[CurrentPlayerTurn].IsAI)
            {
                // Player bid
                if (CurrentPlayerTurn == 0)
                {
                    MessageText.text = string.Empty;
                    bidMenu.BidStart();
                }
                else
                {
                    MessageText.text = $"{players[CurrentPlayerTurn].name} is bidding...";
                }
            }
            else
            {
                // AI bid
                MessageText.text = $"{players[CurrentPlayerTurn].name} is bidding...";

                if (NetworkClient.Instance.IsHost)
                {
                    BidEnumerator(CurrentPlayerTurn);
                }
            }
        }

        public override void BidEnd(int amount)
        {
            if (gameState == GameState.Bid && CurrentPlayerTurn == 0)
            {
                players[CurrentPlayerTurn].BidAmount = amount;

                playerBidTexts[CurrentPlayerTurn].text = amount.ToString();

                netCode.NotifyHostPlayerBidSelected(amount);
            }
        }

        protected override void SetTrump()
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
                if (NetworkClient.Instance.IsHost)
                {
                    gameState = GameState.Play;
                    //GameFlow();
                    Encrypt();
                    netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first

                    netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
                }
            }
            else if (!players[CurrentPlayerTurn].IsAI)
            {
                // Player trump
                if (CurrentPlayerTurn == 0)
                {
                    MessageText.text = string.Empty;

                    trumpMenu.TrumpStart();
                }
                else
                {
                    MessageText.text = $"{players[CurrentPlayerTurn].name} is selecting trump...";
                }
            }
            else
            {
                // AI trump
                MessageText.text = $"{players[CurrentPlayerTurn].name} is selecting trump...";

                TrumpEnumerator(CurrentPlayerTurn);
            }
        }

        public override void TrumpEnd(int trump)
        {
            if (gameState == GameState.Trump && CurrentPlayerTurn == 0)
            {
                netCode.NotifyHostPlayerTrumpSelected(trump);
            }
        }

        protected override void SetPlay()
        {
            CurrentPlayerTurn = -1;
            for (int i = InitialPlayerTurn; i < (InitialPlayerTurn + 4); i++)
            {
                if (players[i % 4]?.TurnComplete == false)
                {
                    CurrentPlayerTurn = i % 4;
                    break;
                }
            }
            
            if (CurrentPlayerTurn == -1)
            {
                if (NetworkClient.Instance.IsHost)
                {
                    //determine Game winner
                    gameState = GameState.RoundWinner;
                    //GameFlow();
                    Encrypt();
                    netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first

                    netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
                }
            }
            else if (!players[CurrentPlayerTurn].IsAI)
            {
                // Player playing
                if (CurrentPlayerTurn == 0)
                {
                    MessageText.text = "Your turn...";
                }
                else
                {
                    MessageText.text = $"{players[CurrentPlayerTurn].name} is selecting domino...";
                }
            }
            else
            {
                // AI bid
                MessageText.text = $"{players[CurrentPlayerTurn].name}'s turn...";

                if (NetworkClient.Instance.IsHost)
                {
                    PlayEnumerator(CurrentPlayerTurn);
                }
            }
        }

        public override void SelectDominoFromHand(int playerIndex, byte selectedDomino)
        {
            Debug.Log($"MultiplayerGame -> SelectDominoFromHand:{selectedDomino}");
            // Verify selected domino is in hand...?

            netCode.NotifyOtherPlayerDominoSelected(selectedDomino);
        }
        
        public override void AllAnimationsFinished()
        {
            Debug.Log("MultiplayerGame -> AllAnimationsFinished");
            if (NetworkClient.Instance.IsHost)
            {
                netCode.NotifyOtherPlayersGameStateChanged();
            }
        }

        private void SetAIPlayers()
        {
            if (!NetworkClient.Instance.IsHost && hostPlayerIndex == -1)
            {
                hostPlayerIndex = players.FindIndex(p => p.IsDealer);

                if (hostPlayerIndex == -1) return;

                for (int i = 1; i < players.Count; i++)
                {
                    var index = (i + hostPlayerIndex) % 4;
                    if (players[index].IsAI)
                    {
                        players[index].Id = i.ToString();
                        players[index].name = $"Player Name {(i + 1).ToString()}";
                        PlayerNames[index].text = players[index].name;
                    }
                }
            }
        }

        //****************** NetCode Events *********************//

        public void OnGameDataReady(EncryptedData encryptedData)
        {
            Debug.Log("MultiplayerGame -> OnGameDataReady");

            if (encryptedData == null || encryptedData.data.Length == 0)
            {
                Debug.Log("New game");
                
                if (NetworkClient.Instance.IsHost)
                {
                    Debug.Log("Host");
                    gameState = GameState.Shuffle;

                    Encrypt();

                    netCode.ModifyGameData(EncryptedData());

                    netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
                }
                else
                {
                    Debug.Log("NOT Host");

                    SetAIPlayers();
                }
            }
            else
            {
                Debug.Log("ELSE");
                ApplyEncrptedData(encryptedData);
                Decrypt();

                if (gameState > GameState.Shuffle)
                {
                    Debug.Log("Restore the game state");

                    ////restore player's cards
                    //cardAnimator.DealDisplayingCards(localPlayer, gameDataManager.PlayerCards(localPlayer).Count, false);
                    //cardAnimator.DealDisplayingCards(remotePlayer, gameDataManager.PlayerCards(remotePlayer).Count, false);

                    ////restore player's books
                    //List<byte> booksForLocalPlayer = gameDataManager.PlayerBooks(localPlayer);
                    //foreach (byte rank in booksForLocalPlayer)
                    //{
                    //    localPlayer.RestoreBook((Ranks)rank, cardAnimator);
                    //}

                    //List<byte> booksForRemotePlayer = gameDataManager.PlayerBooks(remotePlayer);
                    //foreach (byte rank in booksForRemotePlayer)
                    //{
                    //    remotePlayer.RestoreBook((Ranks)rank, cardAnimator);
                    //}

                    base.GameFlow();
                }
            }
        }

        public void OnGameDataChanged(EncryptedData encryptedData)
        {
            Debug.Log("MultiplayerGame -> OnGameDataChanged");
            ApplyEncrptedData(encryptedData);
            SetAIPlayers();
            Decrypt();
            Encrypt();
        }

        public void OnGameOptions(int marks, bool nelO, bool forceBid)
        {
            Debug.Log($"MultiplayerGame -> OnGameOptions:{marks}-{nelO.ToString()}-{forceBid.ToString()}");

            marksToWin = marks;
            MarkText.text = $"{marksToWin}M";

            isNelO = nelO;

            isForceBid = forceBid;
        }

        public void OnGameStateChanged()
        {
            Debug.Log("MultiplayerGame -> OnGameStateChanged");
            base.GameFlow();
        }

        public void OnBidSelected(int amount)
        {
            Debug.Log($"MultiplayerGame -> OnBidSelected:{amount}");

            players[CurrentPlayerTurn].BidAmount = amount;
            players[CurrentPlayerTurn].BidComplete = true;

            playerBidTexts[CurrentPlayerTurn].text = amount.ToString();
        }

        public void OnTrumpSelected(int trump)
        {
            Debug.Log($"MultiplayerGame -> OnTrumpSelected:{trump}");

            players[CurrentPlayerTurn].Trump = (Trump)trump;
            Trump = (Trump)trump;

            trumpText.text = trump.ToString();
        }

        public void OnDominoSelected(byte selectedDomino)
        {
            Debug.Log($"MultiplayerGame -> OnDominoSelected:{selectedDomino}");
            //// Verify selected domino is in hand...?

            var selectedDominoTransform = players[CurrentPlayerTurn].transform.Find(dominoes[selectedDomino]);

            if (selectedDominoTransform == null)
            {
                Debug.LogError("MultiplayerGame -> OnDominoSelected: selected domino not found...");
                return;
            }

            var selectedDominoGameObject = selectedDominoTransform.gameObject;
            var selectableDomino = selectedDominoGameObject.GetComponent<Selectable>();

            Vector3 initPos = selectedDominoGameObject.transform.position;
            Quaternion initRot = selectedDominoGameObject.transform.rotation;

            // Remove domino from hand
            Destroy(selectedDominoGameObject);
            players[CurrentPlayerTurn].Hand.Remove(selectedDomino);

            // Move domino to play area
            GameObject newDomino = Instantiate(
                dominoPrefab,
                initPos,
                initRot,
                playerSpots[CurrentPlayerTurn].transform
            );
            newDomino.name = dominoes[selectedDomino];
            newDomino.GetComponent<Selectable>().faceUp = true;

            dominoAnimator.AddDominoAnimation(
                newDomino,
                new Vector2(
                    playerSpots[CurrentPlayerTurn].transform.position.x,
                    playerSpots[CurrentPlayerTurn].transform.position.y),
                Quaternion.Euler(0, 0, 90),
                //false);
                true);
            
            players[CurrentPlayerTurn].TurnComplete = true;
            players[CurrentPlayerTurn].SelectedDomino = selectableDomino;
        }

        public void OnResetRound()
        {
            Debug.Log("MultiplayerGame -> OnResetRound");

            ResetRound();

            if (NetworkClient.Instance.IsHost)
            {
                gameState = GameState.Play;
                //GameFlow();
                Encrypt();
                netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first

                netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
            }
        }

        public void OnResetSet()
        {
            Debug.Log("MultiplayerGame -> OnResetSet");

            ResetSet();

            if (NetworkClient.Instance.IsHost)
            {
                gameState = GameState.Shuffle;
                //GameFlow();
                Encrypt();
                netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first

                netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
            }
        }

        public void OnResetMatch()
        {
            Debug.Log("MultiplayerGame -> OnResetMatch");

            ResetMatch();

            if (NetworkClient.Instance.IsHost)
            {
                gameState = GameState.Shuffle;
                //GameFlow();
                Encrypt();
                netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first

                netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
            }
        }

        public void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }
    }
}