using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Grid),typeof(Pathfinding),typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class MazeGeneration : MonoBehaviour
{
    #region Variables
    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject doorPrefab;
    public GameObject pickupPrefab;
    [Header("PickupConfig")]
    public int pickups = 5;
    public Vector3 pickupSpawnOffset;
    [Header("Difficulty")]
    public int pathDistanceTreshold = 40;
    [Header("WallPlacement")]
    public Vector3 wallOffset;
    [Header("References")]
    public GameObject Player;

    //Privates
    private List<Node>Cellset;
    private Node EndNode;
    private Pathfinding pf;
    //Usefull references
    private List<GameObject>Walls;
    //Mesh
    private MeshFilter mf;
    private MeshCollider mc;
    //Grid
    private Grid grid;
    private Node[,] nodeGrid;
    #endregion Variables
    private void Start ()
    {
        #region variableInstantiating
        //References
        mc = GetComponent<MeshCollider>();
        mf = GetComponent<MeshFilter>();
        pf = GetComponent<Pathfinding>();
        grid = GetComponent<Grid>();
        nodeGrid = grid.grid;
        //Lists
        Walls = new List<GameObject>();
        #endregion
        initializeMazeConstruction();
    }
    private void initializeMazeConstruction ()
    {
        GenerateMaze();
        GenerateExit();
        InstantiateWalls();
        MergeWalls();
        GeneratePickups();
    }
    #region GenerateMaze
    /// <summary>
    /// Generates the maze using Prims algorithme
    /// </summary>
    private void GenerateMaze ()
    {
        Node startnode = nodeGrid[1,1];
        ImplementPrims(startnode);
    }
    private void ImplementPrims (Node startNode)
    {
        Cellset = new List<Node>();
        Cellset.Add(startNode);

        while (Cellset.Count > 0)
        {
            Node randomNode = Cellset[Random.Range(0,Cellset.Count)];
            List<Node> neighbours = grid.Get4Neighbours(randomNode);
            int walkableNeighbours = GetAmountOfWalkableNeighbours(neighbours);
            if (walkableNeighbours < 2)
            {
                randomNode.walkable = true;
                List<Node>UnvisitedNeighbours = GetUnvisitedNeighbours(neighbours);
                UnvisitedNeighbours = setNodesToVisited(UnvisitedNeighbours);
                Cellset.AddRange(UnvisitedNeighbours);
            }
            bool isRandomNodeWalkable = CheckIfNodeIsEdgeNode(randomNode);
            if (isRandomNodeWalkable == true) randomNode.walkable = false;
            Cellset.Remove(randomNode);
        }
    }
    private int GetAmountOfWalkableNeighbours (List<Node> neighbours)
    {
        int count = 0;
        foreach (Node node in neighbours)
        {
            if (node.walkable == true) count++;
        }
        return count;
    }
    private List<Node> GetUnvisitedNeighbours (List<Node> neighbours)
    {
        List<Node>unvisitedNeighbours = new List<Node>();
        foreach (Node node in neighbours)
        {
            if (node.visited == false) unvisitedNeighbours.Add(node);
        }
        return unvisitedNeighbours;
    }
    private bool CheckIfNodeIsEdgeNode (Node node)
    {
        if (node.gridX == 0 || node.gridX == grid.gridSizeX - 1 || node.gridY == 0 || node.gridY == grid.gridSizeY - 1)
        {
            return true;
        }
        else return false;
    }
    private List<Node> setNodesToVisited (List<Node>Nodes)
    {
        List<Node>unvisitedNodes = new List<Node>();
        foreach (Node node in Nodes)
        {
            node.visited = true;
            unvisitedNodes.Add(node);
        }
        return unvisitedNodes;
    }
    #endregion
    #region GenerateExit
    /// <summary>
    /// Looks where the exit must be and spawns all necesary doors
    /// </summary>
    private void GenerateExit ()
    {
        Node EndNode = GetFarthestNode();
        ClearEnd(EndNode);
        InstantiateDoors();
    }
    private Node GetFarthestNode ()
    {
        List<Node> PossibleExits = GetPossibleExits();
        if (PossibleExits.Count <= 0) SceneManager.LoadScene(1);
        List<Queue<Vector3>>PathList = GetListOfPathsToPossibleExits(PossibleExits);
        List<Vector3>longestPath = GetLongestPath(PathList).ToList();
        if (longestPath.Count < pathDistanceTreshold) SceneManager.LoadScene(1);
        EndNode = grid.NodeFromWorldPoint(longestPath[longestPath.Count - 1]);
        return EndNode;
    }
    private List<Node> GetPossibleExits ()
    {
        List<Node>possibleExits = new List<Node>();
        for (int x = 0; x < grid.gridSizeX; x++)
        {
            for (int y = 0; y < grid.gridSizeY; y++)
            {
                Node node = nodeGrid[x,y];
                if (node.gridX == 1 && node.gridY > 0 || node.gridX == grid.gridSizeX - 2 && node.gridY < grid.gridSizeY - 2 || node.gridY == 1 && node.gridX > 0 || node.gridY == grid.gridSizeY - 2 && node.gridX < grid.gridSizeX - 2)
                {
                    possibleExits.Add(node);
                }
            }
        }
        return possibleExits;
    }
    private List<Queue<Vector3>> GetListOfPathsToPossibleExits (List<Node> possibleExits)
    {
        List<Queue<Vector3>>paths = new List<Queue<Vector3>>();
        foreach (var item in possibleExits)
        {
            Queue<Vector3>Path = pf.FindPath(nodeGrid[1, 1].worldPosition, item.worldPosition);
            if (Path != null)
            {
                paths.Add(Path);
            }
        }
        return paths;
    }
    private Queue<Vector3> GetLongestPath (List<Queue<Vector3>>paths) 
    {
        paths = paths.OrderBy(f => f.Count).ToList();
        return paths[paths.Count - 1];
    }
    /// <summary>
    /// Clears the end of the maze of walls
    /// </summary>
    /// <param name="node"></param>
    private void ClearEnd (Node node)
    {
        List<Node>neigbours = grid.Get4Neighbours(node);
        foreach (Node node1 in neigbours)
        {
            node1.walkable = true;
        }
    }
    private void InstantiateDoors ()
    {
        foreach (Node node in grid.edgeNodes)
        {
            if (node.walkable == true)
            {
                BuildWallPiece(doorPrefab, node);
            }
        }
    }
    #endregion
    #region GenerateWalls
    /// <summary>
    /// Spawns wall prefabs according to the nodegrid
    /// </summary>
    private void InstantiateWalls ()
    {
        foreach (Node node in nodeGrid)
        {
            if (node.walkable == false)
            {
                GameObject Wall = BuildWallPiece(wallPrefab,node);
                Walls.Add(Wall);
            }
        }
    }
    private GameObject BuildWallPiece (GameObject prefab,Node node)
    {
        GameObject wall = Instantiate(prefab,gameObject.transform);
        wall.transform.position = node.worldPosition + wallOffset;
        return wall;
    }
    #endregion
    #region MergeWalls
    /// <summary>
    /// Comibines all the meshes from the gameobjects in "Walls" into one mesh
    /// </summary>
    private void MergeWalls ()
    {
        List<CombineInstance> combineInstances;
        combineInstances = BuildCombineInstances(Walls);
        CombineInstance[] combineInstanceArray = combineInstances.ToArray();
        mf.mesh = BuildMesh(combineInstanceArray);
        mc.sharedMesh = mf.mesh;
        DisableWalls(Walls);
    }
    private List<CombineInstance> BuildCombineInstances (List<GameObject>walls)
    {
        List<CombineInstance>buildedCombineInstances = new List<CombineInstance>();
        foreach (GameObject Wall in walls)
        {
            MeshFilter mf = Wall.GetComponent<MeshFilter>();
            CombineInstance ci = new CombineInstance
            {
                mesh = mf.sharedMesh,
                transform = mf.transform.localToWorldMatrix
            };
            buildedCombineInstances.Add(ci);
        }
        return buildedCombineInstances;
    }
    private void DisableWalls (List<GameObject> Walls)
    {
        foreach (GameObject Wall in Walls)
        {
            Wall.SetActive(false);
        }
    }
    private Mesh BuildMesh (CombineInstance[] CombineInstances)
    {
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(CombineInstances);
        RecalculateMesh(mesh);
        return mesh;
    }
    private Mesh RecalculateMesh(Mesh mesh)
    {
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        return mesh;
    }
    #endregion
    #region GeneratePickups

    /// <summary>
    /// Spawns pickups randomly across the map
    /// </summary>
    private void GeneratePickups ()
    {
        List<Node>PickupNodes = GatherPossiblePickupPoints(Player.transform.position);
        for (int i = 0; i < pickups; i++)
        {
            Instantiate(pickupPrefab, PickupNodes[Random.Range(0, PickupNodes.Count)].worldPosition + pickupSpawnOffset,Quaternion.identity);
        }
    }
    /// <summary>
    /// Gathers positions where pickups can be spawned
    /// </summary>
    /// <param name="StartPos"></param>
    /// <returns></returns>
    private List<Node> GatherPossiblePickupPoints (Vector3 StartPos)
    {
        List<Node> spawnableNodes = new List<Node>();
        foreach (Node node in grid.grid)
        {
            bool nodeIsReachable = checkIfNodeIsReachable(node, StartPos);
            if (nodeIsReachable == true) spawnableNodes.Add(node);
        }
        return spawnableNodes;
    }
    private bool checkIfNodeIsReachable (Node node, Vector3 playerPos)
    {
        if (node.walkable && pf.FindPath(playerPos, node.worldPosition) != null) return true;
        else return false;
    }
    #endregion
}