/***************************************************************************
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
    public class R_TXTR
    {
        private bool Test_PrintDebugInfo = false;
        private IPackedFileDescriptor PFD;
        private string sFileName = null;

        public R_TXTR(IPackageFile LotPackage, IPackedFileDescriptor Descriptor)
        {
            PFD = Descriptor;
            IPackedFile PF = LotPackage.Read(PFD);
            BinaryReader BR = SimPe.Helper.GetBinaryReader(PF.UncompressedData);

            /*
            byte bDummy = BR.ReadByte();
            Debug.Assert(bDummy == 0x01);
            bDummy = BR.ReadByte();
            Debug.Assert(bDummy == 0x00);
            bDummy = BR.ReadByte();
            Debug.Assert(bDummy == 0xFF);
            bDummy = BR.ReadByte();
            Debug.Assert(bDummy == 0xFF);
             */
            uint uDummy = BR.ReadUInt32();
            Debug.Assert(0xFFFF0001 == uDummy);

            uDummy = BR.ReadUInt32();
            Debug.Assert(uDummy == 0);
            uDummy = BR.ReadUInt32();
            Debug.Assert(uDummy == 1);

            uint uBlockID = BR.ReadUInt32();
            if (uBlockID != 0x1C4A276C)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid TXTR: Block ID");

            string sBlockName = BR.ReadString();
            if (sBlockName != "cImageData")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid TXTR: ImageData Block Name");

            uint uID = BR.ReadUInt32();
            if (uID != 0x1C4A276C)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid TXTR: ID");

            int iBlockVersion = BR.ReadInt32();
            Debug.Assert(iBlockVersion == 9);   // ToDo: Determine whether other versions are known and handled correctly

            string cSGResource = BR.ReadString();
            if (cSGResource != "cSGResource")
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid TXTR: SGResource Name");

            uDummy = BR.ReadUInt32();
            Debug.Assert(uDummy == 0);
            uDummy = BR.ReadUInt32();
            Debug.Assert(uDummy == 2);

            sFileName = BR.ReadString();

            int iWidth = BR.ReadInt32();
            int iHeight = BR.ReadInt32();
            int iFormatCode = BR.ReadInt32();
            Debug.Assert(iFormatCode == 1); // Raw

            uint uMipMapLevels = BR.ReadUInt32();
            float fPurpose = BR.ReadSingle();
            Debug.Assert(fPurpose == 1);    // Object
        }

        public bool Deletable(uint uInst)
        {
            bool bDeletable = false;

            switch (uInst)
            {
                case 0xFFE7F9AD:    // roofs_txtr
                case 0xFF8AFE8D:    // terrain_txtr
                case 0xFF885F70:    // terrainLmap_txtr
                    {
                        // TXTR Instances which MUST be deleted for LotExpander to work
                        // ToDo: Ensure that we need to delete all of these.
                        if (Test_PrintDebugInfo)
                        {
                            string sInstStr = sFileName.Substring(sFileName.IndexOf('!')+1);
                            Debug.Print("Delete: TXTR {0:X8} = {1}", uInst, sInstStr);
                        }
                        bDeletable = true;
                        break;
                    }

#if DEBUG
                case 0xFF2112A3:    // slices_0_txtr
                case 0xFF17F99E:    // slices_1_txtr
                case 0xFF6E8813:    // walls_0_txtr
                case 0xFF58632E:    // walls_1_txtr
                case 0xFF035E69:    // walls_2_txtr
                case 0xFF35B554:    // walls_3_txtr
                case 0xFFB524E7:    // walls_4_txtr
                case 0xFF83CFDA:    // walls_5_txtr
                case 0xFFD8F29D:    // walls_6_txtr
                case 0xFFEE19A0:    // walls_7_txtr
                case 0xFF5F9D00:    // walls_8_txtr
                case 0xFF69763D:    // walls_9_txtr
                case 0xFF608617:    // walls_10_txtr
                case 0xFF566D2A:    // walls_11_txtr
                case 0xFF0D506D:    // walls_12_txtr
                case 0xFF3BBB50:    // walls_13_txtr
                case 0xFFBB2AE3:    // walls_14_txtr
                case 0xFF8DC1DE:    // walls_15_txtr
                case 0xFFD6FC99:    // walls_16_txtr
                case 0xFFE017A4:    // walls_17_txtr
                case 0xFF519304:    // walls_18_txtr
                case 0xFF677839:    // walls_19_txtr
                case 0xFF2C3483:    // walls_20_txtr
                case 0xFF1ADFBE:    // walls_21_txtr
                case 0xFF41E2F9:    // walls_22_txtr
                case 0xFF7709C4:    // walls_23_txtr
                case 0xFFF79877:    // walls_24_txtr
                case 0xFFC1734A:    // walls_25_txtr
                case 0xFF9A4E0D:    // walls_26_txtr
                case 0xFFACA530:    // walls_27_txtr
                case 0xFF1D2190:    // walls_28_txtr
                case 0xFF2BCAAD:    // walls_29_txtr
                case 0xFF17A50F:    // walls_30_txtr
                case 0xFF214E32:    // walls_31_txtr
                case 0xFF7A7375:    // walls_32_txtr
                case 0xFF4C9848:    // walls_33_txtr
                case 0xFFCC09FB:    // walls_34_txtr
                case 0xFFFAE2C6:    // walls_35_txtr
                case 0xFFA1DF81:    // walls_36_txtr
                case 0xFF9734BC:    // walls_37_txtr
                case 0xFF26B01C:    // walls_38_txtr
                case 0xFF105B21:    // walls_39_txtr
                case 0xFFB551AB:    // walls_40_txtr
                case 0xFF83BA96:    // walls_41_txtr
                    {
                        // TXTR Instances which MAY be deleted without adversely affecting the lot
                        bDeletable = false;
                        break;
                    }

                default:
                    {
                        // TXTR Instances which CANNOT be deleted without adversely affecting the lot
                        if ((sFileName.Contains("!painting_"))          // CANNOT be deleted
                         || (sFileName.Contains("!custom_painting_")))
                        {
                            Debug.Fail("Found one!");
                        }
                        else if (sFileName.Contains("!slices_"))        // MAY be deleted
                        {
                            string s = String.Format("Unknown TXTR Slice Instance {0:X8} = {1}", uInst, sFileName);
                            Debug.Fail(s);                              // Lot122 White Water Mansion
                        }
                        else if (sFileName.Contains("!walls_"))         // MAY be deleted
                        {
                            string s = String.Format("Unknown TXTR Wall Instance {0:X8} = {1}", uInst, sFileName);
                            Debug.Fail(s);                              // Lot122 White Water Mansion
                        }
                        else                                            // Research required
                        {
                            string s = String.Format("Unknown TXTR Instance {0:X8} = {1}", uInst, sFileName);
                            Debug.Fail(s);
                        }
                        bDeletable = false;
                        break;
                    }
#endif
            }
            return bDeletable;
        }
    }
}
