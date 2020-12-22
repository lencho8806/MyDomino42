using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Domino42
{
    public class DominoAnimation
    {
        GameObject domino;
        Vector2 destination;
        Quaternion rotation;

        public DominoAnimation(GameObject d, Vector2 pos)
        {
            domino = d;
            destination = pos;
            rotation = Quaternion.identity;
        }

        public DominoAnimation(GameObject d, Vector2 pos, Quaternion rot)
        {
            domino = d;
            destination = pos;
            rotation = rot;
        }

        public bool Play()
        {
            bool finished = false;

            if (Vector2.Distance(domino.transform.position, destination) < Constants.DOMINO_SNAP_DISTANCE)
            {
                domino.transform.position = destination;
                domino.transform.rotation = rotation;
                finished = true;
            }
            else
            {
                domino.transform.position = Vector2.MoveTowards(domino.transform.position, destination, Constants.DOMINO_MOVEMENT_SPEED * Time.deltaTime);
                domino.transform.rotation = Quaternion.Lerp(domino.transform.rotation, rotation, Constants.DOMINO_ROTATION_SPEED * Time.deltaTime);
            }

            return finished;
        }
    }

    public class DominoAnimator : MonoBehaviour
    {
        public GameObject DominoPrefab;
        public List<GameObject> DisplayingDominos;

        Queue<DominoAnimation> dominoAnimations;
        DominoAnimation currentDominoAnimation;
        Vector2 startPosition = new Vector2(-5f, 1f);

        // invoked when all queued card animations have been played
        public UnityEvent OnAllAnimationsFinished = new UnityEvent();

        bool working = false;
        bool invokeOnAllAnimComplete = true;

        // Start is called before the first frame update
        void Awake()
        {
            dominoAnimations = new Queue<DominoAnimation>();
        }

        // Update is called once per frame
        void Update()
        {
            if (currentDominoAnimation == null)
            {
                NextAnimation();
            }
            else
            {
                if (currentDominoAnimation.Play())
                {
                    NextAnimation();
                }
            }
        }

        void NextAnimation()
        {
            currentDominoAnimation = null;

            if (dominoAnimations.Count > 0)
            {
                DominoAnimation ca = dominoAnimations.Dequeue();
                currentDominoAnimation = ca;
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

        public void AddDominoAnimation(GameObject domino, Vector2 position, bool invokeOnAllAnimationComplete = true)
        {
            DominoAnimation ca = new DominoAnimation(domino, position);
            dominoAnimations.Enqueue(ca);
            working = true;
            invokeOnAllAnimComplete = invokeOnAllAnimationComplete;
        }

        public void AddDominoAnimation(GameObject domino, Vector2 position, Quaternion rotation, bool invokeOnAllAnimationComplete = true)
        {
            DominoAnimation ca = new DominoAnimation(domino, position, rotation);
            dominoAnimations.Enqueue(ca);
            working = true;
            invokeOnAllAnimComplete = invokeOnAllAnimationComplete;
        }
    }

}