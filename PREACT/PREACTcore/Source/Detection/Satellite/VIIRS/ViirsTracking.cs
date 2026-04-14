using PREACT.Math;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.Util;
using System;
using System.Collections.Generic;
using static System.Math;

namespace PREACT.Detection
{
    public struct SatelliteDetectionStatus
    {
        public Satellite Satellite;
        public double OffNadirAngle;
        public double ToEdgeAngle;
        public bool IsInside;
        public bool CanDetect;
        public GeodeticCoordinate GeoCoord;

        public SatelliteDetectionStatus(Satellite satellite, double offNadirAngle, double toEdgeAngle, bool isInside, bool didDetect, GeodeticCoordinate geoCoord)
        {
            Satellite = satellite;
            OffNadirAngle = offNadirAngle;
            ToEdgeAngle = toEdgeAngle;
            IsInside = isInside;
            CanDetect = didDetect;
            GeoCoord = geoCoord;
        }
    }

    public class ViirsTracking
    {
        // from https://db.satnogs.org/
        const string noaa20_0 = "NOAA-20";
        const string noaa20_1 = "1 43013U 17073A   26100.12372390  .00000078  00000-0  57908-4 0  9993";
        const string noaa20_2 = "2 43013  98.7727  40.2056 0002117  95.1662 264.9756 14.19544664434813";
        static Satellite noaa20 = new Satellite(noaa20_0, noaa20_1, noaa20_2);

        const string noaa21_0 = "NOAA-21";
        const string noaa21_1 = "1 54234U 22150A   26100.15807663  .00000079  00000-0  58485-4 0  9993";
        const string noaa21_2 = "2 54234  98.7590  40.0705 0002510  51.6969 308.4432 14.19560919176898";
        static Satellite noaa21 = new Satellite(noaa21_0, noaa21_1, noaa21_2);

        const string s_npp_0 = "SUOMI NPP";
        const string s_npp_1 = "1 37849U 11061A   26100.18014890  .00000086  00000-0  61725-4 0  9999";
        const string s_npp_2 = "2 37849  98.7907  41.5389 0002267 133.0055 227.1311 14.19502607748794";
        static Satellite s_npp = new Satellite(s_npp_0, s_npp_1, s_npp_2);

        Satellite[] _satellies;
        SatelliteDetectionStatus[] _satellitesStatus;

        //static TimeSpan _timeSpan = new TimeSpan(0, 0, 30);

        public ViirsTracking()
        {
            _satellies = new Satellite[] { noaa20, noaa21, s_npp };
            _satellitesStatus = new SatelliteDetectionStatus[_satellies.Length];
        }

        public SatelliteDetectionStatus[] UpdateDetectionStatus(DateTime dateTime, Vector2d ignitionLatLon, double elevation, double fireArea)
        {
            for (int i = 0; i < _satellies.Length; ++i)
            {
                Satellite sat = _satellies[i];
                _satellitesStatus[i] = ViirsSwathInfo(sat, dateTime, ignitionLatLon.x, ignitionLatLon.y, elevation, fireArea);
            }

            return _satellitesStatus;
        }

        /*public (DateTime, Satellite) GetFirstObservationOfIgnition(DateTime dateTime, Vector2d ignitionLatLon, double elevation)
        {
            DateTime tenDaysAhead = dateTime.AddDays(10.0);

            GeodeticCoordinate ignitionLocation = new GeodeticCoordinate(Angle.FromDegrees(ignitionLatLon.x), Angle.FromDegrees(ignitionLatLon.y), elevation * 0.001); //altitude in km
            GroundStation ignitionStation = new GroundStation(ignitionLocation);

            DateTime firstObservation = tenDaysAhead;
            Satellite sat = _satellies[0];
            for(int i = 0; i < _satellies.Length; ++i)
            {
                _visibilityPeriods[i] = ignitionStation.Observe(_satellies[i], dateTime, tenDaysAhead, _timeSpan);

                for(int j = 0; j < _visibilityPeriods[i].Count; ++j)
                {
                    SatelliteVisibilityPeriod period = _visibilityPeriods[i][j];
                    if(DateTime.Compare(period.Start, firstObservation) < 0)
                    {
                        firstObservation = period.Start;
                        sat = _satellies[i];
                    }
                }
            }

            return (firstObservation, sat);
        }*/

        public static SatelliteDetectionStatus ViirsSwathInfo(Satellite sat, DateTime utc, double latDeg, double lonDeg, double elevationMeters, double fireArea)
        {
            const double MaxScanDeg = 0.5 * 112.56;
            const double Deg2Rad = PI / 180.0;

            //Satellite state at this DateTime
            EciCoordinate eci = sat.Predict(utc);

            //Satellite spherical ECEF (lon, lat, radius)
            Vector3 satEcef = eci.ToSphericalEcef();

            //Ground spherical ECEF
            GeodeticCoordinate groundGeo = new GeodeticCoordinate(latDeg, lonDeg, elevationMeters * 0.001);
            Vector3 groundEcef = groundGeo.ToSphericalEcef();

            //Satellite to ground vector
            Vector3D los = new Vector3D(groundEcef.X - satEcef.X, groundEcef.Y - satEcef.Y, groundEcef.Z - satEcef.Z);

            //Satellite to Earth center (nadir)
            Vector3D nadir = new Vector3D(-satEcef.X, -satEcef.Y, -satEcef.Z);

            //Off-nadir scan angle
            double angleRad = Acos(Dot(los, nadir) / (los.Length * nadir.Length));

            double scanAngleDeg = angleRad / Deg2Rad;

            //Signed distance from swath edge
            double distanceFromEdge = scanAngleDeg - MaxScanDeg;

            bool inside = distanceFromEdge <= 0.0;

            bool canDetect = false;
            double areaLimit = 60 + 190 * (scanAngleDeg / MaxScanDeg);
            if(inside && fireArea > areaLimit)
            {
                canDetect = true;
            }

            return new SatelliteDetectionStatus(sat, scanAngleDeg, distanceFromEdge, inside, canDetect, eci.ToGeodetic());
        }

        public struct Vector3D
        {
            public double X, Y, Z;

            public Vector3D(double x, double y, double z) => (X, Y, Z) = (x, y, z);
            public double Length => Sqrt(X * X + Y * Y + Z * Z);
        }

        static double Dot(Vector3D a, Vector3D b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
}
