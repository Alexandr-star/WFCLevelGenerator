using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataModel
{
	[Serializable]
	public class Subset
	{
		[SerializeField]
		public string name;

		[SerializeField]
		public List<TileModel> tiles;
	}
}