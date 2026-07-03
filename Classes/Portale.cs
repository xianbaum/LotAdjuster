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
    public class Portale
    {
        private bool Test_PrintDebugInfo = false;   // Enable (T) or disable (F) printing of debug information
        private bool Test_AssertDirection = false;  // Enable (T) or disable (F) assert on change of direction
        private const int iLotTilesPerNeighborhoodTile = 10;
        private IPackageFile Package;
        private byte[] Data;

        struct PortalInfo
        {
            public uint   uPortalType;      // Portal Type (DWORD)
            public string sPortalString;    // Portal Type (String)
            public ushort uReference;       // Reference number linking MOBJ metadata and XOBJ & OBJT Instances
            public bool   bTreatAsObject;   // Treat as object, rather than as portal
            public uint[] uInstances;       // Portal Instances
        };
        private const int iMaxPortals = 9;  // We currently only handle 5 Portal Types + Mailbox / Phone booth / Trash
        private PortalInfo[] PortalArray = new PortalInfo[iMaxPortals];

        private const uint PortalPedestrian      = 0x81E6BEF9;
        private const uint PortalCarStart        = 0xCC203477;
        private const uint PortalCarStop         = 0x865A6812;
        private const uint PortalCarServiceStart = 0xCC7B08F8;
        private const uint PortalCarServiceStop  = 0x6C7B0905;

        // Although these are not strictly portals, they are located relative to the road like portals
        private const uint ResidentialMailbox    = 0x39CCF441;
        private const uint ResidentialTrashCan   = 0xD20C0AEE;
        private const uint CommunityPhoneBooth   = 0xECA6E041;
        private const uint CommunityTrashCan     = 0X8CB284A3;

        // Do nothing with beach portals
        private const uint BeachPortal           = 0x132F810B;


        public Portale(IPackageFile LotPackage, IPackedFileDescriptor MOBJTDescriptor)
        {
            Package = LotPackage;
            IPackedFile PF = Package.Read(MOBJTDescriptor);
            Data = PF.UncompressedData;
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);

            // ToDo: Determine format of 0x6F626A74 record: MOBJT - Main Lot Object
            int iDumm;
            short sDumm;
            const int iLeadingZeros = 64;
            for (int d = 0; d < iLeadingZeros / 4; d++)
            {
                iDumm = BR.ReadInt32();
                Debug.Assert(iDumm == 0);
            }
            uint uBlockID = BR.ReadUInt32();
            if (uBlockID != 0x6F626A74)
                if (LETools.ErrorChecking)
                    throw new InvalidDataException("Invalid MOBJT: Block ID");

            int iBlockVersion = BR.ReadInt32();
            Debug.Assert((iBlockVersion == 78)
                      || (iBlockVersion == 79)
                      || (iBlockVersion == 81)
                      || (iBlockVersion == 84)
                      || (iBlockVersion == 85)
                        );   // ToDo: Determine whether other versions are known and handled correctly

            iDumm = BR.ReadInt32();
            Debug.Assert(iDumm == 0);

            int z = 0;
            uint GUID = BR.ReadUInt32();
            if (Test_PrintDebugInfo)
                Debug.Print("Find all Portal Types and Names:");
            while (GUID != 0)
            {
                sDumm = BR.ReadInt16();
                sDumm = BR.ReadInt16();
                sDumm = BR.ReadInt16();
                sDumm = BR.ReadInt16();
                uint uPrivateAttributes = BR.ReadUInt32();
                uint uNumObjectArrays = BR.ReadUInt32();
                uint uSemiGlobalAttributes = BR.ReadUInt32();
                ushort Reference = BR.ReadUInt16();
                ushort uType = BR.ReadUInt16();
                string sObjectName = BR.ReadString();
                iDumm = BR.ReadInt32();
                uint ID = BR.ReadUInt32();  // Fallback GUID
                switch (ID)
                {
                    case PortalPedestrian:
                    case PortalCarStart:
                    case PortalCarStop:
                    case PortalCarServiceStart:
                    case PortalCarServiceStop:
                    case ResidentialMailbox:
                    case ResidentialTrashCan:
                    case CommunityPhoneBooth:
                    case CommunityTrashCan:
                        {
                            if (Test_PrintDebugInfo)
                                Debug.Print("  Portal[{0}] {1:X8} = {2:X4} {3}", z, ID, Reference, sObjectName);
                            PortalArray[z].uPortalType = ID;
                            PortalArray[z].sPortalString = sObjectName;
                            PortalArray[z].uReference = Reference;
                            PortalArray[z].bTreatAsObject = false;
                            PortalArray[z].uInstances = null;
                            z++;
                            break;
                        }
                    default:
                        {
                            // ToDo: Handle beach portals?
                            // if (Test_PrintDebugInfo)
                            //     Debug.Print("{0:X8} = {1:X8} = {2}", GUID, ID, sObjectName);
                            break;
                        }
                }
                GUID = BR.ReadUInt32();
            }
            // Expect either Residential Mailbox and Trash or Commercial Phone Booth and Trash
            // Debug.Assert((z >= iMaxPortals - 2) && (z <= iMaxPortals));
            GetInstances();
        }

        // Use PortalArray.uReference to find all Portal Instances in OBJM
        private void GetInstances()
        {
            const int iLeadingZeros = 68;
            IPackedFileDescriptor OBJM = Package.FindFile(0x4F626A4D, 0, 0xFFFFFFFF, 1);
            IPackedFile PF = Package.Read(OBJM);
            Data = PF.UncompressedData;
            BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);

            // ToDo: Determine format of 0x4F626A4D record: OBJM - Object Material
            for (int d = 0; d < iLeadingZeros / 4; d++)
            {
                int Dumm = BR.ReadInt32();
                Debug.Assert(Dumm == 0);
            }
            int iDumm = BR.ReadInt32();
            Debug.Assert(iDumm != 0);
            // Debug.Assert((iDumm == 203) || (iDumm == 210) || (iDumm == 215) || (iDumm == 216));
            // Debug.Print("OBJM int = {0}", iDumm);
            uint uBlockID = BR.ReadUInt32();
            Debug.Assert(uBlockID == 0x4F626A4D);
            int Count = BR.ReadInt32();
            if (Test_PrintDebugInfo)
                Debug.Print("Find all Portal Instances:");
            for (int iObj = 0; iObj < Count; iObj++)
            {
                uint Instanz = BR.ReadUInt32();
                uint Reference = BR.ReadUInt32();
                for (int i = 0; i < iMaxPortals; i++)
                {
                    if (Reference == PortalArray[i].uReference)
                    {
                        int j = 0;
                        if (PortalArray[i].uInstances == null)
                            PortalArray[i].uInstances = new uint[1];
                        else
                        {
                            uint uPortalType = PortalArray[i].uPortalType;
                            j = PortalArray[i].uInstances.Length;
#if ADJUST
                            // Expect 2 Pedestrian Portals and 1 of all other Portals
                            if (j > ((uPortalType == PortalPedestrian) ? 1 : 0))
                            {
                                if ((uPortalType == ResidentialMailbox)
                                 || (uPortalType == ResidentialTrashCan)
                                 || (uPortalType == CommunityPhoneBooth)
                                 || (uPortalType == CommunityTrashCan))
                                {
                                    // Do not treat this instance as a portal;
                                    // instead move them with the building, like other objects
                                    PortalArray[i].bTreatAsObject = true;
                                    if (Test_PrintDebugInfo)
                                        Debug.Print("  Too many Portals {0:X8} = {1:X4} = Instance {2:X8} {3}",
                                            PortalArray[i].uPortalType, Reference, Instanz, PortalArray[i].sPortalString);
                                    break;
                                }
                                else
                                {
                                    // ToDo: Must determine how to change portals if more instances than expected.
                                    // Debug.Fail("Portal Type has more Instances than expected");
                                }
                            }
#endif
                            uint[] uTemp = new uint[j + 1];
                            Array.Copy(PortalArray[i].uInstances, uTemp, j);
                            PortalArray[i].uInstances = uTemp;
                        }
                        PortalArray[i].uInstances[j] = Instanz;
                        if (Test_PrintDebugInfo)
                            Debug.Print("  Portal[{0}, {1}] {2:X8} = {3:X4} = Instance {4:X8} {5}",
                                i, j, PortalArray[i].uPortalType, Reference, Instanz, PortalArray[i].sPortalString);
                        break;
                    }
                }
            }
        }

#if ADJUST
        public void Clear()
        {
            for (int i = 0; i < iMaxPortals; i++)
            {
                PortalArray[i].bTreatAsObject = true;
            }
        }
#endif

        public bool IsPortal(uint Instance)
        {
            bool bPortal = false;
            for (int i = 0; i < iMaxPortals; i++)
            {
                if ((PortalArray[i].uInstances == null) || (PortalArray[i].bTreatAsObject))
                    continue;
                for (int j = 0; j < PortalArray[i].uInstances.Length; j++)
                {
                    if (Instance == PortalArray[i].uInstances[j])
                        bPortal = true;
                }
            }
            return bPortal;
        }

#if ADJUST
        public float ChangeWidth(float Y, int iWidthOld, int iWidthNew)
        {
            int iWidthDiff = (iWidthNew - iWidthOld);
            if (Y > 10)
            {
                if (Y < iWidthOld - 10)
                    Y -= iWidthDiff / 2;
                else
                    Y += iWidthDiff;
            }
            return Y;
        }

        public float ChangeHeight(float X, int iHeightOld, int iHeightNew)
        {
            int iHeightDiff = (iHeightNew - iHeightOld);
            if (X > 10)
            {
                if (X < iHeightOld - 10)
                    X -= iHeightDiff / 2;
                else
                    X += iHeightDiff;
            }
            return X;
        }

        private const byte U11_Left = 0x00;
        private const byte U11_Top = 0x01;
        private const byte U11_Right = 0x02;
        private const byte U11_Bottom = 0x03;

        private const float PortalFrontPedestrian = 9.5F;   // Space between front edge and the sidewalk
        private const float PortalSidePedestrian  = 0.5F;   // Space between side edge and pedestrian portal

        // Pedestrian portal at the right edge of the lot (as seem from the road)
        private float RightPedestrianX(uint U11, int iHeightNew)
        {
            float X = 0;
            if (U11 == U11_Left)
                X = PortalSidePedestrian;
            else if (U11 == U11_Top)
                X = iHeightNew - PortalFrontPedestrian;
            else if (U11 == U11_Right)
                X = iHeightNew - PortalSidePedestrian;
            else if (U11 == U11_Bottom)
                X = PortalFrontPedestrian;
            return X;
        }

        private float RightPedestrianY(uint U11, int iWidthNew)
        {
            float Y = 0;
            if (U11 == U11_Left)
                Y = PortalFrontPedestrian;
            else if (U11 == U11_Top)
                Y = PortalSidePedestrian;
            else if (U11 == U11_Right)
                Y = iWidthNew - PortalFrontPedestrian;
            else if (U11 == U11_Bottom)
                Y = iWidthNew - PortalSidePedestrian;
            return Y;
        }

        // Pedestrian portal at the left edge of the lot (as seem from the road)
        private float LeftPedestrianX(uint U11, int iHeightNew)
        {
            float X = 0;
            if (U11 == U11_Left)
                X = iHeightNew - PortalSidePedestrian;
            else if (U11 == U11_Top)
                X = iHeightNew - PortalFrontPedestrian;
            else if (U11 == U11_Right)
                X = PortalSidePedestrian;
            else if (U11 == U11_Bottom)
                X = PortalFrontPedestrian;
            return X;
        }

        private float LeftPedestrianY(uint U11, int iWidthNew)
        {
            float Y = 0;
            if (U11 == U11_Left)
                Y = PortalFrontPedestrian;
            else if (U11 == U11_Top)
                Y = iWidthNew - PortalSidePedestrian;
            else if (U11 == U11_Right)
                Y = iWidthNew - PortalFrontPedestrian;
            else if (U11 == U11_Bottom)
                Y = PortalSidePedestrian;
            return Y;
        }

        private const float PortalFrontCar     = 5.5F;      // Space between front edge and road closest to house
        private const float PortalSideVehicle  = 1.5F;      // Space between side edge and car portal (includes room for trunk access)

        private float CarStartX(uint U11, int iHeightNew)
        {
            float X = 0;
            if (U11 == U11_Left)
                X = PortalSideVehicle;
            else if (U11 == U11_Top)
                X = iHeightNew - PortalFrontCar;
            else if (U11 == U11_Right)
                X = iHeightNew - PortalSideVehicle;
            else if (U11 == U11_Bottom)
                X = PortalFrontCar;
            return X;
        }

        private float CarStartY(uint U11, int iWidthNew)
        {
            float Y = 0;
            if (U11 == U11_Left)
                Y = PortalFrontCar;
            else if (U11 == U11_Top)
                Y = PortalSideVehicle;
            else if (U11 == U11_Right)
                Y = iWidthNew - PortalFrontCar;
            else if (U11 == U11_Bottom)
                Y = iWidthNew - PortalSideVehicle;
            return Y;
        }

        private const float PortalStop       = 0.5F;        // Place vehicle stop portal at halfway point.
        private const float PortalStopOffset = 3.5F;        // Offset from halfway point.

        private float CarStopX(uint U11, int iHeightNew)
        {
            float X = 0;
            if (U11 == U11_Left)
                X = iHeightNew * PortalStop + PortalStopOffset;
            else if (U11 == U11_Top)
                X = iHeightNew - PortalFrontCar;
            else if (U11 == U11_Right)
                X = iHeightNew * PortalStop - PortalStopOffset;
            else if (U11 == U11_Bottom)
                X = PortalFrontCar;
            return X;
        }

        private float CarStopY(uint U11, int iWidthNew)
        {
            float Y = 0;
            if (U11 == U11_Left)
                Y = PortalFrontCar;
            else if (U11 == U11_Top)
                Y = iWidthNew * PortalStop + PortalStopOffset;
            else if (U11 == U11_Right)
                Y = iWidthNew - PortalFrontCar;
            else if (U11 == U11_Bottom)
                Y = iWidthNew * PortalStop - PortalStopOffset;
            return Y;
        }

        private const float PortalFrontService = 4.5F;      // Space between front edge and road furthest from house

        private float ServiceStartX(uint U11, int iHeightNew)
        {
            float X = 0;
            if (U11 == U11_Left)
                X = iHeightNew - PortalSideVehicle;
            else if (U11 == U11_Top)
                X = iHeightNew - PortalFrontService;
            else if (U11 == U11_Right)
                X = PortalSideVehicle;
            else if (U11 == U11_Bottom)
                X = PortalFrontService;
            return X;
        }

        private float ServiceStartY(uint U11, int iWidthNew)
        {
            float Y = 0;
            if (U11 == U11_Left)
                Y = PortalFrontService;
            else if (U11 == U11_Top)
                Y = iWidthNew - PortalSideVehicle;
            else if (U11 == U11_Right)
                Y = iWidthNew - PortalFrontService;
            else if (U11 == U11_Bottom)
                Y = PortalSideVehicle;
            return Y;
        }

        private float ServiceStopX(uint U11, int iHeightNew)
        {
            float X = 0;
            if (U11 == U11_Left)
                X = iHeightNew * PortalStop - PortalStopOffset;
            else if (U11 == U11_Top)
                X = iHeightNew - PortalFrontService;
            else if (U11 == U11_Right)
                X = iHeightNew * PortalStop + PortalStopOffset;
            else if (U11 == U11_Bottom)
                X = PortalFrontService;
            return X;
        }

        private float ServiceStopY(uint U11, int iWidthNew)
        {
            float Y = 0;
            if (U11 == U11_Left)
                Y = PortalFrontService;
            else if (U11 == U11_Top)
                Y = iWidthNew * PortalStop - PortalStopOffset;
            else if (U11 == U11_Right)
                Y = iWidthNew - PortalFrontService;
            else if (U11 == U11_Bottom)
                Y = iWidthNew * PortalStop + PortalStopOffset;
            return Y;
        }

        private const float MailboxFront = 8.5F;            // Mailbox, Phone Booth, Trash Cans go between sidewalk and road

        private float MailboxX(uint U11, bool bLeftRoad, bool bRightRoad, int iHeightNew)
        {
            float X = 0;
            int iLeftRoad = (bLeftRoad) ? iLotTilesPerNeighborhoodTile : 0;
            int iRightRoad = (bRightRoad) ? iLotTilesPerNeighborhoodTile : 0;
            int iBothRoads = iLeftRoad + iRightRoad;
            if (U11 == U11_Left)
                X = (iHeightNew - iBothRoads) * PortalStop + PortalSideVehicle + iRightRoad;
            else if (U11 == U11_Top)
                X = iHeightNew - MailboxFront;
            else if (U11 == U11_Right)
                X = (iHeightNew - iBothRoads) * PortalStop - PortalSideVehicle + iLeftRoad;
            else if (U11 == U11_Bottom)
                X = MailboxFront;
            return X;
        }

        private float MailboxY(uint U11, bool bLeftRoad, bool bRightRoad, int iWidthNew)
        {
            float Y = 0;
            int iLeftRoad = (bLeftRoad) ? iLotTilesPerNeighborhoodTile : 0;
            int iRightRoad = (bRightRoad) ? iLotTilesPerNeighborhoodTile : 0;
            int iBothRoads = iLeftRoad + iRightRoad;
            if (U11 == U11_Left)
                Y = MailboxFront;
            else if (U11 == U11_Top)
                Y = (iWidthNew - iBothRoads) * PortalStop + PortalSideVehicle + iRightRoad;
            else if (U11 == U11_Right)
                Y = iWidthNew - MailboxFront;
            else if (U11 == U11_Bottom)
                Y = (iWidthNew - iBothRoads) * PortalStop - PortalSideVehicle + iLeftRoad;
            return Y;
        }

        private float TrashX(uint U11, bool bLeftRoad, bool bRightRoad, int iHeightNew)
        {
            float X = 0;
            int iLeftRoad = (bLeftRoad) ? iLotTilesPerNeighborhoodTile : 0;
            int iRightRoad = (bRightRoad) ? iLotTilesPerNeighborhoodTile : 0;
            int iBothRoads = iLeftRoad + iRightRoad;
            if (U11 == U11_Left)
                X = (iHeightNew - iBothRoads) * PortalStop - PortalSideVehicle + iRightRoad;
            else if (U11 == U11_Top)
                X = iHeightNew - MailboxFront;
            else if (U11 == U11_Right)
                X = (iHeightNew - iBothRoads) * PortalStop + PortalSideVehicle + iLeftRoad;
            else if (U11 == U11_Bottom)
                X = MailboxFront;
            return X;
        }

        private float TrashY(uint U11, bool bLeftRoad, bool bRightRoad, int iWidthNew)
        {
            float Y = 0;
            int iLeftRoad = (bLeftRoad) ? iLotTilesPerNeighborhoodTile : 0;
            int iRightRoad = (bRightRoad) ? iLotTilesPerNeighborhoodTile : 0;
            int iBothRoads = iLeftRoad + iRightRoad;
            if (U11 == U11_Left)
                Y = MailboxFront;
            else if (U11 == U11_Top)
                Y = (iWidthNew - iBothRoads) * PortalStop - PortalSideVehicle + iRightRoad;
            else if (U11 == U11_Right)
                Y = iWidthNew - MailboxFront;
            else if (U11 == U11_Bottom)
                Y = (iWidthNew - iBothRoads) * PortalStop + PortalSideVehicle + iLeftRoad;
            return Y;
        }

        private const byte Direction_Left   = 0x00;
        private const byte Direction_Top    = 0x02;
        private const byte Direction_Right  = 0x04;
        private const byte Direction_Bottom = 0x06;
        private const byte Direction_None   = 0x08;

        private byte DirectionCar(uint U11)
        {
            byte bDirection = Direction_None;
            if (U11 == U11_Left)
                bDirection = Direction_Top;
            else if (U11 == U11_Top)
                bDirection = Direction_Right;
            else if (U11 == U11_Right)
                bDirection = Direction_Bottom;
            else if (U11 == U11_Bottom)
                bDirection = Direction_Left;
            return bDirection;
        }

        private byte DirectionService(uint U11)
        {
            byte bDirection = Direction_None;
            if (U11 == U11_Left)
                bDirection = Direction_Bottom;
            else if (U11 == U11_Top)
                bDirection = Direction_Left;
            else if (U11 == U11_Right)
                bDirection = Direction_Top;
            else if (U11 == U11_Bottom)
                bDirection = Direction_Right;
            return bDirection;
        }

        private byte DirectionMailbox(uint U11)
        {
            byte bDirection = Direction_None;
            if (U11 == U11_Left)
                bDirection = Direction_Left;
            else if (U11 == U11_Top)
                bDirection = Direction_Top;
            else if (U11 == U11_Right)
                bDirection = Direction_Right;
            else if (U11 == U11_Bottom)
                bDirection = Direction_Bottom;
            return bDirection;
        }

        private byte DirectionPhonebooth(uint U11)
        {
            byte bDirection = Direction_None;
            if (U11 == U11_Left)
                bDirection = Direction_Right;
            else if (U11 == U11_Top)
                bDirection = Direction_Bottom;
            else if (U11 == U11_Right)
                bDirection = Direction_Left;
            else if (U11 == U11_Bottom)
                bDirection = Direction_Top;
            return bDirection;
        }

        // ToDo: Try the following logic, which is smaller but more complex:
        //       1) Translate so that X=0, Y=0 is in center
        //       2) Set portal locations for road at front
        //       3) Rotate based on U11
        //       4) Translate back to original coordinates
        public void Change(uint U11, bool bLeftRoad, bool bRightRoad, int iWidthOld, int iHeightOld, int iWidthNew, int iHeightNew)
        {
            int iWidthDiff = (iWidthNew - iWidthOld);
            int iHeightDiff = (iHeightNew - iHeightOld);

            for (int i = 0; i < iMaxPortals; i++)
            {
                PortalInfo pInfo = PortalArray[i];
                if ((pInfo.uInstances == null) || (pInfo.bTreatAsObject))
                    continue;
                uint uPortalType = pInfo.uPortalType;

                // Usually assume that the first pedestrian portal is on the left side
                // and the second pedestrian portal is on the right side of the lot.
                // However, there is some problem with display of the portal direction,
                // so try to maintain the direction of each pedestrian portal to decrease confusion.
                // True => Swap Pedestrian Portals to maintain direction
                bool bSwapPedestrian = false;

                for (int j = 0; j < pInfo.uInstances.Length; j++)
                {
                    uint uInst = pInfo.uInstances[j];

                    if (Test_PrintDebugInfo)
                        Debug.Print("Change Portal[{0},{1}] {2:X8} = Instance {3:X8} {4}",
                            i, j, pInfo.uPortalType, uInst, pInfo.sPortalString);

                    // ToDo: Use existing XOBJ class?
                    IPackedFileDescriptor XOBJ = Package.FindFile(0x584F424A, 0, 0xFFFFFFFF, uInst);
                    IPackedFile PF = Package.Read(XOBJ);
                    byte[] Data = PF.UncompressedData;
                    BinaryReader BR = SimPe.Helper.GetBinaryReader(Data);

                    // ToDo: Determine format of 0x584F424A record: XOBJ - Object Class Dump
                    const int iLeadingZeros = 80;
                    for (int k = 0; k < iLeadingZeros / 4; k++)
                    {
                        int iDummy = BR.ReadInt32();
                        Debug.Assert(iDummy == 0);
                    }
                    const int XIndex = iLeadingZeros;
                    int DIndex = XIndex;

                    float XOld = BR.ReadSingle();
                    float XNew = XOld;
                    DIndex += 4;

                    float YOld = BR.ReadSingle();
                    float YNew = YOld;
                    DIndex += 4;

                    ushort uVersion = 0;
                    byte bDirection = Direction_None;
                    if ((uPortalType == PortalPedestrian)
                     || (uPortalType == PortalCarStart)
                     || (uPortalType == PortalCarStop)
                     || (uPortalType == PortalCarServiceStart)
                     || (uPortalType == PortalCarServiceStop)
                     || (uPortalType ==  ResidentialMailbox)
//                     || (uPortalType ==  ResidentialTrashCan) // Trash can rotation is irrelevant?
                     || (uPortalType ==  CommunityPhoneBooth)
//                     || (uPortalType ==  CommunityTrashCan)   // Trash can rotation is irrelevant?
                       )
                    {
                        uint uDummy = BR.ReadUInt32();
                        Debug.Assert((uDummy == 1) || (uDummy == 2));
                        DIndex += 4;

                        uDummy = BR.ReadUInt32();
                        Debug.Assert(uDummy == 1);
                        DIndex += 4;

                        short sDummy = BR.ReadInt16();
                        Debug.Assert(sDummy == 0);
                        DIndex += 2;

                        uDummy = BR.ReadUInt32();
                        Debug.Assert(uDummy == 0xFFFFFFFF);
                        DIndex += 4;

                        sDummy = BR.ReadInt16();
                        Debug.Assert(sDummy == 0);
                        DIndex += 2;

                        uVersion = BR.ReadUInt16();
                        Debug.Assert(
                            (uVersion == 0)  // Apartment Life
                         || (uVersion == 8)
                         || (uVersion == 11)
                         || (uVersion == 12)
                        );
                        DIndex += 2;

                        // ToDo: looks like uVersion might actually be a count of shorts > 9.
                        int kMax = 17;
                        if (uVersion == 8)
                            kMax = 17;
                        else if (uVersion == 11)
                            kMax = 20;
                        else if (uVersion == 12)
                            kMax = 21;
                        for (int k = 0; k < kMax; k++)
                        {
                            ushort usDummy = BR.ReadUInt16();
                            DIndex += 2;
                        }

                        sDummy = BR.ReadInt16();
                        Debug.Assert((sDummy == 0) || (sDummy == 1) || (sDummy == 2));
                        DIndex += 2;

                        bDirection = BR.ReadByte();
                        Debug.Assert((bDirection == Direction_Left)
                                  || (bDirection == Direction_Top)
                                  || (bDirection == Direction_Right)
                                  || (bDirection == Direction_Bottom));
                    }
                    if (Test_PrintDebugInfo)
                        Debug.Print("  From: {0} {1} {2:X2} U11={3}", XOld, YOld, bDirection, U11);

                    // ToDo: Break off calculation of portal location into separate function
                    switch (pInfo.uPortalType)
                    {
                        case PortalPedestrian:
                            {
                                byte bDC = DirectionCar(U11);
                                byte bDS = DirectionService(U11);
                                if (Test_AssertDirection)
                                    Debug.Assert((bDirection == bDC) || (bDirection == bDS));
                                if ((j == 0) || bSwapPedestrian)
                                {
                                    if ((bDirection == bDC) && (!bSwapPedestrian))
                                    {
                                        if (Test_PrintDebugInfo)
                                            Debug.Print("Swap pedestrian portals");
                                        bSwapPedestrian = true;
                                    }
                                    else
                                    {
                                        XNew = LeftPedestrianX(U11, iHeightNew);
                                        YNew = LeftPedestrianY(U11, iWidthNew);
                                        if (Test_AssertDirection)
                                            Debug.Assert(bDirection == bDS);
                                        bDirection = bDS;
                                        if (bSwapPedestrian)
                                        {
                                            // Completed: j==0 is Right portal; j==1 is Left portal
                                            // So reset everything and continue with next portal type.
                                            bSwapPedestrian = false;
                                            j = -1;
                                        }
                                    }
                                }
                                if ((j > 0) || bSwapPedestrian)
                                {
                                    XNew = RightPedestrianX(U11, iHeightNew);
                                    YNew = RightPedestrianY(U11, iWidthNew);
                                    if (Test_AssertDirection)
                                        Debug.Assert(bDirection == bDC);
                                    bDirection = bDC;
                                }
                                if (j == -1)
                                    j = 1;
                                break;
                            }
                        case PortalCarStart:
                            {
                                XNew = CarStartX(U11, iHeightNew);
                                YNew = CarStartY(U11, iWidthNew);
                                byte bDC = DirectionCar(U11);
                                if (Test_AssertDirection)
                                    Debug.Assert(bDirection == bDC);
                                bDirection = bDC;
                                break;
                            }
                        case PortalCarStop:
                            {
                                XNew = CarStopX(U11, iHeightNew);
                                YNew = CarStopY(U11, iWidthNew);
                                byte bDC = DirectionCar(U11);
                                if (Test_AssertDirection)
                                    Debug.Assert(bDirection == bDC);
                                bDirection = bDC;
                                break;
                            }
                        case PortalCarServiceStart:
                            {
                                XNew = ServiceStartX(U11, iHeightNew);
                                YNew = ServiceStartY(U11, iWidthNew);
                                byte bDS = DirectionService(U11);
                                if (Test_AssertDirection)
                                    Debug.Assert(bDirection == bDS);
                                bDirection = bDS;
                                break;
                            }
                        case PortalCarServiceStop:
                            {
                                XNew = ServiceStopX(U11, iHeightNew);
                                YNew = ServiceStopY(U11, iWidthNew);
                                byte bDS = DirectionService(U11);
                                if (Test_AssertDirection)
                                    Debug.Assert(bDirection == bDS);
                                bDirection = bDS;
                                break;
                            }
                        case ResidentialMailbox:
                            {
                                XNew = MailboxX(U11, bLeftRoad, bRightRoad, iHeightNew);
                                YNew = MailboxY(U11, bLeftRoad, bRightRoad, iWidthNew);
                                byte bDM = DirectionMailbox(U11);
                                if (Test_AssertDirection)
                                    Debug.Assert(bDirection == bDM);
                                bDirection = bDM;
                                break;
                            }
                        case CommunityPhoneBooth:
                            {
                                XNew = MailboxX(U11, bLeftRoad, bRightRoad, iHeightNew);
                                YNew = MailboxY(U11, bLeftRoad, bRightRoad, iWidthNew);
                                byte bDP = DirectionPhonebooth(U11);
                                // ToDo: Why does this particular assert fail so frequently?  Check for 0 fixes.
                                if (Test_AssertDirection)
                                    Debug.Assert((bDirection == 0) || (bDirection == bDP));
                                bDirection = bDP;
                                break;
                            }
                        case ResidentialTrashCan:
                        case CommunityTrashCan:
                            {
                                XNew = TrashX(U11, bLeftRoad, bRightRoad, iHeightNew);
                                YNew = TrashY(U11, bLeftRoad, bRightRoad, iWidthNew);
                                break;
                            }
                        default:
                            {
                                Debug.Fail("Unknown Portal type");
                                break;
                            }
                    }

                    byte[] B = new byte[8];
                    BinaryWriter BW = new BinaryWriter(new MemoryStream(B));
                    Debug.Assert((0 <= XNew) && (XNew <= iHeightNew));
                    Debug.Assert((0 <= YNew) && (YNew <= iWidthNew));
                    BW.Write(XNew);
                    BW.Write(YNew);
                    Array.Copy(B, 0, Data, XIndex, 8);
                    if (bDirection != Direction_None)
                        Data[DIndex] = bDirection;
                    XOBJ.SetUserData(Data, true);
                    if (Test_PrintDebugInfo)
                        Debug.Print("  To:   {0} {1} {2:X2} U11={3}", XNew, YNew, bDirection, U11);

                    // Set Portal display location in OBJT to match portal location in XOBJ.
                    IPackedFileDescriptor OBJT = Package.FindFile(0xFA1C39F7, 0, 0xFFFFFFFF, uInst);
                    R_OBJT Res = new R_OBJT(Package, OBJT);
                    // ToDo: Should fix X, Y to match other classes...
                    Res.Change(YNew - YOld, XNew - XOld, bDirection, iWidthNew, iHeightNew);
                }
            }
        }

        public void ChangeElevation(float fElevation)
        {
            for (int i = 0; i < iMaxPortals; i++)
            {
                PortalInfo pInfo = PortalArray[i];
                if ((pInfo.uInstances == null) || (pInfo.bTreatAsObject))
                    continue;

                for (int j = 0; j < pInfo.uInstances.Length; j++)
                {
                    uint uInst = pInfo.uInstances[j];

                    if (Test_PrintDebugInfo)
                        Debug.Print("Change Portal[{0},{1}] {2:X8} = Instance {3:X8} {4}",
                            i, j, pInfo.uPortalType, uInst, pInfo.sPortalString);

                    // Set Portal elevation in OBJT to match road elevation.
                    IPackedFileDescriptor OBJT = Package.FindFile(0xFA1C39F7, 0, 0xFFFFFFFF, uInst);
                    R_OBJT Res = new R_OBJT(Package, OBJT);
                    Res.Change(fElevation);
                }
            }
        }
#endif
    }
}
