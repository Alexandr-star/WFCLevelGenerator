using System;
using UnityEngine;

namespace DataModel
{
	[Serializable]
	public class Neighbor
	{
		[SerializeField]
		public string left;

		[SerializeField]
		public string right;
	}
}