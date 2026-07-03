/***************************************************************************
 *   Copyright (C) 2006 by Andi8104                                        *
 *   Andi8104@arcor.de                                                     *
 *                                                                         *
 *   Additional programming:                                               *
 *   Copyright (C) 2007-2013 by Mootilda                                   *
 *   http://www.modthesims.info/member.php?u=589252                        *
 *                                                                         *
 *   This program is free software; you can redistribute it and/or modify  *
 *   it under the terms of the GNU General Public License as published by  *
 *   the Free Software Foundation; either version 2 of the License, or     *
 *   (at your option) any later version.                                   *
 *                                                                         *
 *   This program is distributed in the hope that it will be useful,       *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the         *
 *   GNU General Public License for more details.                          *
 *                                                                         *
 *   You should have received a copy of the GNU General Public License     *
 *   along with this program; if not, write to the                         *
 *   Free Software Foundation, Inc.,                                       *
 *   59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.             *
 ***************************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using SimPe.Interfaces.Files;


namespace LotExpander
{
    public class R_ROOF
    {
        bool Test_PrintDebugInfo = false;   // Enable (T) or disable (F) printing of debug information

        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iLeadingZeros = 64;
        private int iBlockVersion;
        private int iItemCount = 0;
        private int iHeaderSize = 76;
        private int iBytesPerObject = 0;

        public R_ROOF(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
        {
            PFD = Descriptor;
            IPackedFile PF = LotPackage.Read(PFD);
            Data = PF.UncompressedData;

            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            for (int i = 0; i < iLeadingZeros; i++)
            {
                byte bDummy = BR.ReadByte();
                Debug.Assert(bDummy == 0);
            }
            uint uBlockID = BR.ReadUInt32();
            if (uBlockID != 0xAB9406AA)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid ROOF: Block ID");

            iBlockVersion = BR.ReadInt32();
            Debug.Assert((iBlockVersion == 1)   // TS2 and University
                      || (iBlockVersion == 2)   // Nightlife, Open for Business, Pets
                      || (iBlockVersion == 3)   // Seasons and above
            );                                  // ToDo: Determine whether other versions are known and handled correctly

            iItemCount = BR.ReadInt32();

            Debug.Assert(iHeaderSize == BR.BaseStream.Position);
            iHeaderSize = (int)BR.BaseStream.Position;

            if (iBlockVersion == 1)
                iBytesPerObject = 36;
            else if (iBlockVersion == 2)
                iBytesPerObject = 40;
            else if (iBlockVersion == 3)
                iBytesPerObject = 44;
            else if (iItemCount == 0)
                Debug.Fail("Unknown ROOF version");
            else
            {
                Debug.Fail("Unknown ROOF version");
                iBytesPerObject = (Data.Length - iHeaderSize) / iItemCount;
            }

            if (Data.Length != ExpectedLength)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid ROOF: Data Length");

            uint uInst = PFD.Instance;
            // Debug.Print("IPFD Type = AB9406AA = Roof  IPFD Instance = {0:X8} Version {1:D} Length {2:D} has {3:D} items of size {4:F}",
            //     uInst, iBlockVersion, Data.Length, iItemCount, ((float)(Data.Length - iHeaderSize))/iItemCount);
        }

        private int ExpectedLength
        {
            get
            {
                return iItemCount * iBytesPerObject + iHeaderSize;
            }
        }

#if ADJUST
        public void Change(int XAddLow, int XAddHigh, int YAddLow, int YAddHigh, int iWidthNew, int iHeightNew)
        {
            byte[] DataNew = new byte[Data.Length];
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
            BW.Write(BR.ReadBytes(iHeaderSize));
            int iTotalSize = iHeaderSize;
            for (int i = 0; i < iItemCount; i++)
            {
                int iStructureSize = 0;

                int Dumm = BR.ReadInt32();
                BW.Write(Dumm);
                iStructureSize += 4;
                if (Test_PrintDebugInfo)
                    Debug.Print("Roof({0}) ID? = {1:X8}", i, Dumm);

                float XOld1 = BR.ReadSingle();
                Debug.Assert(XOld1 >= 0);
                Debug.Assert((XOld1 % 1.0) == 0.0);
                float XNew1 = XOld1 + XAddLow;
                // No need to restrict:
                // float XFix1 = LETools.KeepOnLot(XOld1, XAddLow, XAddHigh, iWidthNew, 0);
                float XFix1 = LETools.KeepOnLot(XOld1 + XAddLow, 0, 0, iWidthNew, 0);
                if (XNew1 == XFix1)
                {
                    BW.Write(XNew1);
                    iStructureSize += 4;
                    if (Test_PrintDebugInfo)
                        Debug.Print("Roof({0}, 0) X = {1} -> {2}", i, XOld1, XNew1);
                }
                else
                    PrimaryForm.ThrowErrorOffLot("ROOF", "Roof");

                float YOld1 = BR.ReadSingle();
                Debug.Assert(YOld1 >= 0);
                Debug.Assert((YOld1 % 1.0) == 0.0);
                float YNew1 = YOld1 + YAddLow;
                // No need to restrict:
                // float YFix1 = LETools.KeepOnLot(YOld1, YAddLow, YAddHigh, iHeightNew, 0);
                float YFix1 = LETools.KeepOnLot(YOld1 + YAddLow, 0, 0, iHeightNew, 0);
                if (YNew1 == YFix1)
                {
                    BW.Write(YNew1);
                    iStructureSize += 4;
                    if (Test_PrintDebugInfo)
                        Debug.Print("Roof({0}, 0) Y = {1} -> {2}", i, YOld1, YNew1);
                }
                else
                    PrimaryForm.ThrowErrorOffLot("ROOF", "Roof");

                // Level?
                int iLevel1 = BR.ReadInt32();
                BW.Write(iLevel1);
                iStructureSize += 4;
                if (Test_PrintDebugInfo)
                    Debug.Print("Roof({0}, 0) Level = {1}", i, iLevel1);
                Debug.Assert((0 <= iLevel1) && (iLevel1 < 10)); // May go a bit higher, depending upon building...

                float XOld2 = BR.ReadSingle();
                Debug.Assert(XOld2 >= 0);
                Debug.Assert((XOld2 % 1.0) == 0.0);
                float XNew2 = XOld2 + XAddLow;
                // No need to restrict:
                // float XFix2 = LETools.KeepOnLot(XOld2, XAddLow, XAddHigh, iWidthNew, 0);
                float XFix2 = LETools.KeepOnLot(XOld2 + XAddLow, 0, 0, iWidthNew, 0);
                if (XNew2 == XFix2)
                {
                    BW.Write(XNew2);
                    iStructureSize += 4;
                    if (Test_PrintDebugInfo)
                        Debug.Print("Roof({0}, 1) X = {1} -> {2}", i, XOld2, XNew2);
                }
                else
                    PrimaryForm.ThrowErrorOffLot("ROOF", "Roof");

                float YOld2 = BR.ReadSingle();
                Debug.Assert(YOld2 >= 0);
                Debug.Assert((YOld2 % 1.0) == 0.0);
                float YNew2 = YOld2 + YAddLow;
                // No need to restrict:
                // float YFix2 = LETools.KeepOnLot(YOld2, YAddLow, YAddHigh, iHeightNew, 0);
                float YFix2 = LETools.KeepOnLot(YOld2 + YAddLow, 0, 0, iHeightNew, 0);
                if (YNew2 == YFix2)
                {
                    BW.Write(YNew2);
                    iStructureSize += 4;
                    if (Test_PrintDebugInfo)
                        Debug.Print("Roof({0}, 1) Y = {1} -> {2}", i, YOld2, YNew2);
                }
                else
                    PrimaryForm.ThrowErrorOffLot("ROOF", "Roof");

                // Level?
                float iLevel2 = BR.ReadSingle();
                BW.Write(iLevel2);
                iStructureSize += 4;
                if (Test_PrintDebugInfo)
                    Debug.Print("Roof({0}, 1) Level = {1}", i, iLevel2);
                // Debug.Assert(iLevel1 == iLevel2);

                Dumm = BR.ReadInt32();
                BW.Write(Dumm);
                iStructureSize += 4;
                if (Test_PrintDebugInfo)
                    Debug.Print("Roof({0}) Unknown 1 = {1:X8}", i, Dumm);

                Dumm = BR.ReadInt32();
                BW.Write(Dumm);
                iStructureSize += 4;
                if (Test_PrintDebugInfo)
                    Debug.Print("Roof({0}) Unknown 2 = {1:X8}", i, Dumm);

                if (iBlockVersion > 1)
                {
                    // Roof Slope Angle: 15-75; 45 is default
                    float F = BR.ReadSingle();
                    // What is the relationship between these floats and the RoofSlopeAngle cheat value?
                    // ToDo: Look for exact hex values, rather than approximate floating point values.
                    /*
                    Debug.Assert(
                        LETools.FloatEqual((float)0.1339746, F) // 15 degrees
                     || LETools.FloatEqual((float)0.1819851, F) // 20 degrees
                     || LETools.FloatEqual((float)0.2020131, F)
                     || LETools.FloatEqual((float)0.2331538, F) // 25 degrees
                     || LETools.FloatEqual((float)0.2547627, F)
                     || LETools.FloatEqual((float)0.2886751, F) // 30 degrees
                     || LETools.FloatEqual((float)0.3004303, F)
                     || LETools.FloatEqual((float)0.3247038, F)
                     || LETools.FloatEqual((float)0.35,      F)
                     || LETools.FloatEqual((float)0.3501038, F) // 35 degrees
                     || LETools.FloatEqual((float)0.376777,  F)
                     || LETools.FloatEqual((float)0.4195498, F) // 40 degrees
                     || (0.5 == F)                              // 45 degrees (Standard)
                     || LETools.FloatEqual((float)0.5177652, F)
                     || LETools.FloatEqual((float)0.5958768, F) // 50 degrees
                     || LETools.FloatEqual((float)0.6174486, F)
                     || LETools.FloatEqual((float)0.7140739, F) // 55 degrees
                     || LETools.FloatEqual((float)0.8660254, F)
                     // Values of 1.0 and above don't make sense.  May be special types of roofs.
                     || (1.0 == F)                              // 90 degrees?
                     || LETools.FloatEqual((float)1.732051, F)
                     || (3.0 == F)
                    );
                     */
                    BW.Write(F);
                    iStructureSize += 4;
                    if (Test_PrintDebugInfo)
                        Debug.Print("Roof({0}) Unknown 3 = {1} or {2} degrees?", i, F,
                            Math.Round((Math.Atan(F) * (180 / Math.PI)), 0));
                }

                if (iBlockVersion > 2)
                {
                    Dumm = BR.ReadInt32();
                    /* Debug.Assert(
                        (0 == Dumm)
                     || (1 == Dumm)
                     || (2 == Dumm)
                     || (3 == Dumm)
                     || (4 == Dumm)
                     || (5 == Dumm)
                     || (7 == Dumm)
                     || (12 == Dumm)
                     || (13 == Dumm)
                     || (14 == Dumm)
                     || (15 == Dumm)
                     || (17 == Dumm)
                     || (20 == Dumm)
                     || (23 == Dumm)
                     || (25 == Dumm)
                     || (26 == Dumm)
                     || (28 == Dumm)
                    ); */
                    BW.Write(Dumm);
                    iStructureSize += 4;
                    if (Test_PrintDebugInfo)
                        Debug.Print("Roof({0}) Unknown 4 = {1:X8}", i, Dumm);
                }

                Debug.Assert(iStructureSize == iBytesPerObject);
                iTotalSize += iStructureSize;
            }
            Debug.Assert(iTotalSize == Data.Length);
            Data = DataNew;
            PFD.SetUserData(Data, true);
        }
#endif

#if ! ADJUST
        public void AddLevel(int iNewLevel)
        {
            byte[] DataNew = new byte[Data.Length];
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
            BW.Write(BR.ReadBytes(iHeaderSize));
            int iTotalSize = iHeaderSize;
            for (int i = 0; i < iItemCount; i++)
            {
                int iStructureSize = 0;

                int Dumm = BR.ReadInt32();  // Roof Style
                BW.Write(Dumm);
                iStructureSize += 4;
                if (Test_PrintDebugInfo)
                    Debug.Print("Roof({0}) ID? = {1:X8}", i, Dumm);

                float XOld1 = BR.ReadSingle();
                Debug.Assert(XOld1 >= 0);
                Debug.Assert((XOld1 % 1.0) == 0.0);
                BW.Write(XOld1);
                iStructureSize += 4;

                float YOld1 = BR.ReadSingle();
                Debug.Assert(YOld1 >= 0);
                Debug.Assert((YOld1 % 1.0) == 0.0);
                BW.Write(YOld1);
                iStructureSize += 4;

                int iLevel1 = BR.ReadInt32();
                if (Test_PrintDebugInfo)
                    Debug.Print("Roof({0}, 0) Level = {1}", i, iLevel1);
                if (iLevel1 >= iNewLevel)
                    iLevel1++;
                if (Test_PrintDebugInfo)
                    Debug.Print("          To Level = {1}", i, iLevel1);
                BW.Write(iLevel1);
                iStructureSize += 4;

                float XOld2 = BR.ReadSingle();
                Debug.Assert(XOld2 >= 0);
                Debug.Assert((XOld2 % 1.0) == 0.0);
                BW.Write(XOld2);
                iStructureSize += 4;

                float YOld2 = BR.ReadSingle();
                Debug.Assert(YOld2 >= 0);
                Debug.Assert((YOld2 % 1.0) == 0.0);
                BW.Write(YOld2);
                iStructureSize += 4;

                float iLevel2 = BR.ReadSingle();
                if (Test_PrintDebugInfo)
                    Debug.Print("Roof({0}, 0) Level = {1}", i, iLevel2);
                if (iLevel2 >= iNewLevel)
                    iLevel2++;
                if (Test_PrintDebugInfo)
                    Debug.Print("          To Level = {1}", i, iLevel2);
                BW.Write(iLevel2);
                iStructureSize += 4;

                Dumm = BR.ReadInt32();          // int
                BW.Write(Dumm);
                iStructureSize += 4;

                Dumm = BR.ReadInt32();          // GUID: Roof Pattern
                BW.Write(Dumm);
                iStructureSize += 4;

                if (iBlockVersion > 1)
                {
                    float F = BR.ReadSingle();  // Roof slope angle
                    BW.Write(F);
                    iStructureSize += 4;
                }

                if (iBlockVersion > 2)
                {
                    Dumm = BR.ReadInt32();      // int
                    BW.Write(Dumm);
                    iStructureSize += 4;
                }

                Debug.Assert(iStructureSize == iBytesPerObject);
                iTotalSize += iStructureSize;
            }
            Debug.Assert(iTotalSize == Data.Length);
            Data = DataNew;
            PFD.SetUserData(Data, true);
        }
#endif
    }
}
