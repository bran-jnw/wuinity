//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

//#define USE_OLD_METHOD
namespace WUIPlatform.Fire
{
    public class Extension
    {
        public double m_harmonicRos;   //!< cumulative harmonic mean of faster spread rates p/r
        public double m_averageRos;    //!< cumulative average of faster spread rates p/r
        public double m_extHarmonicRos;//!< cumulative harmonic mean of faster spread rates p/r
        public double m_extAverageRos; //!< cumulative average of faster spread rates p/r
        public double m_extCuumProb;   //!< cumulative prob of faster spread rates in extensions
        public Extension m_nextExt;       //!< link to next Extension
        public RandFuel m_rf;            //!< pointer to calculations of RandFuel
        public double[] m_maxRosArray;   //!< 1x array with resulting max spread rates for each block
        public double[][] m_combArray;     //!< 2x array with probabilities in it
        public double[][] m_rosArray;      //!< 2x array with spread rates in it

        double m_prob;          //!< prob of faster spread rates
        double m_latProb;       //!< Probability of lateral fuel combination
        long m_blocks;        //!< number of allocations (blocks)
        long m_cols;          //!< number of columns in block
        long m_rows;          //!< number of rows in block
        long m_cells;         //!< number of cells in block
        long m_fuels;         //!< number of fuel types
        long m_leeFuels;      //!< number of lee fuel combinations
        char m_blockPtr;      //!< Pointer to single dynamic memory block
        double[] m_cuumProb;      //!< cumulative prob of faster spread rates
        double[][] m_latRosArray;   //!< lee side spread rates and probabilities
        double[][] m_latCombArray;  //!< lee side spread rates and probabilities

        Extension()
        {
            init();
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Extension destructor.
        */
        ~Extension()
        {
            freeExtension();
            return;
        }

        //------------------------------------------------------------------------------

        public bool allocExtension(long p_blocks, long p_cols, long p_rows, long p_fuels)
        {
            freeExtension();

            m_blocks = p_blocks;
            m_cols = p_cols;
            m_rows = p_rows;
            m_fuels = p_fuels;
            m_cells = m_cols * m_rows;
            m_leeFuels = m_fuels * m_fuels;

#if USE_OLD_METHOD
            // Allocate one large block of doubles for the following:
            //  Variable        Doubles                 Pointers
            //  m_combArray     m_blocks * m_cells      m_blocks
            //  m_rosArray      m_blocks * m_cells      m_blocks
            //  m_cuumProb      m_blocks                0
            //  m_maxRosArray   m_blocks                0
            int doubles = 2 * m_blocks + 2 * (m_blocks * m_cells);
            int pointers = 2 * m_blocks;
            int blockSize = pointers * sizeof(double*) + doubles * sizeof(double);
            m_blockPtr = new char[blockSize];
            if (!m_blockPtr)
            {
                return (false);
            }
            //memset(m_blockPtr, 0x0, blockSize * sizeof(char));

            int i, len;
            // Start of m_combArray of pointers to doubles
            char* cPtr = m_blockPtr;
            m_combArray = (double**)cPtr;
            len = m_blocks * sizeof(double*);
            cPtr += len;         // Move to end of pointer array
            // Set pointers to start of each cell block of doubles
            for (i = 0; i < m_blocks; i++)
            {
                m_combArray[i] = (double*)cPtr;
                cPtr += (m_cells * sizeof(double));
            }

            // Start of m_rosArray of pointers to doubles
            m_rosArray = (double**)cPtr;
            len = m_blocks * sizeof(double*);
            cPtr += len;           // Move to end of pointer array
            // Set pointers to start of each cell block of doubles
            for (i = 0; i < m_blocks; i++)
            {
                m_rosArray[i] = (double*)cPtr;
                cPtr += (m_cells * sizeof(double));
            }

            // Start of m_maxRosArray data array of doubles
            m_maxRosArray = (double*)cPtr;
            len = m_blocks * sizeof(double);
            cPtr += len;
            // Start of m_cuumProb data array of doubles
            m_cuumProb = (double*)cPtr;

            return (true);
#else
            m_combArray = new double[m_blocks][];
            if (m_combArray == null)
            {
                return (false);
            }

            m_rosArray = new double[m_blocks][];
            if (m_rosArray == null)
            {
                freeExtension();
                return (false);
            }

            for (long i = 0; i < m_blocks; i++)
            {
                m_combArray[i] = new double[m_cells];
                if (m_combArray[i] == null)
                {
                    freeExtension();
                    return (false);
                }
                m_rosArray[i] = new double[m_cells];
                if (m_rosArray[i] == null)
                {
                    freeExtension();
                    return (false);
                }
            }

            m_cuumProb = new double[m_blocks];
            if (m_cuumProb == null)
            {
                freeExtension();
                return (false);
            }
            //memset(m_cuumProb, 0x0, m_blocks * sizeof(double));

            m_maxRosArray = new double[m_blocks];
            if (m_maxRosArray == null)
            {
                freeExtension();
                return (false);
            }
            //memset(m_maxRosArray, 0x0, m_blocks * sizeof(double));

            return (true);
#endif
        }

        //------------------------------------------------------------------------------
        /*! \brief  Calculates the probability of the fuel arrangement occurring
         *  and subtracts the probability of faster combinations from occurring
         *  in outer lateral extensions (m_cuumProb[])
         *
         *  \param BlockNum Index to the sample block to calculate the probability
         *  returns the probability
         */

        public double calcProb(long p_block, bool p_subtFaster)
        {
            double prob = 1.0;

            for (long i = 0; i < m_cells; i++)
            {
                prob *= ((double)m_combArray[p_block][i]);
            }
            prob *= m_latProb;
            if (p_subtFaster)
            {
                prob -= getFasterProbs(p_block);
            }
            return (prob);
        }

        //------------------------------------------------------------------------------
        /*! \brief Frees all dynamically allocated memory and reset all data to
         *  initial values.
         */

        void freeExtension()
        {
#if USE_OLD_METHOD
            /*if (m_blockPtr)
            {
                delete[] m_blockPtr;
            }*/
            m_blockPtr = null;
#else
            for (int i = 0; i < m_blocks; i++)
            {
                if (m_combArray != null)
                {
                    if (m_combArray[i] != null)
                    {
                        m_combArray[i] = null;
                    }
                }
                if (m_rosArray != null)
                {
                    if (m_rosArray[i] != null)
                    {
                        m_rosArray[i] = null;                        
                    }
                }
            }
            if (m_combArray != null)
            {
                m_combArray = null;
            }
            if (m_rosArray != null)
            {
                m_rosArray = null;                
            }
            if (m_cuumProb != null)
            {
                m_cuumProb = null;
            }
            if (m_maxRosArray != null)
            {
                m_maxRosArray = null;
            }
#endif
            init();
            return;
        }

        //------------------------------------------------------------------------------

        double getFasterProbs(long p_block)
        {
            double prob = m_cuumProb[p_block];
            Extension xt = m_nextExt;
            while (xt != null)
            {
                for (int i = 0; i < xt.m_blocks; i++)
                {
                    prob += xt.m_cuumProb[i];
                }
                xt = xt.m_nextExt;
            }
            return (prob);
        }

        //------------------------------------------------------------------------------

        void init()
        {
            // Public data
            m_harmonicRos = 0.0;
            m_averageRos = 0.0;
            m_extHarmonicRos = 0.0;
            m_extAverageRos = 0.0;
            m_extCuumProb = 0.0;
            m_nextExt = new Extension();
            m_rf = null;
            m_maxRosArray = null;
            m_combArray = null;
            m_rosArray = null;

            // Private data
            m_prob = 0.0;
            m_latProb = 0.0;
            m_blocks = 0;
            m_cols = 0;
            m_rows = 0;
            m_cells = 0;
            m_fuels = 0;
            m_leeFuels = 0;
            m_cuumProb = null;
            m_blockPtr = new char();
            m_latRosArray = null;
            m_latCombArray = null;
            return;
        }

        //------------------------------------------------------------------------------

        public void run(long p_lats, ref double[] p_latRos, ref double[] p_latComb, double p_maxRos)
        {
            // Initialize
            m_harmonicRos = 0.0;
            m_averageRos = 0.0;
            m_extHarmonicRos = 0.0;
            m_extAverageRos = 0.0;
            double maxRos = p_maxRos;

            //
            m_rf.calcExtendedSpreadRates2(m_cols, m_rows, m_blocks, ref m_combArray, ref m_rosArray, ref p_latRos, ref m_maxRosArray, p_lats);
            long oldLats = 2 * p_lats;
            m_latProb = 1.0;
            long i;
            for (i = 0; i < oldLats; i++)
            {
                m_latProb *= p_latComb[i];
            }
            //memset(m_cuumProb, 0x0, m_blocks * sizeof(double));     //*m_leeFuels

            // Return if this is the last extension
            if (m_nextExt == null)
            {
                return;
            }

            long newLats = 2 * (p_lats + 1);
            double[] latros2 = new double[newLats];
            double[] latcomb2 = new double[newLats];
            long fuelCombs = 0;
            long j, k;

            m_rf.calcCombinations(1, 2, ref fuelCombs, ref m_latCombArray, ref m_latRosArray);
            for (i = 0; i < m_blocks; i++)
            {
                m_rf.spliceExtensions2(ref m_combArray[i], ref m_rosArray[i], ref m_nextExt.m_combArray, ref m_nextExt.m_rosArray, m_cols);
                for (j = 0; j < fuelCombs; j++)
                {
                    latros2[0] = m_latRosArray[j][0];
                    //memcpy(&latros2[1], p_latRos, oldLats * sizeof(double));
                    latros2[1] = p_latRos[0];
                    latros2[p_lats * 2 + 1] = m_latRosArray[j][1];
                    latcomb2[0] = m_latCombArray[j][0];
                    //memcpy(&latcomb2[1], p_latComb, oldLats * sizeof(double));
                    latcomb2[1] = p_latComb[0];
                    latcomb2[p_lats * 2 + 1] = m_latCombArray[j][1];

                    m_nextExt.run(p_lats + 1, ref latros2, ref latcomb2, m_maxRosArray[i]);
                    for (k = 0; k < m_blocks; k++)
                    {
                        if (m_nextExt.m_maxRosArray[k] > maxRos && m_nextExt.m_maxRosArray[k] > m_maxRosArray[i])
                        {
                            m_prob = m_nextExt.calcProb(k, true);
                            m_cuumProb[i] += m_prob;
                            m_harmonicRos += m_prob / m_nextExt.m_maxRosArray[k];
                            m_averageRos += m_prob * m_nextExt.m_maxRosArray[k];
                        }
                    }
                    // accumulate harmonic ros from next extension
                    m_extHarmonicRos += m_nextExt.m_harmonicRos;
                    // accumulate average ros from next extension
                    m_extAverageRos += m_nextExt.m_averageRos;
                }
                // Accumulate cumulative probabilities from next extension
                m_extCuumProb += m_cuumProb[i];
            }
            if (m_latCombArray != null)
            {
                for (i = 0; i < fuelCombs; i++)
                {
                    m_latCombArray[i] = null;
                }
                m_latCombArray = null;
            }
            if (m_latRosArray != null)
            {
                for (i = 0; i < fuelCombs; i++)
                {
                    m_latRosArray[i] = null;
                }
                m_latRosArray = null;
            }
            if (latros2 != null)
            {
                latros2 = null;
            }
            if (latcomb2 != null)
            {
                latcomb2 = null;
            }
            return;
        }
    }
}