/***************************************************************************
 *   Copyright (C) 2011-2013 by Mootilda                                   *
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
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using SimPe.Interfaces.Files;
using SimPe.Packages;

namespace LotExpander
{
    public class R_NGBH
    {
        private bool Test_PrintDebugInfo = false;   // Enable (T) or disable (F) printing of debug information

        private const uint MemoryBlockVersion_BaseGame = 0x6F;
        private const uint MemoryBlockVersion_University = 0x70;
        private const uint MemoryBlockVersion_Nightlife = 0xBE;
        private const uint MemoryBlockVersion_OpenForBusiness = 0xC2;
        private const uint MemoryBlockVersion_Seasons = 0xCB;

        private const int iMaxGrid = 128;

        private byte[] Data;
        private IPackedFileDescriptor PFD;

        string sTerrainType = "temperate";

        public R_NGBH(IPackageFile NBPackage, IPackedFileDescriptor LotDescriptor)
        {
            PFD = LotDescriptor;
            IPackedFile PF = NBPackage.Read(PFD);

            Data = PF.UncompressedData;
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);

            uint uBlockID = BR.ReadUInt32();
            Debug.Assert(uBlockID == 0x4E474248);

            uint uBlockVersion = BR.ReadUInt32();
            Debug.Assert(
                (uBlockVersion == MemoryBlockVersion_BaseGame)
             || (uBlockVersion == MemoryBlockVersion_University)
             || (uBlockVersion == MemoryBlockVersion_Nightlife)
             || (uBlockVersion == MemoryBlockVersion_OpenForBusiness)
             || (uBlockVersion == MemoryBlockVersion_Seasons)
            );
            // ToDo: Determine whether other versions are known and handled correctly

            // Unused
            uint uDummy = BR.ReadUInt32();

            // Height
            uDummy = BR.ReadUInt32();
            Debug.Assert(iMaxGrid == uDummy);

            // Width
            uDummy = BR.ReadUInt32();
            Debug.Assert(iMaxGrid == uDummy);

            // Terrain Type
            int iStrLen = BR.ReadInt32();
            byte[] bString = BR.ReadBytes(iStrLen);
            sTerrainType = SimPe.Helper.ToString(bString);
            Debug.Assert(("concrete" == sTerrainType.ToLower())
                      || ("desert" == sTerrainType.ToLower())
                      || ("dirt" == sTerrainType.ToLower())
                      || ("temperate" == sTerrainType.ToLower()));

            bString = BR.ReadBytes(28);
        }

        public string TerrainType
        {
            get
            {
                return sTerrainType;
            }
        }
    }
}


