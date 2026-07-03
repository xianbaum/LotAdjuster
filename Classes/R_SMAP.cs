/***************************************************************************
 *   Copyright (C) 2010-2013 by Mootilda                                   *
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using SimPe.Interfaces.Files;

namespace LotExpander
{
    public class R_SMAP
    {
        private bool Test_PrintDebugInfo = false;   // Enable (T) or disable (F) printing of debug information

        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iLeadingZeros = 64;
        private int iHeaderSize = 83;
        private uint iCount = 0;
        private uint iCountNew = 0;

        // Instance 0x0D - Wall covering
        // Instance 0x0E - Floor tiles

        private IDictionary<string, ushort> StringToRef = new Dictionary<string, ushort>();

        public R_SMAP(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
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
            if (uBlockID != 0xCAC4FC40)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid SMAP: Block ID");

            int iBlockVersion = BR.ReadInt32();
            Debug.Assert(iBlockVersion == 1);   // ToDo: Determine whether other versions are known and handled correctly

            byte bBlockNameLen = (byte)BR.PeekChar();
            if ((bBlockNameLen & 0x80) != 0)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid SMAP: Block Name Length");
            string sBlockName = BR.ReadString();
            if (sBlockName != "cStringMap")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid SMAP: Block Name");
            Debug.Assert(iHeaderSize == BR.BaseStream.Position);
            iHeaderSize = (int)BR.BaseStream.Position;

            iCount = BR.ReadUInt32();
            for (int i = 0; i < iCount; ++i)
            {
                string sDesc = BR.ReadString(); // string description
                ushort uRef = BR.ReadUInt16();  // reference number
                uint uCount = BR.ReadUInt32();  // count of quarter tiles on lot
                if (Test_PrintDebugInfo)
                {
                    int iRoad = sDesc.IndexOf("road_");
                    if (-1 < iRoad)
                        Debug.Print("SMAP[{0}] = {1} unk = {2}", uRef, sDesc, uCount);
                    else if (0 == string.Compare(sDesc, "sidewalk"))
                        Debug.Print("SMAP[{0}] = {1} unk = {2}", uRef, sDesc, uCount);
                }
                if (StringToRef.Contains(new KeyValuePair<string, ushort>(sDesc, uRef)))
                    throw new InvalidDataException("Invalid SMAP: Duplicate Reference");
                StringToRef.Add(sDesc, uRef);
            }
        }

        public ushort FindString(string sDesc)
        {
            ushort uRef = 0;

            // First, check whether the string is already in the dictionary
            ICollection<string> sDescriptions = StringToRef.Keys;
            if (sDescriptions.Contains(sDesc))
            {
                uRef = StringToRef[sDesc];
                return uRef;
            }

            // If the string needs to be added, then find an unused reference number
            ICollection<ushort> sReferences = StringToRef.Values;
            for (uRef = 1; ; uRef++)
            {
                if (!sReferences.Contains(uRef))
                    break;
            }

            // We've found an unused reference number, so add to dictionary
            StringToRef.Add(sDesc, uRef);
            iCountNew++;

            // Update the SMAP record
            int iEntrySize = sizeof(byte) + sDesc.Length + sizeof(ushort) + sizeof(uint);
            byte[] DataNew = new byte[Data.Length + iEntrySize];
            BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);

            // Copy up to the entry count
            BW.Write(BR.ReadBytes(iHeaderSize));

            // Read old number of entries, replace with new number of entries
            iCount = BR.ReadUInt32();
            iCount++;
            BW.Write(iCount);

            // Copy the rest of the record
            BW.Write(BR.ReadBytes(Data.Length - iHeaderSize - sizeof(uint)));

            // Now, add the new entry
            BW.Write(sDesc);    // string description
            BW.Write(uRef);     // reference number
            uint uCount = 1;    // hopefully, the count doesn't actually matter, especially for road tiles.
            BW.Write(uCount);   // count of quarter tiles on lot

            Data = DataNew;
            PFD.SetUserData(Data, true);

            return uRef;
        }
    }
}
