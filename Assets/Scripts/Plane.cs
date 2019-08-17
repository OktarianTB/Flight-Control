using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class Plane : MonoBehaviour
{

    public float speed = 1f;
    public Vector3 planeDestination; // the plane will head towards this location
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
        SetInitialRotation();

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

        ManageLineInput();
        Rotate();
    }

    private void ManageLineInput()
    {
        if (Input.GetMouseButtonUp(0) && isDrawingLine) // Stop drawing
        {
            isDrawingLine = false;
            resetLine = true;

            if (positions.Count > 1)
            {
                getNewDestination = true;
            }
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
            print("Error");
            getNewDestination = false;
            lineRenderer.positionCount = 0;
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

    private Vector3 GetInitialDestination() // After spawning, the plane will head to this direction until the player sets it's new trail
    {
        Vector3 planeToOrigin = new Vector3(-transform.position.x, -transform.position.y, 0f);
        float randomAngle = UnityEngine.Random.Range(-20, 20);
        planeToOrigin = Quaternion.Euler(0, 0, randomAngle) * planeToOrigin * 5f;
        return planeToOrigin;
    }

    private void SetInitialRotation()
    {
        Vector2 initialDirection = planeDestination - transform.position;
        GetAngle(initialDirection);
        Debug.DrawRay(transform.position, initialDirection, Color.red, 20f, true);
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

    private void OnTriggerEnter2D(Collider2D collisionObj)
    {
        if (collisionObj.GetComponent<Plane>())
        {
            print("GAME OVER");
        }
    }

    private void OnBecameInvisible()
    {
        resetLine = true;
        getNewDestination = false;
        lineRenderer.positionCount = 0;
        planeDestination = GetInitialDestination();
        GetAngle(planeDestination-transform.position);
    }

}
