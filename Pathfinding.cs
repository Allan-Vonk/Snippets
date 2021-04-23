using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour
{
    Grid grid;
    public Transform seeker, target;
    private void Awake ()
    {
        grid = GetComponent<Grid>();
    }
    public Queue<Vector3> FindPath (Vector3 startPos, Vector3 targetPos)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);
        while (openSet.Count >0)
        {
            Node currentNode = openSet.RemoveFirst();
            closedSet.Add(currentNode);
            if (currentNode == targetNode)
            {
                sw.Stop();
                return PathToQueue(RetracePath(startNode, targetNode));
            }
            foreach (Node neigbour in grid.Get4Neighbours(currentNode))
            {
                if (!neigbour.walkable || closedSet.Contains(neigbour))
                {
                    continue;
                }
                int newMovementCostToNeigbour = currentNode.gCost + GetDistace(currentNode, neigbour);
                if (newMovementCostToNeigbour < neigbour.gCost || !openSet.Contains(neigbour))
                {
                    neigbour.gCost = newMovementCostToNeigbour;
                    neigbour.hCost = GetDistace(neigbour, targetNode);
                    neigbour.parent = currentNode;
                    if (!openSet.Contains(neigbour))
                    {
                        openSet.Add(neigbour);
                    }
                }
            }
        }
        return null;
    }
    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node>path = new List<Node>();
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        grid.path = path;
        return path;
    }
    Queue<Vector3> PathToQueue (List<Node>Path)
    {
        Queue<Vector3>Vector3Path = new Queue<Vector3>();
        
        foreach (Node node in Path)
        {
            Vector3Path.Enqueue(node.worldPosition);
        }
        return Vector3Path;
    }
    int GetDistace(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX>distY)
        {
            return 14 * distY + 10 * (distX - distY);
        }
        else
        {
            return 14 * distX + 10 * (distY - distX);
        }
    }
}