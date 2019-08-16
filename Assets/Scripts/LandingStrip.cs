using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LandingStrip : MonoBehaviour
{

    public Vector2 enterPos, exitPos;
    private List<GameObject> planes;

    public enum StripColor
    {
        red,
        orange,
        yellow
    };
    public StripColor stripColor;


    void Start()
    {
        planes = new List<GameObject>();

        if (enterPos == null || exitPos == null)
        {
            Debug.LogError("Enter/Exit positions have not been defined in the inspector");
        }
    }
    
    void Update()
    {
        CheckIfArrived();
    }

    private void CheckIfArrived()
    {
        if(planes == null)
        {
            return;
        }

        foreach(GameObject plane in planes)
        {
            Vector3 objPos = plane.transform.position;
            float distance = Vector2.Distance(new Vector2(objPos.x, objPos.y), exitPos);

            if(distance < 1.4f)
            {
                planes.Remove(plane);
                StartCoroutine(LandPlane(plane));
                return;
            }
        }
    }

    IEnumerator LandPlane(GameObject plane)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(plane);
    }

    private void OnTriggerEnter2D(Collider2D plane)
    {
        try
        {
            if(plane.GetComponent<Plane>().planeColor.ToString() != stripColor.ToString())
            {
                return;
            }
        }
        catch (MissingComponentException)
        {
            return;
        }

        Vector3 objPos = plane.transform.position;
        float distance = Vector2.Distance(new Vector2(objPos.x, objPos.y), enterPos);

        if(distance < 1.6f)
        {
            planes.Add(plane.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D plane)
    {
        if (!plane.GetComponent<Plane>() || planes == null)
        {
            return;
        }

        try
        {
            planes.Remove(plane.gameObject);
        }
        catch (ArgumentOutOfRangeException)
        {
            return;
        }
    }

}
