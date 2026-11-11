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


        public CDFSampler(double[] probabilities, T[] associatedData)
        {
            _cdf = probabilities ?? throw new ArgumentNullException(nameof(probabilities));
            _associatedData = associatedData ?? throw new ArgumentNullException(nameof(associatedData));

            if (probabilities.Length != associatedData.Length)
                throw new ArgumentException("CDF array and associated data array must be the same length.");

            if (probabilities.Length == 0)
                throw new ArgumentException("Arrays cannot be empty.");

            _length = probabilities.Length;

            //make probabilities into CDF
            double cumulative = 0;
            for (int i = 0; i < _length; i++)
            {
                cumulative += probabilities[i];
                _cdf[i] = cumulative;
            }
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
