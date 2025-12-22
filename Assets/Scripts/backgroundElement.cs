using UnityEngine;

public class backgroundElement : MonoBehaviour
{
    public Sprite sprite; 
    public AnimationClip animationClip;
    //Either uses a sprite or clip. If one is null, use the other one
    public float size; //scale of obj
    public float distance; //higher distance = less parallax movement behind closer objects
    //range from 0 - size. Closer to size means less movement. Closer to 0 means close to camera
    public float sizeVariation;
    public float yBonus; //moves the y pos up for this element
    public float spawnWeight; //higher = more likely to spawn
    public float parrallaxFactor { get; set; }
    public float startX { get; set; }
    public float courseStartX { get; set; }
}