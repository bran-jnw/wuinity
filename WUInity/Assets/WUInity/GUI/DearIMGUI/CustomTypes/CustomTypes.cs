using ImGuiNET;
using System;
using PREACT.Math;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class CustomTypes
    {

        /*public static bool InputDouble2(string label, ref Vector2d value, string format = "%.6f")
        {
            Span<double> buffer = stackalloc double[2]
            {
                value.x, value.y
            };

            bool changed = ImGui.InputScalarN(label, ImGuiDataType.Double, buffer, 2, IntPtr.Zero, IntPtr.Zero, format);

            if (changed)
            {
                value.x = (float)buffer[0];
                value.y = (float)buffer[1];
            }

            return changed;
        }*/


        public static bool InputDateTimePopup(string label, ref DateTime value)
        {
            bool changed = false;

            string display = value.ToString("yyyy-MM-dd HH:mm:ss");

            ImGui.InputText(label, ref display, 24, ImGuiInputTextFlags.ReadOnly);

            if (ImGui.IsItemClicked())
            {
                ImGui.OpenPopup("##popup_" + label);
            }                

            if (ImGui.BeginPopup("##popup_" + label))
            {
                changed |= InputDateTime("##inner_" + label, ref value);
                ImGui.EndPopup();
            }

            return changed;
        }

        private static bool InputDateTime(string label, ref DateTime value)
        {
            bool changed = false;

            int year = value.Year;
            int month = value.Month;
            int day = value.Day;
            int hour = value.Hour;
            int minute = value.Minute;
            int second = value.Second;

            ImGui.PushID(label);

            // Date row
            changed |= ImGui.InputInt("Year", ref year);
            changed |= ImGui.InputInt("Month", ref month);
            changed |= ImGui.InputInt("Day", ref day);

            // Time row
            changed |= ImGui.InputInt("Hour", ref hour);
            changed |= ImGui.InputInt("Minute", ref minute);
            changed |= ImGui.InputInt("Second", ref second);

            ImGui.PopID();

            if (changed)
            {
                // Clamp values to valid ranges
                month = Math.Clamp(month, 1, 12);
                day = Math.Clamp(day, 1, DateTime.DaysInMonth(year, month));
                hour = Math.Clamp(hour, 0, 23);
                minute = Math.Clamp(minute, 0, 59);
                second = Math.Clamp(second, 0, 59);

                value = new DateTime(year, month, day, hour, minute, second);
            }

            return changed;
        }
    }
}
