using SWNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Domino42
{
    [Serializable]
    public class EncryptedData
    {
        public byte[] data;
    }

    public class TestProperty
    {
        public string test = null;
    }

    public class MultiplayerGame : Game
    {
        NetCode netCode;

        protected new void Awake()
        {
            base.Awake();
            Debug.Log("MultiplayerGame -> Awake");


            //MessageText.text = $"[{gameState.ToString()}]";

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

                    //gameDataManager = new GameDataManager(localPlayer, remotePlayer, NetworkClient.Lobby.RoomId);
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
        }

        // Update is called once per frame
        protected new void Update()
        {
            //MessageText.text = $"[{gameState.ToString()}]";
            MessageText.text = $"[{gameState.ToString()} - {dataChangeCount}]";

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

                                myTestProperty.test = "DEAL complete";
                                netCode.ModifyTestProperty(myTestProperty);
                            }
                            break;
                        }
                    case GameState.Bid:
                        {
                            if (CurrentPlayerTurn >= 0 && players[CurrentPlayerTurn].BidAmount != null)
                            {
                                if (players.Exists(player => player.BidAmount == null))
                                {
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
                                    if (SetScoreUs >= 3)
                                    {
                                        gameState = GameState.Win;
                                        GameFlow();
                                    }
                                    else if (SetScoreThem >= 3)
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
        }

        //****************** Game Flow *********************//

        // Shuffle
        protected override void Shuffle()
        {
            if (NetworkClient.Instance.IsHost)
            {
                deck = GenerateDeck();
                Shuffle(deck);

                gameState = GameState.Deal;

                Encrypt();
                netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first
                //GameFlow();
                netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow

                myTestProperty.test = "SHUFFLE complete";
                netCode.ModifyTestProperty(myTestProperty);
            }
        }

        // Deal
        protected override void Deal()
        {
            IsDealing = true;

            //StartCoroutine(DominoDeal());
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
                // why? go to the next section
                gameState = GameState.Trump;
                //GameFlow();
                Encrypt();
                netCode.ModifyGameData(EncryptedData()); //NEED to Encrypt first
                
                netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow
            }
            else if (!players[CurrentPlayerTurn].IsAI)
            {
                // Player bid
                //MessageText.text = string.Empty;

                bidMenu.BidStart();
            }
            else
            {
                // AI bid
                //MessageText.text = $"{players[CurrentPlayerTurn].name} is bidding...";

                BidEnumerator(CurrentPlayerTurn);
            }
        }

        public override void AllAnimationsFinished()
        {
            Debug.Log("MultiplayerGame -> AllAnimationsFinished");
            if (NetworkClient.Instance.IsHost)
            {
                netCode.NotifyOtherPlayersGameStateChanged();
            }
        }

        //****************** NetCode Events *********************//

        public void OnGameDataReady(EncryptedData encryptedData)
        {
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
                }
            }
            else
            {
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
            Decrypt();
            Encrypt();
        }

        public void OnTestPropertyReady(TestProperty testProperty)
        {
            if (testProperty == null)
            {
                Debug.Log("New game");

                if (NetworkClient.Instance.IsHost)
                {
                    //Debug.Log("Host");
                    //gameState = GameState.Shuffle;

                    //Encrypt();

                    //netCode.ModifyGameData(EncryptedData());

                    //netCode.NotifyOtherPlayersGameStateChanged(); // GameFlow

                    myTestProperty.test = "New game";
                    netCode.ModifyTestProperty(myTestProperty);
                }
                else
                {
                    Debug.Log("NOT Host");
                }
            }
            else
            {
                //ApplyEncrptedData(encryptedData);
                //Decrypt();

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

        public void OnTestPropertyChanged(TestProperty testProperty)
        {
            Debug.Log("MultiplayerGame -> OnGameDataChanged");
            ApplyEncrptedTestProperty(testProperty);
            //Decrypt();
            //Encrypt();
        }

        public void OnGameStateChanged()
        {
            Debug.Log("MultiplayerGame -> OnGameStateChanged");
            base.GameFlow();
        }
    }
}