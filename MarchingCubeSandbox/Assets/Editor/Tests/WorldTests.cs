using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using CellWorld;

namespace CellWorld.Tests
{
	public class WorldTests
	{
		private World world;

		[SetUp]
		public void SetUp()
		{
			world = GameObject.FindObjectOfType<World>();
		}

		// A Test behaves as an ordinary method
		[Test]
		public void WorldTests_GetWorldCoord()
		{
			TestWorldIndex(new Vector3Int(1, 1, 1));

			TestWorldIndex(new Vector3Int(-1, -1, -1));

			TestWorldIndex(new Vector3Int(17, 5, 2));
		}

		private void TestWorldIndex(Vector3Int index)
		{
			Debug.Log($"From: {index}");
			Debug.Log($"Chunk: {world.GetChunkCoord(index)}");
			Debug.Log($"Cell: {world.GetCellChunkCoord(index)}");
		}
	}
}
