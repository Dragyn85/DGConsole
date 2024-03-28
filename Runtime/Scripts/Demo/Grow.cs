using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragynGames.Commands.Demo
{
    public class Grow : MonoBehaviour
    {
        public void GrowObject()
        {
            //increase scale of object
            transform.localScale += new Vector3(0.5f, 0.5f, 0.5f);
        }
    }
}
