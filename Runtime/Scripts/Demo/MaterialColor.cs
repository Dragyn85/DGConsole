using System.Collections;
using System.Collections.Generic;
using DragynGames.Commands;
using UnityEngine;

namespace DragynGames
{
    public class MaterialColor : MonoBehaviour
    {
        //A method for getting this objects material anc change its color to a new random color
        [ConsoleAction("ColorSwap", "Changes the color of the material on this gameobject")]
        public void ChangeColor()
        {
            //Get the material of this object
            Material material = GetComponentInChildren<Renderer>().material;

            //Create a new random color
            Color newColor = new Color(Random.value, Random.value, Random.value);

            //Set the material color to the new random color
            material.color = newColor;
        }
    }
}
