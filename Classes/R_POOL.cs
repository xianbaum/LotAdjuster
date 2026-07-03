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
    public class R_POOL
    {
        bool Test_PrintDebugInfo = false;   // Enable (T) or disable (F) printing of debug information

        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iLeadingZeros = 64;
        private int iBlockVersion;
        private int iHeaderSize = 72;

        public R_POOL(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
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
            if (uBlockID != 0x0C900FDB)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid POOL: Block ID");

            iBlockVersion = BR.ReadInt32();
            Debug.Assert(iBlockVersion == 1);   // ToDo: Determine whether other versions are known and handled correctly

            Debug.Assert(BR.BaseStream.Position == iHeaderSize);
        }

        public void Change(int XAddLow, int XAddHigh, int YAddLow, int YAddHigh, int iWidthNew, int iHeightNew)
        {
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

                int iUnk1 = BR.ReadInt32();
                BW.Write(iUnk1);
                iStructureSize += 4;

                float XOld = BR.ReadSingle();
                Debug.Assert(XOld >= 0);
                Debug.Assert((XOld % 1.0) == 0.0);
                float XNew = XOld + XAddLow;
                float XFix = LETools.KeepOnLot(XOld, XAddLow, XAddHigh, iWidthNew, 0);

                // Resolve problem where EA does not correctly resize pool surface when pool is resized.
                int WFix = 0;
                if (XNew != XFix)
                {
                    WFix = (int)Math.Abs(XFix - XNew);
                    XNew = XFix;
                }

                if (XNew == XFix)
                {
                    BW.Write(XNew);
                    iStructureSize += 4;
                    if (Test_PrintDebugInfo)
                        Debug.Print("POOL X = {0} -> {1}", XOld, XNew);
                }
                else
                {
                    // ToDo: Handle off-world coordinates by deleting?
                    // Cannot use KeepOnLot logic, since it confuses game
                    PrimaryForm.ThrowErrorOffLot("POOL", "Swimming Pool Surface");
                }

                float YOld = BR.ReadSingle();
                Debug.Assert(YOld >= 0);
                Debug.Assert((YOld % 1.0) == 0.0);
                float YNew = YOld + YAddLow;
                float YFix = LETools.KeepOnLot(YOld, YAddLow, YAddHigh, iHeightNew, 0);

                // Resolve problem where EA does not correctly resize pool surface when pool is resized.
                int HFix = 0;
                if (YNew != YFix)
                {
                    HFix = (int)Math.Abs(YFix - YNew);
                    YNew = YFix;
                }

                if (YNew == YFix)
                {
                    BW.Write(YNew);
                    iStructureSize += 4;
                    if (Test_PrintDebugInfo)
                        Debug.Print("POOL Y = {0} -> {1}", YOld, YNew);
                }
                else
                {
                    // ToDo: Handle off-world coordinates by deleting?
                    // Cannot use KeepOnLot logic, since it confuses game
                    PrimaryForm.ThrowErrorOffLot("POOL", "Swimming Pool Surface");
                }

                int iLevel = BR.ReadInt32();
                BW.Write(iLevel);
                iStructureSize += 4;
                Debug.Assert(0 == iLevel);

                int iWidth = BR.ReadInt32();
                // Resolve problem where EA does not correctly resize pool surface when pool is resized.
                if (WFix != 0)
                    iWidth = (iWidth > WFix) ? iWidth - WFix : 0;
                BW.Write(iWidth);
                iStructureSize += 4;

                int iHeight = BR.ReadInt32();
                // Resolve problem where EA does not correctly resize pool surface when pool is resized.
                if (HFix != 0)
                    iHeight = (iHeight > HFix) ? iHeight - HFix : 0;
                BW.Write(iHeight);
                iStructureSize += 4;

                int iUnk2 = BR.ReadInt32();
                BW.Write(iUnk2);
                iStructureSize += 4;
                Debug.Assert(1 == iUnk2);

                float fElevation = BR.ReadSingle();
                BW.Write(fElevation);
                iStructureSize += 4;
                Debug.Assert(0.25F == fElevation);

                int iUnk3 = BR.ReadInt32();
                BW.Write(iUnk3);
                iStructureSize += 4;
                Debug.Assert(0 == iUnk3);

                if (Test_PrintDebugInfo)
                    Debug.Print("Pool {0}: {1:X8} X={2}->{3} Y={4}->{5} Level={6} Size=({7}, {8}) {9} Elevation={10} {11}",
                        i, iUnk1, XOld, XNew, YOld, YNew, iLevel, iWidth, iHeight, iUnk2, fElevation, iUnk3);

                iTotalSize += iStructureSize;
            }
            Debug.Assert(iTotalSize == Data.Length);
            Data = DataNew;
            PFD.SetUserData(Data, true);
        }
    }
}
