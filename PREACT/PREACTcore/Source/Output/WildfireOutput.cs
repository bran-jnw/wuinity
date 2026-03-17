using System;
using System.Collections.Generic;
using System.Text;

namespace PREACT.Output
{
    public static class WildfireOutput
    {
        public static void SaveOutputMapsUTM(Simulation _simulation, int _xDim, int _yDim, double CellSizeX, double CellSizeY, string outputFilePath)
        {
            /*try
            {
                using (OSGeo.GDAL.Driver driver = OSGeo.GDAL.Gdal.GetDriverByName("GTiff"))
                {
                    OSGeo.GDAL.Dataset output = driver.Create(outputFilePath, _xDim, _yDim, 4, OSGeo.GDAL.DataType.GDT_Float32, null);

                    double leftX = _simulation.UTMOrigin.x + _originOffset.x;
                    double lowerLeftY = _simulation.UTMOrigin.y + _originOffset.y;
                    double[] geoTransform = new double[] { leftX, GetCellSizeX(), 0.0, lowerLeftY, 0.0, GetCellSizeY() };
                    output.SetGeoTransform(geoTransform);

                    OSGeo.OSR.SpatialReference reference = new OSGeo.OSR.SpatialReference("");
                    reference.SetProjCS("UTM " + _simulation.UTMData.Zona + " (WGS84)");
                    reference.SetWellKnownGeogCS("WGS84");
                    reference.SetUTM(_simulation.UTMData.ZoneNumber, _simulation.Input.Simulation.LowerLeftLatLon.x > 0 ? 1 : 0); ;
                    output.SetSpatialRef(reference);

                    //time of arrival
                    OSGeo.GDAL.Band band = output.GetRasterBand(1); //starts from 1, not zero                
                    band.SetNoDataValue(-9999f);
                    band.SetDescription($"Time of arrival [hours] from initial ignition.");
                    float[] row = new float[_xDim];
                    for (int y = 0; y < _yDim; ++y)
                    {
                        for (int x = 0; x < _xDim; ++x)
                        {
                            row[x] = (_fuelCells[x, y].TimeOfArrival - (float)_initialIgnition) / 3600; //hours
                            if (row[x] <= 0f)
                            {
                                row[x] = -9999f;
                            }
                        }
                        band.WriteRaster(0, y, _xDim, 1, row, _xDim, 1, 0, 0);
                    }
                    band.FlushCache();

                    // spread rate
                    band = output.GetRasterBand(2);
                    band.SetNoDataValue(-9999f);
                    band.SetDescription("Max rate of spread [m/s].");
                    for (int y = 0; y < _yDim; ++y)
                    {
                        for (int x = 0; x < _xDim; ++x)
                        {
                            row[x] = _maxRosData[x, y];
                            if (row[x] <= 0f)
                            {
                                row[x] = -9999f;
                            }
                        }
                        band.WriteRaster(0, y, _xDim, 1, row, _xDim, 1, 0, 0);
                    }
                    band.FlushCache();

                    // max spread rate direction
                    band = output.GetRasterBand(3);
                    band.SetNoDataValue(-9999f);
                    band.SetDescription("Max rate of spread direction [degree azimuth, counter clock-wise from North].)");
                    for (int y = 0; y < _yDim; ++y)
                    {
                        for (int x = 0; x < _xDim; ++x)
                        {
                            row[x] = _maxRosDirectionData[x, y];
                            if (row[x] <= 0f)
                            {
                                row[x] = -9999f;
                            }
                        }
                        band.WriteRaster(0, y, _xDim, 1, row, _xDim, 1, 0, 0);
                    }
                    band.FlushCache();

                    // fire intenisty
                    band = output.GetRasterBand(4);
                    band.SetNoDataValue(-9999f);
                    band.SetDescription("Fire intensity [kW/m].)");
                    for (int y = 0; y < _yDim; ++y)
                    {
                        for (int x = 0; x < _xDim; ++x)
                        {
                            row[x] = _maxFireIntensityData[_fuelCells[x, y].LinearIndex];
                            if (row[x] <= 0f)
                            {
                                row[x] = -9999f;
                            }
                        }
                        band.WriteRaster(0, y, _xDim, 1, row, _xDim, 1, 0, 0);
                    }
                    band.FlushCache();

                    //close
                    output.FlushCache();
                    //reminder, output.Close() crashes violently, do not use or investigate further why...
                }

            }
            catch (Exception e)
            {
                Engine.Message(null, Engine.LogType.Warning, e.Message);
            }*/
        }
    }
}
