﻿// https://github.com/unitycoder/PointCloudConverter
// UCPC (v2) Exporter https://github.com/unitycoder/UnityPointCloudViewer/wiki/Binary-File-Format-Structure#custom-v2-ucpc-binary-format

using UnityEngine;
using PointCloudConverter.Structs;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using ConverterBounds = PointCloudConverter.Structs.ConverterBounds;
using Debug = UnityEngine.Debug;
using PointCloudHelpers;

namespace PointCloudConverter.Writers
{
    public class UCPC : IWriter
    {
        ImportSettings importSettings;

        //Bounds bounds;
        float minX = Mathf.Infinity;
        float minY = Mathf.Infinity;
        float minZ = Mathf.Infinity;
        float maxX = Mathf.NegativeInfinity;
        float maxY = Mathf.NegativeInfinity;
        float maxZ = Mathf.NegativeInfinity;

        int pointCount;

        BufferedStream bsPoints = null;
        BinaryWriter writerPoints = null;
        BufferedStream bsColorsV2 = null;
        BinaryWriter writerColorsV2 = null;
        BufferedStream bsHeaderV2 = null;
        BinaryWriter writerHeaderV2 = null;

        string pointsTempFile;
        string colorsTempFile;
        string headerTempFile;

        bool IWriter.InitWriter(ImportSettings _importSettings, int _pointCount)
        {
            importSettings = _importSettings;
            pointCount = _pointCount;

            pointsTempFile = importSettings.outputFile + "_PointsTemp";
            colorsTempFile = importSettings.outputFile + "_ColorsTemp";
            headerTempFile = importSettings.outputFile + "_Header";

            try
            {
                bsPoints = new BufferedStream(new FileStream(pointsTempFile, FileMode.Create, FileAccess.Write, FileShare.None));
                writerPoints = new BinaryWriter(bsPoints);

                bsColorsV2 = new BufferedStream(new FileStream(colorsTempFile, FileMode.Create, FileAccess.Write, FileShare.None));
                writerColorsV2 = new BinaryWriter(bsColorsV2);

                bsHeaderV2 = new BufferedStream(new FileStream(headerTempFile, FileMode.Create, FileAccess.Write, FileShare.None));
                writerHeaderV2 = new BinaryWriter(bsHeaderV2);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        void IWriter.CreateHeader(int pointCount, ConverterBounds _bounds)
        {
            // create header data file 
            // write header v2 : 34 bytes
            byte[] magic = new byte[] { 0x75, 0x63, 0x70, 0x63 }; // ucpc
            writerHeaderV2.Write(magic); // 4b
            writerHeaderV2.Write((byte)2); // 1b
            writerHeaderV2.Write(importSettings.readRGB); // 1b

            if (importSettings.skipPoints == true)
            {
                pointCount = (int)Math.Floor(pointCount - (pointCount / (float)importSettings.skipEveryN));
                //Console.WriteLine("skipPoints, total = " + (pointCount - (pointCount / (float)importSettings.skipEveryN)));
            }

            if (importSettings.keepPoints == true) pointCount = pointCount / importSettings.keepEveryN;

            writerHeaderV2.Write(pointCount); // 4b
            // output bounds 4+4+4+4+4+4
            writerHeaderV2.Write(_bounds.minX);
            writerHeaderV2.Write(_bounds.minY);
            writerHeaderV2.Write(_bounds.minZ);
            writerHeaderV2.Write(_bounds.maxX);
            writerHeaderV2.Write(_bounds.maxY);
            writerHeaderV2.Write(_bounds.maxZ);

            // close header
            writerHeaderV2.Close();
            bsHeaderV2.Dispose();
        }

        void IWriter.WriteXYZ(float x, float y, float z)
        {
            writerPoints.Write(x);
            writerPoints.Write(y);
            writerPoints.Write(z);
        }

        void IWriter.WriteRGB(float r, float g, float b)
        {
            writerColorsV2.Write(r);
            writerColorsV2.Write(g);
            writerColorsV2.Write(b);
        }

        void IWriter.Randomize()
        {
            writerPoints.Flush();
            bsPoints.Flush();
            writerPoints.Close();
            bsPoints.Dispose();

            Debug.Log("(UCPC) Randomizing " + pointCount + " points...");
            // randomize points and colors
            byte[] tempBytes = null;
            using (FileStream fs = File.Open(pointsTempFile, FileMode.Open, FileAccess.Read, FileShare.None))
            using (BufferedStream bs = new BufferedStream(fs))
            using (BinaryReader binaryReader = new BinaryReader(bs))
            {
                tempBytes = binaryReader.ReadBytes(pointCount * 4 * 3);
            }
            float[] tempFloats = new float[pointCount * 3];

            // convert to float array
            GCHandle vectorPointer = GCHandle.Alloc(tempFloats, GCHandleType.Pinned);
            IntPtr pV = vectorPointer.AddrOfPinnedObject();
            Marshal.Copy(tempBytes, 0, pV, pointCount * 4 * 3);
            vectorPointer.Free();

            PointCloudMath.ShuffleXYZ(PointCloudMath.rnd, ref tempFloats);
            PointCloudMath.ResetRandom();

            // create new file on top, NOTE seek didnt work?
            bsPoints = new BufferedStream(new FileStream(pointsTempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read));
            writerPoints = new BinaryWriter(bsPoints);

            // TODO why not use writeallbytes?
            for (int i = 0; i < pointCount * 3; i++)
            {
                writerPoints.Write(tempFloats[i]);
            }

            // new files for colors
            writerColorsV2.Flush();
            bsColorsV2.Flush();
            writerColorsV2.Close();
            bsColorsV2.Dispose();

            Debug.Log("(UCPC) Randomizing " + pointCount + " colors...");

            tempBytes = null;
            using (FileStream fs = File.Open(colorsTempFile, FileMode.Open, FileAccess.Read, FileShare.None))
            using (BufferedStream bs = new BufferedStream(fs))
            using (BinaryReader binaryReader = new BinaryReader(bs))
            {
                tempBytes = binaryReader.ReadBytes(pointCount * 4 * 3);
            }

            tempFloats = new float[pointCount * 3];

            // convert to float array, TODO no need if can output writeallbytes
            vectorPointer = GCHandle.Alloc(tempFloats, GCHandleType.Pinned);
            pV = vectorPointer.AddrOfPinnedObject();
            Marshal.Copy(tempBytes, 0, pV, pointCount * 4 * 3);
            vectorPointer.Free();

            // actual point randomization
            //Tools.ShuffleXYZ(Tools.rnd, ref tempFloats);
            PointCloudMath.ShuffleXYZ(PointCloudMath.rnd, ref tempFloats);

            // create new file on top, seek didnt work?
            bsColorsV2 = new BufferedStream(new FileStream(colorsTempFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read));
            writerColorsV2 = new BinaryWriter(bsColorsV2);

            // TODO why not use writeallbytes? check 2gb file limit then use that
            for (int i = 0; i < pointCount * 3; i++)
            {
                writerColorsV2.Write(tempFloats[i]);
            }
        }

        void IWriter.AddPoint(int index, float x, float y, float z, float r, float g, float b)
        {
            // skip points
            if (importSettings.skipPoints == true && (index % importSettings.skipEveryN == 0)) return;

            // keep points
            if (importSettings.keepPoints == true && (index % importSettings.keepEveryN != 0)) return;

            // get bounds
            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (z < minZ) minZ = z;
            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
            if (z > maxZ) maxZ = z;

            importSettings.writer.WriteXYZ(x, y, z);
            importSettings.writer.WriteRGB(r, g, b);
        }

        void IWriter.Save()
        {
            // calculate bounds
            var bounds = new ConverterBounds();
            bounds.minX = minX;
            bounds.minY = minY;
            bounds.minZ = minZ;
            bounds.maxX = maxX;
            bounds.maxY = maxY;
            bounds.maxZ = maxZ;

            importSettings.writer.CreateHeader(pointCount, bounds);
            if (importSettings.randomize == true) importSettings.writer.Randomize();
            importSettings.writer.Close();
            importSettings.writer.Cleanup();
        }

        void IWriter.Cleanup()
        {
            Debug.Log("(UCPC) Combining files: " + Path.GetFileName(headerTempFile) + "," + Path.GetFileName(pointsTempFile) + "," + Path.GetFileName(colorsTempFile));
            Debug.Log("(UCPC) Output: " + importSettings.outputFile);

            var sep = '"';

            // fix slashes, forward slashes fail in command prompt too
            headerTempFile = headerTempFile.Replace("/", "\\");
            pointsTempFile = pointsTempFile.Replace("/", "\\");
            colorsTempFile = colorsTempFile.Replace("/", "\\");

            // combine files using commandline binary append
            var args = "/C copy /b " + sep + headerTempFile + sep + "+" + sep + pointsTempFile + sep + "+" + sep + colorsTempFile + sep + " " + sep + importSettings.outputFile + sep;
            //Debug.Log("args = " + args);
            Process proc = new Process();
            proc.StartInfo.FileName = "CMD.exe";
            proc.StartInfo.Arguments = args;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();
            proc.WaitForExit();

            Debug.Log("(UCPC) Deleting temporary files: " + Path.GetFileName(headerTempFile) + "," + Path.GetFileName(pointsTempFile) + "," + Path.GetFileName(colorsTempFile));
            File.Delete(headerTempFile);
            File.Delete(pointsTempFile);
            File.Delete(colorsTempFile);
        }

        void IWriter.Close()
        {
            writerPoints.Flush();
            bsPoints.Flush();
            writerPoints.Close();
            bsPoints.Dispose();
            writerColorsV2.Close();
            bsColorsV2.Dispose();
        }
    }
}
