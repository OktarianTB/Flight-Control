﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class Plane : MonoBehaviour
{

    public float speed = 0.005f;
    public Transform[] destinations;
    private Vector3 planeDestination;
    private Vector2 startVector;
    private bool getNewDestination = false;
    private int pointsIndex;

    LineRenderer lineRenderer;
    private List<Vector3> points;
    private List<Vector3> pointsCopy; // Used to access trajectory whilst original is modified
    private float minDistance = 0.07f;
    bool isDrawingLine = false;
    bool resetLine = false;

    void Start()
    {
        planeDestination = GetInitialDestination();
        startVector = new Vector2(1, 1);

        StartCoroutine(SetInitialRotation());

        try
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        catch (MissingComponentException)
        {
            Debug.LogWarning("Component is missing from Plane");
        }
    }
    
    void Update()
    {
        if (getNewDestination && (Vector3.Distance(transform.position, planeDestination) <= 0.005f || pointsIndex == 0))
        {
            GetPlaneDirection();
        }

        transform.position = Vector3.MoveTowards(transform.position, planeDestination, speed * Time.deltaTime);

        if(Input.GetMouseButtonUp(0) && isDrawingLine) // Stop drawing
        {
            isDrawingLine = false;
            resetLine = true;

            RayTrajectoryDebug();
        }
        else if (isDrawingLine)
        {
            DrawLine();
        }
    }

    private void GetPlaneDirection()
    {
        try
        {
            if(pointsIndex < pointsCopy.Count - 1)
            {
                Vector2 newDestination = pointsCopy[pointsIndex];
                planeDestination = new Vector3(newDestination.x, newDestination.y, 0);

            }
            else
            {
                Vector2 newDestination = pointsCopy[pointsCopy.Count -1] + (pointsCopy[pointsCopy.Count-1] - pointsCopy[pointsCopy.Count-2])*100f;
                planeDestination = new Vector3(newDestination.x, newDestination.y, 0);
                getNewDestination = false;
            }
            pointsIndex++;
        }
        catch (ArgumentOutOfRangeException)
        {
            planeDestination = GetInitialDestination();
        }
    }

    private void RayTrajectoryDebug()
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 newDirection = points[i + 1] - points[i];
            Debug.DrawRay(points[i], newDirection*5, Color.blue, 2000f, true);
        }
    }

    private Vector3 GetInitialDestination() // After spawning, the plane will head to this direction until the player sets it's new trail
    {
        Transform random_destination = destinations[UnityEngine.Random.Range(0, destinations.Length)];
        Vector3 dest = new Vector3(random_destination.position.x, random_destination.position.y, 0);
        print(dest);
        return dest;
    }

    private IEnumerator SetInitialRotation() // Using scalar product formulas to calculate the rotation of the plane
    {
        yield return new WaitForEndOfFrame();

        Vector2 destinationVector = new Vector2(planeDestination.x - transform.position.x, planeDestination.y - transform.position.y);

        float x = Vector2.Dot(startVector, destinationVector) / (destinationVector.magnitude * startVector.magnitude);
        float angleToTurn = Mathf.Rad2Deg * Mathf.Acos(x);

        // Check what direction to turn:
        float a = planeDestination.x + transform.position.y - transform.position.x;
        if(a > planeDestination.y)
        {
            angleToTurn *= -1f;
        } 

        print(angleToTurn);

        transform.Rotate(0f, 0f, angleToTurn, Space.Self);

        Debug.DrawRay(transform.position, destinationVector, Color.red, 2000f, true);
    }

    private void OnMouseDown()
    {
        isDrawingLine = true;
    }

    private void DrawLine()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (resetLine)
        {
            points = null;
            pointsCopy = null;
            lineRenderer.positionCount = 0;
            resetLine = false;
        }

        if (points == null)
        {
            points = new List<Vector3>();
            pointsCopy = new List<Vector3>();
            SetPoint(mousePosition);
            return;
        }

        if(points.Count == 2)
        {
            getNewDestination = true;
            pointsIndex = 0;
        }

        float distance = Vector2.Distance(points.Last(), mousePosition);
        if (distance > minDistance)
        {
            SetPoint(mousePosition);
        }

    }

    private void SetPoint(Vector2 point)
    {
        points.Add(point);
        pointsCopy.Add(point);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(points.Count - 1, point);
    }

}