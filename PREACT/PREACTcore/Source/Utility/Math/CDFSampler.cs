using System;

namespace PREACT.Utility.Math
{
    /// <summary>
    /// A generic sampler for drawing samples from a cumulative distribution function (CDF).
    /// </summary>
    public class CDFSampler<T>
    {
        private readonly double[] _cdf;
        private readonly T[] _associatedData;
        private readonly int _length;

        public CDFSampler(double[] cdf, T[] associatedData)
        {
            _cdf = cdf ?? throw new ArgumentNullException(nameof(cdf));
            _associatedData = associatedData ?? throw new ArgumentNullException(nameof(associatedData));

            if (cdf.Length != associatedData.Length)
                throw new ArgumentException("CDF array and associated data array must be the same length.");

            if (cdf.Length == 0)
                throw new ArgumentException("Arrays cannot be empty.");

            _length = cdf.Length;
        }

        /// <summary>
        /// Draws a random uniform number and returns the associated generic data item.
        /// </summary>
        public T DrawSample()
        {
            //uniform random draw scaled by the final cumulative element
            double target = PREACT.Math.Random.valueD * _cdf[^1];

            //Linear scan optimization for small arrays (highly likely for categories)
            if (_length <= 12)
            {
                for (int i = 0; i < _length; i++)
                {
                    if (_cdf[i] >= target)
                    {
                        return _associatedData[i];
                    }
                }
                return _associatedData[_length - 1];
            }

            //Binary search fallback for larger data arrays
            int index = Array.BinarySearch(_cdf, target);

            if (index < 0)
            {
                index = ~index;
            }

            if (index >= _length)
            {
                index = _length - 1;
            }

            return _associatedData[index];
        }
    }
}
