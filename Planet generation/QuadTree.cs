using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class QuadTree
{
    //Variables
    public List<Point3>Points = new List<Point3>();
    public bool divided = false;
    public List<QuadTree>Children = new List<QuadTree>();
    public MarchingChunk marchingChunk;
    //Privates
    private GameObject ColliderObject;
    private GameObject Player;
    private MarchingCubeContext MarchingContext;
    private QuadTreeStarter Root;
    private Cube Boundary;
    private Mesh mesh = new Mesh();
    private Material material;
    private bool ColliderState = true;
    private int lod;

    public QuadTree (Cube Boundary, QuadTreeStarter Root, Material material, int lod, MarchingCubeContext Context)
    {
        //Setting context
        MarchingContext = Context;
        //Setting other variables
        this.material = material;
        this.Boundary = Boundary;
        this.lod = lod + 1;
        this.Root = Root;
        //For gizmos
        this.Root.Boundaries.Add(Boundary);
    }
    //Update being called from a monobehavior
    public void CalledUpdate ()
    {
        //Check if chunk should still be alive or not
        if (ColliderObject && ColliderState != ColliderObject.activeSelf)
        {
            //Kill the chunk
            ColliderObject.SetActive(ColliderState);
            Root.KillChunk(ColliderObject);
        }
        //Get the player
        if (!Player)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        //Manage if the chunk should divide or undivide
        Vector3 pos = Player.transform.position;
        float distance = Vector3.Distance(pos,Boundary.position);
        if (distance < Boundary.size.x)
        {
            if (lod < MarchingContext.MaxLod)
            {
                if (divided == false)
                {
                    SubDivide();
                }
            }
        }
        else
        {
            if (divided == true)
            {
                UnDivide();
                return;
            }
        }

        //Draw mesh
        if (divided == false && marchingChunk != null && mesh != null)
        {
            Graphics.DrawMesh(mesh, MarchingContext.CentreOfPlanet, Quaternion.identity,material,0);
        }
    }
    //Generate mesh & meshCollider
    public async void CreateChunk ()
    {
        if (divided == true)
        {
            return;
        }
        //Starting a asynchronous task to generate mesh data
        var result = await Task.Run(()=>
        {
            return new MarchingChunk(MarchingContext,Boundary);
        });
        //Making mesh here instead of in the MarchingChunk for easier acces and because its not possible to create a new mesh on another threat
        marchingChunk = result;
        mesh.vertices = marchingChunk.m_vertices.ToArray();
        mesh.triangles = marchingChunk.m_triangles.ToArray();
        mesh.RecalculateNormals();
        marchingChunk.mesh = mesh;

        GenerateCollider();
    }
    //Undivide the Quadtree
    public void GenerateCollider ()
    {
        if (mesh != null && mesh.vertexCount > 0)
        {
            ColliderObject = new GameObject();
            MeshCollider collider = ColliderObject.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }
    }
    //Get the leaves in the quad tree 
    public void UnDivide ()
    {
        Children.Clear();
        divided = false;
        ColliderState = true;
    }
    //Devides the Quadtree into 8 more Quads
    public void SubDivide ()
    {
        if (divided == false)
        {
            //Setting variables for easy acces
            float x = Boundary.position.x;
            float y = Boundary.position.y;
            float z = Boundary.position.z;
            float w = Boundary.size.x;
            float h = Boundary.size.y;
            float l = Boundary.size.z;
            //Creating boundaries
            Cube NE1 = new Cube(new Vector3(x + (w/4),y + (h/4),z + (l/4)), new Vector3(w/2,h/2,l/2));
            Cube SE1 = new Cube(new Vector3(x + (w/4),y + (h/4),z - (l/4)), new Vector3(w/2,h/2,l/2));
            Cube SW1 = new Cube(new Vector3(x - (w/4),y + (h/4),z - (l/4)), new Vector3(w/2,h/2,l/2));
            Cube NW1 = new Cube(new Vector3(x - (w/4),y + (h/4),z + (l/4)), new Vector3(w/2,h/2,l/2));
            Cube NE2 = new Cube(new Vector3(x + (w/4),y - (h/4),z + (l/4)), new Vector3(w/2,h/2,l/2));
            Cube SE2 = new Cube(new Vector3(x + (w/4),y - (h/4),z - (l/4)), new Vector3(w/2,h/2,l/2));
            Cube SW2 = new Cube(new Vector3(x - (w/4),y - (h/4),z - (l/4)), new Vector3(w/2,h/2,l/2));
            Cube NW2 = new Cube(new Vector3(x - (w/4),y - (h/4),z + (l/4)), new Vector3(w/2,h/2,l/2));
            //Instantiating Quadtrees
            Children.Add(new QuadTree(NE1, Root, material, lod, MarchingContext));
            Children.Add(new QuadTree(SE1, Root, material, lod, MarchingContext));
            Children.Add(new QuadTree(SW1, Root, material, lod, MarchingContext));
            Children.Add(new QuadTree(NW1, Root, material, lod, MarchingContext));
            Children.Add(new QuadTree(NE2, Root, material, lod, MarchingContext));
            Children.Add(new QuadTree(SE2, Root, material, lod, MarchingContext));
            Children.Add(new QuadTree(SW2, Root, material, lod, MarchingContext));
            Children.Add(new QuadTree(NW2, Root, material, lod, MarchingContext));

            divided = true;
        }
        else
        {
            foreach (QuadTree quadTree in Children)
            {
                quadTree.SubDivide();
            }
        }
        divided = true;
        //Generate Marching cube chunks in the new Quads
        foreach (QuadTree Child in Children)
        {
            Child.CreateChunk();
        }
        ColliderState = false;
    }
    //Generate a collider + Gamobject for the collider to attach to
    public List<QuadTree> GetLeaves ()
    {
        List<QuadTree>leaves = new List<QuadTree>();
        if (divided == false)
        {
            leaves.Add(this);
        }
        else
        {
            foreach (QuadTree quadTree in Children)
            {
                leaves.AddRange(quadTree.GetLeaves());
            }
        }
        return leaves;
    }
    //Get ALL the Quadtree classes in the Quadtree
    public List<QuadTree> GetBranchesAndLeaves ()
    {
        List<QuadTree>BranchAndLeaves = new List<QuadTree>();
        if (divided == true)
        {
            foreach (QuadTree quadTree in Children)
            {
                BranchAndLeaves.AddRange(quadTree.GetBranchesAndLeaves());
            }
            BranchAndLeaves.Add(this);
        }
        else
        {
            BranchAndLeaves.Add(this);
        }
        return BranchAndLeaves;
    }

}
