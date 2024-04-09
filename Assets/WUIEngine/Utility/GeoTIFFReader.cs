using BitMiracle.LibTiff.Classic;

namespace WUIEngine
{
    public class GeoTIFFReader
    {
        public void ReadFile(string path)
        {
            Tiff tiff = Tiff.Open(path, "r");
        }
    }
}

