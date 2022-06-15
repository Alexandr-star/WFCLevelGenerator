using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataModel
{
	[Serializable]
	public class InputTilesData
	{
		[SerializeField]
		public bool unique;

		[SerializeField]
		public List<TileModel> tiles;

		[SerializeField]
		public List<Neighbor> neighbors;

		[SerializeField]
		public List<Subset> subsets;

		public List<string> GetTilesSubset(string subsetName)
		{
			var subset = new List<string>();


			if (subsetName == string.Empty) return subset;

			foreach (var subsetData in subsets)
			{
				if (subsetData.name != subsetName) continue;

				foreach (var subsetTile in subsetData.tiles)
				{
					subset.Add(subsetTile.name);
				}
			}

			return subset;
		}
	}
}