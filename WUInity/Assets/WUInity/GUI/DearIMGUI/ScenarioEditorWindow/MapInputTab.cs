using ImGuiNET;
using PREACT.Input;
using System;

namespace Assets.WUInity.GUI.DearIMGUI
{
    public static class MapInputTab
    {
        static string[] MapProviderStrings;
        static int _mapProviderIndex;

        static MapInputTab()
        {
            MapProviderStrings = Enum.GetNames(typeof(MapInput.MapServiceProvider));
        }

        public static void Draw(MapInput input)
        {
            _mapProviderIndex = (int)input.MapProvider;
            ImGui.Combo(nameof(input.MapProvider), ref _mapProviderIndex, MapProviderStrings, MapProviderStrings.Length);
            input.MapProvider = (MapInput.MapServiceProvider)_mapProviderIndex;
            ImGui.SliderInt(nameof(input.ZoomLevel), ref input.ZoomLevel, 0, 20);
        }
    }
}
