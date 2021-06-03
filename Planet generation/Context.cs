using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

public struct MarchingCubeContext
{
    public Cube Cube;
    public int AmountOfPointsPerAxis;
    public float SurfaceLevel;
    public Vector3 CentreOfPlanet;
    public int MaxLod;
    public float NoiseScale;
    public float Amplitude;
}

