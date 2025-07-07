using UnityEngine;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using PointCloudViewer.Structs;

namespace pointcloudviewer.helpers
{
    public class _PeekHeader : PeekHeader
    {
        private static string LOG_FORMAT = "<color=#27B7AD><b>[_PeekHeader]</b></color> {0}";



        public static _PcdHeaderData PeekHeaderPCD(StreamReader reader, ref bool readRGB, ref bool readIntensity, ref long masterPointCount)
        {
            Debug.LogWarningFormat(LOG_FORMAT, "PeekHeaderPCD()");

            _PcdHeaderData header = new _PcdHeaderData();

            /*
# .PCD v0.7 - Point Cloud Data file format
VERSION 0.7
FIELDS x y z intensity normal_x normal_y normal_z curvature
SIZE 4 4 4 4 4 4 4 4
TYPE F F F F F F F F
COUNT 1 1 1 1 1 1 1 1
WIDTH 327360
HEIGHT 1
VIEWPOINT 0 0 0 1 0 0 0
POINTS 327360
DATA ascii
-1.2488459 -1.7935097 2.7711847 29 0 0 0 0
-1.1719888 -1.9245864 2.7728937 32 0 0 0 0
-1.0921085 -2.0466743 2.7715917 29 0 0 0 0
-1.0012199 -2.1517682 2.7722948 27 0 0 0 0
            ...
            */

            string line = "";
            line = reader.ReadLine();
            if (line.StartsWith("#") == true)
            {
                header.linesRead++;
            }

            // version
            line = reader.ReadLine();
            header.linesRead++;
            if (line.Contains(".7") == false)
            {
                Debug.LogWarningFormat(LOG_FORMAT, "Only version v0.7 is tested.. your file version is: " + line);
            }

            // fields
            line = reader.ReadLine();
            header.linesRead++;
            if (line.Contains("rgb") == false && line.Contains("r g b") == false)
            {
                readRGB = false;
            }
            if (line.Contains("intensity") == false)
            {
                readIntensity = false;
            }

            // size
            line = reader.ReadLine();
            header.linesRead++;

            // type
            line = reader.ReadLine();
            header.linesRead++;

            // count
            line = reader.ReadLine();
            header.linesRead++;

            // width
            line = reader.ReadLine();
            header.linesRead++;

            // heigth
            line = reader.ReadLine();
            header.linesRead++;

            // viewpoint
            line = reader.ReadLine();
            header.linesRead++;
            if (line.ToLower().Contains("viewpoint"))
            {
                // points
                line = reader.ReadLine();
                header.linesRead++;
            }
            else // no viewpoint line, probably v0.5
            {
                // then this line was points
            }

            line = line.Replace("POINTS ", "").Trim();
            if (!long.TryParse(line, out masterPointCount))
            {
                Debug.LogError("Failed to read point count from PCD file");
                header.readSuccess = false;
                return header;
            }

            // datatype
            line = reader.ReadLine();
            header.linesRead++;
            if (line.Contains("ascii") == false)
            {
                Debug.LogError("Only ascii PCD files are currently supported..");
            }

            // first data row
            line = reader.ReadLine();
            header.linesRead++;
            string[] row = line.Split(' ');
            if (readRGB == true)
            { 
                if (row.Length < 4) 
                { 
                    Debug.LogError("No RGB data founded after XYZ, disabling readRGB");
                    readRGB = false;
                }
            }
            // -1.2488459 -1.7935097 2.7711847 29 0 0 0 0
            //if (readIntensity) { if (row.Length < 4) { Debug.LogError("No RGB data founded after XYZ, disabling readRGB"); readRGB = false; } }
            header.x = double.Parse(row[0].Replace(",", "."), CultureInfo.InvariantCulture);
            header.y = double.Parse(row[1].Replace(",", "."), CultureInfo.InvariantCulture);
            header.z = double.Parse(row[2].Replace(",", "."), CultureInfo.InvariantCulture);
            header.readSuccess = true;

            return header;
        }


    }
}