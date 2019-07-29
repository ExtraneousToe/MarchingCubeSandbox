using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralNoiseProject;

/*
 * 0 = Air
 * 1 = Bedrock
 * 2 = Rock
 * 3 = Dirt
 * 4 = Sand
 * 5 = ?
 * 
 */

public class Voxel
{
	public byte VoxelType { get; set; }
	public float Value { get; set; }
	public Vector3Int Index3 { get; set; }

	internal Voxel(Vector3Int index, byte voxelType = 0)
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
	[Range(8, 64)]
	private int chunkSizeXZ = 48;

	[SerializeField]
	[Range(8, 64)]
	private int chunkHeight = 32;

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

	[SerializeField]
	private int m_worldResolution = 64;

	[SerializeField]
	private bool m_useValues = true;

	[SerializeField]
	private NoiseLayerScriptable m_rockNoiseLayer;
	public NoiseLayerScriptable RockNoiseLayer => m_rockNoiseLayer;

	[SerializeField]
	private NoiseLayerScriptable m_dirtNoiseLayer;
	public NoiseLayerScriptable DirtNoiseLayer => m_dirtNoiseLayer;

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
			internalMesh.name = $"VoxelGrid";
			internalMesh.MarkDynamic();
		}

		meshFilter.sharedMesh = internalMesh;
	}

	private void InitialiseGrid()
	{
		// check size and state first
		voxelGrid = new Voxel[chunkSizeXZ, chunkHeight, chunkSizeXZ];

		for (int x = 0; x < chunkSizeXZ; x++)
		{
			for (int y = 0; y < chunkHeight; y++)
			{
				for (int z = 0; z < chunkSizeXZ; z++)
				{
					voxelGrid[x, y, z] = new Voxel(new Vector3Int(x, y, z));
				}
			}
		}
	}

	public void InitialiseVoxels()
	{
		if (RockNoiseLayer) RockNoiseLayer.Initialise();
		if (DirtNoiseLayer) DirtNoiseLayer.Initialise();

		INoise noise = GetNoise(m_noiseType, 0, m_frequency, m_amplitude);
		FractalNoise fractalNoise = new FractalNoise(noise, m_octaves, m_frequency, m_amplitude);

		for (int x = 0; x < chunkSizeXZ; x++)
		{
			for (int z = 0; z < chunkSizeXZ; z++)
			{
				float bedrockHeight = 2;
				float rockHeight = 0;
				float dirtHeight = 0;
				//float sandHeight = 12;

				if (RockNoiseLayer)
				{
					rockHeight += RockNoiseLayer.SampleValue(
						x / (float)(m_worldResolution - 1),
						z / (float)(m_worldResolution - 1)
					);
				}

				if (DirtNoiseLayer)
				{
					dirtHeight += DirtNoiseLayer.SampleValue(
						x / (float)(m_worldResolution - 1),
						z / (float)(m_worldResolution - 1)
					);
				}

				for (int y = 0; y < chunkHeight; y++)
				{
					Voxel v = voxelGrid[x, y, z];

					if (x == 0 || x == chunkSizeXZ - 1 ||
						y == 0 || y == chunkHeight - 1 ||
						z == 0 || z == chunkSizeXZ - 1)
					{
						v.VoxelType = 0;
						v.Value = 0;
					}
					else
					{
						if (y <= bedrockHeight)
						{
							v.VoxelType = 1;
						}
						else if (y <= rockHeight)
						{
							v.VoxelType = 2;
						}
						else if (y <= dirtHeight)
						{
							v.VoxelType = 3;
						}
						//else if (y <= sandHeight)
						//{
						//	v.VoxelType = 4;
						//}
						else
						{
							v.VoxelType = 0;
						}

						float pointValue = fractalNoise.Sample3D(
							x / (float)(m_worldResolution - 1),
							y / (float)(m_worldResolution - 1),
							z / (float)(m_worldResolution - 1)
						);

						v.Value = pointValue;
					}
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

		MarchCubes.March(voxelGrid, verticies, triangles, colours, m_useValues);

		internalMesh.Clear();
		internalMesh.vertices = verticies.ToArray();
		internalMesh.triangles = triangles.ToArray();
		internalMesh.colors = colours.ToArray();

		internalMesh.RecalculateNormals();
	}
}
