using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CellWorld
{
	public static class WorldDefines
	{
		public static Dictionary<byte, Color> TypeColourPairs = new Dictionary<byte, Color>
		{
			[0] = Color.clear,
			[1] = Color.grey / 2,
			[2] = Color.grey,
			[3] = new Color(130 / 255f, 76 / 255f, 0),
			[4] = Color.yellow,
			[5] = Color.green,
		};
	}

	/*
	 * 0 = Air
	 * 1 = Bedrock
	 * 2 = Rock
	 * 3 = Dirt
	 * 4 = Sand
	 * 5 = Grass
	 * 
	 */
	 
	public enum CellWorldSpace
	{
		World,
		Chunk
	}
	
	public class Cell
	{
		public byte CellType { get; set; }
		public float Value { get; set; }

		internal Cell(byte voxelType = 0)
		{
			CellType = voxelType;
		}
	}
}