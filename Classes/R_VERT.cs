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
using System.Diagnostics;
using System.IO;
using SimPe.Interfaces.Files;

namespace LotExpander
{
    public class R_VERT
    {
        bool Test_PrintDebugInfo = false;   // Enable (T) or disable (F) printing of debug information

        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iLeadingZeros = 64;
        private int iItemCount = 0;
        private int iHeaderSize = 89;
        private const int iBytesPerObject = 16;

        public R_VERT(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
        {
            PFD = Descriptor;
            IPackedFile PF = LotPackage.Read(PFD);
            Data = PF.UncompressedData;

            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            for (int i = 0; i < iLeadingZeros / 4; i++)
            {
                int iDummy = BR.ReadInt32();
                Debug.Assert(iDummy == 0);
            }

            uint uBlockID = BR.ReadUInt32();
            if (uBlockID != 0xCB4387A1)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid VERT: Block ID");

            int iBlockVersion = BR.ReadInt32();
            Debug.Assert(iBlockVersion == 1);   // ToDo: Determine whether other versions are known and handled correctly

            byte bBlockNameLen = (byte)BR.PeekChar();
            if ((bBlockNameLen & 0x80) != 0)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid VERT: Block Name Length");
            string sBlockName = BR.ReadString();
            if (sBlockName != "cVertexLayer")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid VERT: Block Name");

            iItemCount = BR.ReadInt32();

            Debug.Assert(iHeaderSize == BR.BaseStream.Position);
            iHeaderSize = (int)BR.BaseStream.Position;

            if (Data.Length != ExpectedLength)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid VERT: Data Length");
        }

        private int ExpectedLength
        {
            get
            {
                return iItemCount * iBytesPerObject + iHeaderSize;
            }
        }

        public void Change(int XAddLow, int XAddHigh, int YAddLow, int YAddHigh, int iWidthNew, int iHeightNew,
            int iMinLevel, int iMaxLevel)
        {
            byte[] DataNew = new byte[Data.Length];
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            BinaryWriter BW = new BinaryWriter(new MemoryStream(DataNew));
            BW.Write(BR.ReadBytes(iHeaderSize));
            int iTotalSize = iHeaderSize;

            for (int i = 0; i < iItemCount; i++)
            {
                int iLevel = BR.ReadInt32();
                BW.Write(iLevel);
                Debug.Assert((iMinLevel <= iLevel) && (iLevel <= iMaxLevel));
                if (Test_PrintDebugInfo)
                    Debug.Print("Vert({0}) Level = {1:X8}", i, iLevel);

                float XOld = BR.ReadSingle();
                Debug.Assert(XOld >= 0);
                // Debug.Assert((XOld % 1.0) == 0.0);
                float XNew = XOld + XAddLow;
                // No need to restrict:
                // float XFix = LETools.KeepOnLot(XOld, XAddLow, XAddHigh, iWidthNew, 0);
                float XFix = LETools.KeepOnLot(XOld + XAddLow, 0, 0, iWidthNew, 0);
                if (XNew == XFix)
                {
                    BW.Write(XNew);
                    if (Test_PrintDebugInfo)
                        Debug.Print("Vert({0}, 0) X = {1} -> {2}", i, XOld, XNew);
                }
                else
                    PrimaryForm.ThrowErrorOffLot("VERT", "Vertex");

                float YOld = BR.ReadSingle();
                Debug.Assert(YOld >= 0);
                // Debug.Assert((YOld % 1.0) == 0.0);
                float YNew = YOld + YAddLow;
                // No need to restrict:
                // float YFix = LETools.KeepOnLot(YOld, YAddLow, YAddHigh, iHeightNew, 0);
                float YFix = LETools.KeepOnLot(YOld + YAddLow, 0, 0, iHeightNew, 0);
                if (YNew == YFix)
                {
                    BW.Write(YNew);
                    if (Test_PrintDebugInfo)
                        Debug.Print("Vert({0}, 0) Y = {1} -> {2}", i, YOld, YNew);
                }
                else
                    PrimaryForm.ThrowErrorOffLot("VERT", "Vertex");

                // Likely object instance?
                int Dumm = BR.ReadInt32();
                BW.Write(Dumm);
                if (Test_PrintDebugInfo)
                    Debug.Print("Vert({0}) Dumm2 = {1:X8}", i, Dumm);
            }
            Data = DataNew;
            PFD.SetUserData(Data, true);
        }
    }
}
