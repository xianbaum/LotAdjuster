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

// ToDo: Add what we know about the record format to www.sims2wiki.info File Formats / Packages / Internal Formats
namespace LotExpander
{
    public class R_TMAP
    {
        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iLeadingZeros = 64;
        private int iWidthIndex = 72;
        private int iWidth = 0;
        private int iHeightIndex = 76;
        private int iHeight = 0;
        private int iNumberOfTextures = 0;
        private int iHeaderSize = 80;
        private string sLotTexture = "";

        public R_TMAP(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
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
            if (uBlockID != 0x4B58975B)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid TMAP: Block ID");

            int iBlockVersion = BR.ReadInt32();
            Debug.Assert(iBlockVersion == 7);   // ToDo: Determine whether other versions are known and handled correctly

            Debug.Assert(iWidthIndex == BR.BaseStream.Position);
            iWidthIndex = (int)BR.BaseStream.Position;
            iWidth = BR.ReadInt32();
            if ((Width < 1) || (Width > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid TMAP: Width");

            Debug.Assert(iHeightIndex == BR.BaseStream.Position);
            iHeightIndex = (int)BR.BaseStream.Position;
            iHeight = BR.ReadInt32();
            if ((Height < 1) || (Height > LETools.MaxLotSize))
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid TMAP: Height");

            Debug.Assert(iHeaderSize == BR.BaseStream.Position);
            iHeaderSize = (int)BR.BaseStream.Position;

            sLotTexture = BR.ReadString();

            iNumberOfTextures = BR.ReadInt32();
            Debug.Assert(iNumberOfTextures <= 0xFF);

            string[] sTextures = new string[iNumberOfTextures];
            for (int i = 0; i < iNumberOfTextures; i++)
            {
                sTextures[i] = BR.ReadString();
            }
            Debug.Assert(Data.Length == BR.BaseStream.Position);

            /* Can we solve this problem by zeroing out the texture name for 0x5CEE? No.
            int iMaxTextures = 0x5CEE - 0x5CBC;
            if (iNumberOfTextures > iMaxTextures)
            {
                int iBadTexture = iMaxTextures + 1;
                byte[] DataNew = new byte[Data.Length - sTextures[iBadTexture].Length];
                Array.Copy(Data, DataNew, iHeaderSize);
                BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                BW.BaseStream.Seek(iHeaderSize, SeekOrigin.Begin);
                BW.Write(sLotTexture);
                BW.Write(iNumberOfTextures);
                string s = "";
                for (int i = 0; i < iNumberOfTextures; i++)
                {
                    if (i == iBadTexture)
                        BW.Write(s);
                    else
                        BW.Write(sTextures[i]);
                }
                PFD.SetUserData(DataNew, true);
            }
             */
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
                return iWidth;
            }
            set
            {
                if (this.Width != value)
                {
                    ReplaceWidth(value);
                }
            }
        }

        public int Height
        {
            get
            {
                return iHeight;
            }
            set
            {
                if (this.Height != value)
                {
                    ReplaceHeight(value);
                }
            }
        }

        public string LotTexture
        {
            get
            {
                return sLotTexture;
            }
            set
            {
                if (0 != string.Compare(value, sLotTexture, true))
                {
                    byte[] DataNew = new byte[Data.Length + value.Length - sLotTexture.Length];
                    BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
                    BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);

                    BW.Write(BR.ReadBytes(iHeaderSize));

                    string s = BR.ReadString();
                    Debug.Assert(0 == string.Compare(s, sLotTexture, true));
                    BW.Write(value);

                    BW.Write(BR.ReadBytes(Data.Length - (int)BR.BaseStream.Position));

                    Data = DataNew;
                    PFD.SetUserData(Data, true);
                }
            }
        }

        public int NumberOfTextures
        {
            get
            {
                return iNumberOfTextures;
            }
        }

        public string[] GetTerrainNames()
        {
            string[] sTextures = new string[0];

            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            BR.BaseStream.Seek(iHeaderSize, SeekOrigin.Begin);

            string sLotTexture = BR.ReadString();

            iNumberOfTextures = BR.ReadInt32();
            Debug.Assert(iNumberOfTextures <= 0xFF);

            sTextures = new string[iNumberOfTextures];
            for (int i = 0; i < iNumberOfTextures; i++)
            {
                sTextures[i] = BR.ReadString();
            }
            return sTextures;
        }

        public int RemoveTerrainPaint(uint uTerrainToRemove)
        {
            int iFound = 0;

            byte[] DataNew = new byte[Data.Length];
            BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);

            BW.Write(BR.ReadBytes(iHeaderSize));

            string sLotTexture = BR.ReadString();
            BW.Write(sLotTexture);

            long iCountIndex = BW.BaseStream.Position;
            int iTextureCount = BR.ReadInt32();
            BW.Write(iTextureCount);
            Debug.Assert(iNumberOfTextures == iTextureCount);

            for (uint i = 0; i < iTextureCount; i++)
            {
                string sTexture = BR.ReadString();
                if (i == uTerrainToRemove)
                    iFound++;
                else
                    BW.Write(sTexture);
            }
            long iSize = BW.BaseStream.Position;

            if (iFound > 0)
            {
                iTextureCount -= iFound;
                BW.BaseStream.Seek(iCountIndex,SeekOrigin.Begin);
                BW.Write(iTextureCount);

                // Now that we know how large the array should be, we can create one of the correct size.
                byte[] DataTruncated = new byte[iSize];
                Array.Copy(DataNew, DataTruncated, DataTruncated.Length);
                Data = DataTruncated;
                PFD.SetUserData(Data, true);

                // Reset the main count
                iNumberOfTextures = iTextureCount;
            }
            return iFound;
        }
    }
}

