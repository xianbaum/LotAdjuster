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
// using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SimPe.Interfaces.Files;


// 3D Array Instance 0xC holds OBJT instance numbers
namespace LotExpander
{
    public class R_3ARY_0C
    {
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
        private const int iConvertToQuarterTile = 4; // Arrays are for quarter tiles, not full tiles.
        private int iBytesPerObject = 4;
        private byte bDefaultValue = 0;

        // private IDictionary<uint, int> InstToLevel = new Dictionary<uint, int>();

        public R_3ARY_0C(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
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
            if(uBlockID != 0x2A51171B)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance C: Block ID");

            int iBlockVersion = BR.ReadInt32();
            Debug.Assert(iBlockVersion == 1);   // ToDo: Determine whether other versions are known and handled correctly

            byte bBlockNameLen = (byte)BR.PeekChar();
            if((bBlockNameLen & 0x80) != 0)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance C: Block Name Length");
            string sBlockName = BR.ReadString();
            if(sBlockName != "c3DArray")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance C: Block Name");

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
                    throw new InvalidDataException("Invalid 3ARY Instance C: Width");
            if ((Height < 1) || (Height > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance C: Height");
            if (Depth < 1)  // ToDo: What are valid values for Depth?
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance C: Depth");

            // Setup table of OBJT instance to level number
            // ToDo: subtract minlevel?
            for (int i = 0; i < iDepth; i++)
            {
                for (int j = 0; j < iWidth; j++)
                {
                    for (int k = 0; k < iHeight; k++)
                    {
                        uint uInst = BR.ReadUInt32();
                        if (0 == uInst)
                            continue;
                        /*
                        try
                        {
                            int iLevel = InstToLevel[uInst];
                            // if (i != iLevel)
                            //     Debug.Fail("Found Instance with two different levels");
                        }
                        catch (KeyNotFoundException)
                        {
                            InstToLevel.Add(uInst, i);
                        }
                         */
                    }
                }
            }

            // ToDo: check length... difficult because variable object size
        }

        private void ReplaceInt(int i, int iIndex)
        {
            byte[] BA = new byte[4];
            BinaryWriter BW = new BinaryWriter(new MemoryStream(BA));
            BW.Write(i);
            Array.Copy(BA, 0, Data, iIndex, 4);
            PFD.SetUserData(Data, true);

            // ToDo: check length... difficult because variable object size
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
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight * iDepth * iConvertToQuarterTile];
                    BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                    BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
                    BW.Write(BR.ReadBytes(iHeaderSize));
                    //Am Ende einfügen
                    int iItemCount = 0;
                    int iData = bDefaultValue;
                    for (int i = 0; i < iDepth; i++)
                    {
                        for (int j = 0; j < iWidth * iHeight; j++)
                        {
                            BW.Write(iItemCount = BR.ReadInt32());
                            if (iItemCount > 0)
                            {
                                BW.Write(BR.ReadBytes(iItemCount * iBytesPerObject));
                            }
                        }
                        iData = bDefaultValue;
                        for (int j = 0; j < iWidthDiff * iHeight; j++)
                        {
                            BW.Write(iData);
                        }
                    }
                    Data = DataNew;
                    ReplaceWidth(value);
                }
                else if (iWidthDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight * iDepth * iConvertToQuarterTile];
                    BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                    BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
                    BW.Write(BR.ReadBytes(iHeaderSize));
                    int iItemCount = 0;
                    for (int i = 0; i < iDepth; i++)
                    {
                        for (int j = 0; j < (iWidth + iWidthDiff) * iHeight; j++)
                        {
                            BW.Write(iItemCount = BR.ReadInt32());
                            if (iItemCount > 0)
                            {
                                BW.Write(BR.ReadBytes(iItemCount * iBytesPerObject));
                            }
                        }
                        for (int j = 0; j < (0 - iWidthDiff) * iHeight; j++)
                        {
                            iItemCount = BR.ReadInt32();
                            if (iItemCount > 0)
                            {
                                BR.ReadBytes(iItemCount * iBytesPerObject);
                            }
                        }
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
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight * iDepth * iConvertToQuarterTile];
                    BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                    BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
                    BW.Write(BR.ReadBytes(iHeaderSize));
                    //Am Anfang einfügen
                    int iItemCount = 0;
                    int iData = bDefaultValue;
                    for (int i = 0; i < iDepth; i++)
                    {
                        iData = bDefaultValue;
                        for (int j = 0; j < iWidthDiff * iHeight; j++)
                        {
                            BW.Write(iData);
                        }
                        for (int j = 0; j < iWidth * iHeight; j++)
                        {
                            BW.Write(iItemCount = BR.ReadInt32());
                            if (iItemCount > 0)
                            {
                                BW.Write(BR.ReadBytes(iItemCount * iBytesPerObject));
                            }
                        }
                    }
                    Data = DataNew;
                    ReplaceWidth(value);
                }
                else if (iWidthDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidthDiff * iHeight * iDepth * iConvertToQuarterTile];
                    BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                    BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
                    BW.Write(BR.ReadBytes(iHeaderSize));
                    int iItemCount = 0;
                    for (int i = 0; i < iDepth; i++)
                    {
                        for (int j = 0; j < (0 - iWidthDiff) * iHeight; j++)
                        {
                            iItemCount = BR.ReadInt32();
                            if (iItemCount > 0)
                            {
                                BR.ReadBytes(iItemCount * iBytesPerObject);
                            }
                        }
                        for (int j = 0; j < (iWidth + iWidthDiff) * iHeight; j++)
                        {
                            BW.Write(iItemCount = BR.ReadInt32());
                            if (iItemCount > 0)
                            {
                                BW.Write(BR.ReadBytes(iItemCount * iBytesPerObject));
                            }
                        }
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
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff * iDepth * iConvertToQuarterTile];
                    BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                    BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
                    BW.Write(BR.ReadBytes(iHeaderSize));
                    //Spaltenweise am Ende einfügen
                    int iItemCount = 0;
                    int iData = bDefaultValue;
                    for (int i = 0; i < iDepth; i++)
                    {
                        for (int j = 0; j < iWidth; j++)
                        {
                            for (int k = 0; k < iHeight; k++)
                            {
                                BW.Write(iItemCount = BR.ReadInt32());
                                if (iItemCount > 0)
                                {
                                    BW.Write(BR.ReadBytes(iItemCount * iBytesPerObject));
                                }
                            }
                            iData = bDefaultValue;
                            for (int k = 0; k < iHeightDiff ; k++)
                            {
                                BW.Write(iData);
                            }
                        }
                    }
                    Data = DataNew;
                    ReplaceHeight(value);
                }
                else if (iHeightDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff * iDepth * iConvertToQuarterTile];
                    BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                    BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
                    BW.Write(BR.ReadBytes(iHeaderSize));
                    int iItemCount = 0;
                    for (int i = 0; i < iDepth; i++)
                    {
                        for (int j = 0; j < iWidth; j++)
                        {
                            for (int k = 0; k < (iHeight + iHeightDiff); k++)
                            {
                                BW.Write(iItemCount = BR.ReadInt32());
                                if (iItemCount > 0)
                                {
                                    BW.Write(BR.ReadBytes(iItemCount * iBytesPerObject));
                                }
                            }
                            for (int k = 0; k < (0 - iHeightDiff); k++)
                            {
                                iItemCount = BR.ReadInt32();
                                if (iItemCount > 0)
                                {
                                    BR.ReadBytes(iItemCount * iBytesPerObject);
                                }
                            }
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
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff * iDepth * iConvertToQuarterTile];
                    BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                    BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
                    BW.Write(BR.ReadBytes(iHeaderSize));
                    //Spaltenweise am Anfang einfügen
                    int iItemCount = 0;
                    int iData = bDefaultValue;
                    for (int i = 0; i < iDepth; i++)
                    {
                        for (int j = 0; j < iWidth; j++)
                        {
                            iData = bDefaultValue;
                            for (int k = 0; k < iHeightDiff; k++)
                            {
                                BW.Write(iData);
                            }
                            for (int k = 0; k < iHeight; k++)
                            {
                                BW.Write(iItemCount = BR.ReadInt32());
                                if (iItemCount > 0)
                                {
                                    BW.Write(BR.ReadBytes(iItemCount * iBytesPerObject));
                                }
                            }
                        }
                    }
                    Data = DataNew;
                    ReplaceHeight(value);
                }
                else if (iHeightDiff < 0)
                {
                    byte[] DataNew = new byte[Data.Length + iWidth * iHeightDiff * iDepth * iConvertToQuarterTile];
                    BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                    BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
                    BW.Write(BR.ReadBytes(iHeaderSize));
                    int iItemCount = 0;
                    for (int i = 0; i < iDepth; i++)
                    {
                        for (int j = 0; j < iWidth; j++)
                        {
                            for (int k = 0; k < (0 - iHeightDiff); k++)
                            {
                                iItemCount = BR.ReadInt32();
                                if (iItemCount > 0)
                                {
                                    BR.ReadBytes(iItemCount * iBytesPerObject);
                                }
                            }
                            for (int k = 0; k < (iHeight + iHeightDiff); k++)
                            {
                                BW.Write(iItemCount = BR.ReadInt32());
                                if (iItemCount > 0)
                                {
                                    BW.Write(BR.ReadBytes(iItemCount * iBytesPerObject));
                                }
                            }
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

        /*
        public int Level(uint uInst)
        {
            Debug.Assert(0 != uInst);
            return InstToLevel[uInst];
        }
         */

#if ! ADJUST
        public void AddLevel(int iNewLevel, int iMinLevel)
        {
            int iLevel = iNewLevel - iMinLevel;
            if (iLevel > iDepth)
                iLevel = iDepth;

            int iCopySize = iWidth * iHeight * iBytesPerObject;
            byte[] DataNew = new byte[Data.Length + iCopySize];
            Array.Copy(Data, DataNew, iHeaderSize);
            int iOldIndex = iHeaderSize;
            int iNewIndex = iHeaderSize;

            for (int i = 0; i < iLevel; i++)
            {
                Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                iOldIndex += iCopySize;
                iNewIndex += iCopySize;
            }
            for (int j = 0; j < iCopySize; j++)
            {
                // ToDo: This is actually the instance number of the OBJT (uint)
                //       Logic works because of 0 default value.
                Debug.Assert(0 == bDefaultValue);
                DataNew[iNewIndex++] = bDefaultValue;
            }
            for (int i = iLevel; i < iDepth; i++)
            {
                Array.Copy(Data, iOldIndex, DataNew, iNewIndex, iCopySize);
                iOldIndex += iCopySize;
                iNewIndex += iCopySize;
            }
            // PFD.SetUserData(DataNew, true);
            Data = DataNew;
            ReplaceDepth(iDepth + 1);
        }
#endif
    }
}

