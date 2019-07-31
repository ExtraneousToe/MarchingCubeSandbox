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

		private const int MIN_GENERATE_CHUNKS = 1;
		[SerializeField]
		private int m_chunkGenerateRange = 5;
		public int ChunkGenerateRange
		{
			get => Mathf.Max(m_chunkGenerateRange, MIN_GENERATE_CHUNKS);
		}

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

			MarchCubes.Surface = m_surface;

			m_chunkList.ForEach(c => c?.Initialise(this, c.ChunkIndex));
			m_chunkList.ForEach(c => c?.GenerateCells());
			m_chunkList.ForEach(c => c?.RedrawMesh());
		}

		private void Awake()
		{

		}
		#endregion

		#region World Functions
		public Vector3Int GetCellCoord(Vector3 worldPoint, CellWorldSpace space = CellWorldSpace.World)
		{
			Vector3Int ret = Vector3Int.zero;

			for (int i = 0; i < 3; i++) ret[i] = Mathf.RoundToInt(worldPoint[i] * CellsPerUnit) % (space == CellWorldSpace.Chunk ? ChunkSize : 1);

			return ret;
		}
		public Vector3Int GetCellCoord(Vector3Int worldCellIndex)
		{
			Vector3Int ret = Vector3Int.zero;

			for (int i = 0; i < 3; i++) ret[i] = worldCellIndex[i] % ChunkSize;

			return ret;
		}

		public Vector3Int GetChunkCoord(Vector3 worldPoint)
		{
			Vector3Int ret = Vector3Int.zero;

			for (int i = 0; i < 3; i++) ret[i] = Mathf.FloorToInt(worldPoint[i] * CellsPerUnit / ChunkSize);

			return ret;
		}

		public Vector3Int GetChunkCoord(Vector3Int worldCellIndex)
		{
			Vector3Int ret = Vector3Int.zero;

			for (int i = 0; i < 3; i++) ret[i] = worldCellIndex[i] * CellsPerUnit / ChunkSize;

			return ret;
		}

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

		public Chunk GetChunk(Vector3Int chunkXYZ, bool generateIfMissing = false)
		{
			if (!m_chunkDict.TryGetValue(chunkXYZ, out Chunk chunk))
			{
				chunk = generateIfMissing ? GenerateChunk(chunkXYZ) : null;
			}

			return chunk;
		}

		public Cell GetCellFromWorldPoint(Vector3 worldPoint)
		{
			return GetCellFromWorldPoint(GetCellCoord(worldPoint, CellWorldSpace.World));
		}

		public Cell GetCellFromWorldCellIndex(Vector3Int worldCellIndex)
		{
			Vector3Int chunkCoord = GetChunkCoord(worldCellIndex);

			if (m_chunkDict.TryGetValue(chunkCoord, out Chunk chunk))
			{
				Vector3Int localCellCoord = GetCellCoord(worldCellIndex);

				return chunk[localCellCoord];
			}

			return null;
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
	}
}