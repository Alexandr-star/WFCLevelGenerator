using System;
using UnityEngine.Tilemaps;

namespace DataModel
{
	[Serializable]
	public class LevelMap
	{
		public int width;
		public int height;
		public Tilemap inputTilemap;
		public Tilemap tilemapOutput;
	}
}