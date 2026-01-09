//This file is part of PREACT Copyright (C) 2025 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

using PREACT.Math;

namespace PREACT
{
    public struct PREACTColor
    {
        public float r, g, b, a;

        public PREACTColor(float r, float g, float b)
        {
            this.r = r; 
            this.g = g; 
            this.b = b;
            this.a = 0f;
        }

        public PREACTColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public PREACTColor(int r, int g, int b, int a)
        {
            this.r = r / 255.0f;
            this.g = g / 255.0f;
            this.b = b / 255.0f;
            this.a = a / 255.0f;
        }

        public static PREACTColor Random()
        {
            return new PREACTColor(Math.Random.valueF, Math.Random.valueF, Math.Random.valueF);
        }

        public static PREACTColor operator *(PREACTColor c, float f) => new PREACTColor(c.r * f, c.g * f, c.b * f, c.a * f );
        public static PREACTColor operator /(PREACTColor c, float f) => new PREACTColor(c.r / f, c.g / f, c.b / f, c.a / f);

        public static PREACTColor red { get { return new PREACTColor(1F, 0F, 0F, 1F); } }
        public static PREACTColor green { get { return new PREACTColor(0F, 1F, 0F, 1F); } }
        public static PREACTColor blue { get { return new PREACTColor(0F, 0F, 1F, 1F); } }
        public static PREACTColor white { get { return new PREACTColor(1F, 1F, 1F, 1F); } }
        public static PREACTColor black { get { return new PREACTColor(0F, 0F, 0F, 1F); } }
        public static PREACTColor yellow { get { return new PREACTColor(1F, 235F / 255F, 4F / 255F, 1F); } }
        public static PREACTColor cyan { get { return new PREACTColor(0F, 1F, 1F, 1F); } }
        public static PREACTColor magenta { get { return new PREACTColor(1F, 0F, 1F, 1F); } }
        public static PREACTColor gray { get { return new PREACTColor(.5F, .5F, .5F, 1F); } }
        public static PREACTColor grey { get { return new PREACTColor(.5F, .5F, .5F, 1F); } }
        public static PREACTColor clear { get { return new PREACTColor(0F, 0F, 0F, 0F); } }

        public static PREACTColor HSVToRGB(float H, float S, float V)
        {
            return HSVToRGB(H, S, V, true);
        }

        // Convert a set of HSV values to an RGB Color.
        public static PREACTColor HSVToRGB(float H, float S, float V, bool hdr)
        {
            PREACTColor retval = PREACTColor.white;
            if (S == 0)
            {
                retval.r = V;
                retval.g = V;
                retval.b = V;
            }
            else if (V == 0)
            {
                retval.r = 0;
                retval.g = 0;
                retval.b = 0;
            }
            else
            {
                retval.r = 0;
                retval.g = 0;
                retval.b = 0;

                //crazy hsv conversion
                float t_S, t_V, h_to_floor;

                t_S = S;
                t_V = V;
                h_to_floor = H * 6.0f;

                int temp = (int)Mathf.Floor(h_to_floor);
                float t = h_to_floor - ((float)temp);
                float var_1 = (t_V) * (1 - t_S);
                float var_2 = t_V * (1 - t_S * t);
                float var_3 = t_V * (1 - t_S * (1 - t));

                switch (temp)
                {
                    case 0:
                        retval.r = t_V;
                        retval.g = var_3;
                        retval.b = var_1;
                        break;

                    case 1:
                        retval.r = var_2;
                        retval.g = t_V;
                        retval.b = var_1;
                        break;

                    case 2:
                        retval.r = var_1;
                        retval.g = t_V;
                        retval.b = var_3;
                        break;

                    case 3:
                        retval.r = var_1;
                        retval.g = var_2;
                        retval.b = t_V;
                        break;

                    case 4:
                        retval.r = var_3;
                        retval.g = var_1;
                        retval.b = t_V;
                        break;

                    case 5:
                        retval.r = t_V;
                        retval.g = var_1;
                        retval.b = var_2;
                        break;

                    case 6:
                        retval.r = t_V;
                        retval.g = var_3;
                        retval.b = var_1;
                        break;

                    case -1:
                        retval.r = t_V;
                        retval.g = var_1;
                        retval.b = var_2;
                        break;
                }

                if (!hdr)
                {
                    retval.r = Mathf.Clamp(retval.r, 0.0f, 1.0f);
                    retval.g = Mathf.Clamp(retval.g, 0.0f, 1.0f);
                    retval.b = Mathf.Clamp(retval.b, 0.0f, 1.0f);
                }
            }
            return retval;
        }
    }
}