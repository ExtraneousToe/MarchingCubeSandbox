using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClampToChunkCell : MonoBehaviour
{
	[SerializeField]
	private Vector3 desiredPosition;
	public Vector3 DesiredPosition
	{
		get => desiredPosition;
		set
		{
			desiredPosition = value;

			for (int i = 0; i < 3; i++)
			{
				chunkPosOutput[i] = Mathf.Round(desiredPosition[i]);
			}
		}
	}

	[SerializeField]
	private Vector3 chunkPosOutput;
	public Vector3 ChunkPosOutput
	{
		get => chunkPosOutput;
	}

	private void OnValidate()
	{
		DesiredPosition = desiredPosition;
	}

	void Update()
	{
		transform.position = ChunkPosOutput;
	}
}
