using System;
using UnityEngine;
using System.Collections.Generic;
using DataModel;
using UnityEngine.Tilemaps;
using WFCModel;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class LevelGenerator : MonoBehaviour
{
	public LevelMap levelMap;
	public TextAsset jsonFile = null;
	public string subset = string.Empty;
	public int seed = 0;
	public bool periodic = false;
	public int iterations = 0;
	public SimpleTiledWFCModel WfcModel = null;
	public bool instantTilemapCollider;

	private TileBase[,] _renderingTile;
	private Dictionary<string, Tile> _obmapTile;
	private bool _undrawn = true;

	void Start()
	{
		Generate();
		Run();
	}

	/// <summary>
	/// Starts the level generation algorithm.
	/// </summary>
	/// <exception cref="ArgumentNullException"></exception>
	private void Run()
	{
		if (WfcModel == null)
		{
			throw new ArgumentNullException(nameof(levelMap.tilemapOutput), "LevelGenerator.Run(), WfcModel cannot be null.");
		}

		if (_undrawn == false) return;

		if (WfcModel.RunWFC(seed, iterations))
		{
			DrawTilemap();
		}
		else
		{
			throw new ArgumentNullException(nameof(LevelGenerator), "WFC found a contradiction.");
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(new Vector3(levelMap.width / 2f, levelMap.height / 2f, 0f),
			new Vector3(levelMap.width, levelMap.height, 0f));
	}

	/// <summary>
	/// Initializes the WFC algorithm model.
	/// </summary>
	public void Generate()
	{
		var inputTilesData = JsonUtility.FromJson<InputTilesData>(jsonFile.text);
		var subsetData = inputTilesData.GetTilesSubset(subset);
		_obmapTile = new Dictionary<string, Tile>();
		_renderingTile = new TileBase[levelMap.width, levelMap.height];
		WfcModel = new SimpleTiledWFCModel(inputTilesData, subsetData, levelMap.width, levelMap.height, periodic, levelMap);
		_undrawn = true;
	}

	/// <summary>
	/// Renders the level on the Tilemap.
	/// </summary>
	/// <exception cref="ArgumentNullException"></exception>
	internal void DrawTilemap()
	{
		if (levelMap.tilemapOutput == null)
		{
			throw new ArgumentNullException(nameof(levelMap.tilemapOutput), "LevelMap.tilemapOutput cannot be null.");
		}

		levelMap.tilemapOutput.ClearAllTiles();

		_undrawn = false;

		for (var y = 0; y < levelMap.height; y++)
		{ 
			for (var x = 0; x < levelMap.width; x++)
			{
				if (_renderingTile[x, y] != null) continue;
				
				var tileName = WfcModel.GetSample(x, y);

				if (tileName != null)
				{
					var rotation = byte.Parse(tileName.Substring(0,1));
					tileName = tileName.Substring(1);

					Tile tile;
					if (!_obmapTile.ContainsKey(tileName))
					{
						tile = (Tile)Resources.Load(tileName, typeof(Tile));
						_obmapTile[tileName] = tile;
					} 
					else
					{
						tile = _obmapTile[tileName];
					}

					if (tile == null) continue;

					var position = new Vector3Int(x, y, 0);
					var tileTransform = tile.transform;

					tileTransform.SetTRS(Vector3.zero, GetRotation(rotation), Vector3.one);

					tile.transform = tileTransform;
					tile.flags = TileFlags.LockTransform;

					levelMap.tilemapOutput.SetTile(position, tile);

					_renderingTile[x, y] = tile;
				} 
				else
				{
					_undrawn = true;
				}
			}
		}

		if (instantTilemapCollider)
		{
			SetColliderOnTilemap();
		}
	}

	private void SetColliderOnTilemap()
	{
		if (levelMap.tilemapOutput == null)
		{
			throw new ArgumentNullException(nameof(levelMap.tilemapOutput), "LevelGenerator.SetColliderOnTilemap(), LevelMap.tilemapOutput cannot be null.");
		}

		if (levelMap.tilemapOutput.GetComponent<TilemapCollider2D>() == null 
			&& levelMap.tilemapOutput.GetComponent<CompositeCollider2D>() == null)
		{
			levelMap.tilemapOutput.gameObject.AddComponent(typeof(TilemapCollider2D));
			levelMap.tilemapOutput.gameObject.AddComponent(typeof(CompositeCollider2D));
		}
	}

	private Quaternion GetRotation(byte mask)
	{
		switch (mask)
		{
			case 1:
				return Quaternion.Euler(0f, 0f, 270f);
			case 2:
				return Quaternion.Euler(0f, 0f, 180f);
			case 3:
				return Quaternion.Euler(0f, 0f, 90f);
		}

		return Quaternion.Euler(0f, 0f, 0f);
	}
}

#if UNITY_EDITOR
[CustomEditor (typeof(LevelGenerator))]
public class GeneratorEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		var me = (LevelGenerator)target;
		
		if (me.jsonFile != null)
		{
			if(GUILayout.Button("generate"))
			{
				me.Generate();
			}

			if (me.WfcModel != null)
			{
				if(GUILayout.Button("RUN"))
				{
					me.WfcModel.RunWFC(me.seed, me.iterations);
					me.DrawTilemap();
				}
			}
		}

		DrawDefaultInspector ();
	}
}
#endif