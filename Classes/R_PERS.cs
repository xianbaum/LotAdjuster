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
    public class R_PERS
    {
        bool Test_PrintDebugInfo = false;   // Enable (T) or disable (F) printing of debug information

        private IPackedFileDescriptor PFD;
        private byte[] Data;
        private const int iLeadingZeros = 80;
        private int iHeightIndex = 80;
        private float fHeight = 0;
        private int iWidthIndex = 84;
        private float fWidth = 0;
        private int iHeaderSize = 88;

        public R_PERS(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
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
            Debug.Assert(iHeightIndex == BR.BaseStream.Position);
            iHeightIndex = (int)BR.BaseStream.Position;
            fHeight = BR.ReadSingle();

            Debug.Assert(iWidthIndex == BR.BaseStream.Position);
            iWidthIndex = (int)BR.BaseStream.Position;
            fWidth = BR.ReadSingle();

            Debug.Assert(iHeaderSize == BR.BaseStream.Position);
            iHeaderSize = (int)BR.BaseStream.Position;
        }

        private void ReplaceFloat(float f, int iIndex)
        {
            byte[] BA = new byte[4];
            BinaryWriter BW = new BinaryWriter(new MemoryStream(BA));
            BW.Write(f);
            Array.Copy(BA, 0, Data, iIndex, 4);
            PFD.SetUserData(Data, true);
        }

        private void ReplaceWidth(float f)
        {
            fWidth = f;
            ReplaceFloat(fWidth, iWidthIndex);
        }

        private void ReplaceHeight(float f)
        {
            fHeight = f;
            ReplaceFloat(fHeight, iHeightIndex);
        }

        public void PrintDebugInfo(bool b)
        {
            Test_PrintDebugInfo = b;
        }

        // Unused
        private void SetXY(float XNew, float YNew)
        {
            float XOld = fWidth;
            float YOld = fHeight;
            ReplaceWidth(XNew);
            ReplaceHeight(YNew);
            if (Test_PrintDebugInfo)
                if ((XOld == XNew) && (YOld == YNew))
                    Debug.Print("    PERS X = {0} Y = {1}", XOld, YOld);
                else
                    Debug.Print("    PERS X = {0} -> {1} Y = {2} -> {3}", XOld, XNew, YOld, YNew);
        }

        public void Change(int XAddLow, int XAddHigh, int YAddLow, int YAddHigh, int iWidthNew, int iHeightNew)
        {
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);
            byte[] bDummy = BR.ReadBytes(iLeadingZeros);

            float YOld = BR.ReadSingle();
            float YNew = YOld;
            if (YOld > 0)
            {
                YNew = LETools.KeepOnLot(YOld, YAddLow, YAddHigh, iHeightNew, 0.5F);
                ReplaceHeight(YNew);
            }

            float XOld = BR.ReadSingle();
            float XNew = XOld;
            if (XOld > 0)
            {
                XNew = LETools.KeepOnLot(XOld, XAddLow, XAddHigh, iWidthNew, 0.5F);
                ReplaceWidth(XNew);
            }
            if (Test_PrintDebugInfo)
                if ((XOld == XNew) && (YOld == YNew))
                    Debug.Print("    PERS X = {0} Y = {1}", XOld, YOld);
                else
                    Debug.Print("    PERS X = {0} -> {1} Y = {2} -> {3}", XOld, XNew, YOld, YNew);
        }
    }
}
