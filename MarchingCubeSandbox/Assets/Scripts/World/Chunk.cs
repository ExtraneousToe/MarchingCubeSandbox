using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellWorld
{
	[RequireComponent(typeof(ChunkRenderer))]
	public class Chunk : MonoBehaviour, IChunk
	{
		#region Static Vars

		#endregion

		#region Vars
		private World m_world = null;
		public World World => m_world;

		[SerializeField]
		private ChunkRenderer m_chunkRenderer;
		public ChunkRenderer ChunkRenderer => m_chunkRenderer;

		private Cell[,,] m_cellGrid;
		public Cell[,,] CellGrid => m_cellGrid;

		[SerializeField]
		private Vector3Int m_chunkIndex;
		public Vector3Int ChunkIndex => m_chunkIndex;

		public Cell this[Vector3Int xyz]
		{
			get
			{
				try
				{
					return CellGrid[xyz.x, xyz.y, xyz.z];
				}
				catch (System.IndexOutOfRangeException ioore)
				{
					// throw for now
					throw ioore;
				}
			}
		}
		#endregion

		#region Mono
		private void Reset()
		{
			m_chunkRenderer = GetComponent<ChunkRenderer>();
		}
		#endregion

		#region Chunk
		public void Initialise(World world, Vector3Int chunkIndex)
		{
			m_world = world;
			m_chunkIndex = chunkIndex;

			m_cellGrid = new Cell[World.ChunkSize, World.ChunkSize, World.ChunkSize];

			for (int x = 0; x < World.ChunkSize; x++)
			{
				for (int y = 0; y < World.ChunkSize; y++)
				{
					for (int z = 0; z < World.ChunkSize; z++)
					{
						m_cellGrid[x, y, z] = new Cell();
					}
				}
			}

			transform.localScale = Vector3.one * 1f / World.CellsPerUnit;
			transform.localPosition = ((Vector3)chunkIndex) * 1f / World.CellsPerUnit * World.ChunkSize;
		}

		public void GenerateCells()
		{
			if (!World) return;

			int gridDimensions = CellGrid.GetLength(0);

			for (int x = 0; x < gridDimensions; x++)
			{
				for (int z = 0; z < gridDimensions; z++)
				{
					float worldX = x + ChunkIndex.x * World.ChunkSize;
					float worldZ = z + ChunkIndex.z * World.ChunkSize;

					float bedrockHeight = 2;
					float rockHeight = 0;
					float dirtHeight = 0;
					float sandHeight = 0;

					if (World.RockNoiseLayer)
					{
						rockHeight += World.RockNoiseLayer.SampleValue(
							worldX / (World.CellsPerUnit * World.ChunkSize),
							worldZ / (World.CellsPerUnit * World.ChunkSize)
						);
					}

					if (World.DirtNoiseLayer)
					{
						dirtHeight += World.DirtNoiseLayer.SampleValue(
							worldX,
							worldZ
						);
					}

					if (World.SandNoiseLayer)
					{
						sandHeight += World.SandNoiseLayer.SampleValue(
							worldX,
							worldZ
						);
					}

					for (int y = 0; y < gridDimensions; y++)
					{
						float worldY = y + ChunkIndex.y * World.ChunkSize;

						Cell c = CellGrid[x, y, z];

						if (//x == 0 || x == gridDimensions - 1 ||
							worldY == 0 //||
							//z == 0 || z == gridDimensions - 1
							)
						{
							c.CellType = 0;
							c.Value = 0;
						}
						else
						{
							if (worldY <= bedrockHeight)
							{
								c.CellType = 1;
							}
							else if (worldY <= rockHeight)
							{
								c.CellType = 2;
							}
							else if (worldY <= dirtHeight)
							{
								c.CellType = 3;
							}
							else if (worldY <= sandHeight)
							{
								c.CellType = 4;
							}
							else
							{
								c.CellType = 0;
							}
						}
					}
				}
			}
		}

		public void RedrawMesh()
		{
			ChunkRenderer?.RedrawMesh(this);
		}
		#endregion
	}
}
