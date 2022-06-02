using UnityEngine;
using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

public class MapGenirator : MonoBehaviour
{
	public enum DrawMode{NoiseMap, ColourMap, Mesh, FallofMap};
	public DrawMode drawMode;

	const int mapChunkSize = 241;	 
	[Range(0, 6)]
	public int levelOfDetail;
	public float noiseScale;

	public int octaves;
	[Range(0, 1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public bool useFallof;

	public float meshHeightMultiplayer;
	public AnimationCurve meshHeightCurve;

	public bool autoUpdate;

	public TerrainType[] regions;

	float[,] fallofMap;

	Queue<MapThreadInfo<MapDatta>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapDatta>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    void Awake()
    {
        fallofMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    public void DrawMapInEditor()
    {
		MapDatta mapDatta = GenerateMapDatta();

		MapDisplay display = FindObjectOfType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapDatta.heightMap));
		}
		else if (drawMode == DrawMode.ColourMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromColourMap(mapDatta.colourMap, mapChunkSize, mapChunkSize));
		}
		else if (drawMode == DrawMode.Mesh)
		{
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapDatta.heightMap, meshHeightMultiplayer, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(mapDatta.colourMap, mapChunkSize, mapChunkSize));
		}
		else if(drawMode == DrawMode.FallofMap)
        {
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
	}

	public void RequestMapData(Action<MapDatta> callback)
    {
		ThreadStart threadStart = delegate
		{
			MapDataThread(callback);
		};

		new Thread(threadStart).Start();
    }
	
	void MapDataThread(Action<MapDatta> callback)
    {
		MapDatta mapDatta = GenerateMapDatta();
		lock (mapDataThreadInfoQueue)
        {
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapDatta>(callback, mapDatta));
		}
    }

	void MeshDataThread(MapDatta mapData, int lod, Action<MeshData> callback)
	{
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplayer, meshHeightCurve, lod);
		lock (meshDataThreadInfoQueue)
		{
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
		}
	}

	void Update()
    {
        if(mapDataThreadInfoQueue.Count > 0)
        {
			for(int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
				 MapThreadInfo<MapDatta> threadInfo = mapDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
            }
        }

		if(meshDataThreadInfoQueue.Count > 0)
        {
			for(int i =0; i < meshDataThreadInfoQueue.Count; i++)
            {
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
				threadInfo.callback (threadInfo.parameter);
            }
        }
    }

    MapDatta GenerateMapDatta()
	{
		float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

		Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
		for(int y = 0; y < mapChunkSize; y++)
		{
			for(int x = 0; x < mapChunkSize; x++)
			{
				if(useFallof)
                {
					noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallofMap[x, y]); 
                }

				float currentHeight = noiseMap[x, y];

				for(int i = 0; i < regions.Length; i++)
				{
					if(currentHeight <= regions[i].height)
					{
						colourMap[y * mapChunkSize + x] = regions[i].colour;
						break;
					}
				}
			}
		}

		return new MapDatta(noiseMap, colourMap);
	}

	void OnValidate()
	{
		if(lacunarity < 1)
		{
			lacunarity = 1;
		}
		if(octaves < 0)
		{
			octaves = 0;
		}
		fallofMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
	}

	struct MapThreadInfo<T>
    {
		public readonly Action<T> callback;
		public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color colour;
}

public struct MapDatta
{
	public readonly float[,] heightMap;
	public readonly Color[] colourMap;

    public MapDatta(float[,] heightMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap; 
    }
}