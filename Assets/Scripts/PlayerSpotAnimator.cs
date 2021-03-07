using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Domino42
{
    public class PlayerSpotAnimation
    {
        GameObject playerSpot;
        Vector2 startingPos;
        Vector2 destination;
        Quaternion rotation;
        public bool isFinished = false;

        float t;

        public PlayerSpotAnimation(GameObject ps, Vector2 pos)
        {
            playerSpot = ps;
            startingPos = ps.transform.localPosition;
            destination = pos;
            rotation = Quaternion.identity;
        }

        public PlayerSpotAnimation(GameObject ps, Vector2 pos, Quaternion rot)
        {
            playerSpot = ps;
            startingPos = ps.transform.localPosition;
            destination = pos;
            rotation = rot;
        }

        public bool Play()
        {
            isFinished = false;

            if (Vector2.Distance(playerSpot.transform.localPosition, destination) < Constants.PLAYER_SPOT_SNAP_DISTANCE)
            {
                playerSpot.transform.localPosition = destination;
                playerSpot.transform.rotation = rotation;
                isFinished = true;
            }
            else
            {
                t += Time.deltaTime / Constants.PLAYER_SPOT_TIME_TO_REACH_TARGET;
                //transform.position = Vector3.Lerp(startPosition, target, t);

                //playerSpot.transform.position = Vector2.MoveTowards(playerSpot.transform.position, destination, Constants.PLAYER_SPOT_MOVEMENT_SPEED * Time.deltaTime);
                playerSpot.transform.localPosition = Vector2.Lerp(startingPos, destination, t);
                playerSpot.transform.rotation = Quaternion.Lerp(playerSpot.transform.rotation, rotation, Constants.PLAYER_SPOT_ROTATION_SPEED * Time.deltaTime);
            }

            return isFinished;
        }
    }

    public class PlayerSpotAnimator : MonoBehaviour
    {
        List<PlayerSpotAnimation> playerSpotAnimations;
        PlayerSpotAnimation currentDominoAnimation;

        // invoked when all queued card animations have been played
        public UnityEvent OnAllAnimationsFinished = new UnityEvent();

        bool working = false;
        bool invokeOnAllAnimComplete = true;

        // Start is called before the first frame update
        void Awake()
        {
            playerSpotAnimations = new List<PlayerSpotAnimation>();
        }

        // Update is called once per frame
        void Update()
        {
            playerSpotAnimations.RemoveAll(ps => ps.isFinished);

            if (playerSpotAnimations.Count > 0)
            {
                playerSpotAnimations.ForEach(ps => ps.Play());
            }
            else
            {
                if (working)
                {
                    working = false;

                    if (invokeOnAllAnimComplete)
                    {
                        OnAllAnimationsFinished.Invoke();
                    }
                }
            }
        }
        
        public void AddPlayerSpotAnimation(GameObject playerSpot, Vector2 position, bool invokeOnAllAnimationComplete = true)
        {
            PlayerSpotAnimation ps = new PlayerSpotAnimation(playerSpot, position);
            playerSpotAnimations.Add(ps);
            working = true;
            invokeOnAllAnimComplete = invokeOnAllAnimationComplete;
        }

        public void AddPlayerSpotAnimation(GameObject playerSpot, Vector2 position, Quaternion rotation, bool invokeOnAllAnimationComplete = true)
        {
            PlayerSpotAnimation pa = new PlayerSpotAnimation(playerSpot, position, rotation);
            playerSpotAnimations.Add(pa);
            working = true;
            invokeOnAllAnimComplete = invokeOnAllAnimationComplete;
        }
    }

}