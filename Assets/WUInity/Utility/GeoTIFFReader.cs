using BitMiracle.LibTiff.Classic;

namespace WUInity
{
    public class GeoTIFFReader
    {
        public void ReadFile(string path)
        {
            Tiff tiff = Tiff.Open(path, "r");
        }
    }
}

