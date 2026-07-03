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
    public class R_NHTG
    {
        private bool Test_PrintDebugInfo = false;   // Enable (T) or disable (F) printing of debug information

        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iMaxGrid = 128;
        private int iHeaderSize = 61;
        private float fWaterTableHeight = 0;

        public R_NHTG(IPackageFile NBPack, IPackedFileDescriptor Descriptor)
        {
            PFD = Descriptor;
            IPackedFile PF = NBPack.Read(PFD);
            Data = PF.UncompressedData;

            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            uint uBlockID = BR.ReadUInt32();
            if (uBlockID != 0xABCB5DA4)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid NHTG: Block ID");

            uint uBlockVersion = BR.ReadUInt32();
            Debug.Assert(uBlockVersion == 4);   // ToDo: Determine whether other versions are known and handled correctly

            // Width and Height may be swapped,
            // but it doesn't matter, since they are constant and equal
            int iGridWidth = BR.ReadInt32();
            Debug.Assert(iGridWidth == iMaxGrid);

            int iGridHeight = BR.ReadInt32();
            Debug.Assert(iGridHeight == iMaxGrid);

            fWaterTableHeight = BR.ReadSingle();
            Debug.Assert(fWaterTableHeight == 312.5);

            int iStrLen = BR.ReadInt32();
            byte[] bString = BR.ReadBytes(iStrLen);
            string sTerrainType = SimPe.Helper.ToString(bString);
            Debug.Assert((sTerrainType == "Concrete")
                      || (sTerrainType == "Desert")
                      || (sTerrainType == "Dirt")
                      || (sTerrainType == "Temperate")
                      );

            uBlockID = BR.ReadUInt32();
            if (uBlockID != 0x6B943B43)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid NHTG: 2DArray Block ID");

            uBlockVersion = BR.ReadUInt32();
            Debug.Assert(uBlockVersion == 1);   // ToDo: Determine whether other versions are known and handled correctly

            iStrLen = BR.ReadInt32();
            bString = BR.ReadBytes(iStrLen);
            string sSectionType = SimPe.Helper.ToString(bString);
            if (sSectionType != "c2DArray")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid NHTG: 2DArray Block Name");

            // Width and Height may be swapped,
            // but it doesn't matter, since they are constant and equal
            int iWidth = BR.ReadInt32();
            Debug.Assert(iWidth == iMaxGrid + 1);

            int iHeight = BR.ReadInt32();
            Debug.Assert(iHeight == iMaxGrid + 1);

            iHeaderSize = (int)BR.BaseStream.Position;   // Size varies, depending upon terrain type
        }

        public float WaterTable
        {
            get
            {
                return fWaterTableHeight;
            }
        }

        // ToDo: Change GetHoodTerrain & ReplaceHoodTerrain functions to HoodTerrain property (get & set)?
        public float[,] GetHoodTerrain(int iLeft, int iTop, int iWidth, int iHeight)
        {
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            BR.ReadBytes(iHeaderSize);

            // Get entire neighborhood terrain
            float[,] fTerrain = new float[iMaxGrid + 1, iMaxGrid + 1];
            for (int i = 0; i <= iMaxGrid; i++)        // Width
            {
                for (int j = 0; j <= iMaxGrid; j++)    // Height
                {
                    fTerrain[i, j] = BR.ReadSingle();
                }
            }

            // Select the portion of the terrain used by the lot
            float[,] fHoodTerrain = new float[iWidth + 1, iHeight + 1];
            for (int iFromX = iLeft, iToX = 0; iToX <= iWidth; iFromX++, iToX++)
            {
                for (int iFromY = iTop, iToY = 0; iToY <= iHeight; iFromY++, iToY++)
                {
                    fHoodTerrain[iToX, iToY] = fTerrain[iFromX, iFromY];
                }
            }
            return fHoodTerrain;
        }

        public void ReplaceHoodTerrain(float[,] fHoodTerrain, int iLeft, int iTop, int iWidth, int iHeight)
        {
            byte[] DataNew = new byte[Data.Length];
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
            BW.Write(BR.ReadBytes(iHeaderSize));

            // Get entire neighborhood terrain
            float[,] fTerrain = new float[iMaxGrid + 1, iMaxGrid + 1];
            for (int i = 0; i <= iMaxGrid; i++)        // Width
            {
                for (int j = 0; j <= iMaxGrid; j++)    // Height
                {
                    fTerrain[i, j] = BR.ReadSingle();
                }
            }

            // Replace the portion of the terrain used by the lot
            for (int iToX = iLeft, iFromX = 0; iFromX <= iWidth; iToX++, iFromX++)
            {
                for (int iToY = iTop, iFromY = 0; iFromY <= iHeight; iToY++, iFromY++)
                {
                    Debug.Print("Change Hood({0}, {1}) = {2} -> {3}",
                        iFromX, iFromY, fTerrain[iToX, iToY], fHoodTerrain[iFromX, iFromY]);
                    fTerrain[iToX, iToY] = fHoodTerrain[iFromX, iFromY];
                }
            }

            // Write out entire fixed neighborhoods terrain
            for (int i = 0; i <= iMaxGrid; i++)        // Width
            {
                for (int j = 0; j <= iMaxGrid; j++)    // Height
                {
                    BW.Write(fTerrain[i, j]);
                }
            }

            /*
            // Water Array?  Try changing to an arbitrary value and see what happens to lots.
            uint uBlockID = BR.ReadUInt32();
            if (uBlockID != 0x6B943B43)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid NHTG: 2DArray Block ID");
            BW.Write(uBlockID);

            uint uBlockVersion = BR.ReadUInt32();
            Debug.Assert(uBlockVersion == 1);   // ToDo: Determine whether other versions are known and handled correctly
            BW.Write(uBlockVersion);

            int iStrLen = BR.ReadInt32();
            BW.Write(iStrLen);

            byte[] bString = BR.ReadBytes(iStrLen);
            string sSectionType = SimPe.Helper.ToString(bString);
            if (sSectionType != "c2DArray")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid NHTG: 2DArray Block Name");
            BW.Write(bString);

            // Width and Height may be swapped,
            // but it doesn't matter, since they are constant and equal
            int iW = BR.ReadInt32();
            Debug.Assert(iW == iMaxGrid + 1);
            BW.Write(iW);

            int iH = BR.ReadInt32();
            Debug.Assert(iH == iMaxGrid + 1);
            BW.Write(iH);

            for (int i = 0; i <= iMaxGrid; i++)        // Width
            {
                for (int j = 0; j <= iMaxGrid; j++)    // Height
                {
                    byte b = BR.ReadByte();
                    Debug.Assert(0 == b);
                    BW.Write((byte)i);
                }
            }
             */

            Data = DataNew;
            PFD.SetUserData(Data, true);
        }
    }
}