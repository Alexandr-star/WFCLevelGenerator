using System;
using System.Collections.Generic;
using System.Linq;
using DataModel;
using UnityEngine;

namespace WFCModel
{
	public class SimpleTiledWFCModel : AbstractWFCModel
	{
		private readonly List<string> _tiles = new List<string>();
		private readonly Dictionary<string, int> _tilesWithNumber = new Dictionary<string, int>();
		private readonly LevelMap _levelMap;
		private readonly List<int[]> _action = new List<int[]>();

		public SimpleTiledWFCModel(InputTilesData inputTilesData, List<string> subset, int width, int height, bool periodic, LevelMap levelMap = null)
			:base(width,height)
		{
			_levelMap = levelMap;
			Periodic = periodic;

			TilesProcessing(inputTilesData, subset);
			InitPropagator(inputTilesData, subset);
		}

		/// <summary>
		/// Retrieves data about the neighborhood of tiles.
		/// </summary>
		/// <param name="inputTilesData">Data about a tiles.</param>
		/// <param name="subset">Subset name.</param>
		private void InitPropagator(InputTilesData inputTilesData, List<string> subset)
		{
			Propagator = new int[4][][];
			var tempPropagator = new bool[4][][];

			for (var d = 0; d < 4; d++)
			{
				tempPropagator[d] = new bool[T][];
				Propagator[d] = new int[T][];

				for (var t = 0; t < T; t++)
				{
					tempPropagator[d][t] = new bool[T];
				}
			}

			foreach (var neighborData in inputTilesData.neighbors)
			{
				var left = neighborData.left.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
				var right = neighborData.right.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);

				if (IsTileInSubset(left[0], subset) || IsTileInSubset(right[0], subset)) continue;

				var leftTile = GetTileFromAction(left); 
				var downTile = GetTileFromAction(leftTile, 1);
				var rightTile = GetTileFromAction(right);
				var upTile = GetTileFromAction(rightTile, 1);

				tempPropagator[0][rightTile][leftTile] = true;
				tempPropagator[0][GetTileFromAction(rightTile, 6)][GetTileFromAction(leftTile, 6)] = true;
				tempPropagator[0][GetTileFromAction(leftTile, 4)][GetTileFromAction(rightTile, 4)] = true;
				tempPropagator[0][GetTileFromAction(leftTile, 2)][GetTileFromAction(rightTile, 2)] = true;

				tempPropagator[1][upTile][downTile] = true;
				tempPropagator[1][GetTileFromAction(downTile, 6)][GetTileFromAction(upTile, 6)] = true;
				tempPropagator[1][GetTileFromAction(upTile, 4)][GetTileFromAction(downTile, 4)] = true;
				tempPropagator[1][GetTileFromAction(downTile, 2)][GetTileFromAction(upTile, 2)] = true;
			}

			for (var t2 = 0; t2 < T; t2++) 
			{
				for (var t1 = 0; t1 < T; t1++)
				{
					tempPropagator[2][t2][t1] = tempPropagator[0][t1][t2];
					tempPropagator[3][t2][t1] = tempPropagator[1][t1][t2];
				}
			}

			var sparsePropagator = new List<int>[4][];

			for (var d = 0; d < 4; d++)
			{
				sparsePropagator[d] = new List<int>[T];

				for (var t = 0; t < T; t++)
				{
					sparsePropagator[d][t] = new List<int>();
				}
			}

			for (var d = 0; d < 4; d++)
			{
				for (var t1 = 0; t1 < T; t1++)
				{
					var sp = sparsePropagator[d][t1];
					var tp = tempPropagator[d][t1];

					for (var t2 = 0; t2 < T; t2++)
					{
						if (tp[t2])
						{
							sp.Add(t2);
						}
					}

					var spCount = sp.Count;
					Propagator[d][t1] = new int[spCount];

					for (var st = 0; st < spCount; st++)
					{
						Propagator[d][t1][st] = sp[st];
					}
				}
			}
		}

		/// <summary>
		/// Extract the tile data from <cref>InputTilesData</cref>.
		/// </summary>
		/// <param name="inputTilesData">Data about a tiles.</param>
		/// <param name="subset">Subset name.</param>
		private void TilesProcessing(InputTilesData inputTilesData, List<string> subset)
		{
			var unique = inputTilesData.unique;

			var tempStationary = new List<double>();

			foreach (var tileData in inputTilesData.tiles)
			{
				var tileName = tileData.name;
				if (IsTileInSubset(tileName, subset)) continue;

				Func<int, int> a, b;
				int cardinality;
				var symmetryType = tileData.symmetry == null ? SymmetryType.X : Enum.Parse<SymmetryType>(tileData.symmetry);

				switch (symmetryType)
				{
					case SymmetryType.L:
						cardinality = 4;
						a = i => (i + 1) % 4;
						b = i => i % 2 == 0 ? i + 1 : i - 1;
						break;

					case SymmetryType.T:
						cardinality = 4;
						a = i => (i + 1) % 4;
						b = i => i % 2 == 0 ? i : 4 - i;
						break;

					case SymmetryType.I:
						cardinality = 2;
						a = i => 1 - i;
						b = i => i;
						break;

					case SymmetryType.Slash:
						cardinality = 2;
						a = i => 1 - i;
						b = i => 1 - i;
						break;

					default:
						cardinality = 1;
						a = i => i;
						b = i => i;
						break;
				}

				T = _action.Count;
				_tilesWithNumber.Add(tileName, T);
				var map = new int[cardinality][];

				for (var t = 0; t < cardinality; t++)
				{
					map[t] = new int[8];

					map[t][0] = t;
					map[t][1] = a(t);
					map[t][2] = a(a(t));
					map[t][3] = a(a(a(t)));
					map[t][4] = b(t);
					map[t][5] = b(a(t));
					map[t][6] = b(a(a(t)));
					map[t][7] = b(a(a(a(t))));

					for (var s = 0; s < 8; s++)
					{
						map[t][s] += T;
					}

					_action.Add(map[t]);
				}

				if (unique)
				{
					for (var t = 0; t < cardinality; t++)
					{
						_tiles.Add("" + "0" + tileName);
					}
				}
				else
				{
					_tiles.Add("0" + tileName);

					for (var t = 1; t < cardinality; t++)
					{
						_tiles.Add(Rotate(_tiles[T + t - 1]));
					}
				}

				for (var t = 0; t < cardinality; t++)
				{
					tempStationary.Add(tileData.weight != 0 ? tileData.weight : 1.0f);
				}
			}

			T = _action.Count;
			Weights = tempStationary.ToArray();
		}

		private bool IsTileInSubset(string tileName, List<string> subset)
		{
			return subset.Count != 0 && !subset.Contains(tileName);
		}

		/// <summary>
		/// Retrieves tiles from the <cref>LevelMap.inputTilemap</cref> and sets them in the wave.
		/// </summary>
		protected override void Clear()
		{
			base.Clear();

			if (_levelMap.inputTilemap == null)
			{
				return;
			}

			var bounds = new BoundsInt
			{
				position = Vector3Int.zero,
				xMax = _levelMap.width,
				xMin = _levelMap.height
			};

			var tilesBlock = _levelMap.inputTilemap.GetTilesBlock(bounds);

			for (var indexTileInWave = 0; indexTileInWave < tilesBlock.Length; indexTileInWave++)
			{
				if (tilesBlock[indexTileInWave] == null) continue;

				var indexTile = _tilesWithNumber
					.First(x => x.Key.Contains(tilesBlock[indexTileInWave].name)).Value;

				for (var tile = 0; tile < T; tile++)
				{
					if (tile == indexTile) continue;

					Ban(indexTileInWave, tile);
				}

				Propagate();
			}
		}

		private string Rotate(string name)
		{
			var rotate = int.Parse(name.Substring(0, 1)) + 1;

			return string.Empty + rotate + name.Substring(1);
		}

		/// <summary>
		/// Returns the tile name from the Wave.
		/// </summary>
		/// <param name="x">Tile x coordinate</param>
		/// <param name="y">Tile y coordinate</param>
		/// <returns>Имя тайла</returns>
		public string GetSample(int x, int y)
		{
			var found = false;
			string resource = null;

			for (var t = 0; t < T; t++) 
			{
				if (Wave[x + y * MX][t])
				{
					if (found) return null;

					found = true;
					resource = _tiles[t];
				}

			}

			return resource;
		}

		private int GetTileFromAction(string[] tileName)
		{
			var firstIndex = _tilesWithNumber[string.Join(" ", tileName.Take(tileName.Length - 1).ToArray())];
			var secondIndex = tileName.Length == 1 ? 0 : int.Parse(tileName.Last());

			return GetTileFromAction(firstIndex, secondIndex);
		}

		private int GetTileFromAction(int firstIndex, int secondIndex)
		{
			return _action[firstIndex][secondIndex];
		}

		protected override bool OnBoundary(int x, int y)
		{
			return !Periodic && (x < 0 || y < 0 || x >= MX || y >= MY);
		}
	}
}