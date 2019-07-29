using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkModifier : MonoBehaviour
{
	[SerializeField]
	private Mesh cubeMesh;
	[SerializeField]
	private Mesh sphereMesh;
	[SerializeField]
	private Material cubeMaterial;

	[SerializeField]
	private ChunkGenerator chunkGenerator;

	public float scale = 2f;
	public float indentMag = 0.5f;

	private Vector3? lastClampedHitPoint;

	// Update is called once per frame
	void Update()
	{
		Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);

		RaycastHit[] hits = Physics.RaycastAll(raycast);

		lastClampedHitPoint = null;

		if (hits.Length == 0) return;

		Vector3 cameraPos = Camera.main.transform.position;

		RaycastHit closestHit = hits[0];

		for (int i = 1; i < hits.Length; i++)
		{
			if (hits[i].distance < closestHit.distance)
			{
				closestHit = hits[i];
			}
		}
		
		Debug.DrawRay(closestHit.point, closestHit.normal, Color.green);
		Debug.DrawRay(closestHit.point, -closestHit.normal * indentMag, Color.red);

		Vector3 clampedPoint = closestHit.point - closestHit.normal * indentMag;

		Graphics.DrawMesh(
			sphereMesh,
			closestHit.point,
			Quaternion.identity,
			cubeMaterial,
			0
			);

		if (chunkGenerator)
		{
			for (int i = 0; i < 3; i++)
			{
				clampedPoint[i] = Mathf.Round(clampedPoint[i]);
			}
		}

		Graphics.DrawMesh(
			cubeMesh,
			Matrix4x4.TRS(
				clampedPoint,
				Quaternion.identity,
				Vector3.one * scale
				),
			cubeMaterial,
			0
			);

		lastClampedHitPoint = clampedPoint;

	}

	private void OnMouseDown()
	{
		if (!lastClampedHitPoint.HasValue) return;

		Vector3 lastPoint = lastClampedHitPoint.Value;

		chunkGenerator.VoxelGrid[
			(int)lastPoint.x,
			(int)lastPoint.y,
			(int)lastPoint.z
			].VoxelType = 0;
		chunkGenerator.GenerateMesh();
	}
}
