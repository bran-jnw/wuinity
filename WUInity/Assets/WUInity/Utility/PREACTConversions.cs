using UnityEngine;

namespace WUInity
{
    static class Extensions
    {   
        public static Color UnityColor(this PREACT.PREACTColor c)
        {
            return new Color(c.r, c.g, c.b, c.a);
        }
    }
    

}
