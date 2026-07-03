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
    public class R_WLL
    {
        bool Test_PrintDebugInfo = false;   // Enable (T) or disable (F) printing of debug information

        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iLeadingZeros = 64;
        private int iBlockVersion;
        private int iHeaderSize = 83;
        private int iCount;

        public R_WLL(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
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
            if (uBlockID != 0x8A84D7B0)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid WLL: Block ID");

            iBlockVersion = BR.ReadInt32();
            Debug.Assert(iBlockVersion == 1);   // ToDo: Determine whether other versions are known and handled correctly

            byte bBlockNameLen = (byte)BR.PeekChar();
            if ((bBlockNameLen & 0x80) != 0)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid WLL: Block Name Length");
            string sBlockName = BR.ReadString();
            if (sBlockName != "cWallLayer")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid WLL: Block Name");

            Debug.Assert(BR.BaseStream.Position == iHeaderSize);

            iCount = BR.ReadInt32();
        }

        public int WallCount
        {
            get
            {
                return iCount;
            }
        }

        private bool KnownStyle(uint uWallNumber)
        {
            if ((uWallNumber == 1) // normal wall
             || (uWallNumber == 2) // picket rail fence
             || (uWallNumber == 3) // attic wall
             || (uWallNumber == 4) // non-rendered deck skirt
             || (uWallNumber == 16) // deck skirt (redwood)
             || (uWallNumber == 23) // foundation wall (brick)
             || (uWallNumber == 24) // deck skirt (minimal)
             || (uWallNumber == 26) // deck aged wood fence arch
             || (uWallNumber == 29) // pool wall
             || (uWallNumber == 300) // normal wall (OFB or later)
             || (uWallNumber == 301) // screen wood (OFB or later)
            )
                return true;
            return false;
        }

        // Find any unknown styles and add them to the list.
        public void AddWallNumbers(ref uint[] uaWallNumbers)
        {
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            BR.ReadBytes(iHeaderSize);
            int iItemCount = BR.ReadInt32();

            for (int i = 0; i < iItemCount; i++)
            {
                // Matches Wall ID in WGRA List of Walls
                uint uWallID = BR.ReadUInt32();

                // Matches either Wall Number in Walls.txt
                // or GUID (no idea what the GUID means at this time)
                uint uWallNumber = BR.ReadUInt32();
                if (!KnownStyle(uWallNumber))
                {
                    int j = 0;
                    for (; j < uaWallNumbers.Length; j++)
                    {
                        if (uaWallNumbers[j] == uWallNumber)
                            break;
                    }
                    if (j == uaWallNumbers.Length)
                    {
                        Array.Resize<uint>(ref uaWallNumbers, uaWallNumbers.Length + 1);
                        uaWallNumbers[j] = uWallNumber;
                        // Debug.Print("New WLL({0}): ID={1:X8} Wall={2:X8}", i, uWallID, uWallNumber);
                    }
                }

                short iPatternID1 = BR.ReadInt16(); // Wall pattern on one side of the wall; corresponds to SMAP
                short iPatternID2 = BR.ReadInt16(); // Wall pattern on the other side of the wall; corresponds to SMAP

                if (Test_PrintDebugInfo)
                    Debug.Print("WLL({0}): ID={1:X8} Wall={2:X8} {3} {4}", i, uWallID, uWallNumber, iPatternID1, iPatternID2);
            }
        }

        // Never used
        public void Change(int XAddLow, int XAddHigh, int YAddLow, int YAddHigh, int iWidthNew, int iHeightNew)
        {
            // Just to be on the safe side:
            Debug.Fail("You have reached unreachable code!");
            return;
/*
            // byte[] DataNew = new byte[Data.Length];
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            // BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
            // BW.Write(BR.ReadBytes(iHeaderSize));
            BR.ReadBytes(iHeaderSize);
            int iTotalSize = iHeaderSize;

            int iItemCount = BR.ReadInt32();
            // BW.Write(iItemCount);
            iTotalSize += 4;

            for (int i = 0; i < iItemCount; i++)
            {
                int iStructureSize = 0;

                uint iUnk1 = BR.ReadUInt32();
                // BW.Write(iUnk1);
                iStructureSize += 4;

                uint iUnk2 = BR.ReadUInt32();
                // BW.Write(iUnk2);
                iStructureSize += 4;

                Int16 iUnk3 = BR.ReadInt16();
                // BW.Write(iUnk3);
                iStructureSize += 2;

                Int16 iUnk4 = BR.ReadInt16();
                // BW.Write(iUnk4);
                iStructureSize += 2;

                if (Test_PrintDebugInfo)
                    Debug.Print("{0}: {1:X8} {2:X8} {3} {4}", i, iUnk1, iUnk2, iUnk3, iUnk4);

                iTotalSize += iStructureSize;
            }
            Debug.Assert(iTotalSize == Data.Length);
            // Data = DataNew;
            // PFD.SetUserData(Data, true);
 */
        }

        // Unused: ChangeWallID pre-code
        public void Change(R_WGRA ResWGRA)
        {
            Debug.Fail("You have reached unreachable code!");
            return;
/*
            byte[] DataNew = new byte[Data.Length];
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
            BW.Write(BR.ReadBytes(iHeaderSize));
            int iTotalSize = iHeaderSize;

            int iItemCount = BR.ReadInt32();
            BW.Write(iItemCount);
            iTotalSize += 4;

            for (int i = 0; i < iItemCount; i++)
            {
                int iStructureSize = 0;

                // Matches Wall ID in WGRA List of Walls
                uint uWallID = BR.ReadUInt32();
                iStructureSize += 4;

                // Matches Wall Number in Walls.txt
                int iWallNumber = BR.ReadInt32();
                if (
                    false                 // Turn off modification of wall number
//                    (iWallNumber == 1)    //   1: TS2 normal wall
//                 || (iWallNumber == 3)    //   3: TS2 attic wall
//                 || (iWallNumber == 4)    //   4: TS2 non-rendered deck skirt
//                 || (iWallNumber == 16)   //  16: TS2 deck skirt (redwood)
//                 || (iWallNumber == 23)   //  23: TS2 foundation wall (brick)
//                 || (iWallNumber == 24)   //  24: TS2 deck skirt (minimal)
//                 || (iWallNumber == 29)   //  29: TS2 pool wall
//                 || (iWallNumber == 40)   // My transparent walls
                )
                {
                    int iLevel = ResWGRA.WallLevel(uWallID);
                    if (iLevel > 0)
                    {
                        if (ResWGRA.WallInRange(uWallID, 0, 21, 0, 21))
                        {
                            // iWallNumber = 1;     // Works:   foundation walls  -> normal walls
                            // Crashes! redwood deck      -> foundation walls
                            // iWallNumber = 3;     // Crashes! normal walls      -> attic walls; requires roof?
                            // iWallNumber = 4;     // Works:   normal walls      -> non-rendered deck skirt
                            // iWallNumber = 16;    // Crashes! normal walls      -> redwood walls
                            // iWallNumber = 23;    // Works:   normal walls      -> foundation walls
                            // Crashes! redwood deck      -> foundation walls
                            // iWallNumber = 24;    // Crashes! non-rendered deck -> deck skirt (minimal)
                            iWallNumber = 40;    // My transparent walls
                        }
                    }
                }
                iStructureSize += 4;

                short iPatternID1 = BR.ReadInt16(); // Wall pattern on one side of the wall; corresponds to SMAP
                iStructureSize += 2;

                short iPatternID2 = BR.ReadInt16(); // Wall pattern on the other side of the wall; corresponds to SMAP
                iStructureSize += 2;

                if (Test_PrintDebugInfo)
                    Debug.Print("WLL({0}): ID={1:X8} Wall={2:X8} {3} {4}", i, uWallID, iWallNumber, iPatternID1, iPatternID2);

                BW.Write(uWallID);
                BW.Write(iWallNumber);
                BW.Write(iPatternID1);
                BW.Write(iPatternID2);

                iTotalSize += iStructureSize;
            }
            Debug.Assert(iTotalSize == Data.Length);
            Data = DataNew;
            PFD.SetUserData(Data, true);
 */
        }

        // ChangeWallID: change wall style from uFromWall to uToWall within range
#if CONVERT
        public int Change(R_WGRA ResWGRA, uint uFromWall, uint uToWall,
            int iFromLevel, int iToLevel, float fFromX, float fToX, float fFromY, float fToY)
        {
            int iChangedCount = 0;

            byte[] DataNew = new byte[Data.Length];
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
            BW.Write(BR.ReadBytes(iHeaderSize));
            int iTotalSize = iHeaderSize;

            int iItemCount = BR.ReadInt32();
            BW.Write(iItemCount);
            iTotalSize += 4;

            int iKnown = 0;
            int iUnknown = 0;
            for (int i = 0; i < iItemCount; i++)
            {
                int iStructureSize = 0;

                // Matches Wall ID in WGRA List of Walls
                uint uWallID = BR.ReadUInt32();
                iStructureSize += 4;

                // Matches either Wall Number in Walls.txt
                // or GUID (no idea what the GUID means at this time)
                uint uWallNumber = BR.ReadUInt32();
                if (KnownStyle(uWallNumber))
                    iKnown++;
                else
                {
                    iUnknown++;
                    if (Test_PrintDebugInfo)
                        Debug.Print("WLL({0}): ID={1:X8} Wall={2:X8}", i, uWallID, uWallNumber);
                }

                if (uWallNumber == uFromWall)
                {
                    int iLevel = ResWGRA.WallLevel(uWallID);
                    if ((iLevel >= iFromLevel) && (iLevel <= iToLevel))
                    {
                        if (ResWGRA.WallInRange(uWallID, fFromX, fToX, fFromY, fToY))
                        {
                            uWallNumber = uToWall;
                            iChangedCount++;
                        }
                    }
                }
                iStructureSize += 4;

                short iPatternID1 = BR.ReadInt16(); // Wall pattern on one side of the wall; corresponds to SMAP
                iStructureSize += 2;

                short iPatternID2 = BR.ReadInt16(); // Wall pattern on the other side of the wall; corresponds to SMAP
                iStructureSize += 2;

                if (Test_PrintDebugInfo)
                    Debug.Print("WLL({0}): ID={1:X8} Wall={2:X8} {3} {4}", i, uWallID, uWallNumber, iPatternID1, iPatternID2);

                BW.Write(uWallID);
                BW.Write(uWallNumber);
                BW.Write(iPatternID1);
                BW.Write(iPatternID2);

                iTotalSize += iStructureSize;
            }
            Debug.Assert(iTotalSize == Data.Length);
            Debug.Print("Walls: known={0} unknown={1}", iKnown, iUnknown);
            if (iChangedCount > 0)
            {
                Data = DataNew;
                PFD.SetUserData(Data, true);
            }
            return iChangedCount;
        }
#endif
    }
}
