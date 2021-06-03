using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchingChunk
{
    //Variables
    public Mesh mesh;
    public List<Vector3> m_vertices = new List<Vector3>();
    public List<int> m_triangles = new List<int>();
    //Privates
    private MarchingCubeContext MarchingContext;
    private Point[,,] Points;
    private Vector3 CornerPosition = Vector3.zero;
    public MarchingChunk (MarchingCubeContext Context, Cube Boundary)
    {
        MarchingContext = Context;
        MarchingContext.Cube = Boundary;
        Points = new Point[MarchingContext.AmountOfPointsPerAxis+1, MarchingContext.AmountOfPointsPerAxis + 1, MarchingContext.AmountOfPointsPerAxis + 1];
        GeneratePositionData();
        GenerateMeshData();
    }
    //instantiate the mapvalues and assign world positions to them
    public void GeneratePositionData ()
    {
        for (int x = 0; x < MarchingContext.AmountOfPointsPerAxis + 1; x++)
        {
            for (int y = 0; y < MarchingContext.AmountOfPointsPerAxis + 1; y++)
            {
                for (int z = 0; z < MarchingContext.AmountOfPointsPerAxis + 1; z++)
                {
                    //Assign position worldPositions
                    CornerPosition = (MarchingContext.Cube.position - new Vector3(MarchingContext.Cube.size.x / 2, MarchingContext.Cube.size.y / 2, MarchingContext.Cube.size.z / 2));
                    Points[x, y, z] = new Point();
                    Vector3 spaceBetweenPoints = MarchingContext.Cube.size / MarchingContext.AmountOfPointsPerAxis;
                    Points[x, y, z].position = CornerPosition + new Vector3(spaceBetweenPoints.x * x, spaceBetweenPoints.y * y, spaceBetweenPoints.z * z);
                    //Assign position values
                    float distancetocenter = Vector3.Distance(MarchingContext.CentreOfPlanet,Points[x,y,z].position);
                    Points[x, y, z].value = distancetocenter / 10000 + (Mathf.PerlinNoise((Points[x, y, z].position.x) * MarchingContext.NoiseScale, (Points[x, y, z].position.z) * MarchingContext.NoiseScale) * MarchingContext.Amplitude);
                }
            }
        }
    }
    //clear mesh and generate new mesh data
    public void GenerateMeshData ()
    {
        ClearMesh();
        for (int x = 0; x < MarchingContext.AmountOfPointsPerAxis; x++)
        {
            for (int y = 0; y < MarchingContext.AmountOfPointsPerAxis; y++)
            {
                for (int z = 0; z < MarchingContext.AmountOfPointsPerAxis; z++)
                {
                    March(new Vector3Int(x, y, z));
                }
            }
        }
    }

    //Clear the mesh
    public void ClearMesh ()
    {
        m_vertices.Clear();
        m_triangles.Clear();
    }

    //Marching cube algorithme to generate mesh data
    public void March (Vector3Int pos)
    {
        //Get cube config
        float[] cube = new float[8];
        for (int i = 0; i < 8; i++)
        {
            cube[i] = SampleTerrain(pos + MarchingCubesLookupTable.instance.CornerTable[i]);
        }
        int index = GetConfig(cube);
        //If the config is 0 or 256 this means that it is either entirely above the surface or below meaning it should be empty space
        if (index == 0 || index == 256)
        {
            return;
        }

        int edgeIndex = 0;
        //For every triangle in cube
        for (int t = 0; t < 5; t++)
        {
            //For every vertices in triangle
            for (int v = 0; v < 3; v++)
            {
                //Get indece from index in the lookuptable
                int indice = MarchingCubesLookupTable.instance.MarchingCubeEdgeTable[index][edgeIndex];
                //No need to run the calculations if indice is -1
                if (indice == -1) 
                {
                    return;
                }
                //Smoothly place edgeposition
                Vector3 vert1 = pos + (MarchingCubesLookupTable.instance.CornerTable[MarchingCubesLookupTable.instance.EdgeIndexes[indice, 0]]);
                Vector3 vert2 = pos + MarchingCubesLookupTable.instance.CornerTable[MarchingCubesLookupTable.instance.EdgeIndexes[indice, 1]];
                Vector3 vertPosition = new Vector3();

                vert1 = Points[(int)vert1.x, (int) vert1.y, (int) vert1.z].position;
                vert2 = Points[(int) vert2.x, (int) vert2.y, (int) vert2.z].position;

                float vert1Sample = cube[MarchingCubesLookupTable.instance.EdgeIndexes[indice, 0]];
                float vert2Sample = cube[MarchingCubesLookupTable.instance.EdgeIndexes[indice, 1]];
                float differ = vert2Sample - vert1Sample;
                if (differ == 0)
                {
                    differ = MarchingContext.SurfaceLevel;
                }
                else
                {
                    differ = (MarchingContext.SurfaceLevel - vert1Sample) / differ;
                }
                vertPosition = vert1 + ((vert2 - vert1) * differ);
                //Add vertice to the triangle list
                m_triangles.Add(VertForIndice(vertPosition));
                edgeIndex++;
            }
        }
    }
    //Remove duplicate vertices
    int VertForIndice (Vector3 vert)
    {
        for (int i = 0; i < m_vertices.Count; i++)
        {
            if (m_vertices[i] == vert)
            {
                return i;
            }
        }
        m_vertices.Add(vert);
        return m_vertices.Count - 1;

    }
    //Sample a value from the valuemap (This is nescecery for readability) 
    float SampleTerrain (Vector3Int point)
    {
        return Points[point.x, point.y, point.z].value;
    }

    //Get the int for triangles from the cube values
    private int GetConfig (float[] cube)
    {
        int index = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cube[i] > MarchingContext.SurfaceLevel)
            {
                index |= 1 << i;
            }
        }
        return index;
    }
}