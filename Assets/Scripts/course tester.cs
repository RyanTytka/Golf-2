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
        GameObject.Find("Course").GetComponent<TextMeshProUGUI>().text = "Course " + course;
    }

    public void cycleCourseType()
    {
        CourseType = (CourseType + 1) % 4;
        GameObject.Find("CourseType").GetComponent<TextMeshProUGUI>().text = CourseType.ToString();
    }

    public void createHole()
    {
        Course c = GameObject.Find("CourseManager").GetComponent<Course>();
        c.courseType = (int)CourseType;
        c.courseNum--;
        c.holeNum = 0;
        c.NewCourse();
    }
}
