using ILGPU.Runtime.Cuda;
using ImGuiNET;
using System.IO;
using UnityEngine;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class Themes
    {       
        public static void ApplyAdobeSpectrum(bool darkTheme)
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            // ----- Layout -----
            style.WindowPadding = new Vector2(6f, 6f);
            style.FramePadding = new Vector2(4f, 3f);
            style.ItemSpacing = new Vector2(6f, 6f);
            style.ItemInnerSpacing = new Vector2(4f, 4f);

            style.IndentSpacing = 20f;
            style.ScrollbarSize = 16f;
            style.GrabMinSize = 10f;

            style.WindowBorderSize = 1f;
            style.ChildBorderSize = 1f;
            style.PopupBorderSize = 1f;
            style.FrameBorderSize = 1f;
            style.TabBorderSize = 1f;

            style.WindowRounding = 0f;
            style.ChildRounding = 0f;
            style.FrameRounding = 0f;
            style.PopupRounding = 0f;
            style.ScrollbarRounding = 0f;
            style.GrabRounding = 0f;
            style.TabRounding = 0f;

            if (darkTheme)
            {
                ImGui.StyleColorsDark();
                return;
                // ----- Colors -----
                colors[(int)ImGuiCol.Text] = new Vector4(0.95f, 0.95f, 0.95f, 1f);
                colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.55f, 0.55f, 0.55f, 1f);

                colors[(int)ImGuiCol.WindowBg] = new Vector4(0.11f, 0.11f, 0.11f, 1f);
                colors[(int)ImGuiCol.ChildBg] = new Vector4(0.13f, 0.13f, 0.13f, 1f);
                colors[(int)ImGuiCol.PopupBg] = new Vector4(0.16f, 0.16f, 0.16f, 1f);

                colors[(int)ImGuiCol.Border] = new Vector4(0.25f, 0.25f, 0.25f, 1f);
                colors[(int)ImGuiCol.BorderShadow] = new Vector4(0f, 0f, 0f, 0f);

                colors[(int)ImGuiCol.FrameBg] = new Vector4(0.18f, 0.18f, 0.18f, 1f);
                colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.23f, 0.23f, 0.23f, 1f);
                colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.28f, 0.28f, 0.28f, 1f);

                colors[(int)ImGuiCol.TitleBg] = new Vector4(0.10f, 0.10f, 0.10f, 1f);
                colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.16f, 0.16f, 0.16f, 1f);
                colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.10f, 0.10f, 0.10f, 0.5f);

                colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1f);

                colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.10f, 0.10f, 0.10f, 1f);
                colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.30f, 0.30f, 0.30f, 1f);
                colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.35f, 0.35f, 0.35f, 1f);
                colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.40f, 0.40f, 0.40f, 1f);

                colors[(int)ImGuiCol.CheckMark] = new Vector4(0.90f, 0.90f, 0.90f, 1f);

                colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.50f, 0.50f, 0.50f, 1f);
                colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.70f, 0.70f, 0.70f, 1f);

                colors[(int)ImGuiCol.Button] = new Vector4(0.20f, 0.20f, 0.20f, 1f);
                colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.30f, 0.30f, 0.30f, 1f);
                colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.35f, 0.35f, 0.35f, 1f);

                colors[(int)ImGuiCol.Header] = new Vector4(0.25f, 0.25f, 0.25f, 1f);
                colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.30f, 0.30f, 0.30f, 1f);
                colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.35f, 0.35f, 0.35f, 1f);

                colors[(int)ImGuiCol.Separator] = new Vector4(0.25f, 0.25f, 0.25f, 1f);
                colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.35f, 0.35f, 0.35f, 1f);
                colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.40f, 0.40f, 0.40f, 1f);

                colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.25f, 0.25f, 0.25f, 1f);
                colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.35f, 0.35f, 0.35f, 1f);
                colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.40f, 0.40f, 0.40f, 1f);

                colors[(int)ImGuiCol.Tab] = new Vector4(0.15f, 0.15f, 0.15f, 1f);
                colors[(int)ImGuiCol.TabHovered] = new Vector4(0.30f, 0.30f, 0.30f, 1f);
                colors[(int)ImGuiCol.TabActive] = new Vector4(0.25f, 0.25f, 0.25f, 1f);
                colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.10f, 0.10f, 0.10f, 1f);
                colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.18f, 0.18f, 0.18f, 1f);

                colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1f);
                colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1f, 0.43f, 0.35f, 1f);
                colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.73f, 0.60f, 0.15f, 1f);
                colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1f, 0.60f, 0f, 1f);

                colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
            }
            else
            {
                ImGui.StyleColorsLight();
                return;
                
                // ----- Colors (Spectrum Light) -----
                colors[(int)ImGuiCol.Text] = new Vector4(0.10f, 0.10f, 0.10f, 1f);
                colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.55f, 0.55f, 0.55f, 1f);

                colors[(int)ImGuiCol.WindowBg] = new Vector4(0.98f, 0.98f, 0.98f, 1f);
                colors[(int)ImGuiCol.ChildBg] = new Vector4(0.97f, 0.97f, 0.97f, 1f);
                colors[(int)ImGuiCol.PopupBg] = new Vector4(1f, 1f, 1f, 1f);

                colors[(int)ImGuiCol.Border] = new Vector4(0.78f, 0.78f, 0.78f, 1f);
                colors[(int)ImGuiCol.BorderShadow] = new Vector4(0f, 0f, 0f, 0f);

                colors[(int)ImGuiCol.FrameBg] = new Vector4(0.95f, 0.95f, 0.95f, 1f);
                colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.90f, 0.90f, 0.90f, 1f);
                colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.85f, 0.85f, 0.85f, 1f);

                colors[(int)ImGuiCol.TitleBg] = new Vector4(0.96f, 0.96f, 0.96f, 1f);
                colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.92f, 0.92f, 0.92f, 1f);
                colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.96f, 0.96f, 0.96f, 0.5f);

                colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.94f, 0.94f, 0.94f, 1f);

                colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.98f, 0.98f, 0.98f, 1f);
                colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.80f, 0.80f, 0.80f, 1f);
                colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.70f, 0.70f, 0.70f, 1f);
                colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.60f, 0.60f, 0.60f, 1f);

                colors[(int)ImGuiCol.CheckMark] = new Vector4(0.00f, 0.45f, 0.90f, 1f);

                colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.40f, 0.40f, 0.40f, 1f);
                colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.20f, 0.20f, 0.20f, 1f);

                colors[(int)ImGuiCol.Button] = new Vector4(0.93f, 0.93f, 0.93f, 1f);
                colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.88f, 0.88f, 0.88f, 1f);
                colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.82f, 0.82f, 0.82f, 1f);

                colors[(int)ImGuiCol.Header] = new Vector4(0.90f, 0.90f, 0.90f, 1f);
                colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.85f, 0.85f, 0.85f, 1f);
                colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.80f, 0.80f, 0.80f, 1f);

                colors[(int)ImGuiCol.Separator] = new Vector4(0.78f, 0.78f, 0.78f, 1f);
                colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.65f, 0.65f, 0.65f, 1f);
                colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.55f, 0.55f, 0.55f, 1f);

                colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.85f, 0.85f, 0.85f, 1f);
                colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.75f, 0.75f, 0.75f, 1f);
                colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.65f, 0.65f, 0.65f, 1f);

                colors[(int)ImGuiCol.Tab] = new Vector4(0.92f, 0.92f, 0.92f, 1f);
                colors[(int)ImGuiCol.TabHovered] = new Vector4(0.85f, 0.85f, 0.85f, 1f);
                colors[(int)ImGuiCol.TabActive] = new Vector4(0.88f, 0.88f, 0.88f, 1f);
                colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.96f, 0.96f, 0.96f, 1f);
                colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.92f, 0.92f, 0.92f, 1f);

                colors[(int)ImGuiCol.PlotLines] = new Vector4(0.35f, 0.35f, 0.35f, 1f);
                colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1f, 0.43f, 0.35f, 1f);
                colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.73f, 0.60f, 0.15f, 1f);
                colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1f, 0.60f, 0f, 1f);

                colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.26f, 0.59f, 0.98f, 0.35f);
            }
        }
    }
}
