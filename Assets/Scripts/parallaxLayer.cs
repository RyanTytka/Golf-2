using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parallaxLayer : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layer;
        [Range(0f, 1f)]
        public float parallaxFactor = 0.3f;

        [HideInInspector]
        public float startX; // cached when dragging starts
    }
}
