using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static parallaxLayer;

public class backgroundManager : MonoBehaviour
{
    public List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();
    public backgroundSet[] backgrounds;

    public Sprite[] skyImages, cloudImages; //variety images to set bg elements to
    public List<GameObject> bgElements; //list of bg object prefabs to instantiate
    public List<GameObject> cloudObjs, bgObjs; //list of currently instantiated bg elements
    public GameObject skyObj; //ref to sky bg img obj
    public GameObject bgCanvas; //ref to bg canvas obj
    //public GameObject bgElementPrefab; //prefab to instantiate as bg elements
    public float cloudSpeed; //constant cloud speed set for each hole
    public RuntimeAnimatorController baseController; // animation controller that is used for each bg element

    public void SetSprites(int courseType)
    {
        //create random objs to move across bg
        //sky
        skyObj.GetComponent<Image>().sprite = skyImages[Random.Range(0, skyImages.Length)];
        //clouds
        cloudSpeed = Random.Range(1, 2) / 10000f;
        int cloudType = Random.Range(0, cloudImages.Length);
        for (int i = 0; i < 5; i++)
        {
            GameObject go = Instantiate(bgElements[0], this.transform); //just use any bg object for clouds
            go.GetComponent<SpriteRenderer>().sprite = cloudImages[cloudType];
            go.GetComponent<Animator>().enabled = false;
            go.transform.position = new Vector3(Random.Range(-10,10), Random.Range(2f,5f), 100);
            float size = Random.Range(1f, 1.5f);
            go.transform.localScale = new Vector3(size, size, Random.Range(0, 50)); //z scale is used as random speed mod per cloud
            cloudObjs.Add(go);
        }
        //misc bg elements
        float totalDistance = Random.Range(0f,3f);
        int courseLength = GameObject.Find("CourseManager").GetComponent<Course>().courseLayout.Count;
        //calculate total weight
        int totalWeight = 0;
        foreach(GameObject go in bgElements)
        {
            totalWeight += (int)go.GetComponent<backgroundElement>().spawnWeight;
        }
        //spawn new objs
        while (totalDistance < courseLength)
        {
            //caluclate random obj
            int randValue = Random.Range(0, totalWeight);
            GameObject newElement = null;
            foreach(GameObject randomGo in bgElements)
            {
                randValue -= (int)randomGo.GetComponent<backgroundElement>().spawnWeight;
                if (randValue < 0)
                {
                    newElement = randomGo;
                    break;
                }
            }
            //spawn it and init it
            GameObject go = Instantiate(newElement, GameObject.Find("CourseDisplay").transform);
            backgroundElement be = go.GetComponent<backgroundElement>();
            go.transform.localPosition = new Vector3(totalDistance, 1.25f + be.yBonus, 1 + be.distance);
            be.startX = go.transform.localPosition.x;
            float size = be.size + Random.Range(0f, be.sizeVariation);
            be.parrallaxFactor = 1f / be.size * be.distance;
            go.transform.localScale = new Vector3(size, size);
            totalDistance += Random.Range(0.25f,5f);
            if (be.sprite != null)
            {
                go.GetComponent<SpriteRenderer>().sprite = be.sprite;
                go.GetComponent<Animator>().enabled = false;
            }
            else
            {
                AnimationClip clip = be.animationClip;
                Animator animator = go.GetComponent<Animator>();
                AnimatorOverrideController overrideController = new(animator.runtimeAnimatorController);
                overrideController[overrideController.animationClips[0]] = clip;
                animator.runtimeAnimatorController = overrideController;
                animator.speed = Random.Range(0.5f, 1.25f);
                animator.Play("DefaultClip", 0, Random.Range(0f, 1f));
            }
            bgObjs.Add(go);
        }
    }

    public void Update()
    {
        float halfHeight = Camera.main.orthographicSize;
        float halfWidth = halfHeight * Camera.main.aspect;

        //move clouds
        foreach (GameObject go in cloudObjs)
        {
            float speedMod = go.transform.localScale.z / 100000f;
            go.transform.Translate(new Vector3(cloudSpeed + speedMod, 0));
            float spriteWidth = go.GetComponent<SpriteRenderer>().bounds.size.x;
            if (go.transform.position.x - spriteWidth > halfWidth)
            {
                //loop back to left side and change stats
                float newYPos = Random.Range(2f, 5f);
                go.transform.position = new Vector3(-halfWidth - spriteWidth, newYPos, 100);
                float size = Random.Range(1f, 1.5f);
                go.transform.localScale = new Vector3(size, size, Random.Range(0, 50)); //z scale is used as random speed mod per cloud
            }
        }
    }

    public void StorePositions()
    {
        //cache CourseDisplay start pos
        foreach (GameObject go in bgObjs)
        {
            if (go.GetComponent<backgroundElement>().courseStartX == 0)
                go.GetComponent<backgroundElement>().courseStartX = GameObject.Find("CourseDisplay").transform.position.x;
        }
    }

    public void UpdatePositions(float courseDelta)
    {
        //move trees
        foreach(GameObject go in bgObjs)
        {
            float parallaxX = GameObject.Find("CourseDisplay").transform.position.x - go.GetComponent<backgroundElement>().courseStartX;
            Vector3 pos = go.transform.localPosition;
            pos.x = go.GetComponent<backgroundElement>().startX - parallaxX * go.GetComponent<backgroundElement>().parrallaxFactor;
            go.transform.localPosition = pos;

        }
    }
}
