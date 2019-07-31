using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CellWorld
{
	[CustomEditor(typeof(World))]
	public class WorldEditor : Editor
	{
		private World world;

		private bool rockDropdown = false;
		private Editor rockNoiseEditor;
		private Editor RockNoiseEditor => rockNoiseEditor ?? (rockNoiseEditor = world.RockNoiseLayer != null ? CreateEditor(world.RockNoiseLayer) : null);

		private bool dirtDropdown = false;
		private Editor dirtNoiseEditor;
		private Editor DirtNoiseEditor => dirtNoiseEditor ?? (dirtNoiseEditor = world.DirtNoiseLayer != null ? CreateEditor(world.DirtNoiseLayer) : null);

		private bool sandDropdown = false;
		private Editor sandNoiseEditor;
		private Editor SandNoiseEditor => sandNoiseEditor ?? (sandNoiseEditor = world.SandNoiseLayer != null ? CreateEditor(world.SandNoiseLayer) : null);

		private bool grassDropdown = false;
		private Editor grassNoiseEditor;
		private Editor GrassNoiseEditor => grassNoiseEditor ?? (grassNoiseEditor = world.GrassNoiseLayer != null ? CreateEditor(world.GrassNoiseLayer) : null);

		private void OnEnable()
		{
			world = target as World;
		}

		private void OnDisable()
		{
			rockNoiseEditor = null;
			dirtNoiseEditor = null;
			sandNoiseEditor = null;
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

			if (SandNoiseEditor)
			{
				DrawEditor(SandNoiseEditor, "Sand");
			}

			if (GrassNoiseEditor)
			{
				DrawEditor(GrassNoiseEditor, "Grass");
			}
			
			if (EditorGUI.EndChangeCheck())
			{
				world.ValidateContext();
			}

			if (GUILayout.Button($"Generate {world.ChunkGenerateRange * 2 + 1}x{world.ChunkGenerateRange * 2 + 1} Chunk Grid"))
			{
				for (int x = -world.ChunkGenerateRange; x <= world.ChunkGenerateRange; x++)
				{
					for (int z = -world.ChunkGenerateRange; z <= world.ChunkGenerateRange; z++)
					{
						world.GenerateColumn(x, z);
					}
				}
				world.ValidateContext();
			}

			if (GUILayout.Button("Clear World"))
			{
				world.ClearWorld();
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
}