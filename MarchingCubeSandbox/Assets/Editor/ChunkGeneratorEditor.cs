using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChunkGenerator))]
public class ChunkGeneratorEditor : Editor
{
	private ChunkGenerator chunkGenerator;

	private Editor rockNoiseEditor;
	private Editor RockNoiseEditor => rockNoiseEditor ?? (rockNoiseEditor = chunkGenerator.RockNoiseLayer != null ? CreateEditor(chunkGenerator.RockNoiseLayer) : null);

	private Editor dirtNoiseEditor;
	private Editor DirtNoiseEditor => dirtNoiseEditor ?? (dirtNoiseEditor = chunkGenerator.DirtNoiseLayer != null ? CreateEditor(chunkGenerator.DirtNoiseLayer) : null);

	private void OnEnable()
	{
		chunkGenerator = target as ChunkGenerator;
	}

	private void OnDisable()
	{
		rockNoiseEditor = null;
		dirtNoiseEditor = null;
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUI.BeginChangeCheck();

		if (RockNoiseEditor)
		{
			DrawEditor(RockNoiseEditor, "Rock");
		}

		if (DirtNoiseEditor)
		{
			DrawEditor(DirtNoiseEditor, "Dirt");
		}

		if (EditorGUI.EndChangeCheck())
		{
			chunkGenerator.ValidateContext();
		}
	}

	private void DrawEditor(Editor toDraw, string label)
	{
		var origFontStyle = EditorStyles.label.fontStyle;
		EditorStyles.label.fontStyle = FontStyle.Bold;

		EditorGUILayout.Space();
		EditorGUILayout.LabelField(label);
		EditorStyles.label.fontStyle = origFontStyle;
		toDraw.DrawDefaultInspector();
	}
}
