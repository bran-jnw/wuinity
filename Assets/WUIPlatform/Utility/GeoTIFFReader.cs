using BitMiracle.LibTiff.Classic;

namespace WUIPlatform
{
    public class GeoTIFFReader
    {
        public void ReadFile(string path)
        {
            Tiff tiff = Tiff.Open(path, "r");
        }
    }
}

