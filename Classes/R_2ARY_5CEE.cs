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

// IPFD.Instance = 0x00005CEE must be handled separately from most 2D Arrays
// because structure lengths are imbedded in the data.
namespace LotExpander
{
    public class R_2ARY_5CEE
    {
        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iLeadingZeros = 64;
        private int iWidthIndex = 81;
        private int iWidth = 0;
        private int iHeightIndex = 85;
        private int iHeight = 0;
        private int iHeaderSize = 89;
        private const int iConvertToQuarterTile = 4;    // Arrays are for quarter tiles, not full tiles.
        private const int iConvertTilesToVertices = 1;  // Add: tiles -> vertices; Subtract: vertices -> tiles
        private byte bDefaultValue = 0;


        public R_2ARY_5CEE(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
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
                    throw new InvalidDataException("Invalid 2ARY Instance 5CEE: Block ID");

            int iBlockVersion = BR.ReadInt32();
            Debug.Assert(iBlockVersion == 1);   // ToDo: Determine whether other versions are known and handled correctly

            byte bBlockNameLen = (byte)BR.PeekChar();
            if ((bBlockNameLen & 0x80) != 0)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 2ARY Instance 5CEE: Block Name Length");
            string sBlockName = BR.ReadString();
            if (sBlockName != "c2DArray")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 2ARY Instance 5CEE: Block Name");

            Debug.Assert(iWidthIndex == BR.BaseStream.Position);
            iWidthIndex = (int)BR.BaseStream.Position;
            iWidth = BR.ReadInt32();

            Debug.Assert(iHeightIndex == BR.BaseStream.Position);
            iHeightIndex = (int)BR.BaseStream.Position;
            iHeight = BR.ReadInt32();

            Debug.Assert(iHeaderSize == BR.BaseStream.Position);
            iHeaderSize = (int)BR.BaseStream.Position;

            if ((Width < 1) || (Width > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 2ARY Instance 5CEE: Width");
            if ((Height < 1) || (Height > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 2ARY Instance 5CEE: Height");

            // ToDo: Is there any way to check length?
        }

        private void ReplaceInt(int i, int iIndex)
        {
            byte[] BA = new byte[4];
            BinaryWriter BW = new BinaryWriter(new MemoryStream(BA));
            BW.Write(i);
            Array.Copy(BA, 0, Data, iIndex, 4);
            PFD.SetUserData(Data, true);

            // ToDo: Is there any way to check length?
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
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight];
                    // Insert "quarterlinewise" at the end (i.e. in increments of 1/4th of a line or row)
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    for (int i = 0; i < iHeight; i++)
                    {
                        for (int j = 0; j < iWidth; j++)
                        {
                            byte bItemCount = DataNew[iNewIndex++] = Data[iOldIndex++];
                            if (bItemCount > 0)
                            {
                                Array.Copy(Data, iOldIndex, DataNew, iNewIndex, bItemCount);
                                iOldIndex += bItemCount;
                                iNewIndex += bItemCount;
                            }
                        }
                        for (int j = 0; j < iWidthDiff; j++)
                        {
                            DataNew[iNewIndex++] = bDefaultValue;
                        }
                    }
                    Data = DataNew;
                    ReplaceWidth(value * iConvertToQuarterTile + iConvertTilesToVertices);
                }
                else if (iWidthDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    for (int i = 0; i < iHeight; i++)
                    {
                        for (int j = 0; j < (iWidth + iWidthDiff); j++)
                        {
                            byte bItemCount = DataNew[iNewIndex++] = Data[iOldIndex++];
                            if (bItemCount > 0)
                            {
                                Array.Copy(Data, iOldIndex, DataNew, iNewIndex, bItemCount);
                                iOldIndex += bItemCount;
                                iNewIndex += bItemCount;
                            }
                        }
                        for (int j = 0; j < (0 - iWidthDiff); j++)
                        {
                            byte bItemCount = Data[iOldIndex++];
                            if (bItemCount > 0)
                            {
                                iOldIndex += bItemCount;
                            }
                        }
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
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight];
                    // Insert "quarterlinewise" at the beginning (i.e. in increments of 1/4th of a line or row)
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    for (int i = 0; i < iHeight; i++)
                    {
                        for (int j = 0; j < iWidthDiff; j++)
                        {
                            DataNew[iNewIndex++] = bDefaultValue;
                        }
                        for (int j = 0; j < iWidth; j++)
                        {
                            byte bItemCount = DataNew[iNewIndex++] = Data[iOldIndex++];
                            if (bItemCount > 0)
                            {
                                Array.Copy(Data, iOldIndex, DataNew, iNewIndex, bItemCount);
                                iOldIndex += bItemCount;
                                iNewIndex += bItemCount;
                            }
                        }
                    }
                    Data = DataNew;
                    ReplaceWidth(value * iConvertToQuarterTile + iConvertTilesToVertices);
                }
                else if (iWidthDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    for (int i = 0; i < iHeight; i++)
                    {
                        for (int j = 0; j < (0 - iWidthDiff); j++)
                        {
                            byte bItemCount = Data[iOldIndex++];
                            if (bItemCount > 0)
                            {
                                iOldIndex += bItemCount;
                            }
                        }
                        for (int j = 0; j < (iWidth + iWidthDiff); j++)
                        {
                            byte bItemCount = DataNew[iNewIndex++] = Data[iOldIndex++];
                            if (bItemCount > 0)
                            {
                                Array.Copy(Data, iOldIndex, DataNew, iNewIndex, bItemCount);
                                iOldIndex += bItemCount;
                                iNewIndex += bItemCount;
                            }
                        }
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
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff];
                    // Header and existing data
                    Array.Copy(Data, DataNew, Data.Length);
                    //Am Ende einfügen
                    for (int i = Data.Length; i < DataNew.Length; i++)
                    {
                        DataNew[i] = bDefaultValue;
                    }
                    Data = DataNew;
                    ReplaceHeight(value * iConvertToQuarterTile + iConvertTilesToVertices);
                }
                else if (iHeightDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff];
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
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff];
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
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize + (0 - iHeightDiff) * iWidth;
                    Array.Copy(Data, iOldIndex, DataNew, iHeaderSize, Data.Length - iOldIndex);
                    Data = DataNew;
                    ReplaceHeight(value * iConvertToQuarterTile + iConvertTilesToVertices);
                }
            }
        }

        public int[] GetUsage(int iLength)
        {
            int[] iUsage = new int[iLength];

            int iIndex = iHeaderSize;
            for (int i = 0; i < iHeight; i++)
            {
                for (int j = 0; j < iWidth; j++)
                {
                    byte bItemCount = Data[iIndex++];
                    for (int k = 0; k < bItemCount; k++)
                    {
                        byte bTerrain = Data[iIndex++];
                        if (bTerrain >= iLength)
                        {
                            int[] iNew = new int[bTerrain];
                            Array.Copy(iUsage, iNew, iLength);
                            iUsage = iNew;
                        }
                        iUsage[bTerrain]++;
                    }
                }
            }
            return iUsage;
        }

        public int RemoveTerrainPaint(byte bTerrainToRemove)
        {
            int iReduceSize = 0;
            bool bChanged = false;
            byte[] DataNew = new byte[Data.Length];
            Array.Copy(Data, DataNew, iHeaderSize);
            int iOldIndex = iHeaderSize;
            int iNewIndex = iHeaderSize;
            for (int i = 0; i < iHeight; i++)
            {
                for (int j = 0; j < iWidth; j++)
                {
                    int iCountIndex = iNewIndex;
                    byte bItemCount = DataNew[iNewIndex++] = Data[iOldIndex++];
                    for (int k = 0; k < bItemCount; k++)
                    {
                        byte bTerrain = Data[iOldIndex++];
                        if (bTerrain == bTerrainToRemove)
                        {
                            DataNew[iCountIndex] -= (byte)1;
                            iReduceSize++;
                            bChanged = true;
                            continue;
                        }
                        if (bTerrain > bTerrainToRemove)
                        {
                            bTerrain--;
                            bChanged = true;
                        }
                        DataNew[iNewIndex++] = bTerrain;
                    }
                }
            }
            if (bChanged)
            {
                // Now that we know how large the array should be, we can create one of the correct size.
                byte[] DataTruncated = new byte[Data.Length - iReduceSize];
                Array.Copy(DataNew, DataTruncated, DataTruncated.Length);
                Data = DataTruncated;
                PFD.SetUserData(Data, true);
            }
            return iReduceSize;
        }
    }
}

