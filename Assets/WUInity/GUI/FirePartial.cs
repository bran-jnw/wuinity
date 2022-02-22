using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WUInity
{
    public partial class WUInityGUI
    {
        void FireMenu()
        {
            FireInput fI = WUInity.INPUT.fire;

            GUI.Box(new Rect(120, 0, columnWidth + 40, Screen.height - consoleHeight), "");
            int buttonIndex = 0;

            //name
            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Load LCP file"))
            {
                OpenLoadLCP();
            }
            ++buttonIndex;

            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "LCP file:");
            ++buttonIndex;
            GUI.Label(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), fI.lcpFile);
            ++buttonIndex;

            if (WUInity.INSTANCE.IsPainterActive())
            {
                if (WUInity.PAINTER.GetPaintMode() == WUInityPainter.PaintMode.WUIArea)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Add WUI area"))
                    {
                        WUInity.PAINTER.SetWUIAreaColor(true);
                    }
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove WUI area"))
                    {
                        WUInity.PAINTER.SetWUIAreaColor(false);
                    }
                    ++buttonIndex;
                }
                else if (WUInity.PAINTER.GetPaintMode() == WUInityPainter.PaintMode.RandomIgnitionArea)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Add random ignition area"))
                    {
                        WUInity.PAINTER.SetRandomIgnitionAreaColor(true);
                    }
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove random ignition area"))
                    {
                        WUInity.PAINTER.SetRandomIgnitionAreaColor(false);
                    }
                    ++buttonIndex;
                }
                else if (WUInity.PAINTER.GetPaintMode() == WUInityPainter.PaintMode.RandomIgnitionArea)
                {
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Add initial ignition"))
                    {
                        WUInity.PAINTER.SetInitialIgnitionAreaColor(true);
                    }
                    ++buttonIndex;
                    if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Remove initial ignition"))
                    {
                        WUInity.PAINTER.SetInitialIgnitionAreaColor(false);
                    }
                    ++buttonIndex;
                }

                //add some extra space
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Stop painting"))
                {
                    WUInity.INSTANCE.StopPainter();
                }
                ++buttonIndex;
            }
            else
            {
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Paint WUI area"))
                {
                    WUInity.INSTANCE.StartPainter(WUInityPainter.PaintMode.WUIArea);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Paint random ignition area"))
                {
                    WUInity.INSTANCE.StartPainter(WUInityPainter.PaintMode.RandomIgnitionArea);
                }
                ++buttonIndex;
                if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Paint initial ignition"))
                {
                    WUInity.INSTANCE.StartPainter(WUInityPainter.PaintMode.InitialIgnition);
                }
                ++buttonIndex;
            }

            if (GUI.Button(new Rect(buttonColumnStart, buttonIndex * (buttonHeight + 5) + 10, columnWidth, buttonHeight), "Farsite import"))
            {
                menuChoice = ActiveMenu.Farsite;
                WUInity.INSTANCE.SetSampleMode(WUInity.DataSampleMode.Farsite);
            }
            ++buttonIndex;
        }
    }
}
