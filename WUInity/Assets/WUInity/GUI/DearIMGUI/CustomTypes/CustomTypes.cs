using ImGuiNET;
using System;
using PREACT.Math;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class CustomTypes
    {
        public static unsafe bool InputDouble2(string label, ref Vector2d value)
        {
            fixed (Vector2d* ptr = &value)
            {
                return ImGui.InputScalarN(label, ImGuiDataType.Double, (IntPtr)ptr, 2);
            }
        }

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
