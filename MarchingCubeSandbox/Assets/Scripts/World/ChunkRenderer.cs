using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellWorld
{
	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
	public class ChunkRenderer : MonoBehaviour, IChunk
	{
		#region Vars
		[SerializeField]
		private MeshFilter m_meshFilter;
		[SerializeField]
		private MeshRenderer m_meshRenderer;
		[SerializeField]
		private MeshCollider m_meshCollider;

		private Mesh m_internalMesh;

		private readonly List<Vector3> m_workingVerticies = new List<Vector3>();
		private readonly List<int> m_workingTriangles = new List<int>();
		private readonly List<Color> m_workingColours = new List<Color>();
		#endregion

		#region Mono
		protected void Reset()
		{
			m_meshFilter = GetComponent<MeshFilter>();
			m_meshRenderer = GetComponent<MeshRenderer>();
			m_meshCollider = GetComponent<MeshCollider>();
		}

		protected void Awake()
		{
			m_meshFilter = m_meshFilter ?? GetComponent<MeshFilter>();
			m_meshRenderer = m_meshRenderer ?? GetComponent<MeshRenderer>();
			m_meshCollider = m_meshCollider ?? GetComponent<MeshCollider>();

			CheckInitialised();
		}
		#endregion

		#region ChunkRenderer
		private void CheckInitialised()
		{
			if (m_internalMesh == null)
			{
				m_internalMesh = new Mesh
				{
					name = $"VoxelGrid"
				};
				m_internalMesh.MarkDynamic();
			}

			m_meshFilter.sharedMesh = m_internalMesh;
			m_meshCollider.sharedMesh = m_internalMesh;
		}

		public void RedrawMesh(Chunk chunk)
		{
			CheckInitialised();

			m_workingVerticies.Clear();
			m_workingTriangles.Clear();
			m_workingColours.Clear();

			chunk.MarchChunk(m_workingVerticies, m_workingTriangles, m_workingColours, chunk.World.UseValues);

			m_internalMesh.Clear();
			m_internalMesh.vertices = m_workingVerticies.ToArray();
			m_internalMesh.triangles = m_workingTriangles.ToArray();
			m_internalMesh.colors = m_workingColours.ToArray();

			m_internalMesh.RecalculateNormals();

			m_meshCollider.sharedMesh = m_internalMesh;
		}
		#endregion
	}
}
