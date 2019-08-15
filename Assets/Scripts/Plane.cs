using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class Plane : MonoBehaviour
{

    public float speed = 1f;
    public Transform[] destinations;
    private Vector3 planeDestination; // the plane will head towards this location
    private Vector2 startVector = new Vector2(1, 1);
    private bool getNewDestination = false;
    private int pointsIndex;
    float targetRotation;

    LineRenderer lineRenderer;
    private List<Vector3> points;
    private List<Vector3> positions; // Used to access trajectory whilst original is modified - this one only resets when new line is created
    private float minDistance = 0.07f;
    bool isDrawingLine = false;
    bool resetLine = false;

    public enum PlaneColor
    {
        red,
        orange,
        yellow
    };
    public PlaneColor planeColor;

    void Start()
    {
        planeDestination = GetInitialDestination();

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

        if (Input.GetMouseButtonUp(0) && isDrawingLine) // Stop drawing
        {
            isDrawingLine = false;
            resetLine = true;

            //RayTrajectoryDebug();
        }
        else if (isDrawingLine)
        {
            DrawLine();
        }

        Rotate();
    }

    private void GetPlaneDirection()
    {
        try
        {
            if (pointsIndex < positions.Count - 1)
            {
                Vector2 newDestination = positions[pointsIndex];
                planeDestination = new Vector3(newDestination.x, newDestination.y, 0);

                if(pointsIndex > 0)
                {
                    points.Remove(points.First());
                    lineRenderer.positionCount = points.Count;
                    lineRenderer.SetPositions(points.ToArray());
                }
            }
            else
            {
                planeDestination = positions[positions.Count - 1] + (positions[positions.Count - 1] - positions[positions.Count - 2]) * 100f;
                GetAngle(positions[positions.Count - 1] - positions[positions.Count - 2]);
                getNewDestination = false;
                lineRenderer.positionCount = 0;
            }
            pointsIndex++;
            SetPlaneRotation();
        }
        catch (ArgumentOutOfRangeException)
        {
            planeDestination = GetInitialDestination();
        }
    }

    private void SetPlaneRotation()
    {
        if(pointsIndex + 1 < positions.Count)
        {
            Vector2 newDirection = new Vector2(positions[pointsIndex+1].x - positions[pointsIndex].x, positions[pointsIndex+1].y - positions[pointsIndex].y);
            GetAngle(newDirection);

            //Debug.DrawRay(pointsCopy[pointsIndex], newDirection * 2, Color.red, 20f, true);
            //Debug.DrawRay(pointsCopy[pointsIndex], startVector, Color.blue, 20f, true);
        }

    }

    private void RayTrajectoryDebug()
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 newDirection = points[i + 1] - points[i];
            Debug.DrawRay(points[i], newDirection * 5, Color.blue, 20f, true);
        }
    }

    private Vector3 GetInitialDestination() // After spawning, the plane will head to this direction until the player sets it's new trail
    {
        Transform random_destination = destinations[UnityEngine.Random.Range(0, destinations.Length)];
        Vector3 dest = new Vector3(random_destination.position.x, random_destination.position.y, 0);
        return dest;
    }

    private IEnumerator SetInitialRotation() // Calculate the initial rotation of the plane
    {
        yield return new WaitForEndOfFrame();

        Vector2 destinationVector = new Vector2(planeDestination.x - transform.position.x, planeDestination.y - transform.position.y);
        GetAngle(destinationVector);
        Debug.DrawRay(transform.position, destinationVector, Color.red, 20f, true);
    }

    private void GetAngle(Vector2 directionVector)
    {
        float angle = Vector2.Angle(startVector, directionVector);

        // Check what direction to turn:
        if (directionVector.x > directionVector.y)
        {
            angle *= -1f;
        }

        targetRotation = angle;
    }

    private void Rotate()
    {
        float currentRotation = UnityEditor.TransformUtils.GetInspectorRotation(transform).z;

        if (Mathf.Abs(currentRotation - targetRotation) > 100f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, targetRotation);
        }
        else
        {
            if(currentRotation >= 0)
                    {
                        if(targetRotation >= 0)
                        {
                            if(currentRotation > targetRotation)
                            {
                                transform.rotation = Quaternion.Euler(0f, 0f, currentRotation - 0.5f);
                            }
                            else
                            {
                                transform.rotation = Quaternion.Euler(0f, 0f, currentRotation + 0.5f);
                            }
                        }
                        else
                        {
                            transform.rotation = Quaternion.Euler(0f, 0f, currentRotation - 0.5f);
                        }
                    }
                    else
                    {
                        if(targetRotation >= 0)
                        {
                            transform.rotation = Quaternion.Euler(0f, 0f, currentRotation + 0.5f);
                        }
                        else
                        {
                            if(currentRotation > targetRotation)
                            {
                                transform.rotation = Quaternion.Euler(0f, 0f, currentRotation - 0.5f);
                            }
                            else
                            {
                                transform.rotation = Quaternion.Euler(0f, 0f, currentRotation + 0.5f);
                            }
                        }
                    }
        }

        


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
            positions = null;
            lineRenderer.positionCount = 0;
            resetLine = false;
        }

        if (points == null)
        {
            points = new List<Vector3>();
            positions = new List<Vector3>();
            SetPoint(mousePosition);
            return;
        }

        if (positions.Count == 2)
        {
            pointsIndex = 0;
            getNewDestination = true;
        }

        float distance = Vector2.Distance(points.Last(), mousePosition);

        if(distance > 0.5f)
        {
            Vector2 midPoint = new Vector2((mousePosition.x + points.Last().x) / 2, (mousePosition.y + points.Last().y) / 2);
            SetPoint(midPoint);
        }

        if (distance > minDistance)
        {
            SetPoint(mousePosition);
        }

    }

    private void SetPoint(Vector2 point)
    {
        points.Add(point);
        positions.Add(point);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPosition(points.Count - 1, point);
    }

}
