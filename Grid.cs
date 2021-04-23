using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{

	public bool onlyDisplayPathGizmos;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	public Node[,] grid;
	public List<Node>edgeNodes;

	float nodeDiameter;
	public int gridSizeX, gridSizeY;

	void Start ()
	{
		edgeNodes = new List<Node>();
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
		CreateGrid();
	}

	public int MaxSize
	{
		get
		{
			return gridSizeX * gridSizeY;
		}
	}

	void CreateGrid ()
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));
				grid[x, y] = new Node(false, worldPoint, x, y);
				// Ehhh edges :(
                if (x == 0 || x == gridSizeX - 1 || y == 0 || y == gridSizeY-1)
                {
					edgeNodes.Add(grid[x, y]);
                }
			}
		}
	}

	public List<Node> GetNeighbours (Node node)
	{
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
				{
					neighbours.Add(grid[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}
	public List<Node> Get4Neighbours (Node node)
	{
		List<Node> neighbours = new List<Node>();
		//X
        if (node.gridX-1 >= 0)
        {
			neighbours.Add(grid[node.gridX - 1, node.gridY]);
		}
		if (node.gridX + 1 < gridSizeX)
		{
			neighbours.Add(grid[node.gridX + 1, node.gridY]);
		}
		//Y
		if (node.gridY - 1 >= 0)
		{
			neighbours.Add(grid[node.gridX, node.gridY -1]);
		}
		if (node.gridY + 1 < gridSizeY)
		{
			neighbours.Add(grid[node.gridX, node.gridY + 1]);
		}
		return neighbours;
	}

	public Node NodeFromWorldPoint (Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridWorldSize.x/2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y/2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX-1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY-1) * percentY);
		return grid[x, y];
	}

	public List<Node> path;
	void OnDrawGizmos ()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

		if (onlyDisplayPathGizmos)
		{
			if (path != null)
			{
				foreach (Node n in path)
				{
					Gizmos.color = Color.black;
					Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
				}
			}
		}
		else
		{
			if (grid != null)
			{
				foreach (Node n in grid)
				{
					Gizmos.color = (n.walkable) ? Color.white : Color.red;
					if (n.Edge) Gizmos.color = Color.blue;
					if (path != null)
						if (path.Contains(n))
							Gizmos.color = Color.black;
					Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
				}
			}
		}
	}
}