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
    public class R_2ARY
    {
        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iLeadingZeros = 64;
        private int iWidthIndex = 81;
        private int iWidth = 0;
        private int iHeightIndex = 85;
        private int iHeight = 0;
        private int iHeaderSize = 89;
        private const int iConvertToQuarterTile = 4; // Arrays are for quarter tiles, not full tiles.
        private const int iConvertTilesToVertices = 1;  // Add: tiles -> vertices; Subtract: vertices -> tiles
        private const int iBytesPerObject = 1;
        private const byte bDefaultValue = 0;

        public R_2ARY(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
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
            if (uBlockID != 0x6B943B43)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 2ARY: Block ID");

            int iBlockVersion = BR.ReadInt32();
            Debug.Assert(iBlockVersion == 1);   // ToDo: Determine whether other versions are known and handled correctly

            byte bBlockNameLen = (byte)BR.PeekChar();
            if ((bBlockNameLen & 0x80) != 0)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 2ARY: Block Name Length");
            string sBlockName = BR.ReadString();
            if (sBlockName != "c2DArray")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 2ARY: Block Name");

            Debug.Assert(iWidthIndex == BR.BaseStream.Position);
            iWidthIndex = (int)BR.BaseStream.Position;
            iWidth = BR.ReadInt32();

            Debug.Assert(iHeightIndex == BR.BaseStream.Position);
            iHeightIndex = (int)BR.BaseStream.Position;
            iHeight = BR.ReadInt32();

            Debug.Assert(iHeaderSize == BR.BaseStream.Position);
            iHeaderSize = (int)BR.BaseStream.Position;

            uint uInst = PFD.Instance;
            // Debug.Print("2ARY Instance {0:X8}", uInst);
#if DEBUG
            switch (uInst)
            {
                case 0x00005CBC:    // terrain-cliff
                case 0x00005CBD:    // terrain-seabed-shallow
                case 0x00005CBE:    // terrain-seabed-deep
                case 0x00005CBF:    // terrain-beach
                case 0x00005CC0:    // Terrain paints up to but excluding 0x5CEE
                case 0x00005CC1:
                case 0x00005CC2:
                case 0x00005CC3:
                case 0x00005CC4:
                case 0x00005CC5:
                case 0x00005CC6:
                case 0x00005CC7:
                case 0x00005CC8:
                case 0x00005CC9:
                case 0x00005CCA:
                case 0x00005CCB:
                case 0x00005CCC:
                case 0x00005CCD:
                case 0x00005CCE:
                case 0x00005CCF:
                case 0x00005CD0:
                case 0x00005CD1:
                case 0x00005CD2:
                case 0x00005CD3:
                case 0x00005CD4:
                case 0x00005CD5:
                case 0x00005CD6:
                case 0x00005CD7:
                case 0x00005CD8:
                case 0x00005CD9:
                case 0x00005CDA:
                case 0x00005CDB:
                case 0x00005CDC:
                case 0x00005CDD:
                case 0x00005CDE:
                case 0x00005CDF:
                case 0x00005CE0:
                case 0x00005CE1:
                case 0x00005CE2:
                case 0x00005CE3:
                case 0x00005CE4:
                case 0x00005CE5:
                case 0x00005CE6:
                case 0x00005CE7:
                case 0x00005CE8:
                case 0x00005CE9:
                case 0x00005CEA:
                case 0x00005CEB:
                case 0x00005CEC:
                case 0x00005CED:
                    {
                        // Instances that we know how to handle.
                        break;
                    }
                default:
                    {
                        // Debug.Fail("Unknown 2D Array Instance - may not be handled correctly");
                        break;
                    }
            }
#endif

            if ((Width < 1) || (Width > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 2ARY: Width");
            if ((Height < 1) || (Height > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 2ARY: Height");

            if (Data.Length != ExpectedLength)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 2ARY: Data Length");
        }

        private int ExpectedLength
        {
            get
            {
                return iWidth * iHeight * iBytesPerObject + iHeaderSize;
            }
        }

        private void ReplaceInt(int i, int iIndex)
        {
            byte[] BA = new byte[4];
            BinaryWriter BW = new BinaryWriter(new MemoryStream(BA));
            BW.Write(i);
            Array.Copy(BA, 0, Data, iIndex, 4);
            PFD.SetUserData(Data, true);

            Debug.Assert(Data.Length == ExpectedLength);
        }

        private void ReplaceWidth(int i)
        {
            iWidth = i;
            ReplaceInt(iWidth, iWidthIndex);
        }

        private void ReplaceHeight(int i)
        {
            iHeight = i;
            ReplaceInt(iHeight, iHeightIndex);
        }

        public int Width
        {
            get
            {
                return (iWidth - iConvertTilesToVertices) / iConvertToQuarterTile;
            }
            set
            {
                int iWidthDiff = (value - this.Width) * iConvertToQuarterTile;
                if (iWidthDiff > 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight * iBytesPerObject];
                    // Insert "quarterlinewise" at the end (i.e. in increments of 1/4th of a line or row)
                    //Zeilenweise am Ende einfügen
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    for (int i = 0; i < iHeight; i++)
                    {
                        int iCopySize = iWidth * iBytesPerObject;
                        Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                        iOldIndex += iCopySize;
                        iNewIndex += iCopySize;
                        for (int j = 0; j < iWidthDiff * iBytesPerObject; j++)
                        {
                            DataNew[iNewIndex++] = bDefaultValue;
                        }
                    }
                    Data = DataNew;
                    ReplaceWidth(value * iConvertToQuarterTile + iConvertTilesToVertices);
                }
                else if (iWidthDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight * iBytesPerObject];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    for (int i = 0; i < iHeight; i++)
                    {
                        int iCopySize = (iWidth + iWidthDiff) * iBytesPerObject;
                        Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                        iOldIndex += iCopySize + (0 - iWidthDiff) * iBytesPerObject;
                        iNewIndex += iCopySize;
                    }
                    Data = DataNew;
                    ReplaceWidth(value * iConvertToQuarterTile + iConvertTilesToVertices);
                }
            }
        }

        public int WidthRev
        {
            set
            {
                int iWidthDiff = (value - this.Width) * iConvertToQuarterTile;
                if (iWidthDiff > 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight * iBytesPerObject];
                    // Insert "quarterlinewise" at the beginning (i.e. in increments of 1/4th of a line or row)
                    //Zeilenweise am Anfang einfügen
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    for (int i = 0; i < iHeight; i++)
                    {
                        for (int j = 0; j < iWidthDiff * iBytesPerObject; j++)
                        {
                            DataNew[iNewIndex++] = bDefaultValue;
                        }
                        int iCopySize = iWidth * iBytesPerObject;
                        Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                        iOldIndex += iCopySize;
                        iNewIndex += iCopySize;
                    }
                    Data = DataNew;
                    ReplaceWidth(value * iConvertToQuarterTile + iConvertTilesToVertices);
                }
                else if (iWidthDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight * iBytesPerObject];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    for (int i = 0; i < iHeight; i++)
                    {
                        int iCopySize = (iWidth + iWidthDiff) * iBytesPerObject;
                        iOldIndex += (0 - iWidthDiff) * iBytesPerObject;
                        Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                        iOldIndex += iCopySize;
                        iNewIndex += iCopySize;
                    }
                    Data = DataNew;
                    ReplaceWidth(value * iConvertToQuarterTile + iConvertTilesToVertices);
                }
            }
        }

        public int Height
        {
            get
            {
                return (iHeight - iConvertTilesToVertices) / iConvertToQuarterTile;
            }
            set
            {
                int iHeightDiff = (value - this.Height) * iConvertToQuarterTile;
                if (iHeightDiff > 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff * iBytesPerObject];
                    // Header and existing data
                    Array.Copy(Data, DataNew, Data.Length);
                    // Insert at end
                    for (int i = Data.Length; i < DataNew.Length; i++)
                    {
                        DataNew[i] = bDefaultValue;
                    }
                    Data = DataNew;
                    ReplaceHeight(value * iConvertToQuarterTile + iConvertTilesToVertices);
                }
                else if (iHeightDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff * iBytesPerObject];
                    Array.Copy(Data, DataNew, DataNew.Length);
                    Data = DataNew;
                    ReplaceHeight(value * iConvertToQuarterTile + iConvertTilesToVertices);
                }
            }
        }

        public int HeightRev
        {
            set
            {
                int iHeightDiff = (value - this.Height) * iConvertToQuarterTile;
                if (iHeightDiff > 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff * iBytesPerObject];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    //Am Anfang einfügen
                    int iNewIndex = iHeaderSize;
                    for (int i = Data.Length; i < DataNew.Length; i++)
                    {
                        DataNew[iNewIndex++] = bDefaultValue;
                    }
                    //vorhandene Daten
                    Array.Copy(Data, iHeaderSize, DataNew, iNewIndex, Data.Length - iHeaderSize);
                    Data = DataNew;
                    ReplaceHeight(value * iConvertToQuarterTile + iConvertTilesToVertices);
                }
                else if (iHeightDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff * iBytesPerObject];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize + (0 - iHeightDiff) * iWidth * iBytesPerObject;
                    Array.Copy(Data, iOldIndex, DataNew, iHeaderSize, Data.Length - iOldIndex);
                    Data = DataNew;
                    ReplaceHeight(value * iConvertToQuarterTile + iConvertTilesToVertices);
                }
            }
        }

        public int Usage
        {
            get
            {
                int iUsage = 0;
                for (int i = 0; i < iHeight * iWidth * iBytesPerObject; i++)
                {
                    if (Data[iHeaderSize + i] != 0)
                        iUsage++;
                }
                return iUsage;
            }
        }

    }
}

