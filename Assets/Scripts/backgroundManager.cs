using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static parallaxLayer;

public class backgroundManager : MonoBehaviour
{
    public List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();
    public backgroundSet[] backgrounds;

    public void SetSprites(int courseType)
    {
        //Set background
        foreach (ParallaxLayer pl in parallaxLayers)
        {
            pl.layer.gameObject.SetActive(false);
        }
        int bgNum = courseType * 3 + Random.Range(0, 3); //0-2. 3 variants for each course type
        int index = 0;
        foreach (Sprite sprite in backgrounds[bgNum].layers)
        {
            //bgParent.GetComponentsInChildren<>
            parallaxLayers[index].layer.gameObject.SetActive(true);
            parallaxLayers[index].layer.gameObject.GetComponent<Image>().sprite = sprite;
            parallaxLayers[index].parallaxFactor = backgrounds[bgNum].moveRatios[index];
            index++;
        }
    }

    public void StorePositions()
    {
        // Cache each parallax layer's start position
        foreach (var layer in parallaxLayers)
        {
            if (layer.layer != null)
                layer.startX = layer.layer.localPosition.x;
        }
    }

    public void UpdatePositions(float courseDelta)
    {
        //move them
        foreach (var layer in parallaxLayers)
        {
            if (layer.layer != null)
            {
                float parallaxX = layer.startX + courseDelta * layer.parallaxFactor * 3;
                Vector3 pos = layer.layer.localPosition;
                pos.x = parallaxX;
                layer.layer.localPosition = pos;
            }
        }
    }
}
