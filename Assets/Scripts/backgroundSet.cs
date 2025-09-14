using UnityEngine;

[CreateAssetMenu(fileName = "NewBackgroundSet", menuName = "Background/Background Set")]
public class backgroundSet : ScriptableObject
{
    public Sprite[] layers; // Up to 5 layers
    public float[] moveRatios; //0-1. 1 = max movement
}