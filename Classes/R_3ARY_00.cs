/***************************************************************************
 *   Copyright (C) 2008-2013 by Mootilda                                   *
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
    // Floor and road surface tiles
    public class R_3ARY_00
    {
        private bool Test_PrintDebugInfo = false;   // Enable (T) or disable (F) printing of debug information

        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iLeadingZeros = 64;
        private int iWidthIndex = 81;
        private int iWidth = 0;
        private int iHeightIndex = 85;
        private int iHeight = 0;
        private int iDepthIndex = 89;
        private int iDepth = 0;
        private int iHeaderSize = 93;
        // private const int iLotTilesPerNeighborhoodTile = 10;
        private const int iSectionsPerTile = 4;
        private int iBytesPerObject = 8;
        private byte bDefaultValue = 0;

        public R_3ARY_00(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
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
            if (uBlockID != 0x2A51171B)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance 0: Block ID");

            int iBlockVersion = BR.ReadInt32();
            Debug.Assert(iBlockVersion == 1);   // ToDo: Determine whether other versions are known and handled correctly

            byte bBlockNameLen = (byte)BR.PeekChar();
            if ((bBlockNameLen & 0x80) != 0)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance 0: Block Name Length");
            string sBlockName = BR.ReadString();
            if (sBlockName != "c3DArray")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance 0: Block Name");

            Debug.Assert(iWidthIndex == BR.BaseStream.Position);
            iWidthIndex = (int)BR.BaseStream.Position;
            iWidth = BR.ReadInt32();

            Debug.Assert(iHeightIndex == BR.BaseStream.Position);
            iHeightIndex = (int)BR.BaseStream.Position;
            iHeight = BR.ReadInt32();

            Debug.Assert(iDepthIndex == BR.BaseStream.Position);
            iDepthIndex = (int)BR.BaseStream.Position;
            iDepth = BR.ReadInt32();

            Debug.Assert(iHeaderSize == BR.BaseStream.Position);
            iHeaderSize = (int)BR.BaseStream.Position;

            if ((Width < 1) || (Width > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance 0: Width");
            if ((Height < 1) || (Height > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance 0: Height");
            if (Depth < 1)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance 0: Depth");
 
            if (Data.Length != ExpectedLength)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance 0: Data Length");
#if DEBUG
            for (int i = 0; i < iDepth; i++)
            {
                for (int j = 0; j < iWidth; j++)
                {
                    for (int k = 0; k < iHeight; k++)
                    {
                        for (int l = 0; l < iSectionsPerTile; l++)
                        {
                            ushort u = BR.ReadUInt16();
                            if (0 != (u & 0XFF00))
                                Debug.Fail("Found a floor tile > 256!");
                        }
                    }
                }
            }
#endif
        }

        private int ExpectedLength
        {
            get
            {
                return iWidth * iHeight * iDepth * iBytesPerObject + iHeaderSize;
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

#if ADJUST
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
#endif

#if ! ADJUST
        private void ReplaceDepth(int i)
        {
            iDepth = i;
            ReplaceInt(iDepth, iDepthIndex);
        }
#endif

        // First, add width to end of array:
        //
        // |-------------------|
        // | Original  | Width |
        // |-------------------|
        //
        //
        // Second, add height to end of array:
        // 
        // |-------------------|
        // | Original  | Width |
        // |-------------------|
        // |      Height       |
        // |-------------------|
        //
        //
        // Third, add width to start of array:
        //
        // |---------------------------|
        // | Width | Original  | Width |
        // |  Rev  |-------------------|
        // |       |      Height       |
        // |---------------------------|
        //
        // Finally, add height to start of array:
        // 
        // |---------------------------|
        // |         HeightRev         |
        // |---------------------------|
        // | Width | Original  | Width |
        // |  Rev  |-------------------|
        // |       |      Height       |
        // |---------------------------|

        public int Width
        {
            get
            {
                return iWidth;
            }
#if ADJUST
            set
            {
                int iWidthDiff = (value - this.Width);
                if (iWidthDiff > 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight * iDepth * iBytesPerObject];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    int iCopySize = iWidth * iHeight * iBytesPerObject;
                    for (int i = 0; i < iDepth; i++)
                    {
                        Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                        iOldIndex += iCopySize;
                        iNewIndex += iCopySize;
                        for (int j = 0; j < iWidthDiff * iHeight * iBytesPerObject; j++)
                        {
                            DataNew[iNewIndex++] = bDefaultValue;
                        }
                    }
                    Data = DataNew;
                    ReplaceWidth(value);
                }
                else if (iWidthDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight * iDepth * iBytesPerObject];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    int iCopySize = (iWidth + iWidthDiff) * iHeight * iBytesPerObject;
                    for (int i = 0; i < iDepth; i++)
                    {
                        Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                        iOldIndex += iCopySize + (0 - iWidthDiff) * iHeight * iBytesPerObject;
                        iNewIndex += iCopySize;
                    }
                    Data = DataNew;
                    ReplaceWidth(value);
                }
            }
#endif
        }

#if ADJUST
        public int WidthRev
        {
            set
            {
                int iWidthDiff = (value - this.Width);
                if (iWidthDiff > 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight * iDepth * iBytesPerObject];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    int iCopySize = iWidth * iHeight * iBytesPerObject;
                    for (int i = 0; i < iDepth; i++)
                    {
                        for (int j = 0; j < iWidthDiff * iHeight * iBytesPerObject; j++)
                        {
                            DataNew[iNewIndex++] = bDefaultValue;
                        }
                        Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                        iOldIndex += iCopySize;
                        iNewIndex += iCopySize;
                    }
                    Data = DataNew;
                    ReplaceWidth(value);
                }
                else if (iWidthDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight * iDepth * iBytesPerObject];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    int iCopySize = (iWidth + iWidthDiff) * iHeight * iBytesPerObject;
                    for (int i = 0; i < iDepth; i++)
                    {
                        iOldIndex += (0 - iWidthDiff) * iHeight * iBytesPerObject;
                        Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                        iOldIndex += iCopySize;
                        iNewIndex += iCopySize;
                    }
                    Data = DataNew;
                    ReplaceWidth(value);
                }
            }
        }
#endif

        public int Height
        {
            get
            {
                return iHeight;
            }
#if ADJUST
            set
            {
                int iHeightDiff = (value - this.Height);
                if (iHeightDiff > 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff * iDepth * iBytesPerObject];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    int iCopySize = iHeight * iBytesPerObject;
                    for (int i = 0; i < iDepth; i++)
                    {
                        for (int j = 0; j < iWidth; j++)
                        {
                            Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                            iOldIndex += iCopySize;
                            iNewIndex += iCopySize;
                            for (int k = 0; k < iHeightDiff * iBytesPerObject; k++)
                            {
                                DataNew[iNewIndex++] = bDefaultValue;
                            }
                        }
                    }
                    Data = DataNew;
                    ReplaceHeight(value);
                }
                else if (iHeightDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff * iDepth * iBytesPerObject];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    int iCopySize = (iHeight + iHeightDiff) * iBytesPerObject;
                    for (int i = 0; i < iDepth; i++)
                    {
                        for (int j = 0; j < iWidth; j++)
                        {
                            Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                            iOldIndex += iCopySize + (0 - iHeightDiff) * iBytesPerObject;
                            iNewIndex += iCopySize;
                        }
                    }
                    Data = DataNew;
                    ReplaceHeight(value);
                }
            }
#endif
        }

#if ADJUST
        public int HeightRev
        {
            set
            {
                int iHeightDiff = (value - this.Height);
                if (iHeightDiff > 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff * iDepth * iBytesPerObject];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    int iCopySize = iHeight * iBytesPerObject;
                    for (int i = 0; i < iDepth; i++)
                    {
                        for (int j = 0; j < iWidth; j++)
                        {
                            for (int k = 0; k < iHeightDiff * iBytesPerObject; k++)
                            {
                                DataNew[iNewIndex++] = bDefaultValue;
                            }
                            Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                            iOldIndex += iCopySize;
                            iNewIndex += iCopySize;
                        }
                    }
                    Data = DataNew;
                    ReplaceHeight(value);
                }
                else if (iHeightDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff * iDepth * iBytesPerObject];
                    Array.Copy(Data, DataNew, iHeaderSize);
                    int iOldIndex = iHeaderSize;
                    int iNewIndex = iHeaderSize;
                    int iCopySize = (iHeight + iHeightDiff) * iBytesPerObject;
                    for (int i = 0; i < iDepth; i++)
                    {
                        for (int j = 0; j < iWidth; j++)
                        {
                            iOldIndex += (0 - iHeightDiff) * iBytesPerObject;
                            Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                            iOldIndex += iCopySize;
                            iNewIndex += iCopySize;
                        }
                    }
                    Data = DataNew;
                    ReplaceHeight(value);
                }
            }
        }
#endif

        public int Depth
        {
            get
            {
                return iDepth;
            }
        }

#if ADJUST
        private ushort[, , ,] TileArray = null;

        public void BeginUpdate()
        {
            Debug.Assert(null == TileArray);

            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            BR.ReadBytes(iHeaderSize);

            // Read everything in
            TileArray = new ushort[iDepth, iWidth, iHeight, iSectionsPerTile];
            if (Test_PrintDebugInfo)
                Debug.Print("3ARY Instance 0 before Adjustments:");
            for (int i = 0; i < iDepth; i++)
            {
                if (Test_PrintDebugInfo)
                    Debug.Print("Depth = {0}:", i);
                for (int j = 0; j < iWidth; j++)
                {
                    string s = string.Format("{0,2}:", j);
                    for (int k = 0; k < iHeight; k++)
                    {
                        for (int l = 0; l < iSectionsPerTile; l++)
                        {
                            TileArray[i, j, k, l] = BR.ReadUInt16();
                        }
                        s = string.Format("{0}  {1:X4} {2:X4} {3:X4} {4:X4}", s,
                            TileArray[i, j, k, 0], TileArray[i, j, k, 1], TileArray[i, j, k, 2], TileArray[i, j, k, 3]);
                    }
                    if (Test_PrintDebugInfo)
                        Debug.Print(s);
                }
            }
        }

        public void EndUpdate(bool bWrite)
        {
            if (null == TileArray)
                return;

            if (bWrite)
            {
                byte[] DataNew = new byte[Data.Length];
                Array.Copy(Data, DataNew, Data.Length);
                BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
                BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                BW.Write(BR.ReadBytes(iHeaderSize));

                // Write everything out
                if (Test_PrintDebugInfo)
                    Debug.Print("3ARY Instance 0 after Adjustments:");
                for (int i = 0; i < iDepth; i++)
                {
                    if (Test_PrintDebugInfo)
                        Debug.Print("Depth = {0}:", i);
                    for (int j = 0; j < iWidth; j++)
                    {
                        string s = string.Format("{0,2}:", j);
                        for (int k = 0; k < iHeight; k++)
                        {
                            for (int l = 0; l < iSectionsPerTile; l++)
                            {
                                ushort u = BR.ReadUInt16();
                                BW.Write(TileArray[i, j, k, l]);
                                s = string.Format("{0} {1:X16}->{2:X16}", s, u, TileArray[i, j, k, l]);
                            }
                        }
                        if (Test_PrintDebugInfo)
                            Debug.Print(s);
                    }
                }
                Data = DataNew;
                PFD.SetUserData(Data, true);
            }
            TileArray = null;
        }

        public ushort[,] GetRoad(int iGroundLevel, int iWidthLow, int iWidthHigh, int iHeightLow, int iHeightHigh)
        {
            if (null == TileArray)
            {
                Debug.Fail("BeginUpdate never called");
                BeginUpdate();
            }

            ushort[,] tiles = new ushort[iWidthHigh - iWidthLow, iHeightHigh - iHeightLow];

            int i = iGroundLevel;   // Process depth = ground
            for (int iFromX = iWidthLow, iToX = 0; iFromX < iWidthHigh; iFromX++, iToX++)
            {
                for (int iFromY = iHeightLow, iToY = 0; iFromY < iHeightHigh; iFromY++, iToY++)
                {
                    for (int l = 0; l < iSectionsPerTile; l++)
                    {
                        Debug.Assert(TileArray[i, iFromX, iFromY, 0] == TileArray[i, iFromX, iFromY, l]);
                        tiles[iToX, iToY] = TileArray[i, iFromX, iFromY, l];
                    }
                }
            }
            return tiles;
        }

        // Note: there must be enough tiles to cover the entire area
        public void PutRoad(int iGroundLevel, ushort[] tiles,
            int iWidthLow, int iWidthHigh, int iHeightLow, int iHeightHigh)
        {
            if (null == TileArray)
            {
                Debug.Fail("BeginUpdate never called");
                BeginUpdate();
            }

            int i = iGroundLevel;   // Process depth = ground
            for (int iFromX = 0, iToX = iWidthLow; iToX < iWidthHigh; iFromX++, iToX++)
            {
                for (int iFromY = 0, iToY = iHeightLow; iToY < iHeightHigh; iFromY++, iToY++)
                {
                    for (int l = 0; l < iSectionsPerTile; l++)
                    {
                        TileArray[i, iToX, iToY, l] = tiles[iFromX + iFromY];
                    }
                }
            }
        }

        public void ClearRoad(int iGroundLevel, int iWidthLow, int iWidthHigh, int iHeightLow, int iHeightHigh)
        {
            if (null == TileArray)
            {
                Debug.Fail("BeginUpdate never called");
                BeginUpdate();
            }

            int i = iGroundLevel;   // Process depth = ground
            for (int j = iWidthLow; j < iWidthHigh; j++)
            {
                for (int k = iHeightLow; k < iHeightHigh; k++)
                {
                    for (int l = 0; l < iSectionsPerTile; l++)
                    {
                        TileArray[i, j, k, l] = 0;
                    }
                }
            }
        }

        public void PlaceRoad(int iGroundLevel, byte bRoad, int iRotation,
            int iWidthLow, int iWidthHigh, int iHeightLow, int iHeightHigh)
        {
            Debug.Fail("R_3ARY_00::PlaceRoad should never be called; not yet implemented!");

            if (null == TileArray)
            {
                Debug.Fail("BeginUpdate never called");
                BeginUpdate();
            }

            int i = iGroundLevel;   // Process depth = ground
            for (int j = iWidthLow; j < iWidthHigh; j++)
            {
                for (int k = iHeightLow; k < iHeightHigh; k++)
                {
                    ushort t = 0;
                    // bRoad is an individual byte from the U10 (roads) field, log 2
                    /*
                    if (0 == bRoad)
                        t = RoadTiles[iRotation][j];
                    else if (1 == bRoad)
                        t = RoadTiles[iRotation][iHeightHigh - k - 1];
                    else if (2 == bRoad)
                        t = RoadTiles[iRotation][iWidthHigh - j - 1];
                    else if (3 == bRoad)
                        t = RoadTiles[iRotation][k];
                    for (int l = 0; l < iSectionsPerTile; l++)
                    {
                        // Debug.Assert(TileArray[i, j, k, l] == t);
                        TileArray[i, j, k, l] = t;
                    }
                     */
                }
            }
        }

        public void PlaceCorner(int iGroundLevel, byte bRoad, int iWidthLow, int iWidthHigh, int iHeightLow, int iHeightHigh)
        {
            Debug.Fail("R_3ARY_00::PlaceCorner should never be called; not yet implemented!");

            if (null == TileArray)
            {
                Debug.Fail("BeginUpdate never called");
                BeginUpdate();
            }

            int i = iGroundLevel;   // Process depth = ground
            for (int j = iWidthLow; j < iWidthHigh; j++)
            {
                for (int k = iHeightLow; k < iHeightHigh; k++)
                {
                    ushort t = 0;
                    // bRoad is an individual byte from the U10 (roads) field, log 2
                    /*
                    if (0 == bRoad)
                        t = CornerTiles[j];
                    else if (1 == bRoad)
                        t = CornerTiles[iHeightHigh - k - 1];
                    else if (2 == bRoad)
                        t = CornerTiles[iWidthHigh - j - 1];
                    else if (3 == bRoad)
                        t = CornerTiles[k];
                    for (int l = 0; l < iSectionsPerTile; l++)
                    {
                        TileArray[i, j, k, l] = t;
                    }
                     */
                }
            }
        }
#endif

#if ! ADJUST
        public void AddLevel(int iNewLevel, int iMinLevel, byte U10)
        {
            int iLevel = iNewLevel - iMinLevel;
            if (iLevel > iDepth)
                iLevel = iDepth;

            int iCopySize = iWidth * iHeight * iBytesPerObject;
            byte[] DataNew = new byte[Data.Length + iCopySize];
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
            BW.Write(BR.ReadBytes(iHeaderSize));

            byte[] bLevelData = new byte[iCopySize];
            for (int i = 0; i < iDepth; i++)
            {
                // Pre-read existing level
                bLevelData = BR.ReadBytes(iCopySize);

                // If this is the new level
                if (i == iLevel)
                {
                    // Add new level
                    for (int j = 0; j < iWidth; j++)
                    {
                        for (int k = 0; k < iHeight; k++)
                        {
                            for (int l = 0; l < iBytesPerObject; l++)
                            {
                                // ToDo: This is actually 4 shorts, rather than 8 bytes
                                //       Logic works because of 0 default value.
                                Debug.Assert(0 == bDefaultValue);
                                byte b = bDefaultValue;

                                // If adding ground level
                                if (0 == iNewLevel)
                                {
                                    // Add road here, then clear road from pre-read data
                                    if (((0x01 == (U10 & 0x01)) && (j < iLotTilesPerNeighborhoodTile))
                                     || ((0x02 == (U10 & 0x02)) && (k > (iHeight - 1 - iLotTilesPerNeighborhoodTile)))
                                     || ((0x04 == (U10 & 0x04)) && (j > (iWidth - 1 - iLotTilesPerNeighborhoodTile)))
                                     || ((0x08 == (U10 & 0x08)) && (k < iLotTilesPerNeighborhoodTile)))
                                    {
                                        int iIndex = (j * iHeight + k) * iBytesPerObject + l;
                                        b = bLevelData[iIndex];
                                        bLevelData[iIndex] = bDefaultValue;
                                    }
                                }
                                BW.Write(b);
                            }
                        }
                    }
                }
                // Write existing level, without road if no longer ground level
                BW.Write(bLevelData);
            }

            // If the new level is above all existing levels, write it now.
            if (iLevel == iDepth)
            {
                for (int j = 0; j < iCopySize; j++)
                {
                    BW.Write(bDefaultValue);
                }
            }

            // PFD.SetUserData(DataNew, true);
            Data = DataNew;
            ReplaceDepth(iDepth + 1);

            Debug.Assert(Data.Length == ExpectedLength);
        }
#endif
    }
}

