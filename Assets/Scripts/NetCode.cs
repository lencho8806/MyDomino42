using SWNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Domino42
{
    [Serializable]
    public class GameDataEvent : UnityEvent<EncryptedData>
    {

    }

    [Serializable]
    public class BidSelectedEvent : UnityEvent<int>
    {

    }

    [Serializable]
    public class TrumpSelectedEvent : UnityEvent<int>
    {

    }

    [Serializable]
    public class DominoSelectedEvent : UnityEvent<byte>
    {

    }

    public class NetCode : MonoBehaviour
    {
        public GameDataEvent OnGameDataReadyEvent = new GameDataEvent();
        public GameDataEvent OnGameDataChangedEvent = new GameDataEvent();
        
        public UnityEvent OnGameStateChangedEvent = new UnityEvent();

        public BidSelectedEvent OnBidSelectedEvent = new BidSelectedEvent();

        public TrumpSelectedEvent OnTrumpSelectedEvent = new TrumpSelectedEvent();

        public DominoSelectedEvent OnDominoSelectedEvent = new DominoSelectedEvent();

        public UnityEvent OnResetRoundEvent = new UnityEvent();
        public UnityEvent OnResetSetEvent = new UnityEvent();
        public UnityEvent OnResetMatchEvent = new UnityEvent();

        RoomPropertyAgent roomPropertyAgent;
        RoomRemoteEventAgent roomRemoteEventAgent;

        const string ENCRYPTED_DATA = "EncryptedData";
        const string GAME_STATE_CHANGED = "GameStateChanged";
        const string BID_SELECTED = "BidSelected";
        const string TRUMP_SELECTED = "TrumpSelected";
        const string DOMINO_SELECTED = "DominoSelected";
        const string RESET_ROUND = "ResetRound";
        const string RESET_SET = "ResetSet";
        const string RESET_MATCH = "ResetMatch";

        private void Awake()
        {
            Debug.Log("NetCode -> Awake");
            roomPropertyAgent = FindObjectOfType<RoomPropertyAgent>();
            roomRemoteEventAgent = FindObjectOfType<RoomRemoteEventAgent>();

            //roomRemoteEventAgent.AddListener(BID_SELECTED, OnBidSelectedRemoteEvent);
        }

        private void OnDestroy()
        {
            //roomRemoteEventAgent.RemoveListener(BID_SELECTED, OnBidSelectedRemoteEvent);
        }

        public void EnableRoomPropertyAgent()
        {
            Debug.Log("NetCode -> EnableRoomPropertyAgent");
            roomPropertyAgent.Initialize();
        }

        public void ModifyGameData(EncryptedData encryptedData)
        {
            Debug.Log("NetCode -> ModifyGameData");
            roomPropertyAgent.Modify(ENCRYPTED_DATA, encryptedData);
        }
        
        public void NotifyOtherPlayersGameStateChanged()
        {
            Debug.Log("NetCode -> NotifyOtherPlayersGameStateChanged");
            roomRemoteEventAgent.Invoke(GAME_STATE_CHANGED);
        }

        public void NotifyHostPlayerBidSelected(int amount)
        {
            Debug.Log("NetCode -> NotifyHostPlayerBidSelected");

            SWNetworkMessage message = new SWNetworkMessage();
            message.Push(amount);
            
            roomRemoteEventAgent.Invoke(BID_SELECTED, message);
        }

        public void NotifyHostPlayerTrumpSelected(int trump)
        {
            Debug.Log("NetCode -> NotifyHostPlayerTrumpSelected");

            SWNetworkMessage message = new SWNetworkMessage();
            message.Push(trump);

            roomRemoteEventAgent.Invoke(TRUMP_SELECTED, message);
        }
        
        public void NotifyOtherPlayerDominoSelected(byte selectedDomino)
        {
            Debug.Log("NetCode -> NotifyOtherPlayerDominoSelected");

            SWNetworkMessage message = new SWNetworkMessage();
            message.Push(selectedDomino);

            roomRemoteEventAgent.Invoke(DOMINO_SELECTED, message);
        }

        public void NotifyOtherPlayerResetRound()
        {
            Debug.Log("NetCode -> NotifyOtherPlayerResetRound");
            
            roomRemoteEventAgent.Invoke(RESET_ROUND);
        }

        public void NotifyOtherPlayerResetSet()
        {
            Debug.Log("NetCode -> NotifyOtherPlayerResetSet");
            
            roomRemoteEventAgent.Invoke(RESET_SET);
        }

        public void NotifyOtherPlayerResetMatch()
        {
            Debug.Log("NetCode -> NotifyOtherPlayerResetMatch");
            
            roomRemoteEventAgent.Invoke(RESET_MATCH);
        }

        //****************** Room Property Events *********************//

        public void OnEncryptedDataReady()
        {
            Debug.Log("NetCode -> OnEncryptedDataReady");
            EncryptedData encryptedData = roomPropertyAgent.GetPropertyWithName(ENCRYPTED_DATA).GetValue<EncryptedData>();
            OnGameDataReadyEvent.Invoke(encryptedData);
        }

        public void OnEncryptedDataChanged()
        {
            Debug.Log("NetCode -> OnEncryptedDataChanged");
            EncryptedData encryptedData = roomPropertyAgent.GetPropertyWithName(ENCRYPTED_DATA).GetValue<EncryptedData>();
            OnGameDataChangedEvent.Invoke(encryptedData);
        }

        //****************** Room Remote Events *********************//
        
        public void OnGameStateChangedRemoteEvent()
        {
            Debug.Log("NetCode -> OnGameStateChangedRemoteEvent");
            OnGameStateChangedEvent.Invoke();
        }
        
        public void OnBidSelectedRemoteEvent(SWNetworkMessage message)
        {
            int amount = message.PopInt32();
            Debug.Log($"NetCode -> OnBidSelectedRemoteEvent:{amount}");
            
            OnBidSelectedEvent.Invoke(amount);
        }

        public void OnTrumpSelectedRemoteEvent(SWNetworkMessage message)
        {
            int trump = message.PopInt32();
            Debug.Log($"NetCode -> OnTrumpSelectedRemoteEvent:{trump}");

            OnTrumpSelectedEvent.Invoke(trump);
        }

        public void OnDominoSelectedRemoteEvent(SWNetworkMessage message)
        {
            byte selectedDomino = message.PopByte();
            Debug.Log($"NetCode -> OnDominoSelectedRemoteEvent:{selectedDomino}");

            OnDominoSelectedEvent.Invoke(selectedDomino);
        }

        public void OnResetRoundRemoteEvent()
        {
            Debug.Log("NetCode -> OnResetRoundRemoteEvent");
            OnResetRoundEvent.Invoke();
        }

        public void OnResetSetRemoteEvent()
        {
            Debug.Log("NetCode -> OnResetSetRemoteEvent");
            OnResetSetEvent.Invoke();
        }

        public void OnResetMatchRemoteEvent()
        {
            Debug.Log("NetCode -> OnResetMatchRemoteEvent");
            OnResetMatchEvent.Invoke();
        }
    }

}