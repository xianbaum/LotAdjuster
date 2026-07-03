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
    // Road tile locking
    public class R_3ARY_03
    {
        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private int iWidth = 0;
        private int iHeight = 0;
        private int iDepth = 0;

        private const int iLeadingZeros = 64;
        private int iWidthIndex = 81;
        private int iHeightIndex = 85;
        private int iDepthIndex = 89;
        private int iHeaderSize = 93;
        private int iBytesPerObject = 1;
        private byte bDefaultValue = 0;

        public R_3ARY_03(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
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
                    throw new InvalidDataException("Invalid 3ARY Instance 3: Block ID");

            int iBlockVersion = BR.ReadInt32();
            Debug.Assert(iBlockVersion == 1);   // ToDo: Determine whether other versions are known and handled correctly

            byte bBlockNameLen = (byte)BR.PeekChar();
            if ((bBlockNameLen & 0x80) != 0)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance 3: Block Name Length");
            string sBlockName = BR.ReadString();
            if (sBlockName != "c3DArray")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance 3: Block Name");

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
                    throw new InvalidDataException("Invalid 3ARY Instance 3: Width");
            if ((Height < 1) || (Height > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance 3: Height");
            if (Depth < 1)  // ToDo: What are valid values for Depth?
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid 3ARY Instance 3: Depth");

            Debug.Assert(Data.Length == ExpectedLength);
        }

        private int ExpectedLength
        {
            get
            {
                return iWidth * iHeight * iDepth + iHeaderSize;
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
        }

          public int Height
        {
            get
            {
                return iHeight;
            }
        }

          public int Depth
        {
            get
            {
                return iDepth;
            }
        }

#if ADJUST
        public void Change(int iWidthNew, int iHeightNew)
        {
            // Unlock road tiles by setting to zeros
            byte[] DataNew = new byte[iHeaderSize + iWidthNew * iHeightNew * iDepth];
            Array.Copy(Data, DataNew, iHeaderSize);
            for (int i = iHeaderSize; i < DataNew.Length; i++)
            {
                DataNew[i] = 0;
            }
            Data = DataNew;
            ReplaceWidth(iWidthNew);
            ReplaceHeight(iHeightNew);

            Debug.Assert(Data.Length == ExpectedLength);
        }
#endif

#if ! ADJUST
        public void AddLevel()
        {
            // Unlock road tiles by setting to zeros
            int iCopySize = iWidth * iHeight * iBytesPerObject;
            byte[] DataNew = new byte[Data.Length + iCopySize];
            Array.Copy(Data, DataNew, iHeaderSize);
            for (int i = iHeaderSize; i < DataNew.Length; i++)
            {
                DataNew[i] = bDefaultValue;
            }
            // PFD.SetUserData(DataNew, true);
            Data = DataNew;
            ReplaceDepth(iDepth + 1);

            Debug.Assert(Data.Length == ExpectedLength);
        }
#endif
    }
}
