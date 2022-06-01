using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(MapGenirator))]
public class MapGeniratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenirator mapGen = (MapGenirator)target;

        if(DrawDefaultInspector())
        {
            if(mapGen.autoUpdate)
            {
                mapGen.GenerateMap();
            }    
        }

        if (EditorGUILayout.LinkButton("Generate"))
        {
            mapGen.GenerateMap();
        }   
    }
}
