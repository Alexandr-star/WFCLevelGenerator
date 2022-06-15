using System;
using Helpers;
using Random = System.Random;

namespace WFCModel
{
	public abstract class AbstractWFCModel
	{
		protected bool[][] Wave;
		protected int[][][] Propagator;
		protected readonly int MX;
		protected readonly int MY;
		protected int T;
		protected bool Periodic;
		protected double[] Weights;

		private int[][][] _compatible;
		private int[] _observed;
		private Tuple<int, int>[] _stack;
		private int _stackSize;
		private Random _random;
		private int[] _sumsOfOnes;
		private double _sumOfWeights, _sumOfWeightLogWeights, _startingEntropy;
		private double[] _weightLogWeights, _sumsOfWeights, _sumsOfWeightLogWeights, _entropies;

		protected AbstractWFCModel(int width, int height)
		{
			MX = width;
			MY = height;
		}

		/// <summary>
		/// Start WFC.
		/// </summary>
		/// <param name="seed">All internal random values are derived from this seed, providing 0 results in a random number.</param>
		/// <param name="limit">How many iterations to run, providing 0 will run until completion or a contradiction.</param>
		/// <returns></returns>
		public bool RunWFC(int seed, int limit)
		{
			if (Wave == null)
			{
				Init();
			}

			Clear();

			_random = seed == 0 ? new Random() : new Random(seed);

			for (var l = 0; l < limit || limit == 0; l++)
			{
				var result = Observe();

				if (result != null)
					return (bool)result;

				Propagate();
			}

			return true;
		}

		private void Init()
		{
			Wave = new bool[MX * MY][];
			_compatible = new int[Wave.Length][][];

			for (var i = 0; i < Wave.Length; i++)
			{
				Wave[i] = new bool[T];
				_compatible[i] = new int[T][];

				for (var t = 0; t < T; t++)
				{
					_compatible[i][t] = new int[4];
				}
			}

			_observed = new int[MX * MY];
			_weightLogWeights = new double[T];
			_sumOfWeights = 0;
			_sumOfWeightLogWeights = 0;

			for (var t = 0; t < T; t++)
			{
				_weightLogWeights[t] = Weights[t] * Math.Log(Weights[t]);
				_sumOfWeights += Weights[t];
				_sumOfWeightLogWeights += _weightLogWeights[t];
			}

			_startingEntropy = Math.Log(_sumOfWeights) - _sumOfWeightLogWeights / _sumOfWeights;
			_sumsOfOnes = new int[MX * MY];
			_sumsOfWeights = new double[MX * MY];
			_sumsOfWeightLogWeights = new double[MX * MY];
			_entropies = new double[MX * MY];
			_stack = new Tuple<int, int>[Wave.Length * T];
			_stackSize = 0;
		}

		private bool? Observe()
		{
			if (TryGetUnobserveNode(out var unobserveNode))
			{
				if (unobserveNode < 0)
				{
					for (var i = 0; i < Wave.Length; i++)
					{
						for (var t = 0; t < T; t++)
						{
							if (Wave[i][t])
							{
								_observed[i] = t;
								break;
							}
						}
					}

					return true;
				}

				var distribution = new double[T];
				var waveValue = Wave[unobserveNode];

				for (var tile = 0; tile < T; tile++)
				{
					distribution[tile] = waveValue[tile]
						? Weights[tile] 
						: 0.0;
				}

				var randomTile = distribution.Random(_random.NextDouble());

				for (var tile = 0; tile < T; tile++)
				{
					if (waveValue[tile] != (tile == randomTile))
					{
						Ban(unobserveNode, tile);
					}
				}

				return null;
			}

			return false;
		}

		private bool TryGetUnobserveNode(out int unobserveNode)
		{
			var min = 1E+3;
			unobserveNode = -1;

			for (var i = 0; i < Wave.Length; i++)
			{
				if (OnBoundary(i % MX, i / MX)) continue;

				var remainingValues = _sumsOfOnes[i];

				if (remainingValues == 0) return false;

				var entropy = _entropies[i];

				if (remainingValues > 1 && entropy <= min)
				{
					var noise = 1E-6 * _random.NextDouble();

					if (entropy + noise < min)
					{
						min = entropy + noise;
						unobserveNode = i;
					}
				}
			}

			return true;
		}

		protected void Propagate()
		{
			while (_stackSize > 0)
			{
				var (item1, item2) = _stack[_stackSize - 1];
				_stackSize--;

				var x1 = item1 % MX;
				var y1 = item1 / MX;

				for (var d = 0; d < 4; d++)
				{
					var dx = DX[d];
					var dy = DY[d];
					var x2 = x1 + dx;
					var y2 = y1 + dy;

					if (OnBoundary(x2, y2)) continue;

					if (x2 < 0)
					{
						x2 += MX;
					}
					else if (x2 >= MX)
					{
						x2 -= MX;
					}

					if (y2 < 0)
					{
						y2 += MY;
					}
					else if (y2 >= MY)
					{
						y2 -= MY;
					}

					var i2 = x2 + y2 * MX;
					var p = Propagator[d][item2];
					var compat = _compatible[i2];

					foreach (var t2 in p)
					{
						var comp = compat[t2];
						comp[d]--;

						if (comp[d] == 0)
						{
							Ban(i2, t2);
						}
					}
				}
			}
		}

		protected void Ban(int i, int t)
		{
			Wave[i][t] = false;

			var comp = _compatible[i][t];

			for (var d = 0; d < 4; d++)
			{
				comp[d] = 0;
			}

			_stack[_stackSize] = new Tuple<int, int>(i, t);
			_stackSize++;

			_sumsOfOnes[i] -= 1;
			_sumsOfWeights[i] -= Weights[t];
			_sumsOfWeightLogWeights[i] -= _weightLogWeights[t];

			var sum = _sumsOfWeights[i];
			_entropies[i] -= _sumsOfWeightLogWeights[i] / sum - Math.Log(sum);
		}

		protected virtual void Clear()
		{
			for (var i = 0; i < Wave.Length; i++)
			{
				for (var t = 0; t < T; t++)
				{
					Wave[i][t] = true;
					
					for (var d = 0; d < 4; d++)
					{
						_compatible[i][t][d] = Propagator[opposite[d]][t].Length;
					}
				}

				_sumsOfOnes[i] = Weights.Length;
				_sumsOfWeights[i] = _sumOfWeights;
				_sumsOfWeightLogWeights[i] = _sumOfWeightLogWeights;
				_entropies[i] = _startingEntropy;
				_observed[i] = -1;
			}
		}

		protected abstract bool OnBoundary(int x, int y);

		private static int[] DX = { -1, 0, 1, 0 };

		private static int[] DY = { 0, 1, 0, -1 };

		private static int[] opposite = { 2, 3, 0, 1 };
	}
}