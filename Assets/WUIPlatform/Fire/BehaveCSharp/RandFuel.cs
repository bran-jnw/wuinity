//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using static WUIPlatform.Fire.MathWrap;

namespace WUIPlatform.Fire
{
    public struct FuelType
    {
        public double m_relRos;   //!< relative spread rate 0-1
        public double m_absRos;   //!< actual spread rate
        public double m_fract;    //!< fraction of landscape occupied
    }

    public class RandFuel
    {
        long m_samples;          //!< number of cols in block
        long m_depths;           //!< number rows in block
        long m_fuels;            //!< number of fuel types
        long m_combs;            //!< number of combinations in block
        long m_exts;             //!< number of combinations in extension
        long m_threads;          //!< number of threads allocated (<64)
        long m_lessIgns;         //!< number of ignition points FEWER than NumSamples;
        double m_lbRatio;          //!< length to breadth ratio of fire
        double m_cellSize;         //!< size of raster cell
        double[][] m_combArray;        //!< array of block probabilities
        double[][] m_rosArray;         //!< array of spread rates in block
        double[][] m_combExtArray;     //!< lateral extension array of prob
        double[][] m_rosExtArray;      //!< lateral extension array of ros
        double[] m_maxRosArray;      //!< max spread rate for all blocks
        double[] m_maxRosExtArray;   //!< max spread rate for all blocks in extension
        FuelType[] m_fuelTypeArray;    //!< array of FuelType structs
        RandThread[] m_randThread;       //!< array of RandThread classes=m_threads

        public RandFuel()
        {
            init();
            return;
        }

        ~RandFuel()
        {
            freeBlockArrays();
            /*if (m_maxRosArray != null)
            {
                m_maxRosArray = null;
            }
            m_maxRosArray = 0*/
            m_maxRosArray = null;
            freeFuels();
            return;
        }

        public bool allocFuels(long p_fuels)
        {
            freeFuels();
            m_fuelTypeArray = new FuelType[p_fuels];
            if (m_fuelTypeArray == null)
            {
                return (false);
            }
            m_fuels = p_fuels;
            return (true);
        }

        //------------------------------------------------------------------------------

        bool allocRandThreads()
        {
            closeRandThreads();
            m_randThread = new RandThread[m_threads];
            if (m_randThread == null)
            {
                return (false);
            }
            return (true);
        }


        //------------------------------------------------------------------------------
        /*! \brief
         *
         *  Performs the following steps:
         *  -#  Calculates factorial combinations of all fuel types,
         *  -#  puts probabilities into *comb and spread rates into *ros
         *  -#  fills **ca with factorial combinations of probabilities (from comb)
         *      for all rows and columns in test block ( p_nX x p_nY )
         *  -#  Fills **ra with factorial combinations of spread rates (from ros)
         *      for all rows and columns in test block ( p_nX x p_nY )
         */

        public bool calcCombinations(long p_nX, long p_nY, ref long p_nT, ref double[][] p_ca, ref double[][] p_ra)
        {
            double[] comb;                          // array of probability distribution
            double[] ros;                           // array of spread rate distribution

            ulong types = (ulong)(pow(m_fuels, p_nX) * p_nX);
            comb = new double[types];
            if (comb == null)
            {
                return (false);
            }
            ////memset(comb, 0x0, types * sizeof(double));

            ros = new double[types];
            if (ros == null)
            {
                comb = null;
                return (false);
            }
            //memset(ros, 0x0, types * sizeof(double));

            // calculate the combinations that form the breadth of the fuel patch
            long terms = 1;
            long i, j, k, m, n, p, q;
            for (i = 0; i < p_nX; i++)
            {
                m = 0;
                for (j = 0; j < m_fuels; j++)
                {
                    for (k = 0; k < terms; k++)
                    {
                        comb[m + i] = m_fuelTypeArray[j].m_fract;
                        ros[m + i] = m_fuelTypeArray[j].m_relRos;
                        m += p_nX;
                    }
                }
                if (i < (p_nX - 1))
                {
                    m = 1;
                    do
                    {
                        //memcpy(&comb[m * m_fuels * p_nX * terms], &comb[0], m_fuels * terms * p_nX * sizeof(double));
                        //memcpy(&ros[m * m_fuels * p_nX * terms], &ros[0], m_fuels * terms * p_nX * sizeof(double));
                        comb[m * m_fuels * p_nX * terms] = comb[0];
                        ros[m * m_fuels * p_nX * terms] = ros[0];
                        m += 1;
                    } while (m < m_fuels);
                }
                terms *= m_fuels;
            }

            long cols = (long)pow((double)m_fuels, (int)p_nX);
            //if (p_nT)
            //{
                p_nT = (long)pow((double)cols, (int)p_nY);
            //}

            p_ca = new double[p_nT][];
            if (p_ca == null)
            {
                comb = null;
                ros = null;
                return false;
            }

            p_ra = new double[p_nT][];
            if (p_ra == null)
            {
                comb = null;
                ros = null;
                p_ca = null;
                return false;
            }

            // calculate block array probabilities and spread rates
            for (i = 0; i < p_nT; i++)
            {
                p_ca[i] = new double[p_nX * p_nY];
                p_ra[i] = new double[p_nX * p_nY];
            }

            terms = 1;
            for (i = 0; i < p_nY; i++)
            {
                m = 0;
                for (j = 0; j < cols; j++)
                {
                    for (k = 0; k < terms; k++)
                    {
                        //memcpy(&((*p_ca)[m][i * p_nX]), &comb[j * p_nX], p_nX * sizeof(double));
                        //memcpy(&((*p_ra)[m++][i * p_nX]), &ros[j * p_nX], p_nX * sizeof(double));
                        p_ca[m][i * p_nX] = comb[j * p_nX];
                        p_ra[m++][i * p_nX] = ros[j * p_nX];
                    }
                }
                n = cols * m;
                if (i < (p_nY - 1))
                {
                    q = m;
                    do
                    {
                        for (p = 0; p < q; p++)
                        {
                            //memcpy(&((*p_ca)[m][0]), &((*p_ca)[p][0]), p_nY * p_nX * sizeof(double));
                            //memcpy(&((*p_ra)[m++][0]), &((*p_ra)[p][0]), p_nY * p_nX * sizeof(double));'
                            p_ca[m][0] = p_ca[p][0];
                            p_ra[m++][0]= p_ra[p][0];
                        }
                    } while (m < n);
                }
                terms *= cols;
            }

            comb  = null;
            ros = null;
            return (true);
        }

        //-----------------------------------------------------------------------------------
        /*! \brief Calculates the spread rates for a sample block with
         *  count=p_laterals extensions (columns) on either side.
         *
         *  \param p_cols        Number of columns of the sample block.
         *  \param p_rows        Number of rows of the sample block.
         *  \param p_laterals    Number of fuel arrangements in the block to calculate.
         *  \param p_combArray   2d array of probabilities for the block.
         *  \param p_rosArray    2d array of spread rates for the block.
         *  \param p_latRosArray Array of spread rates in the new lee-side row.
         *  \param p_maxRosExtArray The return array with the maximum spread rates
         *                      for count=NumLatCombs arrangements.
         */

        public void calcExtendedSpreadRates2(long p_cols, long p_rows, long p_latCombs, ref double[][] p_combArray, ref double[][] p_rosArray, ref double[] p_latRosArray, ref double[] p_maxRosExtArray, long p_laterals)
        {
            double ipart = 0.0;
            double interval = ((double)p_latCombs) / ((double)m_threads);
            double fract = modf(interval, ref ipart);
            long range = (long)interval;
            if (fract > 0.0)
            {
                range++;
            }
            long begin = 0;
            long end;
            for (int i = 0; i < m_threads; i++)
            {
                end = begin + range;
                if (begin >= p_latCombs)
                {
                    continue;
                }
                if (end > p_latCombs)
                {
                    end = p_latCombs;
                }
                m_randThread[i].setThreadData(p_cols, p_rows, p_latCombs, m_lbRatio, ref p_combArray, ref p_rosArray, ref p_maxRosExtArray, begin, end, p_laterals, (p_cols - p_laterals), ref p_latRosArray, m_lessIgns);
                begin = end;
            }
            m_randThread[0].calcSpreadPaths2();
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief
         *
         *  -#  Allocates threads and m_maxRosArray array to store max spread rates
         *      from all blocks
         *  -#  Divides the Number of Combinations (m_combs) into parts for each thread.
         *  -#  Runs each thread and wait until they are all finished (hRandSyncEvent)
         *  -#  Calculates Expected Spread Rates by Prob[i] X MaxSpread[i]
         *
         */

        void calcSpreadRates()
        {
            if (m_maxRosArray != null)
            {
                m_maxRosArray = null;
            }
            m_maxRosArray = new double[m_combs];
            //memset(m_maxRosArray, 0x0, m_combs * sizeof(double));

            double interval = ((double)m_combs) / ((double)m_threads);
            double ipart = 0.0;
            double fract = modf(interval, ref ipart);
            long range = (long)interval;
            if (fract > 0.0)
            {
                range++;
            }
            long begin = 0;
            long end;
            for (int i = 0; i < m_threads; i++)
            {
                end = begin + range;
                if (begin >= m_combs)
                {
                    continue;
                }
                if (end > m_combs)
                {
                    end = m_combs;
                }
                //bran-jnw, 0 (NULL) was sent before, does not work with C# ref
                double[] dummy = new double[0];
                m_randThread[i].setThreadData(m_samples, m_depths, m_combs, m_lbRatio, ref m_combArray, ref m_rosArray, ref m_maxRosArray, begin, end, 0, m_samples, ref dummy, m_lessIgns);
                begin = end;
            }
            m_randThread[0].calcSpreadPaths2();
            return;
        }

        //------------------------------------------------------------------------------

        void closeRandThreads()
        {
            if (m_randThread != null)
            {
                m_randThread = null;
            }
            return;
        }
        //------------------------------------------------------------------------------
        /*! \brief Computes Expected Spread Rate from all factorial combinations
         *  of fuels and their probabilities.
         *
         *  Also adds ROS from lateral extensions=Extend.
         */

        public double computeSpread2(long p_samples, long p_depths, double p_lbRatio, long p_threads, ref double p_maxRos, ref double p_harmonicRos, long p_exts, long p_lessIgns)
        {
            long i, j, k, m, fuelCombs = 0;
            double maxRos = 0.0;
            double minRos = 1e12;
            double harmonic = 0.0;
            double average = 0.0;

            if (p_samples < 1 || p_samples > 50)
            {
                return (0.0);
            }

            m_samples = p_samples;
            m_depths = p_depths;
            m_threads = p_threads;
            m_lessIgns = p_lessIgns;
            m_lbRatio = p_lbRatio;

            for (i = 0; i < m_fuels; i++)
            {
                if (maxRos < m_fuelTypeArray[i].m_absRos)
                {
                    maxRos = m_fuelTypeArray[i].m_absRos;
                }
                if (minRos > m_fuelTypeArray[i].m_absRos)
                {
                    minRos = m_fuelTypeArray[i].m_absRos;
                }
            }
            for (i = 0; i < m_fuels; i++)
            {
                m_fuelTypeArray[i].m_relRos = m_fuelTypeArray[i].m_absRos / maxRos;
            }

            // base combinations for sample block
            calcCombinations(m_samples, m_depths, ref m_combs, ref m_combArray, ref m_rosArray);
            if (!allocRandThreads())
            {
                return (-1.0);
            }

            calcSpreadRates(); // ri for sample block
            double[][] latComb = new double[0][];
            double[][] latRos = new double[0][];

            double prob, cuumProb;

            if (p_exts > 0)
            {
                calcCombinations(2, m_depths, ref m_exts, ref m_combExtArray, ref m_rosExtArray); // extensions only
                calcCombinations(1, 2, ref fuelCombs, ref latComb, ref latRos);
                Extension[] ext = new Extension[p_exts];

                // allocate the number of extensions
                for (j = 0; j < p_exts; j++)
                {
                    ext[j].allocExtension(m_exts, (m_samples + (j * 2) + 2), m_depths, m_fuels);
                    ext[j].m_rf = this;
                }
                // assign a pointer to the next one
                for (j = p_exts - 2; j >= 0; j--)
                {
                    ext[j].m_nextExt = ext[j + 1];
                }

                // for all original combinations of the sample block
                //fprintf(stderr, "%ld extensions: ", m_combs);
                for (j = 0; j < m_combs; j++)
                {
                    cuumProb = 0.0;
                    // don't need to do this if spread rate is already 1.0
                    if (m_maxRosArray[j] < 1.0)
                    {
                        spliceExtensions2(ref m_combArray[j], ref m_rosArray[j], ref ext[0].m_combArray, ref ext[0].m_rosArray, m_samples);

                        for (k = 0; k < fuelCombs; k++)
                        {
                            ext[0].run(1, ref latRos[k], ref latComb[k], m_maxRosArray[j]);
                            for (m = 0; m < m_exts; m++)
                            {
                                if (ext[0].m_maxRosArray[m] > m_maxRosArray[j])
                                {
                                    prob = ext[0].calcProb(m, true);
                                    cuumProb += prob;
                                    harmonic += prob / ext[0].m_maxRosArray[m];
                                    average += prob * ext[0].m_maxRosArray[m];
                                }
                            }
                        }
                        // accumulate harmonc ros from ext[0] only
                        harmonic += ext[0].m_harmonicRos;
                        // accumulate average ros from ext[0] only
                        average += ext[0].m_averageRos;
                        // accumulate harmonic ros from all extensions
                        // (ext[0] contains all >ext[0]
                        harmonic += ext[0].m_extHarmonicRos;
                        // accumulate average ros from all extensions
                        // (ext[0] contains all >ext[0]
                        average += ext[0].m_extAverageRos;
                    }
                    prob = 1.0;
                    for (i = 0; i < m_samples * m_depths; i++)
                    {
                        prob *= ((double)m_combArray[j][i]);
                    }
                    for (i = 0; i < p_exts; i++)
                    {
                        cuumProb += ext[i].m_extCuumProb;
                        ext[i].m_extCuumProb = 0.0;
                    }
                    // subtract probabilities of faster spread rates
                    prob -= cuumProb;
                    // harmonic mean from sample block with faster spread rates than extensions
                    harmonic += prob / m_maxRosArray[j];
                    // average from sample block with faster spread rates than extensions
                    average += prob * m_maxRosArray[j];

                    //fprintf(stderr, ".");
                }
                //fprintf(stderr, "\n");
                ext = null;
                if (latComb != null)
                {
                    /*for (i = 0; i < fuelCombs; i++)
                    {
                        delete[] latComb[i];
                    }
                    delete[] latComb;
                    latComb = 0;*/
                    latComb = null;
                }
                if (latRos != null)
                {
                    /*for (i = 0; i < fuelCombs; i++)
                    {
                        delete[] latRos[i];
                    }
                    delete[] latRos;
                    latRos = 0;*/
                    latRos = null;
                }
            }
            else
            {
                for (i = 0; i < m_combs; i++)
                {
                    prob = 1.0;
                    for (j = 0; j < m_depths; j++)
                    {
                        for (k = 0; k < m_samples; k++)
                        {
                            prob *= m_combArray[i][j * m_samples + k];
                        }
                    }
                    average += m_maxRosArray[i] * prob;
                    if (m_maxRosArray[i] > 0.0)
                    {
                        harmonic += prob / m_maxRosArray[i];
                    }
                }
            }

            if (p_maxRos != 0.0)
            {
                p_maxRos = maxRos;
            }
            if (p_harmonicRos != 0.0)
            {
                p_harmonicRos = 0.0;
                if (harmonic > 0.0)
                {
                    p_harmonicRos = (1.0 / harmonic);
                }
            }
            closeRandThreads();
            freeBlockArrays();
            return (average);
        }

        //------------------------------------------------------------------------------

        public void freeFuels()
        {
            if (m_fuelTypeArray != null)
            {
                m_fuelTypeArray = null;
            }
            //m_fuelTypeArray = 0;
            m_fuels = 0;
            return;
        }

        //------------------------------------------------------------------------------

        void freeBlockArrays()
        {
            long i;
            if (m_combArray != null)
            {
                /*for (i = 0; i < m_combs; i++)
                {
                    delete[] m_combArray[i];
                }
                delete[] m_combArray;
                m_combArray = 0;*/
                m_combArray = null;
            }
            if (m_rosArray != null)
            {
                /*for (i = 0; i < m_combs; i++)
                {
                    delete[] m_rosArray[i];
                }
                delete[] m_rosArray;
                m_rosArray = 0;*/
                m_rosArray = null;
            }
            if (m_combExtArray != null)
            {
                /*for (i = 0; i < m_exts; i++)
                {
                    delete[] m_combExtArray[i];
                }
                delete[] m_combExtArray;
                m_combExtArray = 0;*/
                m_combExtArray = null;
            }
            if (m_rosExtArray != null)
            {
                /*for (i = 0; i < m_exts; i++)
                {
                    delete[] m_rosExtArray[i];
                }
                delete[] m_rosExtArray;
                m_rosExtArray = 0;*/
                m_rosExtArray = null;

            }
            if (m_maxRosExtArray!= null)
            {
                /*delete[] m_maxRosExtArray;
                m_maxRosExtArray = 0;*/
                m_maxRosExtArray = null;
            }
            m_combs = 0;
            m_exts = 0;
            return;
        }

        //------------------------------------------------------------------------------

        void init()
        {
            m_samples = 0;
            m_depths = 0;
            m_fuels = 0;
            m_combs = 0;
            m_exts = 0;
            m_threads = 0;
            m_lessIgns = 0;
            m_lbRatio = 0.0;
            m_cellSize = 0.0;
            m_combArray = null;
            m_rosArray = null;
            m_combExtArray = null;
            m_rosExtArray = null;
            m_maxRosArray = null;
            m_maxRosExtArray = null;
            m_fuelTypeArray = null;
            m_randThread = null;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Recomputes spread using the existing spread rate array m_maxRosArray after
         *  the user has run ComputeSpread().
         *
         *  RecomputeSpread() assumes the user has loaded only new fuel data,
         *  changing only the proportions of fuels on the landscape.
         *  All that is needed then is to recompute the probabilities of the fuel
         *  arrangement for weighting in the harmonic mean calculation.
         */

        double recomputeSpread(ref double p_harmonicRos)
        {
            if (m_maxRosArray == null)
            {
                p_harmonicRos = -1.0;
                return (-1.0);
            }

            double expectedRos = 0.0;
            double harmonicRos = 0.0;
            double totalProb = 0.0;
            calcCombinations(m_samples, m_depths, ref m_combs, ref m_combArray, ref m_rosArray);
            for (int i = 0; i < m_combs; i++)
            {
                double prob = 1.0;
                for (int j = 0; j < m_depths; j++)
                {
                    for (int k = 0; k < m_samples; k++)
                    {
                        prob *= ((double)(m_combArray[i][j * m_samples + k]));
                    }
                }
                expectedRos += ((double)m_maxRosArray[i]) * prob;
                if (m_maxRosArray[i] > 0.0)
                {
                    totalProb += prob;
                    harmonicRos += prob / ((double)m_maxRosArray[i]);
                }
            }

            // convert harmonicRos to double for return
            p_harmonicRos = (double)totalProb / harmonicRos;
            freeBlockArrays();
            return (expectedRos);
        }

        //------------------------------------------------------------------------------

        public void setCellDimensions(double p_cellSize)
        {
            m_cellSize = p_cellSize;
        }

        //------------------------------------------------------------------------------

        public void setFuelData(long p_type, double p_ros, double p_fract)
        {
            if (p_type <= m_fuels)
            {
                m_fuelTypeArray[p_type].m_absRos = p_ros;
                m_fuelTypeArray[p_type].m_relRos = -1.0;
                m_fuelTypeArray[p_type].m_fract = p_fract;
            }
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Splices *p_ca into m_combExtArray and *p_ra into m_rosExtArray
         *  and puts the results into **p_cs and **p_rs.
         *
         *  \param p_ca Array of combinations (probabilities of the fuels occurring)
         *  \param p_ra Array of spread rates for each fuel type
         *  \param p_cs 2x array of all combinations of the fuels
         *              in m_combExtArray and p_ca
         *  \param p_rs 2x array of all spread rates of the fuels
         *              in m_combExtArray and p_ra
         */

        public void spliceExtensions2(ref double[] p_ca, ref double[] p_ra, ref double[][] p_cs, ref double[][] p_rs, long p_oldCols)
        {
            long newCols;
            ulong loc;
            newCols = p_oldCols + 2;          // adding 2 columns to p_oldCols
            for (long m = 0; m < m_exts; m++)
            {
                for (long j = 0; j < m_depths; j++)
                {
                    //memcpy(&((*p_cs)[m][j * newCols]), &m_combExtArray[m][j * 2], sizeof(double));
                    p_cs[m][j * newCols] = m_combExtArray[m][j * 2];
                    p_rs[m][j * newCols] = m_rosExtArray[m][j * 2];
                    loc = (ulong)(j * newCols + 1);
                    //memcpy(&((*p_cs)[m][loc]), &p_ca[j * p_oldCols], p_oldCols * sizeof(double));
                    //memcpy(&((*p_rs)[m][loc]), &p_ra[j * p_oldCols], p_oldCols * sizeof(double));
                    p_cs[m][loc] = p_ca[j * p_oldCols];
                    p_rs[m][loc] = p_ra[j * p_oldCols];
                    loc = (ulong)(j * newCols + p_oldCols + 1);
                    //memcpy(&((*p_cs)[m][loc]), &m_combExtArray[m][j * 2 + 1], sizeof(double));
                    p_cs[m][loc] = m_combExtArray[m][j * 2 + 1];
                    p_rs[m][j * newCols + p_oldCols + 1] = m_rosExtArray[m][j * 2 + 1];
                }
            }
            return;
        }
    }
}

