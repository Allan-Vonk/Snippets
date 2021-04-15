using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewChunk : MonoBehaviour
{
    MeshFilter m_meshFilter;
    MeshCollider m_meshCollider;

    float m_terrainSurface;
    public int width = 20;
    public int height = 10;

    [SerializeField]private float m_noiseScale;
    [SerializeField] private float m_terrainHeightScale;
    [SerializeField] private bool m_Smooth;
    [SerializeField] private bool m_FlatShaded;
    float[,,] m_terrainMap;

    List<Vector3> m_vertices = new List<Vector3>();
    List<int> m_triangles = new List<int>();
    private void Start()
    {
        m_terrainMap = new float[width + 1, height + 1, width + 1];
        m_meshFilter = GetComponent<MeshFilter>();
        m_meshCollider = GetComponent<MeshCollider>();
        transform.tag = "Terrain";
        GenerateTerrainData();
        GenerateMeshData();
    }
    private void GenerateTerrainData()
    {
        //set values for all the generated points
        for (int x = 0; x < width+1; x++)
        {
            for (int z = 0; z < width + 1; z++)
            {
                for (int y = 0; y < height + 1; y++)
                {
                    //Get perlin data
                    float height = (float)this.height * Mathf.PerlinNoise(((float)x + transform.position.x) / m_noiseScale * m_terrainHeightScale + 0.001f, ((float)z + transform.position.z) / m_noiseScale * m_terrainHeightScale + 0.001f);
                    //set value in the terrain map
                    m_terrainMap[x, y, z] = (float)y -height;
                }
            }
        }
    }
    private void GenerateMeshData()
    {
        ClearMesh();
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                for (int y = 0; y < height; y++)
                {

                    March(new Vector3Int(x, y, z));
                }
            }
        }
        GenerateMesh();

    }
    private void March(Vector3Int position)
    {
        float[] cube = new float[8];
        for (int i = 0; i < 8; i++)
        {
            cube[i] = SampleTerrain(position + MarchingCubesLookupTable.instance.CornerTable[i]);
        }

        int index = GetConfig(cube);
        if (index == 0 || index ==256)
        {
            return;
        }
        int edgeIndex = 0;
        for (int T = 0; T < 5; T++)
        {
            for (int V = 0; V < 3; V++)
            {
                int indice = MarchingCubesLookupTable.instance.MarchingCubeEdgeTable[index][edgeIndex];
                if (indice == -1)
                {
                    return;
                }
                Vector3 vert1 = position + MarchingCubesLookupTable.instance.CornerTable[MarchingCubesLookupTable.instance.EdgeIndexes[indice, 0]];
                Vector3 vert2 = position + MarchingCubesLookupTable.instance.CornerTable[MarchingCubesLookupTable.instance.EdgeIndexes[indice, 1]];
                Vector3 vertPosition = new Vector3();
                if (m_Smooth)
                {
                    float vert1Sample = cube[MarchingCubesLookupTable.instance.EdgeIndexes[indice, 0]];
                    float vert2Sample = cube[MarchingCubesLookupTable.instance.EdgeIndexes[indice, 1]];
                    float differ = vert2Sample - vert1Sample;
                    if (differ == 0)
                    {
                        differ = m_terrainSurface;
                    }
                    else
                    {
                        differ = (m_terrainSurface - vert1Sample) / differ;
                    }

                    vertPosition = vert1 + ((vert2 - vert1) * differ);

                }
                else
                {
                    vertPosition = (vert1 + vert2) / 2f;
                }
                if (m_FlatShaded)
                {
                    m_vertices.Add(vertPosition);
                    m_triangles.Add(m_vertices.Count - 1);
                }
                else
                {
                    m_triangles.Add(VertForIndice(vertPosition));
                }
                edgeIndex++;
            }
        }
    }
    void ClearMesh()
    {

        m_vertices.Clear();
        m_triangles.Clear();

    }
    void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = m_vertices.ToArray();
        mesh.triangles = m_triangles.ToArray();
        mesh.RecalculateNormals();
        m_meshFilter.mesh = mesh;
        m_meshCollider.sharedMesh = mesh;

    }
    float SampleTerrain(Vector3Int point)
    {
        return m_terrainMap[point.x, point.y, point.z];
    }
    int VertForIndice(Vector3 vert)
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
    private int GetConfig(float[] cube)
    {
        int index = 0;
        for (int i = 0; i < 8; i++)
        {
            if (cube[i] > m_terrainSurface)
                index |= 1 << i;
        }
        return index;
    }

}