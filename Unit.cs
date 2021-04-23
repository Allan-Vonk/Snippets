using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public float speed = 1;
    public float checkRadius = .5f;
    public Queue<Vector3> path;
    public bool pathState;
    public Vector3 nextTarget;
    public Vector3 Target
    {
        get { return target; }
        set { SetTarget(value); }
    }
    private Vector3 target;
    private Pathfinding pathfinding;
    private void Start ()
    {
        pathfinding = FindObjectOfType<Pathfinding>();
    }
    public void SetTarget (Vector3 value)
    {
        target = value;
        if (target != Vector3.zero)
        {
            path = pathfinding.FindPath(transform.position, target);
        }
    }
    private void Update ()
    {
        if (path != null && path.Count >0)
        {
            if (Vector3.Distance(transform.position, path.Peek()) < checkRadius)
            {
                path.Dequeue();
            }
        }
    }
    private void FixedUpdate ()
    {
        if (path != null && path.Count > 0)
        {
            if (Vector3.Distance(transform.position, path.Peek()) > checkRadius) MoveToNextPosition();
        }
        else if (path != null)
        {
            StopPath();
        }
    }
    private void MoveToNextPosition ()
    {
        Vector3 dir =  (path.Peek() - transform.position).normalized;
        dir.y = 0;
        transform.position += dir * speed;
    }
    public void StopPath ()
    {
        path = null;
        target = Vector3.zero;
    }
    private void OnDrawGizmos ()
    {
        if (path != null&&path.Count > 0)
        {
            foreach (Vector3 vector3 in path)
            {
                Gizmos.color = (vector3 == path.Peek()) ? Color.blue : Color.red;
                Gizmos.DrawWireSphere(vector3 + new Vector3(0,1,0),.1f);
            }
        }
    }
}