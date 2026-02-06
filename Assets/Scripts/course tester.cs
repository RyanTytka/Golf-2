using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class coursetester : MonoBehaviour
{
    public Course.CourseType CourseType;
    public int par = 4;
    public int course = 1;

    public void modifyPar(int mod)
    {
        par += mod;
        GameObject.Find("Par").GetComponent<TextMeshProUGUI>().text = "Par " + par;
    }

    public void modifyCourse(int mod)
    {
        course += mod;
        GameObject.Find("CourseManager").GetComponent<Course>().courseNum = course;
        GameObject.Find("Course").GetComponent<TextMeshProUGUI>().text = "Course " + course;
    }

    public void cycleCourseType()
    {
        CourseType = (Course.CourseType)(((int)CourseType + 1) % 5);
        GameObject.Find("CourseType").GetComponent<TextMeshProUGUI>().text = CourseType.ToString();
    }

    public void createHole()
    {
        Course c = GameObject.Find("CourseManager").GetComponent<Course>();
        //clear any existing hole first
        foreach (GameObject go in c.courseLayout)
        {
            Destroy(go);
        }
        c.courseLayout.Clear();
        //new hole
        c.test_par = par;
        c.nextCourse = (int)CourseType;
        c.courseNum--;
        c.holeNum = 0;
        c.NewCourse();
        //update display
        GameObject.Find("Total Distance").GetComponent<TextMeshProUGUI>().text = "Course Length: " + c.courseLayout.Count;
        GameObject.Find("To Pin").GetComponent<TextMeshProUGUI>().text = "To Pin: " + c.courseLayout.FindIndex((go) =>
        {
            //Debug.Log(go);
            return go.GetComponent<CoursePiece>().myType == (int)Course.CoursePieces.HOLE;
        });
    }
}
