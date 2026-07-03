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
    public class R_WRLD
    {
        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iLeadingZeros = 64;
        private const int iTrailingZeros = 2;
        private int iMinLevel = 0;
        private int iMaxLevel = 0;
        private int iWidthIndex = 93;
        private int iWidth = 0;
        private int iHeightIndex = 97;
        private int iHeight = 0;
        private const int iConvertTilesToVertices = 1;  // Add: tiles -> vertices; Subtract: vertices -> tiles

        public R_WRLD(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
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
            if (uBlockID != 0x49FF7D76)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid WRLD: Block ID");

            int iBlockVersion = BR.ReadInt32();
            Debug.Assert((iBlockVersion == 3)
                      || (iBlockVersion == 4)
                      || (iBlockVersion == 5)
                      || (iBlockVersion == 7)   // Apartment Life
            );   // ToDo: Determine whether other versions are known and handled correctly

            byte bBlockNameLen = (byte)BR.PeekChar();
            if ((bBlockNameLen & 0x80) != 0)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid WRLD: Block Name Length");
            string sBlockName = BR.ReadString();
            if (sBlockName != "cWorldDB")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid WRLD: Block Name");
            
            for (int i = 0; i < iTrailingZeros; i++)
            {
                uint uDummy = BR.ReadUInt32();
                Debug.Assert(uDummy == 0);
            }

            // ToDo: Is this the minimum level?
            iMinLevel = BR.ReadInt32();
            Debug.Assert((0 == iMinLevel) || (-1 == iMinLevel));

            Debug.Assert(iWidthIndex == BR.BaseStream.Position);
            iWidthIndex = (int)BR.BaseStream.Position;
            iWidth = BR.ReadInt32();
            if ((Width < 1) || (Width > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid WRLD: Width");

            Debug.Assert(iHeightIndex == BR.BaseStream.Position);
            iHeightIndex = (int)BR.BaseStream.Position;
            iHeight = BR.ReadInt32();
            if ((Height < 1) || (Height > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid WRLD: Height");

            iMaxLevel = BR.ReadInt32();
            Debug.Assert((iMaxLevel >= 0) || (iMaxLevel <= 5));

            if (iBlockVersion >= 4)
            {
                int iCount = BR.ReadInt32();
                for (int i = 0; i < iCount; i++)
                {
                    // ToDo: Handle strings > 128 chars and multi-byte chars.
                    string s = BR.ReadString();
                }
            }
            Debug.Assert(BR.BaseStream.Position == Data.Length);
        }

        public int MinimumLevel
        {
            get
            {
                return iMinLevel;
            }
        }

        public int MaximumLevel
        {
            get
            {
                return iMaxLevel;
            }
        }

        private void ReplaceInt(int i, int iIndex)
        {
            byte[] BA = new byte[4];
            BinaryWriter BW = new BinaryWriter(new MemoryStream(BA));
            BW.Write(i);
            Array.Copy(BA, 0, Data, iIndex, 4);
            PFD.SetUserData(Data, true);
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
                return (iWidth + iConvertTilesToVertices);
            }
            set
            {
                if (this.Width != value)
                {
                    ReplaceWidth(value - iConvertTilesToVertices);
                }
            }
        }

        public int Height
        {
            get
            {
                return (iHeight + iConvertTilesToVertices);
            }
            set
            {
                if (this.Height != value)
                {
                    ReplaceHeight(value - iConvertTilesToVertices);
                }
            }
        }

    }
}

