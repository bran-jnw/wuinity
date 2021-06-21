/******************************************************************************
*
* Project:  CodeBlocks
* Purpose:  Part of Mark Finney's EXRATE package for determining expected
*           and harmonic mean spread rate in randomly arranged fuels
* Author:   William Chatham <wchatham@fs.fed.us>
* Credits:  Some of the code in this file is, in part or in whole, from
*           BehavePlus5 and EXRATE source originally authored by Collin D.
*           Bevins and Mark Finney respectively, and is used with or without
*           modification.
*
*******************************************************************************
*
* THIS SOFTWARE WAS DEVELOPED AT THE ROCKY MOUNTAIN RESEARCH STATION (RMRS)
* MISSOULA FIRE SCIENCES LABORATORY BY EMPLOYEES OF THE FEDERAL GOVERNMENT
* IN THE COURSE OF THEIR OFFICIAL DUTIES. PURSUANT TO TITLE 17 SECTION 105
* OF THE UNITED STATES CODE, THIS SOFTWARE IS NOT SUBJECT TO COPYRIGHT
* PROTECTION AND IS IN THE PUBLIC DOMAIN. RMRS MISSOULA FIRE SCIENCES
* LABORATORY ASSUMES NO RESPONSIBILITY WHATSOEVER FOR ITS USE BY OTHER
* PARTIES,  AND MAKES NO GUARANTEES, EXPRESSED OR IMPLIED, ABOUT ITS QUALITY,
* RELIABILITY, OR ANY OTHER CHARACTERISTIC.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
* OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
* THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
* FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
* DEALINGS IN THE SOFTWARE.
*
******************************************************************************/
//-----------------------------------------------------------------------------
/*! \file randthread.h
 *  \version BehavePlus3
 *  \author Copyright (C) 2002-2004 by Collin D. Bevins.  All rights reserved.
 *
 *  \brief Part of Mark Finney's EXRATE package for determining expected
 *  and harmonic mean spread rate in randomly arranged fuels.
 *
 *  Original code by Mark Finney.  CDB has renamed all functions and
 *  variables according to BehavePlus3 coding style.  The following files
 *  make up the entire code package:
 *  - newext.cpp
 *  - newext.h
 *  - randfuel.cpp
 *  - randfuel.h
 *  - randthread.cpp
 *  - randthread.h
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WUInity.Fire.MathWrap;

namespace WUInity.Fire
{
    public struct PathStruct
    {
        public long m_loc;            // location in combarray and rosarray
        public long m_ignitionPt;     // 0=center, -1=left, 1=right
        public double m_pathTime;       // time for fire to reach this cell
        public double m_relCellSize;    // cumulative relative cell size for this point
    }   

    public class RandThread
    {
        double m_lbRatio;      //!< elliptical fire length-to-breadth ratio
        double m_a;            //!< elliptical fire spread rate dimensions
        double m_b;            //!< elliptical fire spread rate dimensions
        double m_c;            //!< elliptical fire spread rate dimensions
        double m_cellSize;     //!< size of raster cell
        long m_samples;      //!< number of columns in the block
        long m_depths;       //!< number of rows in the block
        long m_combs;        //!< number of combinations in block
        long m_lessIgns;     //!< number of ignition points fewer than m_samples
        long m_start;        //!< start and end for thread
        long m_end;          //!< start and end for thread
        long m_firstSample;  //!< specify ignition pts along the x axis
        long m_lastSample;   //!< specify ignition pts along the x axis
        double[][] m_combArray;    //!< probability array for all blocks, from RandFuel
        double[][] m_rosArray;     //!< spread rate array for all blocks, from RandFuel
        double[] m_maxRosArray;  //!< max ROS for all blocks, passed in from RandFuel
        double m_latRos;       //!< lateral spread rate from ig pt
        PathStruct[] m_firstPath;    //!< pointer to array of PathStructs
        PathStruct[] m_curPath;      //!< pointer to array of PathStructs
        PathStruct[] m_newPath;      //!< pointer to array of PathStructs
        double[][] m_startDelay = new double[2][];//!< pointer to delay data for extra row
        double[] m_latRosArray;  //!< pointer to delay data for extra row

        const long REFRACT_LATERAL = 0;
        const long REFRACT_FORWARD = 1;

        public RandThread()
        {
            m_lbRatio = 0.0;
            m_a = 0.0;
            m_b = 0.0;
            m_c = 0.0;
            m_cellSize = 10.0;
            m_samples = 0;
            m_depths = 0;
            m_combs = 0;
            m_lessIgns = 0;
            m_start = 0;
            m_end = 0;
            m_firstSample = 0;
            m_lastSample = 0;
            m_combArray = null;
            m_rosArray = null;
            m_maxRosArray = null;
            m_latRos = 0.0;
            m_firstPath = null;
            m_curPath = null;
            m_newPath = null;
            m_startDelay[0] = null;
            m_startDelay[1] = null;
            m_latRosArray = null;
            return;
        }

        ~RandThread()
        {
            return;
        }

        void addNewPath(ref long NumPath2, double p_time, long p_loc, long p_ignitionPt, double p_relCellSize)
        {
            if (p_loc < 0
                || p_loc > m_samples - 1)
            {
                return;
            }

            m_newPath[NumPath2].m_pathTime = p_time;
            m_newPath[NumPath2].m_loc = p_loc;
            m_newPath[NumPath2].m_ignitionPt = p_ignitionPt;
            m_newPath[NumPath2].m_relCellSize = p_relCellSize;
            NumPath2 += 1;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Calculates the elliptical dimensions given the m_lbRatio ratio input.
            *  m_a = Lateral spread rate
            *  b+c = forward spread rate.
            */

        void calcEllipticalDimensions()
        {
            double hbRatio, hfRatio;
            hbRatio = (m_lbRatio + sqrt(pow2(m_lbRatio) - 1.0))
                / (m_lbRatio - sqrt(pow2(m_lbRatio) - 1.0));
            m_a = 0.5 * (1.0 + 1.0 / hbRatio) / m_lbRatio;
            m_b = (1.0 + 1.0 / hbRatio) / 2.0;
            m_c = m_b - 1.0 / hbRatio;
            hfRatio = (m_b + m_c) / m_a;
            calcLateralRos(1.0);
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Calculates total travel time for a point source ignition,
            *  given its spread at a constant angle B (Overlap/Separation).
            *
            *  \param *LatDist Contains count==NumLayers lateral distances of adjacent cells.
            *  \param *Ros     Contains count==NumLayers spread rates of adjacent cells.
            *  \param  RefractDir  Determines if the refraction is for the forward or
            *                      lateral direction.
            */

        double calcFlankingTime(long p_numLayers, double p_separation, double p_overlap, ref double[] p_latDist, ref double[] p_ros, long p_refractDir)
        {
            double beta = atan2(p_overlap, p_separation);
            double cosB = cos(beta);
            double sinB = sin(beta);
            double cosB2 = pow2(cosB);
            double sinB2 = pow2(sinB);

            // calculate theta, angle from center of ellipse
            double cosT = (m_a * cosB * sqrt(pow2(m_a) * cosB2 + (pow2(m_b) - pow2(m_c)) * sinB2) - m_b * m_c * sinB2) / (pow2(m_a) * cosB2 + pow2(m_b) * sinB2);
            double theta = acos(cosT);

            double ros;
            double travelTime = 0.0;
            for (int i = 0; i < p_numLayers; i++)
            {
                ros = m_a * sin(theta) * p_ros[i];
                travelTime += p_latDist[i] / ros;
            }
            return (travelTime);
        }

        //------------------------------------------------------------------------------

        double calcLateralRos(double p_forwardRos)
        {
            double theta = acos(m_c / m_b);
            m_latRos = fabs(m_a * sin(theta)) * p_forwardRos;
            return (m_latRos);
        }

        /*  \brief Calculates maximum spread rates and stores in
            *  RandFuel.m_maxRosArray for for each column in the test block
            *
            *  It does this by computing the spread rate from the 1st cell in the column
            *  through all other cells in the next row, either by flanking or by heading
            *  or by a combination of flanking and heading.
            *
            *  Stores only the maximum ROS (minimum path time) for each block in
            *  m_maxRosArray because this represents the path that fire would first
            *  emerge from the block.
            *
            *  This version 2 does a one time allocation of data needed,
            *  rather than linked list (it is probably slightly faster than version 1)
        */
        public void calcSpreadPaths2()
        {
            bool Lateral = false;
            long i, j, k, m, n, p;
            long NumPath1, NumPath2, ParentLoc, NumMax, StraightNum;
            ulong NumAlloc;
            double[] SampleTime;
            double Delay, ParentRos, ParentTime;
            double Separation, Overlap, OldOverlap, OldSeparation, StraightTime;
            double DirectTime;
            double[] ExitTime, LateralDistances, SpreadRates;

            if (m_firstSample != 0)
            {
                Lateral = true;
            }

            ExitTime = new double[m_samples];
            //memset(ExitTime, 0x0, m_samples * sizeof(double));
            if (m_firstSample > 0)
            {
                // store number of startdelays
                m_startDelay[0] = new double[m_firstSample];
                m_startDelay[1] = new double[m_firstSample];
                //memset(m_startDelay[0], 0x0, m_firstSample * sizeof(double));
                //memset(m_startDelay[1], 0x0, m_firstSample * sizeof(double));
            }
            SampleTime = new double[m_samples];
            NumAlloc = (ulong)pow((double)m_samples, (int)m_depths);
            m_firstPath = new PathStruct[NumAlloc];
            m_newPath = new PathStruct[NumAlloc];
            NumMax = m_samples;
            if (m_depths > m_samples)
            {
                NumMax = m_depths;
            }
            LateralDistances = new double[NumMax];
            SpreadRates = new double[NumMax + 1];
            calcEllipticalDimensions();
            if (m_firstSample > 0)
            {
                // calculate all start delays for lateral extensions, left
                calcStartDelay(m_firstSample, 0);
                // calculate all start delays for lateral extensions, right
                calcStartDelay(m_firstSample, 1);
            }
            for (i = m_start; i < m_end; i++)
            {
                m_maxRosArray[i] = 0.0;
                for (p = 0; p < m_samples; p++)   // make it very large
                {
                    SampleTime[p] = 9e12;
                }
                for (k = m_lessIgns; k < m_samples - m_lessIgns; k++)
                {
                    j = 0;
                    m_curPath = m_firstPath; //bran-jnw: correct???
                    m_curPath[0].m_loc = k;
                    Delay = 0.0;
                    if (Lateral)
                    {
                        if (k < m_firstSample)
                        {
                            // centered=0, -1=left, 1=right
                            m_curPath[0].m_ignitionPt = -1;
                            Delay = m_startDelay[0][m_firstSample - k - 1];
                            // no delay possible because spread rate ==0.0
                            if (Delay < 0.0)
                            {
                                continue;
                            }
                        }
                        else if (k >= m_lastSample)
                        {
                            m_curPath[0].m_ignitionPt = 1;
                            Delay = m_startDelay[1][k - m_lastSample];
                            // no delay possible because spread rate ==0.0
                            if (Delay < 0.0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            m_curPath[0].m_ignitionPt = 0;
                        }
                    }
                    else
                    {
                        // centered=0, -1=left, 1=right
                        m_curPath[0].m_ignitionPt = 0;
                    }
                    m_curPath[0].m_relCellSize = 0.0;
                    m_curPath[0].m_pathTime = Delay;
                    NumPath1 = 1;
                    NumPath2 = 0;
                    for (n = 0; n < NumPath1; n++)
                    {
                        ParentRos = m_rosArray[i][j * m_samples + m_curPath[0].m_loc];
                        if (ParentRos > 0.0)
                        {
                            Separation = m_cellSize;
                            Overlap = m_cellSize;
                            // if not ignition point centered
                            if (m_curPath[0].m_ignitionPt == 0)
                            {
                                Overlap /= 2.0;
                            }
                            Separation += m_curPath[0].m_relCellSize;
                            p = 0;
                            do
                            {
                                LateralDistances[p] = Overlap;
                                SpreadRates[p] = m_rosArray[i][p * m_samples + m_curPath[0].m_loc];
                                if (Separation > m_cellSize)
                                {
                                    LateralDistances[p] = Overlap / (double)(j + 1);
                                }
                                p++;
                            } while (p <= j);
                            Delay = calcFlankingTime((long)(Separation / m_cellSize), Separation, Overlap, ref LateralDistances, ref SpreadRates, REFRACT_FORWARD);
                            ParentLoc = m_curPath[0].m_loc;
                            ParentTime = m_curPath[0].m_pathTime;
                            addNewPath(ref NumPath2, (ParentTime + m_cellSize / ParentRos), ParentLoc, m_curPath[0].m_ignitionPt, Separation);  // go straight ahead
                            if (j < m_depths - 1) // && Delay<NextTime)
                            {
                                StraightNum = (long)(m_curPath[0].m_relCellSize / m_cellSize);
                                StraightTime = 0.0;
                                for (p = 0; p < StraightNum; p++)
                                {
                                    StraightTime += m_cellSize
                                        / m_rosArray[i]
                                        [(j - p - 1) * m_samples + m_curPath[0].m_loc];
                                }
                                Delay += (ParentTime - StraightTime);
                                Separation = m_cellSize;

                                switch (m_curPath[0].m_ignitionPt)
                                {
                                    case -1:
                                        addNewPath(ref NumPath2, Delay, ParentLoc - 1, -1, 0.0);
                                        break;
                                    case 1:
                                        addNewPath(ref NumPath2, Delay, ParentLoc + 1, 1, 0.0);
                                        break;
                                    case 0:
                                        addNewPath(ref NumPath2, Delay, ParentLoc - 1, -1, 0.0);
                                        addNewPath(ref NumPath2, Delay, ParentLoc + 1, 1, 0.0);
                                        break;
                                }

                                // go left
                                //LastDelay=ParentDelay;
                                OldOverlap = Overlap;
                                OldSeparation = Separation;
                                LateralDistances[0] = Overlap;
                                for (p = 1; p < m_samples; p++)
                                {
                                    if (j * m_samples + ParentLoc - p < 0)
                                    {
                                        break;
                                    }
                                    SpreadRates[p] = m_rosArray[i]
                                        [j * m_samples + ParentLoc - p];
                                }
                                for (p = 1; p < m_samples - 1; p++)
                                {
                                    if (m_curPath[0].m_ignitionPt > 0 && m_curPath[0].m_relCellSize == 0)
                                    {
                                        break;
                                    }
                                    if (j * m_samples + ParentLoc - p < 0)
                                    {
                                        break;
                                    }
                                    LateralDistances[p] = m_cellSize;
                                    //SpreadRates[p]=m_rosArray[i][j*m_samples+ParentLoc-p];
                                    Overlap += m_cellSize;
                                    Delay = calcFlankingTime(p + 1, Separation, Overlap, ref LateralDistances, ref SpreadRates, REFRACT_LATERAL);
                                    addNewPath(ref NumPath2, (Delay + ParentTime), ParentLoc - (p + 1), -1, 0.0);
                                }

                                // go right
                                Overlap = OldOverlap;
                                Separation = OldSeparation;
                                for (p = 1; p < m_samples; p++)
                                {
                                    if (j * m_samples + ParentLoc + p > (long)NumAlloc - 1)
                                    {
                                        break;
                                    }
                                    SpreadRates[p] = m_rosArray[i]
                                        [j * m_samples + ParentLoc + p];
                                }
                                for (p = 1; p < m_samples - 1; p++)
                                {
                                    if (m_curPath[0].m_ignitionPt < 0 && m_curPath[0].m_relCellSize == 0)
                                    {
                                        break;
                                    }
                                    if (j * m_samples + ParentLoc + p > (long)NumAlloc - 1)
                                    {
                                        break;
                                    }
                                    //SpreadRates[p]=m_rosArray[i][j*m_samples+ParentLoc+p];
                                    LateralDistances[p] = m_cellSize;
                                    Overlap += m_cellSize;
                                    Delay = calcFlankingTime(p + 1, Separation, Overlap, ref LateralDistances, ref SpreadRates, REFRACT_LATERAL);
                                    addNewPath(ref NumPath2, (Delay + ParentTime), ParentLoc + (p + 1), 1, 0.0);
                                }
                            }
                        }
                        else
                        {
                            m_curPath[0].m_pathTime = 0.0;
                        }

                        // if this is the last one
                        if (n == NumPath1 - 1)
                        {
                            // if there are more paths to process
                            if (NumPath2 > 0)
                            {
                                NumPath1 = NumPath2;
                                NumPath2 = 0;
                                m_curPath = m_firstPath;
                                m_firstPath = m_newPath;
                                m_newPath = m_curPath;
                                m_curPath = m_firstPath;
                                n = -1;
                                j++;
                            }
                            else
                            {
                                NumPath1 = 0;
                            }
                        }
                        else
                        {
                            m_curPath[0] = m_firstPath[n + 1]; //bran-jnw: correctly assumed that m_curPath is m_curpath[0]???
                        }
                        if (j >= m_depths)
                        {
                            break;
                        }
                    }
                    DirectTime = 9e12;
                    p = -1;

                    for (j = 0; j < NumPath1; j++)
                    {
                        if (m_firstPath[j].m_pathTime > 0.0 && m_firstPath[j].m_pathTime < SampleTime[k])
                        {
                            SampleTime[k] = m_firstPath[j].m_pathTime;
                        }
                        if (m_firstPath[j].m_loc == k)
                        {
                            if (m_firstPath[j].m_pathTime < DirectTime)
                            {
                                DirectTime = m_firstPath[j].m_pathTime;
                                p = j;
                            }
                        }
                    }
                }   // all Depths are done
                    // find min time of all samples (max spread rate)
                m = 0;
                for (j = 0; j < m_samples; j++)
                {
                    if (SampleTime[j] == 0.0 || SampleTime[j] == 9e12)
                    {
                        m++;
                        continue;
                    }
                    // calculate overall spread rate
                    SampleTime[j] = (m_depths * m_cellSize) / SampleTime[j];
                    if (SampleTime[j] > m_maxRosArray[i])
                    {
                        m_maxRosArray[i] = SampleTime[j];
                    }
                }
            } // all combinations are done

            // Release resources
            /*delete[] SampleTime;
            delete[] ExitTime;
            for (i = 0; i < 2; i++)
            {
                if (m_startDelay[i])
                {
                    delete[] m_startDelay[i];
                    m_startDelay[i] = 0;
                }
            }
            delete[] m_firstPath;
            delete[] m_newPath;
            delete[] LateralDistances;
            delete[] SpreadRates;*/
            SampleTime = null;
            ExitTime = null;
            m_startDelay = null;
            m_firstPath = null;
            m_newPath = null;
            LateralDistances = null;
            SpreadRates = null;
            return;
        }

        //------------------------------------------------------------------------------
        /*! \brief Computes array of starting delays for flanking
            *  within the additional lee-side row of fuels for lateral extensions.
            */

        void calcStartDelay(long p_laterals, long p_leftRight)
        {
            // reverse the spread rate orders
            double[] lateralDist = new double[p_laterals];
            double[] spreadRates = new double[p_laterals];
            double separation = m_cellSize;
            double overlap = m_cellSize / 2.0;

            lateralDist[0] = overlap;
            if (p_leftRight != 0.0)
            {
                spreadRates[0] = m_latRosArray[p_leftRight * p_laterals];
            }
            else
            {
                spreadRates[0] = m_latRosArray[p_leftRight * p_laterals + p_laterals - 1];
            }

            long i;
            for (i = 1; i < p_laterals; i++)
            {
                lateralDist[i] = m_cellSize;
                if (p_leftRight != 0.0)
                {
                    // start from inside out
                    spreadRates[i] = m_latRosArray[p_leftRight * p_laterals + i];
                }
                else
                {
                    // start from inside out
                    spreadRates[i] = m_latRosArray[p_leftRight * p_laterals + (p_laterals - i - 1)];
                }
            }
            for (i = 0; i < p_laterals; i++)
            {
                m_startDelay[p_leftRight][i] = calcFlankingTime(i + 1, separation, overlap, ref lateralDist, ref spreadRates, REFRACT_LATERAL);
                overlap += m_cellSize;
            }
            //delete[] lateralDist;
            lateralDist = null;
            //delete[] spreadRates;
            spreadRates = null;
            return;
        }

        double fastFlankTime(long XStart, long YStart, double Xmid, long XEnd, long YEnd, long NumX, ref double[] Ros)
        {
            long NumCells, loc, sX, sY, NumVert;
            double TravelTime = 0.0;
            double Theta, TestDir, Fract, num = 0.0;
            double SinG, CosG, Xc = 0.0, Yc = 0.0, aXmid;
            double dX, dY, ExpRate, dist, TravelDist, VertDist;
            double[] Dist = { 0.0, 0.0 };
            double[] TDist = new double[2];
            double nX = 0.0, nY = 0.0, ROS;

            // Calculate Travel Angle and total distance through cell (beta)
            dX = (XEnd - XStart) * m_cellSize + Xmid * m_cellSize;
            dY = m_c / m_a * fabs(dX);
            VertDist = (YEnd - YStart) * m_cellSize;
            VertDist -= dY;
            dist = sqrt(pow2(dY) + pow2(dX));
            // will be different than CosB and SinB because of reference angle
            SinG = fabs(dY / dist);
            CosG = fabs(dX / dist);

            // Calculate Theta from Beta
            if (dX < 0.0)
            {
                Theta = -PI / 2.0;
            }
            else
            {
                Theta = PI / 2.0;
            }

            // Find quadrant
            if (Theta < 0.0)
            {
                Theta += PI;
            }
            ExpRate = sqrt(pow2(m_a) + pow2(m_c));

            // Obtain directional signs for sampling offsets from start point
            if (dX < 0.0)
            {
                sX = -1;
            }
            else
            {
                sX = 1;
            }
            if (dY < 0.0)
            {
                sY = -1;
            }
            else
            {
                sY = 1;
            }

            Fract = modf(fabs(dX) / m_cellSize, ref Xc);
            Xc -= 1;
            if (Fract > 0.0)
            {
                Xc += 1.0;
            }
            Fract = modf(dY / m_cellSize, ref Yc);
            Yc -= 1;
            if (Fract > 0.0)
            {
                Yc += 1.0;
            }
            NumCells = (long)(Xc + Yc) + 1;  //abs(XEnd-XStart)+abs(YEnd-YStart);
            aXmid = fabs(Xmid);

            while ((nX + nY) < NumCells)
            {
                loc = (long)(YStart + nY * sY) * NumX + XStart + ((long)nX) * sX;
                ROS = Ros[loc];
                if (ROS == 0.0)
                {
                    ROS = 1e-6;
                }

                if (fabs(CosG) < 1e-9)
                {
                    TDist[0] = 1e9;
                }
                else
                {
                    TDist[0] = (m_cellSize * (aXmid + nX)) / CosG;    // hypotenuse
                }
                if (fabs(SinG) < 1e-9)
                {
                    TDist[1] = 1e9;
                }
                else
                {
                    TDist[1] = (m_cellSize * (1 + nY)) / SinG;    // hypotenuse
                }
                if (fabs(TDist[1]) < 1e-9)
                {
                    TestDir = 0.5;    // just make if smaller than 1.0
                }
                else
                {
                    TestDir = fabs(TDist[0] / TDist[1]);
                }
                if (TestDir < 1.0)
                {
                    TDist[1] = sqrt(pow2(TDist[0]) - pow2(m_cellSize * (aXmid + nX)));
                    TDist[0] = m_cellSize * (aXmid + nX);
                    nX += 1.0;
                }
                else if (TestDir > 1.0)
                {
                    TDist[0] = sqrt(pow2(TDist[1]) - pow2(m_cellSize * (1.0 + nY)));
                    TDist[1] = m_cellSize * (1.0 + nY);
                    nY += 1.0;
                }
                else
                {
                    TDist[1] = sqrt(pow2(TDist[0]) - pow2(m_cellSize * (aXmid + nX)));
                    TDist[0] = m_cellSize * (aXmid + nX);
                    nY += 1.0;
                    nX += 1.0;
                }
                Dist[0] = TDist[0] - Dist[0];
                Dist[1] = TDist[1] - Dist[1];

                TravelDist = sqrt(pow2(Dist[0]) + pow2(Dist[1]));
                TravelTime += TravelDist / (ExpRate * ROS);

                // Copy new dimensions to old dimensions
                Dist[0] = TDist[0];
                Dist[1] = TDist[1];
            }

            // vertical stretch
            Fract = modf(VertDist / m_cellSize, ref num) * m_cellSize;
            num += 1.0;
            loc = (long)(YStart + nY * sY) * NumX + XStart + ((long)nX) * sX;
            NumVert = 0;
            do
            {
                ROS = Ros[loc];
                if (ROS > 0.0)
                {
                    TravelTime += Fract / ROS;
                }
                loc += NumX;
                Fract = m_cellSize;
                NumVert += 1;
            } while (NumVert < ((long)num));

            return (TravelTime);
        }

        //------------------------------------------------------------------------------

        public void setThreadData(long p_samples, long p_depths, long p_combs, double p_lbRatio, ref double[][] p_combArray, ref double[][] p_rosArray,
            ref double[] p_maxRosArray, long p_start, long p_end, long p_firstSample, long p_lastSample, ref double[] p_m_latRosArray, long p_lessIgns)
        {
            m_samples = p_samples;
            m_depths = p_depths;
            m_combs = p_combs;
            m_lbRatio = p_lbRatio;
            m_combArray = p_combArray;
            m_rosArray = p_rosArray;
            m_maxRosArray = p_maxRosArray;
            m_start = p_start;
            m_end = p_end;
            m_firstSample = p_firstSample;
            m_lastSample = p_lastSample;
            m_latRosArray = p_m_latRosArray;
            m_lessIgns = p_lessIgns;
            return;
        }

        //------------------------------------------------------------------------------

        double spreadTime(long XStart, long YStart, double Xmid, long XEnd, long YEnd, long NumX, ref double[] Ros, long Flank)
        {
            long NumCells, loc, sX, sY;
            double TravelTime = 0.0, FlankTime;
            double Beta, Theta, TestDir, aXmid;
            double CosB, SinB, SinB2, CosB2, CosT, SinG, CosG, SinT;
            double dX, dY, ExpRate, dist, TravelDist;
            double[] Dist = { 0.0, 0.0 };
            double[] TDist = new double[2];
            double nX = 0.0, nY = 0.0, ROS;

            // Calculate Travel Angle and total distance through cell (beta)
            dX = (XEnd - XStart) * m_cellSize + Xmid * m_cellSize;
            dY = (YEnd - YStart) * m_cellSize;
            dist = sqrt(pow2(dY) + pow2(dX));
            SinG = fabs(dY / dist);  // will be different than CosB and SinB because of reference angle
            CosG = fabs(dX / dist);
            Beta = atan2(dX, dY);
            CosB = cos(Beta);
            SinB = sin(Beta);
            CosB2 = pow2(CosB);
            SinB2 = pow2(SinB);

            // Calculate Theta from Beta
            CosT = (m_a * CosB * sqrt(pow2(m_a) * CosB2 + (pow2(m_b) - pow2(m_c)) * SinB2) - m_b * m_c * SinB2) / (pow2(m_a) * CosB2 + pow2(m_b) * SinB2);
            if (CosT >= 1.0)
            {
                Theta = 0.0;
            }
            else if (CosT <= -1.0)
            {
                Theta = PI;
            }
            else
            {
                Theta = acos(CosT);
            }

            // Find quadrant
            if (Theta < 0.0)
            {
                Theta += PI;
            }
            SinT = sin(Theta);
            ExpRate = sqrt(pow2(m_a * SinT) + pow2(m_b * CosT + m_c));

            // Obtain directional signs for sampling offsets from start point
            if (dX < 0.0)
            {
                sX = -1;
            }
            else
            {
                sX = 1;
            }
            if (dY < 0.0)
            {
                sY = -1;
            }
            else
            {
                sY = 1;
            }

            NumCells = (long)(abs(XEnd - XStart) + abs(YEnd - YStart));
            aXmid = fabs(Xmid);
            //  if ( dX > 5.0 && dY != 0.0 )
            //      NumCells -= 1;

            while (nX + nY < NumCells)
            {
                loc = (long)(YStart + nY * sY) * NumX + XStart + ((long)nX) * sX;
                ROS = Ros[loc];

                if (ROS == 0.0)
                {
                    ROS = 1e-6;
                }

                if (fabs(CosG) < 1e-9)
                {
                    TDist[0] = 1e9;
                }
                else
                {
                    TDist[0] = (m_cellSize * (aXmid + nX)) / CosG;  // hyptenuse
                }

                if (fabs(SinG) < 1e-9)
                {
                    TDist[1] = 1e9;
                }
                else
                {
                    TDist[1] = (m_cellSize * (1 + nY)) / SinG;    // hypotenuse
                }

                if (fabs(TDist[1]) < 1e-9)
                {
                    TestDir = 0.5;    // just make if smaller than 1.0
                }
                else
                {
                    TestDir = fabs(TDist[0] / TDist[1]);
                }

                if (TestDir < 1.0)
                {
                    TDist[1] = sqrt(pow2(TDist[0]) - pow2(m_cellSize * (aXmid + nX)));
                    TDist[0] = m_cellSize * (aXmid + nX);
                    nX += 1.0;
                }
                else if (TestDir > 1.0)
                {
                    TDist[0] = sqrt(pow2(TDist[1]) - pow2(m_cellSize * (1.0 + nY)));
                    TDist[1] = m_cellSize * (1.0 + nY);
                    nY += 1.0;
                }
                else
                {
                    TDist[1] = sqrt(pow2(TDist[0]) - pow2(m_cellSize * (aXmid + nX)));
                    TDist[0] = m_cellSize * (aXmid + nX);
                    nY += 1.0;
                    nX += 1.0;
                }
                Dist[0] = TDist[0] - Dist[0];
                Dist[1] = TDist[1] - Dist[1];

                TravelDist = sqrt(pow2(Dist[0]) + pow2(Dist[1]));
                TravelTime += TravelDist / (ExpRate * ROS);

                // Copy new dimensions to old dimensions
                Dist[0] = TDist[0];
                Dist[1] = TDist[1];
            }
            if (Flank != 0.0 && m_lbRatio > 1.0)
            {
                if (fabs(Theta) < PI / 2.0)
                {
                    FlankTime = fastFlankTime(XStart, YStart, Xmid, XEnd, YEnd, NumX, ref Ros);
                    if (FlankTime < TravelTime)
                    {
                        TravelTime = FlankTime;
                    }
                }
            }

            return (TravelTime);
        }
    }
}

