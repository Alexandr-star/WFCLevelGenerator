using System.Linq;

namespace Helpers
{
	public static class Helper
	{
		public static int Random(this double[] a, double r)
		{
			var sum = a.Sum();

			if (sum == 0)
			{
				for (int j = 0; j < a.Count(); j++) a[j] = 1;
				sum = a.Sum();
			}

			for (int j = 0; j < a.Count(); j++)
			{
				a[j] /= sum;
			}

			var i = 0;
			var x = 0.0;

			while (i < a.Count())
			{
				x += a[i];
				if (r <= x) return i;
				i++;
			}

			return 0;
		}
	}
}