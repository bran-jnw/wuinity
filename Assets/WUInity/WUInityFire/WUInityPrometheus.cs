using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace WUInity.Fire
{
    public class WUInityPrometheus
    {
        CWFGM.PromeAppClass prome;

        public WUInityPrometheus()
        {
            try
            {
                CWFGM.PromeAppClass prome = new CWFGM.PromeAppClass();
                CWFGM.IPromeApp p = (CWFGM.IPromeApp)prome;
                p.Initialize();                
            }
            catch(COMException e)
            {
                MonoBehaviour.print(e.ErrorCode);
            }
        }
    }
}

