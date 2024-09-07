using System;
using UnityEngine;

public class enemy : MonoBehaviour
{
    public Vector3 position1;
    public Vector3 position2;
    public float speed;
    Vector3 goalPosition;
    Boolean waiting;

    void Start()
    {
        goalPosition = position2;
        waiting = false;
    }

    void Update()
    {
        if (!waiting)
        {
            if(transform.position == position2) { goalPosition = position1; waiting = true; Invoke("allow", 2f); }
            if(transform.position == position1) { goalPosition = position2; waiting = true; Invoke("allow", 2f); }
            transform.position = Vector3.MoveTowards(transform.position, goalPosition, speed);
        }
    }

    void allow(){
        waiting = false;
    }
}