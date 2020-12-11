using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Domino42
{
    public class Selectable : MonoBehaviour
    {
        public bool faceUp = false;
        public string name;
        public int high;
        public int low;
        public int sum;
        public bool isDoubles;

        // Start is called before the first frame update
        void Start()
        {
            if (CompareTag("Domino"))
            {
                name = transform.name;

                var splitVal = name.Split('_');

                int leftVal;
                int rightVal;
                int.TryParse(splitVal[0], out leftVal);
                int.TryParse(splitVal[1], out rightVal);

                if (leftVal > rightVal)
                {
                    high = leftVal;
                    low = rightVal;
                }
                else
                {
                    high = rightVal;
                    low = leftVal;
                }

                sum = leftVal + rightVal;

                isDoubles = high == low;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}