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
    public class TestPropertyEvent : UnityEvent<TestProperty>
    {

    }

    public class NetCode : MonoBehaviour
    {
        public GameDataEvent OnGameDataReadyEvent = new GameDataEvent();
        public GameDataEvent OnGameDataChangedEvent = new GameDataEvent();

        public TestPropertyEvent OnGameDataReadyTestPropertyEvent = new TestPropertyEvent();
        public TestPropertyEvent OnGameDataChangedTestPropertyEvent = new TestPropertyEvent();

        public UnityEvent OnGameStateChangedEvent = new UnityEvent();

        RoomPropertyAgent roomPropertyAgent;
        RoomRemoteEventAgent roomRemoteEventAgent;

        const string ENCRYPTED_DATA = "EncryptedData";
        const string GAME_STATE_CHANGED = "GameStateChanged";

        private void Awake()
        {
            Debug.Log("NetCode -> Awake");
            roomPropertyAgent = FindObjectOfType<RoomPropertyAgent>();
            roomRemoteEventAgent = FindObjectOfType<RoomRemoteEventAgent>();
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

        public void ModifyTestProperty(TestProperty testProperty)
        {
            Debug.Log("NetCode -> ModifyGameData");
            roomPropertyAgent.Modify("TestProperty", testProperty);
        }

        public void NotifyOtherPlayersGameStateChanged()
        {
            Debug.Log("NetCode -> NotifyOtherPlayersGameStateChanged");
            roomRemoteEventAgent.Invoke(GAME_STATE_CHANGED);
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

        public void OnTestPropertyReady()
        {
            Debug.Log("NetCode -> OnTestPropertyReady");
            TestProperty testProperty = roomPropertyAgent.GetPropertyWithName("TestProperty").GetValue<TestProperty>();
            OnGameDataReadyTestPropertyEvent.Invoke(testProperty);
        }

        public void OnTestPropertyChanged()
        {
            Debug.Log("NetCode -> OnTestPropertyChanged");
            TestProperty testProperty = roomPropertyAgent.GetPropertyWithName("TestProperty").GetValue<TestProperty>();
            OnGameDataChangedTestPropertyEvent.Invoke(testProperty);
        }

        //****************** Room Remote Events *********************//

        public void OnGameStateChangedRemoteEvent()
        {
            Debug.Log("NetCode -> OnGameStateChangedRemoteEvent");
            OnGameStateChangedEvent.Invoke();
        }
    }

}