using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static parallaxLayer;

public class backgroundManager : MonoBehaviour
{
    public List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();
    public backgroundSet[] backgrounds;

    public Sprite[] skyImages, cloudImages; //variety images to set bg elements to
    public AnimationClip[] trees;
    public List<GameObject> cloudObjs, treeObjs; //list of currently instantiated bg elements
    public GameObject skyObj; //ref to sky bg img obj
    public GameObject bgCanvas; //ref to bg canvas obj
    public GameObject bgElementPrefab; //prefab to instantiate as bg elements
    public float cloudSpeed; //constant cloud speed set for each hole

    public void SetSprites(int courseType)
    {
        //Set background
        //foreach (ParallaxLayer pl in parallaxLayers)
        //{
        //    pl.layer.gameObject.SetActive(false);
        //}
        //int bgNum = courseType * 3 + Random.Range(0, 3); //0-2. 3 variants for each course type
        //int index = 0;
        //foreach (Sprite sprite in backgrounds[bgNum].layers)
        //{
        //    //bgParent.GetComponentsInChildren<>
        //    parallaxLayers[index].layer.gameObject.SetActive(true);
        //    parallaxLayers[index].layer.gameObject.GetComponent<Image>().sprite = sprite;
        //    parallaxLayers[index].parallaxFactor = backgrounds[bgNum].moveRatios[index];
        //    index++;
        //}


        //v2: create random objs to move across bg
        //sky
        skyObj.GetComponent<Image>().sprite = skyImages[Random.Range(0, skyImages.Length)];
        //clouds
        //cloudSpeed = Random.Range(1,2);
        //for (int i = 0; i < 5; i++)
        //{
        //    GameObject go = Instantiate(bgElementPrefab, bgCanvas.transform);
        //    go.GetComponent<Image>().sprite = cloudImages[Random.Range(0, cloudImages.Length)];
        //    cloudObjs.Add(go); 
        //}
        //trees
        for (int i = 0; i < 5; i++)
        {
            GameObject go = Instantiate(bgElementPrefab, bgCanvas.transform);
            go.GetComponent<Animator>().anima = trees[Random.Range(0, trees.Length)];
            go.GetComponent<Animator>().Play();
            treeObjs.Add(go);
        }
    }

    public void Update()
    {
        //move clouds
        foreach(GameObject go in cloudObjs)
        {
            go.GetComponent<RectTransform>().localPosition = 
                new Vector3(go.GetComponent<RectTransform>().localPosition.y, go.GetComponent<RectTransform>().localPosition.x + cloudSpeed);
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
