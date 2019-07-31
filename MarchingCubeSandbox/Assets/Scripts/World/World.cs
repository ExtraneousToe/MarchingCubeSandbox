using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CellWorld
{
	public class World : MonoBehaviour
	{
		#region Static Vars
		private static World s_instance;
		public static World Instance
		{
			get => s_instance ?? (s_instance = FindObjectOfType<World>());
		}
		#endregion

		#region Vars
		private const int MIN_CHUNK_SIZE = 2;
		[Tooltip("Number of cells per chunk dimension.")]
		[SerializeField]
		private int m_chunkSize = 16;
		public int ChunkSize
		{
			get => Mathf.Max(m_chunkSize, MIN_CHUNK_SIZE);
		}

		private const int MIN_CELLS_PER_UNIT = 1;
		[Tooltip("Number of cells per Unity unit.")]
		[SerializeField]
		private int m_cellsPerUnit = 1;
		public int CellsPerUnit
		{
			get => Mathf.Max(m_cellsPerUnit, MIN_CELLS_PER_UNIT);
		}

		private const int MIN_VERTICAL_CHUNKS = 1;
		[Tooltip("Number of chunks vertically in the world.")]
		[SerializeField]
		private int m_verticalChunkCount = 2;
		public int VerticalChunkCount
		{
			get => Mathf.Max(m_verticalChunkCount, MIN_VERTICAL_CHUNKS);
		}

		private const int MIN_GENERATE_CHUNKS = 0;
		[SerializeField]
		private int m_chunkGenerateRange = 1;
		public int ChunkGenerateRange
		{
			get => Mathf.Max(m_chunkGenerateRange, MIN_GENERATE_CHUNKS);
		}

		[SerializeField]
		private readonly Dictionary<Vector3Int, Chunk> m_chunkDict = new Dictionary<Vector3Int, Chunk>();

		[SerializeField]
		private List<Chunk> m_chunkList = new List<Chunk>();

		[SerializeField]
		private float m_surface = 0.0f;

		[SerializeField]
		private bool m_useValues = false;
		public bool UseValues => m_useValues;

		[SerializeField]
		private NoiseLayerScriptable m_rockNoiseLayer;
		public NoiseLayerScriptable RockNoiseLayer => m_rockNoiseLayer;

		[SerializeField]
		private NoiseLayerScriptable m_dirtNoiseLayer;
		public NoiseLayerScriptable DirtNoiseLayer => m_dirtNoiseLayer;

		[SerializeField]
		private NoiseLayerScriptable m_sandNoiseLayer;
		public NoiseLayerScriptable SandNoiseLayer => m_sandNoiseLayer;

		[SerializeField]
		private NoiseLayerScriptable m_grassNoiseLayer;
		public NoiseLayerScriptable GrassNoiseLayer => m_grassNoiseLayer;

		[SerializeField]
		private Chunk m_chunkPrefab;
		#endregion

		#region Mono
		private void Reset()
		{

		}

		[ContextMenu("Validate")]
		public void ValidateContext()
		{
			OnValidate();
		}

		private void OnValidate()
		{
			if (RockNoiseLayer) RockNoiseLayer.Initialise();
			if (DirtNoiseLayer) DirtNoiseLayer.Initialise();
			if (SandNoiseLayer) SandNoiseLayer.Initialise();
			if (GrassNoiseLayer) GrassNoiseLayer.Initialise();

			MarchCubes.Surface = m_surface;

			PrefillCollections();

			m_chunkList.ForEach(c => c?.Initialise(this, c.ChunkIndex));
			m_chunkList.ForEach(c => c?.GenerateCells());
			m_chunkList.ForEach(c => c?.RedrawMesh());
		}

		private void Awake()
		{
			PrefillCollections();
		}
		#endregion

		private void PrefillCollections()
		{
			m_chunkList.Clear();
			m_chunkDict.Clear();

			List<Chunk> chunkChildren = new List<Chunk>(GetComponentsInChildren<Chunk>());

			chunkChildren.ForEach(c => {
				m_chunkList.Add(c);
				m_chunkDict.Add(c.ChunkIndex, c);
			});
		}

		#region World Functions
		#region Coordinates
		public Vector3Int ConvertWorldToCell(Vector3 worldPoint)
		{
			Vector3Int ret = Vector3Int.zero;

			for (int i = 0; i < 3; i++) ret[i] = Mathf.RoundToInt(worldPoint[i] * CellsPerUnit);

			return ret;
		}

		public Vector3Int GetChunkCoord(Vector3 worldPoint)
		{
			return GetChunkCoord(ConvertWorldToCell(worldPoint));
		}

		public Vector3Int GetChunkCoord(Vector3Int worldCellIndex)
		{
			Vector3Int ret = worldCellIndex;

			for (int i = 0; i < 3; i++) ret[i] = Mathf.FloorToInt(ret[i] / (float)ChunkSize);

			return ret;
		}

		public Vector3Int GetCellChunkCoord(Vector3 worldPoint)
		{
			return GetCellChunkCoord(ConvertWorldToCell(worldPoint));
		}

		public Vector3Int GetCellChunkCoord(Vector3Int worldCellIndex)
		{
			Vector3Int ret = worldCellIndex;

			for (int i = 0; i < 3; i++) ret[i] = (int)Mathf.Repeat(ret[i], ChunkSize);

			return ret;
		}
		#endregion

		#region ChunkAccess
		public Chunk GetChunk(Vector3Int chunkXYZ, bool generateIfMissing = false)
		{
			if (!m_chunkDict.TryGetValue(chunkXYZ, out Chunk chunk))
			{
				chunk = generateIfMissing ? GenerateChunk(chunkXYZ) : null;
			}

			return chunk;
		}

		private Chunk GenerateChunk(Vector3Int chunkXYZ)
		{
			Chunk chunk = Instantiate(m_chunkPrefab, transform);
			m_chunkList.Add(chunk);
			m_chunkDict.Add(chunkXYZ, chunk);

			chunk.Initialise(this, chunkXYZ);
			chunk.GenerateCells();

			return chunk;
		}
		#endregion

		#region CellAccess
		public Cell GetCell(Vector3 worldPoint)
		{
			return GetCell(GetChunkCoord(worldPoint), GetCellChunkCoord(worldPoint));
		}

		public Cell GetCell(Vector3Int worldCellIndex)
		{
			return GetCell(GetChunkCoord(worldCellIndex), GetCellChunkCoord(worldCellIndex));
		}

		public Cell GetCell(Vector3Int chunkIndex, Vector3Int chunkCellIndex)
		{
			Chunk chunk = GetChunk(chunkIndex);

			return chunk?[chunkCellIndex];
		}
		#endregion

		#region Generation
		public void GenerateColumn(int chunkX, int chunkZ)
		{
			for (int y = 0; y < VerticalChunkCount; y++)
			{
				Chunk c = GetChunk(new Vector3Int(chunkX, y, chunkZ), true);
#if UNITY_EDITOR
				c?.Initialise(this, c.ChunkIndex);
#endif
			}
		}

		public void ClearWorld()
		{
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(transform.GetChild(i).gameObject);
			}

			m_chunkDict.Clear();
			m_chunkList.Clear();
		}
		#endregion
		#endregion
	}
}