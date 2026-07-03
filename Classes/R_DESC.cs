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
    public class R_DESC : R_LotDescription
    {
        private bool Test_PrintDebugInfo = false;   // Enable (T) or disable (F) printing of debug information

        private byte[] Data;
        private IPackedFileDescriptor PFD;
        private ushort uVersion1 = 0;
        private ushort uVersion2 = 0;
        private int iWidthIndex = 4;
        private int iWidth = 0;
        private int iHeightIndex = 8;
        private int iHeight = 0;
        private byte bType = 0xFF;
        private int iU0Index = 15;
        private uint uU0 = 0xFFFFFFFF;
        private int iU10Index = 13;
        private byte bU10 = 0xFF;
        private byte bU11 = 0xFF;
        private int iLotNameLen = 0;
        private string sLotName = null;
        private int iDescIndex;
        private int iLotDescLen = 0;
        private string sLotDesc = null;
        private const int iMaxGrid = 128;
        private int iTerrainIndex;
        private float[] fTerrain = null;
        private int iTopIndex = 0;
        private int iTop = 0;
        private int iLeftIndex = 0;
        private int iLeft = 0;
        private int iElevIndex = 0;
        private float fElevation = 0;
        private int iLotNumberIndex = 0;
        private int iLotNumber = 0;
        private int iTextureIndex = 0;
        private int iTextureLen = 0;
        private string sTexture = null;
        private int iExtraIndex = 0;
        private float fExtra = 0;   // Another copy of the elevation?
        private int iLotClassValueIndex = 0;
        private uint iLotClassValue = 0;
        private int iClassOverrideIndex = 0;
        private byte bClassOverride = 0;
        private byte bOrientation = 0xFF;
        private int iSublotCount = 0;
        private string sFamilyName = null;

        public R_DESC(IPackageFile NBPackage, IPackedFileDescriptor LotDescriptor)
        {
            PFD = LotDescriptor;
            IPackedFile PF = NBPackage.Read(PFD);
            Data = PF.UncompressedData;
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);

            uVersion1 = BR.ReadUInt16();
            Debug.Assert( (uVersion1 == 13)
                       || (uVersion1 == 14)                         // Open for Business
                       || (uVersion1 == 18)                         // Apartment Life
            );   // ToDo: Determine whether other versions are known and handled correctly

            uVersion2 = BR.ReadUInt16();
            Debug.Assert((uVersion2 == 6)
                      || (uVersion2 == 7)                           // Bon Voyage
                      || (uVersion2 == 8)                           // Free Time
                      || (uVersion2 == 11)                          // Apartment Life
            );   // ToDo: Determine whether other versions are known and handled correctly

            Debug.Assert(iWidthIndex == BR.BaseStream.Position);
            iWidthIndex = (int)BR.BaseStream.Position;
            iWidth = BR.ReadInt32();
            if ((Width < 1) || (Width > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid DESC: Width");

            Debug.Assert(iHeightIndex == BR.BaseStream.Position);
            iHeightIndex = (int)BR.BaseStream.Position;
            iHeight = BR.ReadInt32();
            if ((Height < 1) || (Height > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid DESC: Height");

            bType = BR.ReadByte();
            Debug.Assert((bType == 0)   // Residential
                      || (bType == 1)   // Community
                      || (bType == 2)   // University: Dorm
                      || (bType == 3)   // University: Greek House
                      || (bType == 4)   // University: Secret Society?
                      || (bType == 5)   // Bon Voyage: Hotel
                      || (bType == 6)   // Bon Voyage: Hidden Vacation Lot
                      || (bType == 7)   // FreeTime: Hidden Hobby Lot
                      || (bType == 8)   // Apartment Life: Apartment Base
                      || (bType == 9)   // Apartment Life: Apartment Sublot
                      || (bType == 10)  // Apartment Life: Hidden Lot (Witches)
            );

            Debug.Assert(iU10Index == BR.BaseStream.Position);
            iU10Index = (int)BR.BaseStream.Position;
            bU10 = BR.ReadByte();
            Debug.Assert(bU10 < 0x10);

            bU11 = BR.ReadByte();
            Debug.Assert(bU11 < 4);

            Debug.Assert(iU0Index == BR.BaseStream.Position);
            iU0Index = (int)BR.BaseStream.Position;
            uU0 = BR.ReadUInt32();
            if (Test_PrintDebugInfo)
            {
                if (0x08 == (uU0 & 0x08))
                    Debug.Print("Furnishings will be removed");
                if (0x10 == (uU0 & 0x10))
                    Debug.Print("Hidden Lot");
                if (0x80 == (uU0 & 0x80))
                    Debug.Print("Beach Lot");
            }

            iLotNameLen = BR.ReadInt32();
            sLotName = SimPe.Helper.ToString(BR.ReadBytes(iLotNameLen));
            if (Test_PrintDebugInfo)
                Debug.Print("Lot Name: {0}", sLotName);

            iDescIndex = (int)BR.BaseStream.Position;
            iLotDescLen = BR.ReadInt32();
            sLotDesc = SimPe.Helper.ToString(BR.ReadBytes(iLotDescLen));
            if (Test_PrintDebugInfo)
                Debug.Print("Lot Desc: {0}", sLotDesc);

            iTerrainIndex = (int)BR.BaseStream.Position;
            int iArrayLen = BR.ReadInt32();
            fTerrain = new float[iArrayLen];
            // Is this the relative elevations from NHTG for this lot?
            // Debug.Assert(iArrayLen == ((iHeight + 1) * (iWidth + 1)));
            for (int i = 0; i < iArrayLen; i++)
            {
                fTerrain[i] = BR.ReadSingle();
                // Debug.Assert(0 == fTerrain[i]);
            }

            if ((uVersion2 >= 7))
            {
                // LETools.PrintDataTypes(Data, BR.BaseStream.Position);
                iExtraIndex = (int)BR.BaseStream.Position;
                fExtra = BR.ReadSingle();
                if (uVersion2 >= 8)
                {
                    // LETools.PrintDataTypes(Data, BR.BaseStream.Position);
                    int iDummy = BR.ReadInt32();
                    if (uVersion2 == 11)    // Apartment Life
                    {
                        byte bNumberOfApts = BR.ReadByte();

                        int iPrice1 = BR.ReadInt32();
                        int iPrice2 = BR.ReadInt32();

                        // Note: may be unsigned value, instead of signed.
                        iLotClassValueIndex = (int)BR.BaseStream.Position;
                        iLotClassValue = BR.ReadUInt32();

                        iClassOverrideIndex = (int)BR.BaseStream.Position;
                        bClassOverride = BR.ReadByte();
                        Debug.Assert((0 == bClassOverride) || (1 == bClassOverride));

                        if (bNumberOfApts > 0)
                        {
                            Debug.Assert(iPrice2 <= iPrice1);
                            if (Test_PrintDebugInfo)   // if (0 != bClassOverride)
                                Debug.Print("Lot{0} \"{5}\" Apartment price range: {1} - {2} Class {3}({4})", PFD.Instance, iPrice2, iPrice1, bClassOverride, iLotClassValue, sLotName);
                        }
                        else
                        {
                            Debug.Assert((-1 == iPrice1) || (0 == iPrice1));
                            Debug.Assert((0 == iPrice2) || (32000 == iPrice2));
                            if (Test_PrintDebugInfo)   // if (0 != bClassOverride)
                                Debug.Print("Lot{0} \"{5}\" Price range: {1} - {2} Class {3}({4})", PFD.Instance, iPrice2, iPrice1, bClassOverride, iLotClassValue, sLotName);
                        }
                    }
                }
            }

            iTopIndex = (int)BR.BaseStream.Position;
            iTop = BR.ReadInt32();
            if ((iTop < 0) || (iTop > iMaxGrid))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid DESC: Top");

            iLeftIndex = (int)BR.BaseStream.Position;
            iLeft = BR.ReadInt32();
            if ((iLeft < 0) || (iLeft > iMaxGrid))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid DESC: Left");

            iElevIndex = (int)BR.BaseStream.Position;
            fElevation = BR.ReadSingle();

            iLotNumberIndex = (int)BR.BaseStream.Position;
            iLotNumber = BR.ReadInt32();
            Debug.Assert(iLotNumber == PFD.Instance);

            bOrientation = BR.ReadByte();
            Debug.Assert(bOrientation < 4);

            iTextureIndex = (int)BR.BaseStream.Position;
            iTextureLen = BR.ReadInt32();
            sTexture = SimPe.Helper.ToString(BR.ReadBytes(iTextureLen));

            byte U2 = BR.ReadByte();             // Paused?
            // Debug.Assert((0 == U2) || (1 == U2) || (0x20 == U2) || (0x38 == U2) || (0x82 == U2) || 0xFF == U2);

            if (14 <= uVersion1)    // Open for Business
            {
                uint uOwner = BR.ReadUInt32();  // Sim info instance number

                if (Test_PrintDebugInfo && (0 != uOwner))
                    Debug.Print("Business owned by {0:X8}", uOwner);
            }
            if (11 == uVersion2)    // Apartment Life
            {
                if (bType == 8)
                {
                    if (Test_PrintDebugInfo)
                        Debug.Print("Lot{0} Apartment Base: {1}", PFD.Instance, LotName);
                }
                else if (bType == 9)
                {
                    if (Test_PrintDebugInfo)
                        Debug.Print("Lot{0} Apartment Sublot: {1}", PFD.Instance, LotName);
                }

                uint uAptBase = BR.ReadUInt32();
                // Only an apartment sublot should have an associated apartment base:
                if (bType == 9)
                {
                    Debug.Assert(uAptBase != 0);
                    if (Test_PrintDebugInfo)
                        Debug.Print("Base lot: {0}", uAptBase);
                }
                else
                    Debug.Assert(uAptBase == 0);

                for (int i = 0; i < 9; i++)
                {
                    byte b = BR.ReadByte();
                    Debug.Assert(0 == b);
                }

                iSublotCount = BR.ReadInt32();
                Debug.Assert(iSublotCount < 5);
                for (int i = 0; i < iSublotCount; i++)
                {
                    uint uAptSublot = BR.ReadUInt32();
                    uint uFamily = BR.ReadUInt32();  // Family info instance number
                    uint u2 = BR.ReadUInt32();
                    uint u3 = BR.ReadUInt32();
                    Debug.Assert(u3 == 0);

                    if (Test_PrintDebugInfo)
                        Debug.Print("Occupied lot: {0} {1} {2} {3}", uAptSublot, uFamily, u2, u3);
                }

#if DEBUG
                int iCount = BR.ReadInt32();
                for (int i = 0; i < iCount; i++)
                {
                    uint uDummy = BR.ReadUInt32();
                    // Debug.Assert(0 == uDummy);
                }
                Debug.Assert(Data.Length == BR.BaseStream.Position);
#endif
            }

            Debug.Assert(Data.Length == BR.BaseStream.Position);
        }

        public override uint Instance
        {
            get
            {
                return PFD.Instance;
            }
        }

        private void ReplaceUInt(uint iOld, uint iNew, int iIndex)
        {
            byte[] BA = new byte[4];

            Array.Copy(Data, iIndex, BA, 0, 4);
            BinaryReader BR = SimPe.Helper.GetBinaryReader(BA);
            uint iInt = BR.ReadUInt32();
            Debug.Assert(iInt == iOld);

            BinaryWriter BW = new BinaryWriter(new MemoryStream(BA));
            BW.Write(iNew);
            Array.Copy(BA, 0, Data, iIndex, 4);
            PFD.SetUserData(Data, true);
        }

        private void ReplaceInt(int iOld, int iNew, int iIndex)
        {
            byte[] BA = new byte[4];

            Array.Copy(Data, iIndex, BA, 0, 4);
            BinaryReader BR = SimPe.Helper.GetBinaryReader(BA);
            int iInt = BR.ReadInt32();
            Debug.Assert(iInt == iOld);

            BinaryWriter BW = new BinaryWriter(new MemoryStream(BA));
            BW.Write(iNew);
            Array.Copy(BA, 0, Data, iIndex, 4);
            PFD.SetUserData(Data, true);
        }

        public override int Width
        {
            get
            {
                return iWidth;
            }
            set
            {
                if (iWidth != value)
                {
                    ReplaceInt(iWidth, value, iWidthIndex);
                    iWidth = value;
                }
            }
        }

        public override int Height
        {
            get
            {
                return iHeight;
            }
            set
            {
                if (iHeight != value)
                {
                    ReplaceInt(iHeight, value, iHeightIndex);
                    iHeight = value;
                }
            }
        }

        private void ReplaceFloat(float fOld, float fNew, int iIndex)
        {
            byte[] BA = new byte[4];

            Array.Copy(Data, iIndex, BA, 0, 4);
            BinaryReader BR = SimPe.Helper.GetBinaryReader(BA);
            float fFloat = BR.ReadSingle();
            Debug.Assert(fFloat == fOld);

            BinaryWriter BW = new BinaryWriter(new MemoryStream(BA));
            BW.Write(fNew);
            Array.Copy(BA, 0, Data, iIndex, 4);
            PFD.SetUserData(Data, true);
        }

        public float Elevation
        {
            get
            {
                return fElevation;
            }
            set
            {
                if (fElevation != value)
                {
                    if (fElevation == fExtra)
                    {
                        ReplaceFloat(fExtra, value, iExtraIndex);
                        fExtra = value;
                    }
                    ReplaceFloat(fElevation, value, iElevIndex);
                    fElevation = value;
                }
            }
        }

        public override byte LotType
        {
            get
            {
                return bType;
            }
        }

        public override bool CanRemoveFurniture
        {
            get
            {
                return (bType == 0) || (bType == 1) || (bType == 4) || (bType == 5) || (bType == 8);
            }
        }

        public override bool Occupied
        {
            get
            {
                return (0 != iSublotCount);
            }
        }

        public bool HasClassValue
        {
            get
            {
                return (11 == uVersion2);
            }
        }

        public uint LotClassValue
        {
            get
            {
                // If Apartment Life, this should be set, otherwise 0
                return (iLotClassValue);
            }
            set
            {
                // If not Apartment Life, this should never be called:
                Debug.Assert(0 != iClassOverrideIndex);
                Debug.Assert(0 != iLotClassValueIndex);

                // Only called to override value
                Debug.Assert(bClassOverride == Data[iClassOverrideIndex]);
                Data[iClassOverrideIndex] = bClassOverride = 1;
                ReplaceUInt(iLotClassValue, value, iLotClassValueIndex);
            }
        }

        public int LotClassValueOverride
        {
            get
            {
                // If Apartment Life, this should be 0 or 1, otherwise 0
                return (bClassOverride);
            }
        }

        public void ClearLotClassValue(uint value)
        {
            // If not Apartment Life, this should never be called:
            Debug.Assert(0 != iClassOverrideIndex);
            Debug.Assert(0 != iLotClassValueIndex);

            // Only called to clear value
            Debug.Assert(bClassOverride == Data[iClassOverrideIndex]);
            Data[iClassOverrideIndex] = bClassOverride = 0;
            ReplaceUInt(iLotClassValue, value, iLotClassValueIndex);
        }

        public override uint U0
        {
            get
            {
                return uU0;
            }
            set
            {
                if (uU0 != value)
                {
                    ReplaceUInt(uU0, value, iU0Index);
                    uU0 = value;
                }
            }
        }

        public override byte U10
        {
            get
            {
                return bU10;
            }
            set
            {
                Debug.Assert(bU10 == Data[iU10Index]);
                Data[iU10Index] = bU10 = value;
                PFD.SetUserData(Data, true);
            }
        }

        public override byte U11
        {
            get
            {
                return bU11;
            }
        }

        public override string LotName
        {
            get
            {
                return sLotName;
            }
        }

        public string FamilyName
        {
            set
            {
                sFamilyName = value;
            }
        }

        public override string ToString()
        {
            string s = sLotName;
            if (null != sFamilyName)
                s += " [" + sFamilyName + "]";
            return s;
        }

        // Unused
        public override string LotDesc
        {
            get
            {
                return sLotDesc;
            }
#if ADJUST
            set
            {
                // Only called by the LotCorrupter at the very end of processing
                // Will destroy the indexes, so better not process any more.
                Debug.Assert(LETools.Corrupt);
                int iLenNew = value.Length;

                // Unfortunately, BW.Write will write out the length as part of a 7BitStr
                // So, we must convert from string to byte[]
                // Looks like SimPE will do this for us:
                byte[] b = SimPe.Helper.ToBytes(value);

                byte[] DataNew = new byte[Data.Length - iLotDescLen + iLenNew];
                BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
                BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                BW.Write(BR.ReadBytes(iDescIndex));

                int iLenOld = BR.ReadInt32();
                BW.Write(iLenNew);
                Debug.Assert(iLenOld == iLotDescLen);

                string s = SimPe.Helper.ToString(BR.ReadBytes(iLotDescLen));
                Debug.Print("Replace DESC \"{0}\"", s);
                BW.Write(b);
                BW.Write(BR.ReadBytes(Data.Length - iDescIndex - 4 - iLotDescLen));

                Data = DataNew;
                PFD.SetUserData(Data, true);
            }
#endif
        }

        public int Top
        {
            get
            {
                return iTop;
            }
            set
            {
                ReplaceInt(iTop, value, iTopIndex);
                iTop = value;
            }
        }

        public int Left
        {
            get
            {
                return iLeft;
            }
            set
            {
                ReplaceInt(iLeft, value, iLeftIndex);
                iLeft = value;
            }
        }

        public byte Orientation
        {
            get
            {
                return bOrientation;
            }
        }

        public string LotTerrain
        {
            get
            {
                return sTexture;
            }
            set
            {
                if (0 != string.Compare(value, sTexture, true))
                {
                    byte[] DataNew = new byte[Data.Length + value.Length - sTexture.Length];
                    BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                    BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);

                    BW.Write(BR.ReadBytes(iTextureIndex));

                    int iOldLen = BR.ReadInt32();
                    byte[] bOldTexture = BR.ReadBytes(iOldLen);
                    string sOldName = SimPe.Helper.ToString(bOldTexture);
                    Debug.Assert(0 == string.Compare(sTexture, sOldName, true));

                    byte[] bNewTexture = SimPe.Helper.ToBytes(value);
                    int iNewLen = bNewTexture.Length;

                    BW.Write(iNewLen);
                    BW.Write(bNewTexture);

                    BW.Write(BR.ReadBytes(Data.Length - (int)BR.BaseStream.Position));

                    Data = DataNew;
                    PFD.SetUserData(Data, true);
                }
            }
        }

        private void Swap(ref int X, ref int Y)
        {
            int iTemp = X;
            X = Y;
            Y = iTemp;
        }

        // ToDo: I believe that this is incorrect, because the LotTerrain is actually rotated from the HoodTerrain
        public void CheckLocalHoodTerrain(float[,] fHoodTerrain)
        {
Test_PrintDebugInfo = true;
            int iCount = 0;
            int iH = Height + 1;
            int iW = Width + 1;
            /*
            if (0 == (U11 % 2))         // if U11_Left or U11_Right
                Swap(ref iW, ref iH);   //     Meanings of height and width are swapped
            if (1 == (Orientation % 2)) // ir Orientation_Left or Orientation_Right
                Swap(ref iW, ref iH);   //     Meanings of height and width are swapped
             */

            // Select the portion of the terrain used by the lot,
            // rotated clockwise to match the lot terrain
            int iRotation = (4 + Orientation - U11) % 4;
            float[,] fLocalHoodTerrain = Rotate(fHoodTerrain, iW, iH, iRotation, -Elevation);
            for (int i = 0; i < iW; i++)
            {
                for (int j = 0; j < iH; j++)
                {
                    if (fTerrain[i * iH + j] == fLocalHoodTerrain[i, j])
                        iCount++;
                }
            }
            if (Test_PrintDebugInfo)
            {
                if (iCount != fTerrain.Length)
                {
                    for (int i = 0; i < iW; i++)
                    {
                        for (int j = 0; j < iH; j++)
                            Debug.Print("Hood[{0},{1}] = {2}", i, j, fLocalHoodTerrain[i, j]);
                    }
                    for (int i = 0; i < iW; i++)
                    {
                        for (int j = 0; j < iH; j++)
                            Debug.Print("DESC[{0},{1}] = {2}", i, j, fTerrain[i * iH + j]);
                    }
                }
            }
            Debug.Assert(iCount == fTerrain.Length);
            if (Test_PrintDebugInfo)
                Debug.Print("{0}: {1}", LotName, (iCount == fTerrain.Length) ? "Matches" : "Does not match");

            // ToDo: Should also compare major terrain vertices from lot package
Test_PrintDebugInfo = false;
        }

        // ToDo: Change GetLocalHoodTerrain & ReplaceLocalHoodTerrain functions to LocalHoodTerrain property (get & set)?
        public float[,] GetLocalHoodTerrain(float[,] fHoodTerrain)
        {
            int iH = Height + 1;
            int iW = Width + 1;

            // Select the portion of the terrain used by the lot,
            // rotated clockwise to match the lot terrain
            int iRotation = (4 + Orientation - U11) % 4;
            float[,] fLocalHoodTerrain = Rotate(fHoodTerrain, iW, iH, iRotation, - Elevation);
            if (Test_PrintDebugInfo)
            {
                Debug.Print("Hood Terrain adjusted for lot");
                for (int i = 0; i < iW; i++)
                {
                    string s = string.Format("{0}:  ", i);
                    for (int j = 0; j < iH; j++)
                    {
                        s = string.Format("{0}  {1:000.0000}", s, fLocalHoodTerrain[i, j]);
                    }
                    Debug.Print(s);
                }
            }
            return fLocalHoodTerrain;
        }

        public void ReplaceLocalHoodTerrain(float[,] fLocalHoodTerrain)
        {
            // Debug.Fail("R_DESC::FixTerrain should never be called; not yet implemented!");
            int iH = Height + 1;
            int iW = Width + 1;

            int iLengthNew = iW * iH;
            byte[] DataNew = new byte[Data.Length + (iLengthNew - fTerrain.Length) * 4];
            BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);

            // Copy up to the terrain array
            BW.Write(BR.ReadBytes(iTerrainIndex));

            // Read old length, replace with new length
            int iLengthOld = BR.ReadInt32();
            Debug.Assert(iLengthOld == fTerrain.Length);
            BW.Write(iLengthNew);

            // Read old array, replace with new array
            for (int i = 0; i < iLengthOld; i++)
            {
                float f = BR.ReadSingle();
            }
            for (int i = 0; i < iW; i++)
            {
                for (int j = 0; j < iH; j++)
                {
                    BW.Write(fLocalHoodTerrain[i, j]);
                }
            }

            // Copy the rest of the record
            BW.Write(BR.ReadBytes(Data.Length - iTerrainIndex - 4 - iLengthOld * 4));

            Data = DataNew;
            PFD.SetUserData(Data, true);
        }

        public float[,] Rotate(float[,] faOrig, int iWidth, int iHeight, int iRotation, float fAdjustment)
        {
            float[,] faNew = new float[iWidth, iHeight];
            switch (iRotation)
            {
                case 0: // 0 degrees
                    {
                        for (int iFromX = 0, iToY = 0; iToY < iHeight; iFromX++, iToY++)
                        {
                            for (int iFromY = 0, iToX = 0; iToX < iWidth; iFromY++, iToX++)
                            {
                                faNew[iToX, iToY] = faOrig[iFromX, iFromY] + fAdjustment;
                            }
                        }
                        break;
                    }
                case 1: // 90 degrees clockwise
                    {
                        for (int iFromX = 0, iToX = 0; iToX < iWidth; iFromX++, iToX++)
                        {
                            for (int iFromY = 0, iToY = 0; iToY < iHeight; iFromY++, iToY++)
                            {
                                faNew[iWidth - iToX - 1, iToY] = faOrig[iFromX, iFromY] + fAdjustment;
                            }
                        }
                        break;
                    }
                case 2: // 180 degrees
                    {
                        for (int iFromX = 0, iToY = 0; iToY < iHeight; iFromX++, iToY++)
                        {
                            for (int iFromY = 0, iToX = 0; iToX < iWidth; iFromY++, iToX++)
                            {
                                faNew[iWidth - iToX - 1, iHeight - iToY - 1] = faOrig[iFromX, iFromY] + fAdjustment;
                            }
                        }
                        break;
                    }
                case 3: // 270 degrees clockwise
                    {
                        for (int iFromX = 0, iToX = 0; iToX < iWidth; iFromX++, iToX++)
                        {
                            for (int iFromY = 0, iToY = 0; iToY < iHeight; iFromY++, iToY++)
                            {
                                faNew[iToX, iHeight - iToY - 1] = faOrig[iFromX, iFromY] + fAdjustment;
                            }
                        }
                        break;
                    }
                default:
                    {
                        if (LETools.ErrorChecking)
                            throw new InvalidDataException("Invalid Rotation");
                        break;
                    }
            }
            return (faNew);
        }
    }
}
