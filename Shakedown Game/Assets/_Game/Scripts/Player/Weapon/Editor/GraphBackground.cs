using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// Implementation from UnityEditor.Graphs.GraphGUI
public static class GraphBackground
{
	private static readonly Color kGridMinorColorDark = new Color(0f, 0f, 0f, 0.18f);
	private static readonly Color kGridMajorColorDark = new Color(0f, 0f, 0f, 0.28f);
	private static readonly Color kGridMinorColorLight = new Color(0f, 0f, 0f, 0.1f);
	private static readonly Color kGridMajorColorLight = new Color(0f, 0f, 0f, 0.15f);

	private static Color gridMinorColor
	{
		get
		{
			if (EditorGUIUtility.isProSkin)
				return kGridMinorColorDark;
			else
				return kGridMinorColorLight;
		}
	}

	private static Color gridMajorColor
	{
		get
		{
			if (EditorGUIUtility.isProSkin)
				return kGridMajorColorDark;
			else
				return kGridMajorColorLight;
		}
	}

	public static void DrawGraphBackground(Rect position, Rect graphExtents)
	{
		if (Event.current.type == EventType.Repaint)
		{
			UnityEditor.Graphs.Styles.graphBackground.Draw(position, false, false, false, false);
		}
		DrawGrid(graphExtents);
	}
	
	private static void DrawGrid(Rect graphExtents)
	{
		if (Event.current.type == EventType.Repaint)
		{
			HandleUtility.ApplyWireMaterial();
			GL.PushMatrix();
			GL.Begin(1);
			DrawGridLines(graphExtents, 11.55f * 2, gridMinorColor);
			DrawGridLines(graphExtents, 11.55f * 2, gridMajorColor);
			GL.End();
			GL.PopMatrix();
		}
	}
	
	private static void DrawGridLines(Rect graphExtents, float gridSize, Color gridColor)
	{
		GL.Color(gridColor);
		for (float x = graphExtents.xMin - graphExtents.xMin % gridSize; x < graphExtents.xMax; x += gridSize)
		{
			DrawLine(new Vector2(x, graphExtents.yMin), new Vector2(x, graphExtents.yMax));
		}
		GL.Color(gridColor);
		for (float y = graphExtents.yMin - graphExtents.yMin % gridSize; y < graphExtents.yMax; y += gridSize)
		{
			DrawLine(new Vector2(graphExtents.xMin, y), new Vector2(graphExtents.xMax, y));
		}
	}

	private static void DrawLine(Vector2 p1, Vector2 p2)
	{
		GL.Vertex(p1);
		GL.Vertex(p2);
	}

	// Implementation from UnityEditor.HandleUtility
	static class HandleUtility
	{
		static Material s_HandleWireMaterial;
		static Material s_HandleWireMaterial2D;

		internal static void ApplyWireMaterial(CompareFunction zTest = CompareFunction.Always)
		{
			Material handleWireMaterial = HandleUtility.handleWireMaterial;
			handleWireMaterial.SetInt("_HandleZTest", (int)zTest);
			handleWireMaterial.SetPass(0);
		}
		
		private static Material handleWireMaterial
		{
			get
			{
				InitHandleMaterials();
				return (!Camera.current) ? s_HandleWireMaterial2D : s_HandleWireMaterial;
			}
		}

		private static void InitHandleMaterials()
		{
			if (!s_HandleWireMaterial)
			{
				s_HandleWireMaterial = (Material)EditorGUIUtility.LoadRequired("SceneView/HandleLines.mat");
				s_HandleWireMaterial2D = (Material)EditorGUIUtility.LoadRequired("SceneView/2DHandleLines.mat");
			}
		}
	}
}
