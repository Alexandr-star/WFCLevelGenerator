using System;
using UnityEngine;

namespace DataModel
{
	[Serializable]
	public class TileModel
	{
		[SerializeField]
		public string name;

		[SerializeField]
		public string symmetry;

		[SerializeField]
		public double weight;
	}
}