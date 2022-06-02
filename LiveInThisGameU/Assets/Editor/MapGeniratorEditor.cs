using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(MapGenirator))]
public class MapGeneratorEditor : Editor
{

	public override void OnInspectorGUI()
	{
		MapGenirator mapGen = (MapGenirator)target;

		if (DrawDefaultInspector())
		{
			if (mapGen.autoUpdate)
			{
				mapGen.DrawMapInEditor();
			}
		}

		if (EditorGUILayout.LinkButton("Generate"))
		{
			mapGen.DrawMapInEditor();
		}
	}
}