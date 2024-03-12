namespace WUInity
{
    public struct WUInityColor
    {
        public float r, g, b, a;

        public WUInityColor(float r, float g, float b)
        {
            this.r = r; 
            this.g = g; 
            this.b = b;
            this.a = 0f;
        }

        public WUInityColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public WUInityColor(int r, int g, int b, int a)
        {
            this.r = r / 255.0f;
            this.g = g / 255.0f;
            this.b = b / 255.0f;
            this.a = a / 255.0f;
        }
        public static WUInityColor operator *(WUInityColor c, float f) => new WUInityColor(c.r * f, c.g * f, c.b * f, c.a * f );
        public static WUInityColor operator /(WUInityColor c, float f) => new WUInityColor(c.r / f, c.g / f, c.b / f, c.a / f);

        public static WUInityColor red { get { return new WUInityColor(1F, 0F, 0F, 1F); } }
        public static WUInityColor green { get { return new WUInityColor(0F, 1F, 0F, 1F); } }
        public static WUInityColor blue { get { return new WUInityColor(0F, 0F, 1F, 1F); } }
        public static WUInityColor white { get { return new WUInityColor(1F, 1F, 1F, 1F); } }
        public static WUInityColor black { get { return new WUInityColor(0F, 0F, 0F, 1F); } }
        public static WUInityColor yellow { get { return new WUInityColor(1F, 235F / 255F, 4F / 255F, 1F); } }
        public static WUInityColor cyan { get { return new WUInityColor(0F, 1F, 1F, 1F); } }
        public static WUInityColor magenta { get { return new WUInityColor(1F, 0F, 1F, 1F); } }
        public static WUInityColor gray { get { return new WUInityColor(.5F, .5F, .5F, 1F); } }
        public static WUInityColor grey { get { return new WUInityColor(.5F, .5F, .5F, 1F); } }
        public static WUInityColor clear { get { return new WUInityColor(0F, 0F, 0F, 0F); } }

        public static WUInityColor HSVToRGB(float H, float S, float V)
        {
            return HSVToRGB(H, S, V, true);
        }

        // Convert a set of HSV values to an RGB Color.
        public static WUInityColor HSVToRGB(float H, float S, float V, bool hdr)
        {
            WUInityColor retval = WUInityColor.white;
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

        #if USING_UNITY
        public UnityEngine.Color UnityColor
        {
            get { return new UnityEngine.Color(r, g, b, a); }
        }
        #endif
    }
}