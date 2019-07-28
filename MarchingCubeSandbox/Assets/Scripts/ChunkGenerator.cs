using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EVoxelType
{
    Rock,
    Grass,
    Air
}

public class Voxel
{
    public EVoxelType VoxelType { get; set; }
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
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh internalMesh;

    [SerializeField]
    [Range(8,32)]
    private int chunkSize = 16;

    private Voxel[,,] voxelGrid;

	protected void Reset()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

	protected void OnValidate()
	{
        CheckInitialised();
	}

    private void CheckInitialised()
    {
        InitialiseGrid();

        if(internalMesh == null)
        {
            internalMesh = new Mesh();
            internalMesh.name = $"VoxelGrid[{chunkSize}]";
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
}
