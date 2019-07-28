using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralNoiseProject;

public enum EVoxelType
{
    Bedrock,
    Rock,
    Dirt,
    Sand,
    Air
}

public class Voxel
{
    public EVoxelType VoxelType { get; set; }
    public float Value { get; set; }
    public Vector3Int Index3 { get; set; }

    internal Voxel(Vector3Int index, EVoxelType voxelType = EVoxelType.Air)
    {
        Index3 = index;
        VoxelType = voxelType;
    }
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ChunkGenerator : MonoBehaviour
{
    [SerializeField]
    private MeshFilter meshFilter;
    [SerializeField]
    private MeshRenderer meshRenderer;
    private Mesh internalMesh;

    [SerializeField]
    [Range(8, 32)]
    private int chunkSize = 32;

    private Voxel[,,] voxelGrid;

    [SerializeField]
    private NOISE_TYPE m_noiseType = NOISE_TYPE.PERLIN;
    [SerializeField]
    [Min(1)]
    private int m_octaves = 1;
    [SerializeField]
    private float m_frequency = 1f;
    [SerializeField]
    private float m_amplitude = 1f;

    [SerializeField]
    private float m_surface = 0.0f;

    protected void Reset()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    [ContextMenu("Validate")]
    public void ValidateContext()
    {
        OnValidate(); 
    }

    protected void OnValidate()
    {
        CheckInitialised();
        InitialiseVoxels();
        MarchCubes.Surface = m_surface;
        GenerateMesh();
    }

    private void CheckInitialised()
    {
        InitialiseGrid();

        if (internalMesh == null)
        {
            internalMesh = new Mesh();
            internalMesh.name = $"VoxelGrid[{chunkSize}]";
            internalMesh.MarkDynamic();
        }
        meshFilter.sharedMesh = internalMesh;
    }

    private void InitialiseGrid()
    {
        // check size and state first
        voxelGrid = new Voxel[chunkSize, chunkSize, chunkSize];

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    voxelGrid[x, y, z] = new Voxel(new Vector3Int(x, y, z));
                }
            }
        }
    }

    public void InitialiseVoxels()
    {
        System.Array typeArray = System.Enum.GetValues(typeof(EVoxelType));
        int typeCount = typeArray.Length;
        INoise noise = GetNoise(m_noiseType, 0, m_frequency, m_amplitude);
        FractalNoise fractalNoise = new FractalNoise(noise, m_octaves, m_frequency, m_amplitude);

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float yHeight = fractalNoise.Sample3D(
                        x / (float)(chunkSize - 1), 
                        y / (float)(chunkSize - 1), 
                        z / (float)(chunkSize - 1)
                    );
                    //int roundedHeight = Mathf.RoundToInt(yHeight);

                    Voxel v = voxelGrid[x, y, z];

                    v.Value = yHeight;

                    //v.VoxelType = EVoxelType.Air;
                    v.VoxelType = (EVoxelType)Random.Range(0, typeCount-1);

                    v.VoxelType = (EVoxelType)Mathf.RoundToInt(y / 5f);
                }
            }
        }
    }

    private INoise GetNoise(NOISE_TYPE noiseType = NOISE_TYPE.PERLIN, int seed = 0, float frequency = 20f, float amplitude = 1f)
    {
        switch (noiseType)
        {
            case NOISE_TYPE.PERLIN:
                return new PerlinNoise(seed, frequency, amplitude);

            case NOISE_TYPE.VALUE:
                return new ValueNoise(seed, frequency, amplitude);

            case NOISE_TYPE.SIMPLEX:
                return new SimplexNoise(seed, frequency, amplitude);

            case NOISE_TYPE.VORONOI:
                return new VoronoiNoise(seed, frequency, amplitude);

            case NOISE_TYPE.WORLEY:
                return new WorleyNoise(seed, frequency, amplitude);

            default:
                return new PerlinNoise(seed, frequency, amplitude);
        }
    }

    private void GenerateMesh()
    {
        if (!internalMesh) return;

        List<Vector3> verticies = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colours = new List<Color>();

        MarchCubes.March(voxelGrid, verticies, triangles, colours);

        internalMesh.Clear();
        internalMesh.vertices = verticies.ToArray();
        internalMesh.triangles = triangles.ToArray();
        internalMesh.colors = colours.ToArray();

        internalMesh.RecalculateNormals();
    }
}
