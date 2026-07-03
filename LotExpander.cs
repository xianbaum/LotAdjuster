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
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SimPe.Interfaces.Files;
using SimPe.Packages;
using System.Resources;

// ToDo: convert to english

// Done? Rotate mailbox and phonebooth as needed; need to understand XOBJ and OBJT record formats
// Done: Use neighborhood terrain
// Done: Level road
// ToDo: When road is flattened, neighborhood roads can go underground.
// ToDo: Swap the names "Width" and "Height" throughout to match the definitions in www.sims2wiki.info?
//       Better yet, use the terms "Row", "Column", and "Layer" or "Rank"(?), to avoid any confusion with
//       the lot width or frontage (side to side), depth (front to back), and height (elevation).
namespace LotExpander
{
    public partial class PrimaryForm : Form
    {
        private bool Test_PrintDebugInfo = false;   // Enable (T) or disable (F) printing of debug information
        private bool Test_Run = false;              // Enable (T) or disable (F) running tests without user input
        private bool Test_AlwaysAbort = false;      // Enable (T) or disable (F) abort for all lots
        private bool Allow_Shrink = true;           // Enable (T) or disable (F) shrinking

        // ToDo: Problem if Control Panel / Display / Settings / Advanced / DPI settings = 125% of normal
        private int ScreenWidthStandard = 350;
        private int ScreenWidthExplain  = 520;
        private int ScreenWidthAdvanced = 640;

        System.EventHandler ValueHandler = null;

        private const int iLotTilesPerNeighborhoodTile = 10;

        private const uint uVersionNumber = 17;
        // The version strings vary by language, so it's better to check version number
        private string[] sVersionStrings = 
        {
            /*  0 */ "The Sims 2",
            /*  1 */ "The Sims 2 University",
            /*  2 */ "The Sims 2 Nightlife",
            /*  3 */ "The Sims 2 Open For Business",
            /*  4 */ "The Sims 2 Family Fun Stuff",
            /*  5 */ "The Sims 2 Glamour Life Stuff",
            /*  6 */ "The Sims 2 Pets",
            /*  7 */ "The Sims 2 Seasons",
            /*  8 */ "The Sims 2 Celebration! Stuff",
            /*  9 */ "The Sims™ 2 H&M® Fashion Stuff",
            /* 10 */ "The Sims™ 2 Bon Voyage",
            /* 11 */ "The Sims™ 2 Teen Style Stuff",
            /* 12 */ "The Sims™ 2 Store",                   // ToDo: Check this...
            /* 13 */ "The Sims™ 2 FreeTime",
            /* 14 */ "The Sims 2 Kitchen and Bath Stuff",   // ToDo: Check this...
            /* 15 */ "The Sims 2 Ikea Stuff",               // ToDo: Check this...
            /* 16 */ "The Sims™ 2 Apartment Life",
            /* 17 */ "The Sims™ 2 Mansion and Garden Stuff"
        };

        private OpenFileDialog openFileDialog1 = new OpenFileDialog();
        private bool bBackupVersioning = false;     // Keep multiple versions of backup files
        private string sBackupConfigFile = null;

        private Dictionary<object, string> sExpl = new Dictionary<object, string>();

        public PrimaryForm()
        {
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            InitializeComponent();
            // this.CenterToScreen();
            RMF = new ResourceManager("LotExpander.PrimaryForm", typeof(PrimaryForm).Assembly);
            RME = new ResourceManager("LotExpander.LEStrings", typeof(PrimaryForm).Assembly);
            sExpl.Add(AllowShrink,          "Shrink lot by removing an empty area on the lot. Mostly safe. See the download thread for known issues.");
            sExpl.Add(BeachLot,             "This feature is still in testing.  Change between beach and normal lot.  Note that this may only affect lot placement.");
            sExpl.Add(BumpyRoads,           "Roads which are not flat may not work properly. Portals may need to be moved to a flat area.");
            sExpl.Add(ChangeRoads,          "Enable the Road checkboxes. If there is a road on the lot, you must have a road at the front.  Special handling will be required to move and share lots with no roads.");
            sExpl.Add(ClassOverride,        "Set the lot class value. This may affect your lot class (high, middle, low) and the social groups which visit your lot.");
            sExpl.Add(ClassValueChange, sExpl[ClassOverride]);
            sExpl.Add(ClassValueDisplay, sExpl[ClassOverride]);
            sExpl.Add(Hidden,               "This feature is still in testing.  Make lot hidden or visible in the neighborhood view.");
            sExpl.Add(KeepElevation,        "If the lot elevation is not fixed, the lot may be too high (floating) or too low (underground).");
            sExpl.Add(KeepStreet,           "Enlarge the lot above the road, or on the other side of the road.  Special handling will be required to move and share the lot.");
            sExpl.Add(LeavePortals,         "The portals will not be moved to their standard locations on the lot. User must place portals manually. Portals include mailbox, public phone and trash can.");
            sExpl.Add(LotEdges,             "The lot edges will be modified, but the rest of the lot terrain will be unaffected.");
            sExpl.Add(LabelEdges, sExpl[LotEdges]);
            sExpl.Add(MatchHoodTerrain,     "This feature is still in testing.  Override the base terrain paint for the lot with the standard neighborhood terrain paint.");
            sExpl.Add(MoveLot,              "Move the lot in the neighborhood. The lot will not be locked onto a neighborhood road unless moved again within the game.");
            sExpl.Add(MultiBackup,          "Keep multiple versions of backup files. Useful when doing multiple adjustments, but can fill your hard drive unless you delete them occasionally.");
            sExpl.Add(PaveRoads,            "This feature is still in testing.  Only straight roads will be paved, but the game should fill in the corners.");
            sExpl.Add(RemoveFurniture,      "This feature is still in testing.  Remove furniture on the lot if checked.  Removing furniture will reset any sims on the lot.");
            sExpl.Add(RemoveTerrainPaints,  "This feature is still in testing.  Remove unused terrain paints.  This may solve the blue terrain that can occur when custom terrain paints are not installed.");
            sExpl.Add(UpdateHoodTerrain,    "The elevation of the neighborhood under your lot will be forced to match the new lot terrain elevation.");
        }

        private ResourceManager RMF;
        private ResourceManager RME;
        private IDictionary<uint, string> LotFamily = new Dictionary<uint, string>();
        private IPackedFileDescriptor[] LotDescription;
        private GeneratableFile NBPack;

        public static string ThrowErrorOffLot(string sType, string sList)
        {
            ResourceManager RME = new ResourceManager("LotExpander.LEStrings", typeof(PrimaryForm).Assembly);
            throw new ShrinkException(
                string.Format(RME.GetString("ExplainShrinkAbort"), sType, sList));
        }

        public static string ThrowErrorEmptyLot(string sLotName)
        {
            ResourceManager RME = new ResourceManager("LotExpander.LEStrings", typeof(PrimaryForm).Assembly);
            throw new EmptyLotException(
                string.Format(RME.GetString("ExplainEmptyLotAbort"), sLotName));
        }

        // U11 tells us the rotation of the lot in the lot file, ie. where the front of lot is:
        // To better visualize, use SimPE to open lot and look at Texture Image (TXTR) terrain pictures
        //    U11=0         U11=1         U11=2         U11=3
        // Faces Left     Faces Top    Faces Right   Faces Bottom
        // -----------   -----------   -----------   -----------
        // |F        |   |  FRONT  |   |        F|   |         |
        // |R        |   |         |   |        R|   |         |
        // |O        |   |         |   |        O|   |         |
        // |N        |   |         |   |        N|   |         |
        // |T        |   |         |   |        T|   |  FRONT  |
        // -----------   -----------   -----------   -----------
        private const byte U11_Left   = 0x00;
        private const byte U11_Top    = 0x01;
        private const byte U11_Right  = 0x02;
        private const byte U11_Bottom = 0x03;
        private byte U11;

        private const int Screen_Initial = 0;
        private const int Screen_Neighborhood = 1;
        private const int Screen_Lot = 2;
        private const int Screen_Advanced = 3;
        private const int Screen_Expansion = 4;
        private const int Screen_Final = 5;
        private int Screen = Screen_Initial;

        private const int iMaxGrid = 128;
        private int iMoveBack = 0;
        private int iMoveLeft = 0;

        private void NextButton_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            switch (Screen)
            {
                case Screen_Initial:
                    {
                        NeighborhoodScreen();
                        break;
                    }
                case Screen_Neighborhood:
                case Screen_Lot:
                    {
                        Liste_DoubleClick(sender, e);
                        break;
                    }
                case Screen_Advanced:
                case Screen_Expansion:
                    {
                        /*
                        if (string.Equals(WidthOld.Text, WidthNew.Text)
                         && string.Equals(HeightOld.Text, HeightNew.Text)
                         && (iMoveBack == 0)
                         && (iMoveLeft == 0)
                         && (LeavePortals.Checked == true)
                           )
                        {
                            if (MessageBox.Show(this, RME.GetString("MessageNoChange"), RME.GetString("TitleNoChange"),
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question
                                ) == DialogResult.No)
                            {
                                this.Cursor = Cursors.Default;
                                return;
                            }
                        }
                        */
                        FinalScreen();
                        break;
                    }
                case Screen_Final:
                    {
                        LongExpl.Visible = false;
                        LotScreen();
                        break;
                    }
            }
            this.Cursor = Cursors.Default;
        }

        private bool bArrowUp = false;

        private void Liste_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyValue == 0x26) && (Liste.SelectedIndex > 1))
                bArrowUp = true;
        }

        private void Liste_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Liste.SelectedIndex != -1)
            {
                NextButton.Enabled = true;
                if (Screen == Screen_Neighborhood)
                {
                    string s = Liste.SelectedItem.ToString();
                    if ((string.Compare(s, "") == 0) || (s.IndexOf(":", 0) != -1))
                    {
                        if (bArrowUp)
                            Liste.SelectedIndex -= 1;
                        else if ((Liste.SelectedIndex + 1) < Liste.Items.Count)
                            Liste.SelectedIndex += 1;
                    }
                }
            }
            else
            {
                NextButton.Enabled = false;
            }
            bArrowUp = false;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            SetExpansionDefaults();
            switch (Screen)
            {
                case Screen_Initial:
                case Screen_Final:
                    {
                        this.Close();
                        break;
                    }
                case Screen_Neighborhood:
                    {
                        InitialScreen();
                        break;
                    }
                case Screen_Lot:
                    {
                        NeighborhoodScreen();
                        break;
                    }
                case Screen_Advanced:
                case Screen_Expansion:
                    {
                        LotScreen();
                        break;
                    }
            }
            this.Cursor = Cursors.Default;
        }

        private void Liste_DoubleClick(object sender, EventArgs e)
        {
            if (Liste.SelectedIndex > -1)
            {
                this.Cursor = Cursors.WaitCursor;
                switch (Screen)
                {
                    case Screen_Neighborhood:
                        {
                            LotScreen();
                            break;
                        }
                    case Screen_Lot:
                        {
                            ExpansionScreen();
                            break;
                        }
                }
            }
            this.Cursor = Cursors.Default;
        }

        #region Initial Screen
        private void InitialScreen()
        {
            this.Width = ScreenWidthStandard;
            LongExpl.Visible = false;
            Liste.Visible = false;

            Title.Text = RMF.GetString("Title.Text");
            Explanation.Text = string.Format(RMF.GetString("Explanation.Text"), sVersionStrings[uVersionNumber]);

            Explanation.Visible = true;
            AdvancedButton.Visible = false;
            NextButton.Text = RMF.GetString("NextButton.Text");
            NextButton.Enabled = true;
            BackButton.Text = RMF.GetString("BackButton.Text");
            BackButton.Enabled = true;
            NextButton.Focus();
            Screen = Screen_Initial;
        }
        #endregion //  Initial Screen

        #region Neighborhood Screen
        private string GetPath(bool bNeighborhood)
        {
            string sPath;
            string sMyDocs = null;
            string sEAGames = null;
            string sSims2 = null;
            string sNbhds = null;
            try
            {
                sMyDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (Test_PrintDebugInfo)
                    Debug.Print("My Documents:  {0}", sMyDocs);
                sEAGames = Path.Combine(sMyDocs, "EA Games");
                if (Test_PrintDebugInfo)
                    Debug.Print("EA Games:      {0}", sEAGames);
                sSims2 = Path.Combine(sEAGames,
                    Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\EA Games\\The Sims 2").GetValue("DisplayName").ToString());
                if (Test_PrintDebugInfo)
                    Debug.Print("The Sims 2:    {0}", sSims2);
                if (bNeighborhood)
                {
                    sNbhds = Path.Combine(sSims2, "Neighborhoods");
                    if (Test_PrintDebugInfo)
                        Debug.Print("Neighborhoods: {0}", sNbhds);
                }
                else
                {
                    sNbhds = Path.Combine(sSims2, "LotCatalog");
                    if (Test_PrintDebugInfo)
                        Debug.Print("LotCatalog: {0}", sNbhds);
                }
            }
            catch
            {
            }
            sPath = sNbhds;
            if ((null == sPath) || !System.IO.Directory.Exists(sPath))
                sPath = sSims2;
            if ((null == sPath) || !System.IO.Directory.Exists(sPath))
                sPath = sEAGames;
            if ((null == sPath) || !System.IO.Directory.Exists(sPath))
                sPath = sMyDocs;
            if ((null == sPath) || !System.IO.Directory.Exists(sPath))
                sPath = "C:\\";

            return sPath;
        }

        private void NeighborhoodScreen()
        {
//          string[] dirs = { "E*", "F*", "G*", "N*" };
            string[] dirs = { "*" };
            if (NBPack != null)
            {
                NBPack.Close(true);
                NBPack = null;
            }
            this.Tag = 0;
            string sPath = GetPath(true);
            if( openFileDialog1.InitialDirectory == "")
                openFileDialog1.InitialDirectory = sPath;
            Liste.BeginUpdate();
            Liste.Items.Clear();
            Liste.Sorted = false;
            System.Collections.ArrayList FileList = new System.Collections.ArrayList();
            for (int h = 0; h < dirs.Length; h++)
            {
                string[] NBOrdner = Directory.GetDirectories(sPath, dirs[h]);
                Array.Sort(NBOrdner);
                for (int i = 0; i < NBOrdner.Length; i++)
                {
                    string sHoodName = Path.GetFileName(NBOrdner[i]);
                    if (0 == string.Compare("Tutorial", sHoodName))
                        continue;

                    string[] MainNeighborhood = Directory.GetFiles(NBOrdner[i], Path.GetFileName(NBOrdner[i]) + "_Neighborhood.package");
                    Debug.Assert(MainNeighborhood.Length < 2);
                    if (MainNeighborhood.Length == 0)
                        continue;

                    // List main neighborhood first
                    try
                    {
                        GeneratableFile Package = SimPe.Packages.File.LoadFromFile(MainNeighborhood[0]);
                        IPackedFileDescriptor NBDescription = Package.FindFile(0x43545353, 0, 0xFFFFFFFF, 1);
                        IPackedFile PF = Package.Read(NBDescription);
                        int z = 0;
                        while (PF.UncompressedData[69 + z++] != 0)
                        {
                        }
                        byte[] BD = new byte[--z];
                        Array.Copy(PF.UncompressedData, 69, BD, 0, z);
                        string NBName = SimPe.Helper.ToString(BD);
                        NBName.Replace(":", ";");
                        if (FileList.Count > 0)
                        {
                            Liste.Items.Add("");
                            FileList.Add("");
                        }
                        Liste.Items.Add(sHoodName + ":");
                        FileList.Add("");

                        Liste.Items.Add("    " + NBName);
                        FileList.Add(MainNeighborhood[0]);

                        // Print a list of neighborhood names for debugging purposes:
                        if (Test_PrintDebugInfo)
                            Debug.Print("{0}: {1}", Path.GetFileName(NBOrdner[i]), NBName);
                    }
                    catch
                    {
                        // Skip neighborhood which cannot be loaded.
                        continue;
                    }

                    // Then list subneighborhoods
                    string[] AlleNBinOrdner = Directory.GetFiles(NBOrdner[i], "*.package");
                    for (int j = 0; j < AlleNBinOrdner.Length; j++)
                    {
                        // We've already done the main neighborhood...
                        if ((MainNeighborhood.Length == 1) && (MainNeighborhood[0] == AlleNBinOrdner[j]))
                            continue;
                        try
                        {
                            GeneratableFile Package = SimPe.Packages.File.LoadFromFile(AlleNBinOrdner[j]);
                            IPackedFileDescriptor[] LotDesc = Package.FindFiles(0x0BF999E7);
                            // Skip hidden neighborhoods, like Pets, Weather (Seasons), and Exotic Destinations (Bon Voyage)
                            if (LotDesc.Length == 0)
                                continue;
                            FileList.Add(AlleNBinOrdner[j]);
                            IPackedFileDescriptor NBDescription = Package.FindFile(0x43545353, 0, 0xFFFFFFFF, 1);
                            IPackedFile PF = Package.Read(NBDescription);
                            int z = 0;
                            while (PF.UncompressedData[69 + z++] != 0)
                            {
                            }
                            byte[] BD = new byte[--z];
                            Array.Copy(PF.UncompressedData, 69, BD, 0, z);
                            string NBName = SimPe.Helper.ToString(BD);
                            NBName.Replace(":", ";");
                            Liste.Items.Add("    " + NBName);
                        }
                        catch
                        {
                        }
                    }
                }
            }

            // List LotCatalog as final option (no neighborhood package)
            /*
            Liste.Items.Add("");
            FileList.Add("");
            Liste.Items.Add("LotCatalog");
            FileList.Add("");
             */

            Liste.EndUpdate();
            Liste.Tag = FileList;
            Liste.Visible = true;
            Title.Text = RME.GetString("NBTitle");
            Explanation.Visible = false;
            AdvancedButton.Visible = true;
            AdvancedButton.Text = RME.GetString("Browse");
            NextButton.Text = RME.GetString("Next");
            NextButton.Enabled = false;
            BackButton.Text = RME.GetString("Back");
            BackButton.Enabled = true;
            if (Liste.Items.Count == 0)
            {
                Title.Text = RME.GetString("NoNBTitle");
                Explanation.Text = RME.GetString("NoNeighborhood");
                Explanation.Visible = true;
                Liste.Visible = false;
                BackButton.Focus();
            }
            else
            {
                Liste.SelectedIndex = 1;
                Liste.Focus();
            }
            Screen = Screen_Neighborhood;
        }
        #endregion //  Neighborhood Screen

        private void Defaults_Click(object sender, EventArgs e)
        {
            this.Width = ScreenWidthStandard;
            AdvancedFeatures.Visible = false;
            Defaults.Visible = false;
            AdvancedButton.Visible = true;
            AdvancedButton.Text = RME.GetString("Advanced");
            SetAdvancedDefaults();
            Screen = Screen_Expansion;
            return;
        }

        private void AdvancedButton_Click(object sender, EventArgs e)
        {
            if (Screen == Screen_Expansion)
            {
                this.Width = ScreenWidthAdvanced;
                AdvancedFeatures.Visible = true;
                Defaults.Visible = true;
                AdvancedButton.Visible = false;
                Liste.Visible = false;
                NextButton.Enabled = true;
                BackButton.Visible = true;

                bool bOccupiedApartment = (((Lot.LotType == 8) && Lot.Occupied) || (Lot.LotType == 9));
                if (bOccupiedApartment)
                {
                    KeepStreet.Enabled = false;
                    MoveLot.Enabled = false;
                    ChangeRoads.Enabled = false;
                    AllowShrink.Enabled = false;
                    LeavePortals.Checked = true;
                    BumpyRoads.Checked = true;
                    BumpyRoads.Enabled = false;
                    KeepElevation.Enabled = false;
                    LabelEdges.Enabled = false;
                    LotEdges.SelectedItem = "Smooth";
                    LotEdges.Enabled = false;
                    Defaults.Enabled = false;
                }
                else
                {
                    KeepStreet.Enabled = true;
                    MoveLot.Enabled = true;
                    ChangeRoads.Enabled = true;
                    AllowShrink.Enabled = true;
                    LeavePortals.Checked = false;
                    BumpyRoads.Checked = false;
                    BumpyRoads.Enabled = true;
                    KeepElevation.Enabled = true;
                    LabelEdges.Enabled = true;
                    LotEdges.SelectedItem = "Neighborhood";
                    LotEdges.Enabled = true;
                    Defaults.Enabled = true;
                }
                LeavePortals.ForeColor = SystemColors.WindowText;

                AdvancedMouseLeave(null, null);
                NextButton.Focus();
                Screen = Screen_Advanced;
                return;
            }

            DialogResult res = DialogResult.OK;
            string sLotPath = (NBPack != null)
                ? Path.Combine(Path.GetDirectoryName(NBPack.FileName), "Lots") : GetPath(false);
            GeneratableFile LotPack = null;

            while (res == DialogResult.OK)
            {
                if (Screen == Screen_Neighborhood)
                    openFileDialog1.Filter = 
                        "Sims2 Neighborhood files (*.package))|*_Neighborhood.package;*_Downtown*.package;*_Suburb*.package;*_University*.package;*_Vacation*.package";
                else if (NBPack != null)
                {
                    string sFileName = Path.GetFileName(NBPack.FileName);
                    int iUnderscore = sFileName.LastIndexOf('_');
                    string sPrefix = sFileName.Substring(0, iUnderscore);
                    openFileDialog1.Filter = "Sims2 Lot files (*.package))|" + sPrefix + "_Lot*.package";
                }
                else
                    openFileDialog1.Filter = "Sims2 Lot files (*.package))|cx_*.package";
                openFileDialog1.FileName = "";
                openFileDialog1.FilterIndex = 0;
                openFileDialog1.RestoreDirectory = false;
                if (Screen == Screen_Neighborhood)
                {
                    openFileDialog1.Title = RME.GetString("NBTitle");
                    res = openFileDialog1.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        NBPack = SimPe.Packages.File.LoadFromFile(openFileDialog1.FileName);
                        if (NBPack != null)
                            break;
                        MessageBox.Show(this, "Unable to open neighborhood", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else // if (Screen == Screen_Lot)
                {
                    openFileDialog1.InitialDirectory = sLotPath;
                    openFileDialog1.Title = RME.GetString("LotTitle");
                    res = openFileDialog1.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        // ToDo: ensure that selected lot belongs to selected neighborhood
                        LotPack = SimPe.Packages.File.LoadFromFile(openFileDialog1.FileName);
                        if (LotPack != null)
                        {
                            try
                            {
                                string sFilePath = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);
                                if (!System.IO.Path.Equals(sLotPath, sFilePath))
                                    throw new Exception();
                                string sFileName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                                int iUnderscore = sFileName.LastIndexOf('_');
                                string sInstance = sFileName.Substring(iUnderscore + 4); // N001_Lot#
                                uint uInstance = 0;
                                uInstance = Convert.ToUInt32(sInstance);
                                if (null == NBPack) // LotCatalog
                                    break;  // ToDo: Create R_DESC from R_LOT for this lot package in the LotCatalog
                                IPackedFileDescriptor LotDescriptor = NBPack.FindFile(0x0BF999E7, 0, 0xFFFFFFFF, uInstance);
                                R_DESC Lot = new R_DESC(NBPack, LotDescriptor);
                                /*
                                if (((Lot.LotType == 8) && Lot.Occupied) || (Lot.LotType == 9))
                                {
                                    // Cannot modify occupied apartment base or sublot
                                    MessageBox.Show(this, 
                                        RME.GetString("MessageOccupiedApartment"), RME.GetString("TitleOccupiedApartment"),
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    LotPack = null;
                                }
                                else
                                 */
                                {
                                    int i = Liste.FindStringExact(Lot.ToString());
                                    Liste.SelectedIndex = i;
                                    Debug.Assert(Liste.SelectedItem.ToString() == Lot.ToString());
                                }
                                break;
                            }
                            catch
                            {
                                MessageBox.Show(this,
                                    "Unable to open lot. Selected lot may not belong to the selected neighborhood.",
                                    "Error: Cannot Open Lot",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            if (Screen == Screen_Neighborhood)
            {
                if ((res == DialogResult.OK) && (NBPack != null))
                    LotScreen();
                else
                    NeighborhoodScreen();
            }
            else
            {
                if ((res == DialogResult.OK) && (NBPack != null) && (LotPack != null))
                    ExpansionScreen();
                else if ((res == DialogResult.OK) && (NBPack == null) && (LotPack != null))
                    ExpansionScreen();
                else
                    LotScreen();
            }
        }

        R_DESC Lot = null;

        #region Lot Screen
        private void GetFamilies()
        {
            if (null == NBPack)
                return;

            // Must get family information from main neighborhood.
            GeneratableFile MainHood = null;
            int iSuffix = NBPack.FileName.IndexOf("_Neighborhood.package");
            if (-1 == iSuffix)
            {
                int iUnderscore = NBPack.FileName.LastIndexOf('_');
                string sPrefix = NBPack.FileName.Substring(0, iUnderscore);
                string sMainHood = sPrefix + "_Neighborhood.package";
                try
                {
                    MainHood = SimPe.Packages.File.LoadFromFile(sMainHood);
                }
                catch
                {
                    Debug.Fail("Cannot open main hood");
                    return;
                }
            }
            else
                MainHood = NBPack;

            LotFamily.Clear();
            IPackedFileDescriptor[] Families;
            Families = MainHood.FindFiles(0x46414D49);    // FAMI - Family Information
            foreach (IPackedFileDescriptor Family in Families)
            {
                R_FAMI ResF = null;
                try
                {
                    ResF = new R_FAMI(MainHood, Family);
                }
                catch
                {
                    Debug.Fail("Problems parsing FAMI - Family Information");
                }
                uint uLotInstance = ResF.LotInstance;
                if (0 == uLotInstance)
                    continue;
                IPackedFileDescriptor PFD = MainHood.FindFile(0x53545223, 0, 0xFFFFFFFF, Family.Instance);
                if (null != PFD)
                {
                    R_STR ResS = null;
                    try
                    {
                        ResS = new R_STR(MainHood, PFD);
                        string sFamilyName = ResS.FindString(0);
                        if (null != sFamilyName)
                            LotFamily.Add(uLotInstance, sFamilyName);
                    }
                    catch
                    {
                        Debug.Fail("Problems parsing FAMI - Family Information");
                    }
                }
            }
        }

        private void LotScreen()
        {
            if (NBPack == null)
            {
                if ((int)this.Tag > 0)
                {
                    Liste.SelectedIndex = (int)this.Tag;
                }
                else
                {
                    this.Tag = Liste.SelectedIndex;
                }
                // if (Liste.SelectedIndex < Liste.Items.Count - 1) // Final option is LotCatalog
                {
                    System.Collections.ArrayList Dateinamen = (System.Collections.ArrayList)Liste.Tag;
                    NBPack = SimPe.Packages.File.LoadFromFile(Dateinamen[Liste.SelectedIndex].ToString());
                }
            }
            if (null == NBPack) // LotCatalog
                // ToDo: Alternatively, create R_DESC from R_LOT for each lot package in the LotCatalog
                //       May be too little benefit for too much work?
                LotDescription = new IPackedFileDescriptor[0];
            else
            {
                GetFamilies();
                LotDescription = NBPack.FindFiles(0x0BF999E7);
            }
            Liste.BeginUpdate();
            Liste.Items.Clear();
            ICollection<uint> uLotInstances = LotFamily.Keys;
            foreach (IPackedFileDescriptor LotDescriptor in LotDescription)
            {
                Lot = new R_DESC(NBPack, LotDescriptor);
                if (uLotInstances.Contains(Lot.Instance))
                {
                    string sFamilyName = LotFamily[Lot.Instance];
                    Lot.FamilyName = sFamilyName;
                }
//              if (((Lot.LotType == 8) && Lot.Occupied) || (Lot.LotType == 9))
//                  continue;   // Cannot modify occupied apartment base or sublot
                Liste.Items.Add(Lot);
                // Print a list of lot names for debugging purposes:
                if (Test_PrintDebugInfo)
                    Debug.Print("Lot{0:D}:   {1}", LotDescriptor.Instance.ToString(), (string)Lot.LotName);
            }
            Lot = null;
            Liste.Sorted = true;
            Liste.EndUpdate();
            Liste.Visible = true;
            LotProperties.Visible = false;
            Explanation.Visible = false;
            Title.Text = RME.GetString("LotTitle");
            AdvancedButton.Visible = true;
            AdvancedButton.Text = RME.GetString("Browse");
            AdvancedFeatures.Visible = false;
            Defaults.Visible = false;
            SunLocation.Visible = false;
            BackButton.Enabled = true;
            BackButton.Text = RME.GetString("Back");
            NextButton.Enabled = false;
            NextButton.Text = RME.GetString("Next");
            if (null == NBPack) // LotCatalog
            {
                Screen = Screen_Lot;
                AdvancedButton_Click(null, null);
                return;
            }
            else if (Liste.Items.Count == 0)
            {
                Title.Text = RME.GetString("NoLotTitle");
                Explanation.Text = RME.GetString("NoLot");
                Explanation.Visible = true;
                Liste.Visible = false;
                AdvancedButton.Visible = false;
                BackButton.Focus();
            }
            else
            {
                Liste.SelectedIndex = 0;
                Liste.Focus();
            }
            this.Width = ScreenWidthStandard;
            Screen = Screen_Lot;
        }
        #endregion //  Lot Screen

        #region Advanced Options

        private void AdvancedForeColor(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
                ((CheckBox)sender).ForeColor = Color.Maroon;
            else
                ((CheckBox)sender).ForeColor = SystemColors.WindowText;
        }

        private void AdvancedMouseHover(object sender, EventArgs e)
        {
            if (sExpl.ContainsKey(sender))
            {
                AdvancedExpl.Text = sExpl[sender];
                AdvancedExpl.ForeColor = Color.Maroon;
            }
        }

        private void AdvancedMouseLeave(object sender, EventArgs e)
        {
            AdvancedExpl.Text = RMF.GetString("AdvancedExpl.Text");
            AdvancedExpl.ForeColor = SystemColors.WindowText;
        }

        private void PaveRoads_CheckedChanged(object sender, EventArgs e)
        {
            if (PaveRoads.Checked)
                KeepStreet.Enabled = false;
            else if (!MoveLot.Checked && !ChangeRoads.Checked && !AllowShrink.Checked && !PaveRoads.Checked)
                KeepStreet.Enabled = true;
        }

        private void KeepStreet_CheckedChanged(object sender, EventArgs e)
        {
            if (KeepStreet.Checked)
            {
                KeepStreet.ForeColor = Color.Maroon;
                LeavePortals.Checked = true;
                LeavePortals.Enabled = false;
                MoveLot.Checked = false;
                MoveLot.Enabled = false;
                BackYard.Value = 0;
                BackYard.Enabled = false;
                BackYard.Minimum = 0;
                LeftYard.Value = 0;
                LeftYard.Enabled = false;
                LeftYard.Minimum = 0;
                RightYard.Value = 0;
                RightYard.Enabled = false;
                RightYard.Minimum = 0;
                AllowShrink.Enabled = false;
                ChangeRoads.Enabled = false;
                PaveRoads.Checked = false;
                PaveRoads.Enabled = false;
                BumpyRoads.Checked = true;
                BumpyRoads.Enabled = false;
                KeepElevation.Checked = true;
                KeepElevation.Enabled = false;
                FrontYard.Value = ((int)FrontYard.Value / iLotTilesPerNeighborhoodTile) * iLotTilesPerNeighborhoodTile;
                FrontYard.Increment = iLotTilesPerNeighborhoodTile;
                ExpandValueChanged(sender, e);
            }
            else
            {
                KeepStreet.ForeColor = SystemColors.WindowText;
                LeavePortals.Checked = false;
                LeavePortals.Enabled = true;
                MoveLot.Checked = false;
                MoveLot.Enabled = true;
                BackYard.Enabled = true;
                LeftYard.Enabled = true;
                RightYard.Enabled = true;
                AllowShrink.Enabled = Allow_Shrink;
                PaveRoads.Checked = false;
                PaveRoads.Enabled = true;
                BumpyRoads.Checked = false;
                BumpyRoads.Enabled = true;
                KeepElevation.Checked = true;
                KeepElevation.Enabled = true;
                ChangeRoads.Enabled = true;
                FrontYard.Increment = 1;
            }
        }

        private void MultiBackup_CheckedChanged(object sender, EventArgs e)
        {
            bBackupVersioning = MultiBackup.Checked;
        }

        #region Enable Shrinking
        private void RemoveShrinking()
        {
            if (null == ValueHandler)
                ValueHandler = new System.EventHandler(this.ExpandValueChanged);
            if (FrontYard.Value < 0)
            {
                FrontYard.ValueChanged -= ValueHandler;
                FrontYard.Value = 0;
                FrontYard.ValueChanged += ValueHandler;
            }
            if (BackYard.Value < 0)
            {
                BackYard.ValueChanged -= ValueHandler;
                BackYard.Value = 0;
                BackYard.ValueChanged += ValueHandler;
            }
            if (LeftYard.Value < 0)
            {
                LeftYard.ValueChanged -= ValueHandler;
                LeftYard.Value = 0;
                LeftYard.ValueChanged += ValueHandler;
            }
            if (RightYard.Value < 0)
            {
                RightYard.ValueChanged -= ValueHandler;
                RightYard.Value = 0;
                RightYard.ValueChanged += ValueHandler;
            }
        }

        private void AllowShrink_CheckStateChanged(object sender, EventArgs e)
        {
            if (AllowShrink.Checked)
            {
                AllowShrink.ForeColor = Color.Maroon;
                KeepStreet.Enabled = false;
                Explanation.Text = RME.GetString("ShrinkExpl");
            }
            else
            {
                AllowShrink.ForeColor = SystemColors.WindowText;
                RemoveShrinking();
                Explanation.Text = RME.GetString("EnlargeExpl");
                if (!MoveLot.Checked && !ChangeRoads.Checked && !AllowShrink.Checked && !PaveRoads.Checked)
                    KeepStreet.Enabled = true;
            }
            ExpandValueChanged(sender, e);
        }
        #endregion

        #region Move Lot
        private void MoveReset_Click(object sender, EventArgs e)
        {
            LabelMoveBack.Text = "Back";
            iMoveBack = 0;
            MoveBack.Text = iMoveBack.ToString();
            LabelMoveLeft.Text = "Left";
            iMoveLeft = 0;
            MoveLeft.Text = iMoveLeft.ToString();
            MoveLot.Focus();
        }

        private void MoveLot_CheckedChanged(object sender, EventArgs e)
        {
            if (MoveLot.Checked)
            {
                MoveLot.ForeColor = Color.Maroon;
                LabelMoveBack.Enabled = true;
                MoveBack.Enabled = true;
                PictureBack.Visible = true;
                PictureForward.Visible = true;
                LabelMoveLeft.Enabled = true;
                MoveLeft.Enabled = true;
                PictureLeft.Visible = true;
                PictureRight.Visible = true;
                MoveResetLabel.Visible = true;
                KeepStreet.Enabled = false;
            }
            else
            {
                MoveLot.ForeColor = SystemColors.WindowText;
                LabelMoveBack.Enabled = false;
                MoveBack.Enabled = false;
                MoveBack.Text = "0";
                iMoveBack = 0;
                PictureBack.Visible = false;
                PictureForward.Visible = false;
                LabelMoveLeft.Enabled = false;
                MoveLeft.Enabled = false;
                MoveLeft.Text = "0";
                iMoveLeft = 0;
                PictureLeft.Visible = false;
                PictureRight.Visible = false;
                MoveResetLabel.Visible = false;
                if (!MoveLot.Checked && !ChangeRoads.Checked && !AllowShrink.Checked && !PaveRoads.Checked)
                    KeepStreet.Enabled = true;
            }
        }

        private void MoveVertical()
        {
            if (iMoveBack < 0)
            {
                LabelMoveBack.Text = "Front";
                int i = 0 - iMoveBack;
                MoveBack.Text = i.ToString();
            }
            else
            {
                LabelMoveBack.Text = "Back";
                MoveBack.Text = iMoveBack.ToString();
            }
        }

        private void PictureBack_Click(object sender, EventArgs e)
        {
            int iMoveBackMax = 0;
            byte bOrientation = Lot.Orientation;
            if (Orientation_Below == bOrientation)
                iMoveBackMax = Lot.Top;
            else if (Orientation_Left == bOrientation)
                iMoveBackMax = iMaxGrid - Lot.Left;
            else if (Orientation_Above == bOrientation)
                iMoveBackMax = iMaxGrid - Lot.Top;
            else if (Orientation_Right == bOrientation)
                iMoveBackMax = Lot.Left;
            iMoveBack++;
            if (iMoveBack < iMoveBackMax)
                PictureForward.Visible = true;
            else
                PictureBack.Visible = false;
            MoveVertical();
        }

        private void PictureForward_Click(object sender, EventArgs e)
        {
            int iMoveBackMin = 0;
            byte bOrientation = Lot.Orientation;
            if (Orientation_Below == bOrientation)
                iMoveBackMin = Lot.Top - iMaxGrid;
            else if (Orientation_Left == bOrientation)
                iMoveBackMin = 0 - Lot.Left;
            else if (Orientation_Above == bOrientation)
                iMoveBackMin = 0 - Lot.Top;
            else if (Orientation_Right == bOrientation)
                iMoveBackMin = Lot.Left - iMaxGrid;
            iMoveBack--;
            if (iMoveBack > iMoveBackMin)
                PictureBack.Visible = true;
            else
                PictureForward.Visible = false;
            MoveVertical();
        }

        private void MoveHorizontal()
        {
            if (iMoveLeft < 0)
            {
                LabelMoveLeft.Text = "Right";
                int i = 0 - iMoveLeft;
                MoveLeft.Text = i.ToString();
            }
            else
            {
                LabelMoveLeft.Text = "Left";
                MoveLeft.Text = iMoveLeft.ToString();
            }
        }

        private void PictureLeft_Click(object sender, EventArgs e)
        {
            int iMoveLeftMax = 0;
            byte bOrientation = Lot.Orientation;
            if (Orientation_Below == bOrientation)
                iMoveLeftMax = Lot.Left;
            else if (Orientation_Left == bOrientation)
                iMoveLeftMax = iMaxGrid - Lot.Top;
            else if (Orientation_Above == bOrientation)
                iMoveLeftMax = iMaxGrid - Lot.Left;
            else if (Orientation_Right == bOrientation)
                iMoveLeftMax = Lot.Top;
            iMoveLeft++;
            if (iMoveLeft < iMoveLeftMax)
                PictureRight.Visible = true;
            else
                PictureLeft.Visible = false;
            MoveHorizontal();
        }

        private void PictureRight_Click(object sender, EventArgs e)
        {
            int iMoveLeftMin = 0;
            byte bOrientation = Lot.Orientation;
            if (Orientation_Below == bOrientation)
                iMoveLeftMin = Lot.Left - iMaxGrid;
            else if (Orientation_Left == bOrientation)
                iMoveLeftMin = 0 - Lot.Top;
            else if (Orientation_Above == bOrientation)
                iMoveLeftMin = 0 - Lot.Left;
            else if (Orientation_Right == bOrientation)
                iMoveLeftMin = Lot.Top - iMaxGrid;
            iMoveLeft--;
            if (iMoveLeft > iMoveLeftMin)
                PictureLeft.Visible = true;
            else
                PictureRight.Visible = false;
            MoveHorizontal();
        }
        #endregion

        #region Add and Remove Roads
        // The Road checkbox has no text and therefore no visual indication that it is selected.
        // So, change the color to give a visual indication.
        private void Road_Enter(object sender, EventArgs e)
        {
            ((CheckBox)sender).BackColor = SystemColors.ButtonShadow;
        }

        // Once the checkbox is no longer selected, change the color back.
        private void Road_Leave(object sender, EventArgs e)
        {
            ((CheckBox)sender).BackColor = SystemColors.Control;
        }

        private void RoadEnableStatus()
        {
            if (!ChangeRoads.Checked)
            {
                LeftRoad.Enabled = false;
                RightRoad.Enabled = false;
                FrontRoad.Enabled = false;
                BackRoad.Enabled = false;
            }
            else if (FrontRoad.Checked)
            {
                LeftRoad.Enabled = true;
                RightRoad.Enabled = true;
                FrontRoad.Enabled = (LeftRoad.Checked || RightRoad.Checked || BackRoad.Checked) ? false : true;
                BackRoad.Enabled = true;
            }
            else
            {
                LeftRoad.Enabled = (LeftRoad.Checked) ? true : false;
                RightRoad.Enabled = (RightRoad.Checked) ? true : false;
                FrontRoad.Enabled = true;
                BackRoad.Enabled = (BackRoad.Checked) ? true : false;
            }
        }

        private void FrontRoad_CheckStateChanged(object sender, EventArgs e)
        {
            int iHeightOld = Int32.Parse(HeightOld.Text);
            int iNumberOfRoadsDeep = ((FrontRoad.Checked) ? 1 : 0) + ((BackRoad.Checked) ? 1 : 0);
            int iMinHeightRoads = (iNumberOfRoadsDeep + 1) * iLotTilesPerNeighborhoodTile;
            if (iHeightOld + FrontYard.Value + BackYard.Value < iMinHeightRoads)
            {
                if (FrontYard.Value < FrontYard.Maximum)
                    FrontYard.Value += iLotTilesPerNeighborhoodTile;
                else if (BackYard.Value < BackYard.Maximum)
                    BackYard.Value += iLotTilesPerNeighborhoodTile;
                else
                    return;
            }
            else
                ExpandValueChanged(sender, e);
            RoadEnableStatus();
        }

        private void BackRoad_CheckStateChanged(object sender, EventArgs e)
        {
            int iHeightOld = Int32.Parse(HeightOld.Text);
            int iNumberOfRoadsDeep = ((FrontRoad.Checked) ? 1 : 0) + ((BackRoad.Checked) ? 1 : 0);
            int iMinHeightRoads = (iNumberOfRoadsDeep + 1) * iLotTilesPerNeighborhoodTile;
            if (iHeightOld + FrontYard.Value + BackYard.Value < iMinHeightRoads)
            {
                if (BackYard.Value < BackYard.Maximum)
                    BackYard.Value += iLotTilesPerNeighborhoodTile;
                else if (FrontYard.Value < FrontYard.Maximum)
                    FrontYard.Value += iLotTilesPerNeighborhoodTile;
                else
                    return;
            }
            else
                ExpandValueChanged(sender, e);
            RoadEnableStatus();
        }

        private void LeftRoad_CheckStateChanged(object sender, EventArgs e)
        {
            int iWidthOld = Int32.Parse(WidthOld.Text);
            int iNumberOfRoadsWide = ((LeftRoad.Checked) ? 1 : 0) + ((RightRoad.Checked) ? 1 : 0);
            int iMinWidthRoads = (iNumberOfRoadsWide + 1) * iLotTilesPerNeighborhoodTile;
            if (iWidthOld + LeftYard.Value + RightYard.Value < iMinWidthRoads)
            {
                if (LeftYard.Value < LeftYard.Maximum)
                    LeftYard.Value += iLotTilesPerNeighborhoodTile;
                else if (RightYard.Value < RightYard.Maximum)
                    RightYard.Value += iLotTilesPerNeighborhoodTile;
                else
                    return;
            }
            else
                ExpandValueChanged(sender, e);
            RoadEnableStatus();
        }

        private void RightRoad_CheckStateChanged(object sender, EventArgs e)
        {
            int iWidthOld = Int32.Parse(WidthOld.Text);
            int iNumberOfRoadsWide = ((LeftRoad.Checked) ? 1 : 0) + ((RightRoad.Checked) ? 1 : 0);
            int iMinWidthRoads = (iNumberOfRoadsWide + 1) * iLotTilesPerNeighborhoodTile;
            if (iWidthOld + LeftYard.Value + RightYard.Value < iMinWidthRoads)
            {
                if (RightYard.Value < RightYard.Maximum)
                    RightYard.Value += iLotTilesPerNeighborhoodTile;
                else if (LeftYard.Value < LeftYard.Maximum)
                    LeftYard.Value += iLotTilesPerNeighborhoodTile;
                else
                    return;
            }
            else
                ExpandValueChanged(sender, e);
            RoadEnableStatus();
        }

        private void ChangeRoads_CheckedChanged(object sender, EventArgs e)
        {
            if (ChangeRoads.Checked)
            {
                ChangeRoads.ForeColor = Color.Maroon;
                RoadEnableStatus();
                KeepStreet.Enabled = false;
            }
            else
            {
                ChangeRoads.ForeColor = SystemColors.WindowText;
                SetRoads();
                RoadEnableStatus();
                if (!MoveLot.Checked && !ChangeRoads.Checked && !AllowShrink.Checked && !PaveRoads.Checked)
                    KeepStreet.Enabled = true;
            }
        }
        #endregion

        private void ClassOverride_CheckedChanged(object sender, EventArgs e)
        {
            uint iClassValue = Lot.LotClassValue;
            ClassValueChange.Value = iClassValue;
            ClassValueDisplay.Text = SimPe.Helper.ToString(iClassValue);

            bool bOverride = ClassOverride.Checked;
            ClassValueChange.Visible = bOverride;
            ClassValueDisplay.Visible = !bOverride;
        }

        private void SetAdvancedDefaults()
        {
            KeepStreet.Checked = false;
            KeepStreet.Enabled = true;
            LeavePortals.Checked = false;
            LeavePortals.Enabled = true;
            MoveLot.Checked = false;
            MoveLot.Enabled = true;
            LabelMoveBack.Enabled = false;
            MoveBack.Enabled = false;
            MoveBack.Text = "0";
            iMoveBack = 0;
            LabelMoveLeft.Enabled = false;
            MoveLeft.Enabled = false;
            MoveLeft.Text = "0";
            iMoveLeft = 0;
            PaveRoads.Checked = false;
            BumpyRoads.Checked = false;
            AllowShrink.Checked = false;
            AllowShrink.Enabled = Allow_Shrink;
            ChangeRoads.Checked = false;
            KeepElevation.Checked = true;
            MatchHoodTerrain.Checked = false;
            RemoveTerrainPaints.Checked = false;
            if (null == Lot)
            {
                ClassValuePanel.Enabled = false;
                RemoveFurniture.Enabled = false;
                RemoveFurniture.Checked = false;
                Hidden.Checked          = false;
                BeachLot.Checked        = false;
            }
            else
            {
                ClassValuePanel.Enabled = Lot.HasClassValue;
                if (Lot.HasClassValue)
                {
                    ClassOverride.Checked = (Lot.LotClassValueOverride != 0) ? true : false;
                    ClassOverride_CheckedChanged(null, null);
                }
                if (Lot.CanRemoveFurniture)
                    RemoveFurniture.Enabled = true;
                else
                    RemoveFurniture.Enabled = false;
                RemoveFurniture.Checked = ((Lot.U0 & U0_NoFurniture) == U0_NoFurniture);
                Hidden.Checked          = ((Lot.U0 & U0_Hidden) == U0_Hidden);
                BeachLot.Checked        = ((Lot.U0 & U0_BeachLot) == U0_BeachLot);
            }
            UpdateHoodTerrain.Checked = false;
            LotEdges.SelectedItem = "Neighborhood";
            MultiBackup.Checked = bBackupVersioning;
            AdvancedMouseLeave(null, null);
            NextButton.Focus();
        }
#endregion

        private void SetExpansionDefaults()
        {
            FrontYard.Value = 0;
            BackYard.Value = 0;
            LeftYard.Value = 0;
            RightYard.Value = 0;
            SetAdvancedDefaults();
        }

        // U0 is a set of flags.
        private uint U0_NoFurniture = 0x08;
        private uint U0_Hidden      = 0x10;
        private uint U0_BeachLot    = 0x80;

        // U10 tells us the locations of the roads on the lot.
        // Add the values together to specify multiple roads.
        // To better visualize, use SimPE to open lot and look at Texture Image (TXTR) terrain pictures
        //    U10=1         U10=2         U10=4         U10=8
        // Road at Left  Road at Top   Road at Right Road at Bottom
        // -----------   -----------   -----------   -----------
        // |R        |   |  ROAD   |   |        R|   |         |
        // |O        |   |         |   |        O|   |         |
        // |A        |   |         |   |        A|   |         |
        // |D        |   |         |   |        D|   |  ROAD   |
        // -----------   -----------   -----------   -----------
        // These should be const, but then the compiler converts them to int and complains
        private byte U10_Left   = 0x01;
        private byte U10_Top    = 0x02;
        private byte U10_Right  = 0x04;
        private byte U10_Bottom = 0x08;

        private void SetRoads()
        {
            byte U10 = Lot.U10;
            FrontRoad.Checked = false;
            LeftRoad.Checked = false;
            BackRoad.Checked = false;
            RightRoad.Checked = false;
            if (U11_Left == U11)
            {
                if ((U10 & U10_Left) == U10_Left)
                    FrontRoad.Checked = true;
                if ((U10 & U10_Top) == U10_Top)
                    LeftRoad.Checked = true;
                if ((U10 & U10_Right) == U10_Right)
                    BackRoad.Checked = true;
                if ((U10 & U10_Bottom) == U10_Bottom)
                    RightRoad.Checked = true;
            }
            else if (U11_Top == U11)
            {
                if ((U10 & U10_Top) == U10_Top)
                    FrontRoad.Checked = true;
                if ((U10 & U10_Right) == U10_Right)
                    LeftRoad.Checked = true;
                if ((U10 & U10_Bottom) == U10_Bottom)
                    BackRoad.Checked = true;
                if ((U10 & U10_Left) == U10_Left)
                    RightRoad.Checked = true;
            }
            else if (U11_Right == U11)
            {
                if ((U10 & U10_Right) == U10_Right)
                    FrontRoad.Checked = true;
                if ((U10 & U10_Bottom) == U10_Bottom)
                    LeftRoad.Checked = true;
                if ((U10 & U10_Left) == U10_Left)
                    BackRoad.Checked = true;
                if ((U10 & U10_Top) == U10_Top)
                    RightRoad.Checked = true;
            }
            else if (U11_Bottom == U11)
            {
                if ((U10 & U10_Bottom) == U10_Bottom)
                    FrontRoad.Checked = true;
                if ((U10 & U10_Left) == U10_Left)
                    LeftRoad.Checked = true;
                if ((U10 & U10_Top) == U10_Top)
                    BackRoad.Checked = true;
                if ((U10 & U10_Right) == U10_Right)
                    RightRoad.Checked = true;
            }
            RoadEnableStatus();
        }

        private void ExpansionScreen()
        {
            if (null == NBPack)
            {
                // Must create an R_DESC for this lot, based on the R_LOT in the lot package
                // Open the lot package
                // Find the R_LOT
                // Pass the R_LOT to the R_DESC constructor
                FinalScreen(/* Not Really! */);
            }
            else if (null == Lot)
            {
/*              foreach (IPackedFileDescriptor LotDescriptor in LotDescription)
                {
                    Lot = new R_DESC(NBPack, LotDescriptor);
                    if ((string)Lot.LotName == (string)Liste.SelectedItem)
                        break;
                }
*/              Lot = (R_DESC)Liste.SelectedItem;
                SetExpansionDefaults();
            }
            Title.Text = Lot.ToString() + ":";
            int iWidth = Lot.Width * iLotTilesPerNeighborhoodTile;
            int iHeight = Lot.Height * iLotTilesPerNeighborhoodTile;
            WidthOld.Text = iWidth.ToString();
            HeightOld.Text = iHeight.ToString();
            U11 = Lot.U11;
            if (0 == (U11 % 2)) // if ((U11_Left == U11) || (U11_Right == U11))
            {
                // Meanings of height and width are swapped
                iWidth = Lot.Height * iLotTilesPerNeighborhoodTile;
                iHeight = Lot.Width * iLotTilesPerNeighborhoodTile;
                WidthOld.Text = iWidth.ToString();
                HeightOld.Text = iHeight.ToString();
            }
            SetRoads();

            if (U11_Left == U11)
                SunLocation.Text = "Sun: Back left";
            else if (U11_Top == U11)
                SunLocation.Text = "Sun: Front left";
            else if (U11_Right == U11)
                SunLocation.Text = "Sun: Front right";
            else if (U11_Bottom == U11)
                SunLocation.Text = "Sun: Back right";
            SunLocation.Visible = true;

            Explanation.Text = RME.GetString("EnlargeExpl");
            Explanation.Visible = true;
            Liste.Visible = false;
            ExpandValueChanged(this, new EventArgs());
            LotProperties.Visible = true;
            LabelSize.Visible = true;
            AdvancedFeatures.Visible = false;
            AdvancedButton.Visible = true;
            Defaults.Visible = false;
            AdvancedButton.Text = RME.GetString("Advanced");
            BackButton.Enabled = true;
            BackButton.Visible = true;
            BackButton.Text = RME.GetString("Back");
            NextButton.Enabled = true;
            NextButton.Text = RME.GetString("Finish");
            NextButton.Focus();
            Screen = Screen_Expansion;
            if (((Lot.LotType == 8) && Lot.Occupied) || (Lot.LotType == 9))
            {
                LotProperties.Enabled = false;
                Explanation.Text = "Unsafe options for occupied apartments have been disabled.";
                AdvancedButton_Click(null, null);
            }
            else
            {
                LotProperties.Enabled = true;
            }
        }

        // Orientation tells us which direction the lot is facing in the neighborhood
        // To visualize, rotate the neighborhood so that the Top=0 and Left=0 are at the Top Left
        // Orient = 0    Orient = 1    Orient = 2    Orient = 3
        // Below Road    Left of Road  Above Road    Right of Road
        // -----------   -----------   -----------   -----------
        // |  FRONT  |   |        F|   |         |   |F        |
        // |         |   |        R|   |         |   |R        |
        // |         |   |        O|   |         |   |O        |
        // |         |   |        N|   |         |   |N        |
        // |         |   |        T|   |  FRONT  |   |T        |
        // -----------   -----------   -----------   -----------
        const byte Orientation_Below = 0x00;
        const byte Orientation_Left  = 0x01;
        const byte Orientation_Above = 0x02;
        const byte Orientation_Right = 0x03;

        // Make sure that the Lot remains in the correct location in the Neighborhood
        private void FixLotInNeighborhood(R_DESC Lot, int iWidthNew, int iHeightNew)
        {
            byte bOrientation = Lot.Orientation;

            // if (Test_PrintDebugInfo || Test_Run)
            {
                Debug.Print("  {0} U10={1} U11={2} Orientation={3} Type={4}", Lot.LotName, Lot.U10, U11, bOrientation, Lot.LotType);
                Debug.Print("  Top={0} Left={1} Height={2} Width={3} Elevation={4}",
                    Lot.Top, Lot.Left, Lot.Height, Lot.Width, Lot.Elevation);
            }

            Lot.Width = iWidthNew / iLotTilesPerNeighborhoodTile;
            Lot.Height = iHeightNew / iLotTilesPerNeighborhoodTile;

            // We want to keep the existing building and lot in the exact same place in the neighborhood.
            if (KeepStreet.Checked)
            {
                // Keep the original road in the original location
                // Note that this won't work correctly if adjustment is not a multiple of 10 tiles.
                if (Orientation_Below == bOrientation)
                    Lot.Top -= (int)FrontYard.Value / iLotTilesPerNeighborhoodTile;
                else if (Orientation_Right == bOrientation)
                    Lot.Left -= (int)FrontYard.Value / iLotTilesPerNeighborhoodTile;
            }
            else
            {
                if (!MoveLot.Checked)
                {
                    // If MoveLot is unchecked, then iMoveBack and iMoveLeft should be 0...
                    Debug.Assert(iMoveBack == 0);
                    iMoveBack = 0;
                    Debug.Assert(iMoveLeft == 0);
                    iMoveLeft = 0;
                }

                // User decides where to put the lot, after any expansion
                // ToDo: Do we need to fix this if amount is not evenly divisible by 10?
                int iFront = (int)FrontYard.Value / iLotTilesPerNeighborhoodTile;
                int iRemainder = (int)FrontYard.Value % iLotTilesPerNeighborhoodTile;
                int iBack = ((int)BackYard.Value + iRemainder) / iLotTilesPerNeighborhoodTile;
                int iLeft = (int)LeftYard.Value / iLotTilesPerNeighborhoodTile;
                iRemainder = (int)LeftYard.Value % iLotTilesPerNeighborhoodTile;
                int iRight = ((int)RightYard.Value + iRemainder) / iLotTilesPerNeighborhoodTile;
                if (Orientation_Below == bOrientation)
                {
                    Lot.Top += (int)iMoveBack;
                    Lot.Left += (int)iMoveLeft - iRight;
                }
                else if (Orientation_Left == bOrientation)
                {
                    Lot.Top += (int)iMoveLeft - iRight;
                    Lot.Left -= (int)iMoveBack + iFront + iBack;
                }
                else if (Orientation_Above == bOrientation)
                {
                    Lot.Top -= (int)iMoveBack + iFront + iBack;
                    Lot.Left -= (int)iMoveLeft + iLeft;
                }
                else if (Orientation_Right == bOrientation)
                {
                    Lot.Top -= (int)iMoveLeft + iLeft;
                    Lot.Left += (int)iMoveBack;
                }
            }
            if (ChangeRoads.Checked)
            {
                byte U10 = 0;
                if (U11_Left == U11)
                {
                    if (FrontRoad.Checked)
                        U10 |= U10_Left;
                    if (LeftRoad.Checked)
                        U10 |= U10_Top;
                    if (BackRoad.Checked)
                        U10 |= U10_Right;
                    if (RightRoad.Checked)
                        U10 |= U10_Bottom;
                }
                else if (U11_Top == U11)
                {
                    if (FrontRoad.Checked)
                        U10 |= U10_Top;
                    if (LeftRoad.Checked)
                        U10 |= U10_Right;
                    if (BackRoad.Checked)
                        U10 |= U10_Bottom;
                    if (RightRoad.Checked)
                        U10 |= U10_Left;
                }
                else if (U11_Right == U11)
                {
                    if (FrontRoad.Checked)
                        U10 |= U10_Right;
                    if (LeftRoad.Checked)
                        U10 |= U10_Bottom;
                    if (BackRoad.Checked)
                        U10 |= U10_Left;
                    if (RightRoad.Checked)
                        U10 |= U10_Top;
                }
                else if (U11_Bottom == U11)
                {
                    if (FrontRoad.Checked)
                        U10 |= U10_Bottom;
                    if (LeftRoad.Checked)
                        U10 |= U10_Left;
                    if (BackRoad.Checked)
                        U10 |= U10_Top;
                    if (RightRoad.Checked)
                        U10 |= U10_Right;
                }
                Lot.U10 = U10;
            }

            if (Lot.HasClassValue)
            {
                if (ClassOverride.Checked)
                    Lot.LotClassValue = (uint)(ClassValueChange.Value);
                else
                    Lot.ClearLotClassValue((uint)(ClassValueChange.Value));
            }

            if (Test_PrintDebugInfo)
            {
                Debug.Print("  {0} U10={1} U11={2} Orientation={3}", Lot.LotName, Lot.U10, U11, bOrientation);
                Debug.Print("  Top={0} Left={1} Height={2} Width={3} Elevation={4}",
                    Lot.Top, Lot.Left, Lot.Height, Lot.Width, Lot.Elevation);
            }
        }

        private int GetAddToLeft()
        {
            // Return the amount to be added to the LEFT of the lot, based on U11 (rotation of lot)
            //
            //              U11=0         U11=1         U11=2         U11=3
            //           -----------   -----------   -----------   -----------
            // ********  |F        |   |  FRONT  |   |        F|   |         |
            // * ADD  *  |R        |   |         |   |        R|   |         |
            // * HERE *  |O        |   |         |   |        O|   |         |
            // ********  |N        |   |         |   |        N|   |         |
            //           |T        |   |         |   |        T|   |  FRONT  |
            //           -----------   -----------   -----------   -----------
            int iValue = 0;

            if (U11_Left == U11)
                iValue = (int)FrontYard.Value;
            else if (U11_Top == U11)
                iValue = (int)RightYard.Value;
            else if (U11_Right == U11)
                iValue = (int)BackYard.Value;
            else if (U11_Bottom == U11)
                iValue = (int)LeftYard.Value;
            else
                Debug.Fail("Unknown U11 value");

            return iValue;
        }

        private int GetAddToBottom()
        {
            // Return the amount to be added to the BOTTOM of the array, based on U11 (rotation of lot)
            //
            //    U11=0         U11=1         U11=2         U11=3
            // -----------   -----------   -----------   -----------
            // |F        |   |  FRONT  |   |        F|   |         |
            // |R        |   |         |   |        R|   |         |
            // |O        |   |         |   |        O|   |         |
            // |N        |   |         |   |        N|   |         |
            // |T        |   |         |   |        T|   |  FRONT  |
            // -----------   -----------   -----------   -----------
            //                      ********
            //                      * ADD  *
            //                      * HERE *
            //                      ********
            int iValue = 0;

            if (U11_Left == U11)
                iValue = (int)RightYard.Value;
            else if (U11_Top == U11)
                iValue = (int)BackYard.Value;
            else if (U11_Right == U11)
                iValue = (int)LeftYard.Value;
            else if (U11_Bottom == U11)
                iValue = (int)FrontYard.Value;
            else
                Debug.Fail("Unknown U11 value");

            return iValue;
        }

        private int GetAddToRight()
        {
            // Return the amount to be added to the RIGHT of the array, based on U11 (rotation of lot)
            //
            //    U11=0         U11=1         U11=2         U11=3
            // -----------   -----------   -----------   -----------
            // |F        |   |  FRONT  |   |        F|   |         | ********
            // |R        |   |         |   |        R|   |         | * ADD  *
            // |O        |   |         |   |        O|   |         | * HERE *
            // |N        |   |         |   |        N|   |         | ********
            // |T        |   |         |   |        T|   |  FRONT  |
            // -----------   -----------   -----------   -----------
            int iValue = 0;

            if (U11_Left == U11)
                iValue = (int)BackYard.Value;
            else if (U11_Top == U11)
                iValue = (int)LeftYard.Value;
            else if (U11_Right == U11)
                iValue = (int)FrontYard.Value;
            else if (U11_Bottom == U11)
                iValue = (int)RightYard.Value;
            else
                Debug.Fail("Unknown U11 value");

            return iValue;
        }

        private int GetAddToTop()
        {
            // Return the amount to be added to the TOP of the array, based on U11 (rotation of lot)
            //
            //                      ********
            //                      * ADD  *
            //                      * HERE *
            //                      ********
            //    U11=0         U11=1         U11=2         U11=3
            // -----------   -----------   -----------   -----------
            // |F        |   |  FRONT  |   |        F|   |         |
            // |R        |   |         |   |        R|   |         |
            // |O        |   |         |   |        O|   |         |
            // |N        |   |         |   |        N|   |         |
            // |T        |   |         |   |        T|   |  FRONT  |
            // -----------   -----------   -----------   -----------
            int iValue = 0;

            if (U11_Left == U11)
                iValue = (int)LeftYard.Value;
            else if (U11_Top == U11)
                iValue = (int)FrontYard.Value;
            else if (U11_Right == U11)
                iValue = (int)RightYard.Value;
            else if (U11_Bottom == U11)
                iValue = (int)BackYard.Value;
            else
                Debug.Fail("Unknown U11 value");

            return iValue;
        }


        // Determine whether record was already handled during OBJT processing
        private bool OBJTHandled(uint[] aPeople, uint uInst)
        {
            bool bPerson = false;
            for (int i = 0; i < aPeople.Length; i++)
            {
                if (aPeople[i] == uInst)
                {
                    bPerson = true;
                    break;
                }
            }
            return (bPerson);
        }

        private byte NextRoad(ref byte U10,
            ref int iWidthLow, ref int iWidthHigh, ref int iHeightLow, ref int iHeightHigh)
        {
            byte U10Log2 = 0xFF;
            if (U10_Left == (U10 & U10_Left))
            {
                iWidthHigh = iLotTilesPerNeighborhoodTile;
                U10Log2 = U11_Left;
                U10 &= (byte)(~U10_Left);
            }
            else if (U10_Top == (U10 & U10_Top))
            {
                iHeightLow = iHeightHigh - iLotTilesPerNeighborhoodTile;
                U10Log2 = U11_Top;
                U10 &= (byte)(~U10_Top);
            }
            else if (U10_Right == (U10 & U10_Right))
            {
                iWidthLow = iWidthHigh - iLotTilesPerNeighborhoodTile;
                U10Log2 = U11_Right;
                U10 &= (byte)(~U10_Right);
            }
            else if (U10_Bottom == (U10 & U10_Bottom))
            {
                iHeightHigh = iLotTilesPerNeighborhoodTile;
                U10Log2 = U11_Bottom;
                U10 &= (byte)(~U10_Bottom);
            }
            return U10Log2;
        }

        private void SkipCorners(byte U10, byte U11,
            ref int iWidthLow, ref int iWidthHigh, ref int iHeightLow, ref int iHeightHigh)
        {
            byte U10Corner = 0;
            if (U11_Left == U11)
            {
                U10Corner = (byte)(U10_Left | U10_Top);
                if ( U10Corner == (U10Corner & U10))
                    iHeightHigh -= iLotTilesPerNeighborhoodTile;
                U10Corner = (byte)(U10_Left | U10_Bottom);
                if (U10Corner == (U10Corner & U10))
                    iHeightLow += iLotTilesPerNeighborhoodTile;
            }
            else if (U11_Top == U11)
            {
                U10Corner = (byte)(U10_Top | U10_Left);
                if (U10Corner == (U10Corner & U10))
                    iWidthLow += iLotTilesPerNeighborhoodTile;
                U10Corner = (byte)(U10_Top | U10_Right);
                if (U10Corner == (U10Corner & U10))
                    iWidthHigh -= iLotTilesPerNeighborhoodTile;
            }
            else if (U11_Right == U11)
            {
                U10Corner = (byte)(U10_Right | U10_Top);
                if (U10Corner == (U10Corner & U10))
                    iHeightLow += iLotTilesPerNeighborhoodTile;
                U10Corner = (byte)(U10_Right | U10_Bottom);
                if (U10Corner == (U10Corner & U10))
                    iHeightHigh -= iLotTilesPerNeighborhoodTile;
            }
            else if (U11_Bottom == U11)
            {
                U10Corner = (byte)(U10_Bottom | U10_Left);
                if (U10Corner == (U10Corner & U10))
                    iWidthHigh -= iLotTilesPerNeighborhoodTile;
                U10Corner = (byte)(U10_Bottom | U10_Right);
                if (U10Corner == (U10Corner & U10))
                    iWidthLow += iLotTilesPerNeighborhoodTile;
            }
        }

        private void Swap(ref int X, ref int Y)
        {
            int iTemp = X;
            X = Y;
            Y = iTemp;
        }

        private void FinalScreen()
        {
            IPackedFileDescriptor VERT_IPFD = null;

            if (!FrontRoad.Checked && (BackRoad.Checked || LeftRoad.Checked || RightRoad.Checked))
            {
                MessageBox.Show(this, "Game requires road at front when there are other roads on the lot.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Screen = Screen_Advanced;
                return;
            }
            bool bAbort = false;
            string sAbort = "Unknown error";
            bool bShrinkAbort = false;
            bool bEmptyAbort = false;
            bool bUserAbort = false;
            int iMinLevel = 0;
            int iMaxLevel = 0;
            int iWidthOld = Int32.Parse(WidthOld.Text);
            int iHeightOld = Int32.Parse(HeightOld.Text);
            int iWidthNew = Int32.Parse(WidthNew.Text);
            int iHeightNew = Int32.Parse(HeightNew.Text);
            if (0 == (U11 % 2)) // if ((U11_Left == U11) || (U11_Right == U11))
            {
                // Meanings of height and width are swapped
                Swap(ref iWidthOld, ref iHeightOld);
                Swap(ref iWidthNew, ref iHeightNew);
            }
            int AddToLeft = GetAddToLeft();
            int AddToBottom = GetAddToBottom();
            int AddToRight = GetAddToRight();
            int AddToTop = GetAddToTop();
            if (Test_Run)
            {
                Debug.Assert((iWidthOld == LETools.MaxLotSize)
                          || (iWidthOld + iLotTilesPerNeighborhoodTile == iWidthNew));
                Debug.Assert((iHeightOld == LETools.MaxLotSize)
                          || (iHeightOld + iLotTilesPerNeighborhoodTile == iHeightNew));
            }

            this.Width = ScreenWidthStandard;
            AdvancedFeatures.Visible = false;
            Defaults.Visible = false;
            AllowShrink.Visible = Allow_Shrink;

            SunLocation.Visible = false;
            int iNumberOfTerrainTextures = 0;

            GeneratableFile LotPack = null;
            R_DESC Lot = null;
#if !DEBUG
            try
#endif
            {
                // Change Neighborhood Package
/*              int iIndex = -1;
                foreach (IPackedFileDescriptor LotDescriptor in LotDescription)
                {
                    Lot = new R_DESC(NBPack, LotDescriptor);
                    iIndex++;
                    if ((string)Lot.LotName == (string)Liste.SelectedItem)
                        break;
                }
 */
                Debug.Assert(-1 != Liste.SelectedIndex);
                Lot = (R_DESC)Liste.SelectedItem;
                Debug.Assert(null != Lot);

                // This logic is unnecessary; just compares the terrain array in DESC with the one in NHTG
                /* {
                    // We need to know the minimum level in the WGRA structure,
                    // to determine the ground layer of the 3D Array.
                    int iH = iHeightOld / iLotTilesPerNeighborhoodTile;
                    int iW = iWidthOld / iLotTilesPerNeighborhoodTile;
                    if (0 == (U11 % 2)) // if ((U11_Left == U11) || (U11_Right == U11))
                        Swap(ref iW, ref iH);  // Meanings of height and width are swapped
                    if (1 == (Lot.Orientation % 2)) // Orientation_Left || Orientation_Right
                        Swap(ref iW, ref iH);  // Meanings of height and width are swapped

                    // Neighborhood Terrain Geometry
                    IPackedFileDescriptor N = NBPack.FindFile(0xABCB5DA4, 0, 0xFFFFFFFF, 0);
                    R_NHTG T = new R_NHTG(NBPack, N);
                    float[,] fHT = T.GetHoodTerrain(Lot.Left, Lot.Top, iW, iH);
                    // float[,] fLT = T.GetLotTerrain(fHT, Lot);
                    Lot.CheckLocalHoodTerrain(fHT);
                } */

                // Get terrain paint from neighborhood.
                IPackedFileDescriptor HoodMem = NBPack.FindFile(0x4E474248, 0, 0xFFFFFFFF, 1);
                R_NGBH ResMem = new R_NGBH(NBPack, HoodMem);
                string sHoodTerrainType = ResMem.TerrainType;
                if (0 == string.Compare(sHoodTerrainType, "concrete", true))
                    sHoodTerrainType = "lottexture-concrete-01";
                else if (0 == string.Compare(sHoodTerrainType, "desert", true))
                    sHoodTerrainType = "lottexture-canvas-desert";
                else if (0 == string.Compare(sHoodTerrainType, "dirt", true))
                    sHoodTerrainType = "lottexture-canvas-dirt";
                else if (0 == string.Compare(sHoodTerrainType, "temperate", true))
                    sHoodTerrainType = "lottexture-test-01";
                else if (MatchHoodTerrain.Checked)
                {
                    MessageBox.Show(this,
                        string.Format("Cannot override base terrain for the lot because\n" +
                                      "'{0}' is not recognized.\n" +
                                      "Option turned off.", sHoodTerrainType),
                        "Match Neighborhood Terrain Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    MatchHoodTerrain.Checked = false;
                }

                // Override lot terrain in lot description
                string sLotTexture = Lot.LotTerrain;
                if (MatchHoodTerrain.Checked)
                    Lot.LotTerrain = sHoodTerrainType;

                // Change Lot Package
                string sDirectory = Path.Combine(Path.GetDirectoryName(NBPack.FileName), "Lots");
                string sFileName = Path.GetFileName(NBPack.FileName);
                int iUnderscore = sFileName.LastIndexOf('_');
                string sPrefix = sFileName.Substring(0, iUnderscore);
                string LotPackage = Path.Combine(sDirectory,
                     sPrefix + "_Lot" + Lot.Instance.ToString() + ".package");
                LotPack = SimPe.Packages.File.LoadFromFile(LotPackage);

                // Determine ground level
                IPackedFileDescriptor PFD = null;
                try
                {
                    // IPackedFileDescriptor WallGraphs = LotPack.FindFiles(0x0A284D0B);
                    // foreach (IPackedFileDescriptor IPFD in WallGraphs)
                    PFD = LotPack.FindFile(0x0A284D0B, 0, 0xFFFFFFFF, 5);
                    R_WGRA Res = new R_WGRA(LotPack, PFD);
                    iMinLevel = Res.MinimumLevel;
                    iMaxLevel = Res.MaximumLevel;
                }
                catch
                {
                    PrimaryForm.ThrowErrorEmptyLot(Lot.LotName);
                }

                IPackedFileDescriptor SMAP = LotPack.FindFile(0xCAC4FC40, 0, 0xFFFFFFFF, 0x0D);
                R_SMAP ResWallMap = new R_SMAP(LotPack, SMAP);

                SMAP = LotPack.FindFile(0xCAC4FC40, 0, 0xFFFFFFFF, 0x0E);
                R_SMAP ResFloorMap = new R_SMAP(LotPack, SMAP);

                /* This logic is unnecessary; just checks the current front road pattern
                if (PaveRoads.Checked)
                {
                    byte U10_Front = 1;
                    if (1 == U11)
                        U10_Front = 2;
                    else if (2 == U11)
                        U10_Front = 4;
                    else if (3 == U11)
                        U10_Front = 8;
                    if ((Lot.U10 & U10_Front) != 0)
                    {
                        PFD = LotPack.FindFile(0x2A51171B, 0, 0xFFFFFFFF, 0);
                        R_3ARY_00 Res3D00 = new R_3ARY_00(LotPack, PFD);
                        Res3D00.BeginUpdate();

                        int iWidthLow = 0;
                        int iWidthHigh = iWidthOld;
                        int iHeightLow = 0;
                        int iHeightHigh = iHeightOld;

                        NextRoad(ref U10_Front,
                            ref iWidthLow, ref iWidthHigh, ref iHeightLow, ref iHeightHigh);

                        ushort[,] RoadTiles = Res3D00.GetRoad(-iMinLevel,
                            iWidthLow, iWidthHigh,
                            iHeightLow, iHeightHigh);

                        for( int i = 0; i < (iWidthHigh - iWidthLow); ++i)
                        {
                            for (int j = 0; j < (iHeightHigh - iHeightLow); ++j)
                            {
                                ushort u = RoadTiles[i, j];
                                if (0 == u)
                                    continue;   // blank space
                                try
                                {
                                    string s = ResFloorMap.RefToString[u];
                                    Debug.Print("SMAP Ref {0} = {1}", u, s);
                                }
                                catch
                                {
                                    Debug.Print("Cannot find SMAP reference {0}", u);
                                }
                            }
                        }
                        Res3D00.EndUpdate(false);
                    }
                }
                */

                // Remove old roads before we lose track of where they were
                if (PaveRoads.Checked)
                {
                    PFD = LotPack.FindFile(0x2A51171B, 0, 0xFFFFFFFF, 0);
                    R_3ARY_00 Res3D00 = new R_3ARY_00(LotPack, PFD);
                    Res3D00.BeginUpdate();

                    // For each road on the lot:
                    for (byte U10 = Lot.U10; U10 > 0; )
                    {
                        int iWidthLow = 0;
                        int iWidthHigh = iWidthOld;
                        int iHeightLow = 0;
                        int iHeightHigh = iHeightOld;

                        byte U10Log2 = NextRoad(ref U10,
                            ref iWidthLow, ref iWidthHigh, ref iHeightLow, ref iHeightHigh);
                        if (0xFF == U10Log2)
                            break;
                        Res3D00.ClearRoad(-iMinLevel, iWidthLow, iWidthHigh, iHeightLow, iHeightHigh);
                    }
                    Res3D00.EndUpdate(true);
                }

                if (RemoveFurniture.Checked)
                    Lot.U0 |= U0_NoFurniture;
                else
                    Lot.U0 &= ~U0_NoFurniture;

                if (Hidden.Checked)
                    Lot.U0 |= U0_Hidden;
                else
                    Lot.U0 &= ~U0_Hidden;

                if (BeachLot.Checked)
                    Lot.U0 |= U0_BeachLot;
                else
                    Lot.U0 &= ~U0_BeachLot;

                FixLotInNeighborhood(Lot, iWidthNew, iHeightNew);

                //Portale
                IPackedFileDescriptor MOBJT = LotPack.FindFile(0x6F626A74, 0, 0xFFFFFFFF, 0);
                if (MOBJT == null)
                    if (LETools.ErrorChecking)
                        throw new InvalidDataException("A lot can only be modified after it has been built on.");
                Portale Ports = new Portale(LotPack, MOBJT);
                if (LeavePortals.Checked)
                    Ports.Clear();
                else
                    Ports.Change(U11, LeftRoad.Checked, RightRoad.Checked, iWidthOld, iHeightOld, iWidthNew, iHeightNew);

                //Ressourcen
                IPackedFileDescriptor[] Descriptor;
                Descriptor = LotPack.Index;
                Progress.Maximum = Descriptor.Length + 13;
                Progress.Value = 0;
                Progress.Visible = true;
                LabelSize.Visible = false;

                uint[] aPeople = { };   // List of OBJT instances which contain cPerson

                #region Handle Lot Records
                foreach (IPackedFileDescriptor IPFD in Descriptor)
                {
                    if (bAbort)
                        break;  // No use continuing if we've decided to abort the process...
                    uint uInst = IPFD.Instance;
                    uint uType = IPFD.Type;
                    switch (uType)
                    {
                        case 0xEBFEE342: // VERS - Version
                            {
                                // Check that Sims 2 version of file
                                // is not greater than the version that we know how to handle.
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = EBFEE342 = VERS - Version  IPFD Instance = {0:X8}", uInst);
                                R_VERS Ver = new R_VERS(LotPack, IPFD, sVersionStrings);
                                if (Ver.VersionNumber > uVersionNumber)
                                {
                                    if (MessageBox.Show(this,
                                        string.Format(RME.GetString("MessageWrongVer"), Ver.VersionString, sVersionStrings[uVersionNumber]),
                                        RME.GetString("TitleWrongVer"),
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2
                                        ) == DialogResult.No)
                                    {
                                        bAbort = true;
                                        bUserAbort = true;
                                    }
                                    else if (MessageBox.Show(this,
                                        string.Format(RME.GetString("MessageCorruptLot"), Ver.VersionString, sVersionStrings[uVersionNumber]),
                                        RME.GetString("TitleCorruptLot"),
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2
                                        ) == DialogResult.No)
                                    {
                                        bAbort = true;
                                        bUserAbort = true;
                                    }
                                }
                                break;
                            }
                        case 0x2A51171B: // 3ARY - 3D Array
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 2A51171B = 3ARY - 3D Array  IPFD Instance = {0:X8}", uInst);
                                switch (uInst)
                                {
                                    case 0x00000000:
                                        {
                                            R_3ARY_00 Res = new R_3ARY_00(LotPack, IPFD);
                                            Debug.Assert(Res.Width == iWidthOld);
                                            Debug.Assert(Res.Height == iHeightOld);
                                            Debug.Assert(Res.Depth <= iMaxLevel - iMinLevel);
                                            Res.Width = iWidthOld + AddToRight;
                                            Res.Height = iHeightOld + AddToTop;
                                            Res.WidthRev = iWidthNew;
                                            Res.HeightRev = iHeightNew;
                                            Debug.Assert(Res.Width == iWidthNew);
                                            Debug.Assert(Res.Height == iHeightNew);
                                            break;
                                        }
                                    case 0x00000001:
                                        {
                                            R_3ARY_01 Res = new R_3ARY_01(LotPack, IPFD);
                                            Debug.Assert(Res.Width == iWidthOld);
                                            Debug.Assert(Res.Height == iHeightOld);
                                            Debug.Assert(Res.Depth <= iMaxLevel - iMinLevel);
                                            Res.Width = iWidthOld + AddToRight;
                                            Res.WidthRev = iWidthNew;
                                            Res.Height = iHeightOld + AddToTop;
                                            Res.HeightRev = iHeightNew;
                                            Debug.Assert(Res.Width == iWidthNew);
                                            Debug.Assert(Res.Height == iHeightNew);
                                            break;
                                        }
                                    case 0x00000003:
                                        {
                                            R_3ARY_03 Res = new R_3ARY_03(LotPack, IPFD);
                                            // Several Maxis-made houses have incorrect sizes
                                            // so these checks are annoying during automated testing.
                                            // Debug.Assert(Res.Width == iWidthOld);
                                            // Debug.Assert(Res.Height == iHeightOld);
                                            Res.Change(iWidthNew, iHeightNew);
                                            Debug.Assert(Res.Width == iWidthNew);
                                            Debug.Assert(Res.Height == iHeightNew);
                                            break;
                                        }
                                    case 0x0000000C:
                                        {
                                            R_3ARY_0C Res = new R_3ARY_0C(LotPack, IPFD);
                                            Debug.Assert(Res.Width == iWidthOld);
                                            Debug.Assert(Res.Height == iHeightOld);
                                            Debug.Assert(Res.Depth <= iMaxLevel - iMinLevel);
                                            Res.Width = iWidthOld + AddToRight;
                                            Res.Height = iHeightOld + AddToTop;
                                            Res.WidthRev = iWidthNew;
                                            Res.HeightRev = iHeightNew;
                                            Debug.Assert(Res.Width == iWidthNew);
                                            Debug.Assert(Res.Height == iHeightNew);
                                            break;
                                        }
                                    case 0x00005d00:
                                        {
                                            // No idea what this is...
                                            R_3ARY Res = new R_3ARY(LotPack, IPFD);
                                            Debug.Assert(Res.Width == 1);
                                            Debug.Assert(Res.Height == 1);
                                            Debug.Assert(Res.Depth != 0);
                                            Debug.Assert(Res.Depth <= iMaxLevel - iMinLevel);
                                            break;
                                        }
                                    default:
                                        {
                                            R_3ARY Res = new R_3ARY(LotPack, IPFD);
                                            Debug.Assert(Res.Width == iWidthOld);
                                            Debug.Assert(Res.Height == iHeightOld);
                                            Debug.Assert(Res.Depth <= iMaxLevel - iMinLevel);
                                            Res.Width = iWidthOld + AddToRight;
                                            Res.Height = iHeightOld + AddToTop;
                                            Res.WidthRev = iWidthNew;
                                            Res.HeightRev = iHeightNew;
                                            Debug.Assert(Res.Width == iWidthNew);
                                            Debug.Assert(Res.Height == iHeightNew);
                                            break;
                                        }
                                }
                                break;
                            }
                        case 0x49FF7D76: // WRLD - World Database
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 49FF7D76 = WRLD - World Database  IPFD Instance = {0:X8}", uInst);
                                R_WRLD Res = new R_WRLD(LotPack, IPFD);
                                Debug.Assert(iMinLevel == Res.MinimumLevel);
                                Debug.Assert(iMaxLevel >= Res.MaximumLevel);
                                Debug.Assert(Res.Width == iWidthOld);
                                Debug.Assert(Res.Height == iHeightOld);
                                Res.Width = iWidthNew;
                                Res.Height = iHeightNew;
                                Debug.Assert(Res.Width == iWidthNew);
                                Debug.Assert(Res.Height == iHeightNew);
                                break;
                            }
                        case 0x1C4A276C: // TXTR - Texture Image
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 1C4A276C = TXTR - Texture Image  IPFD Instance = {0:X8}", uInst);

                                // ToDo: Is this really necessary?  No, it would appear not.
                                // At the very least, would require a rewrite of the tutorial,
                                // which clearly states that the lot will appear to be empty.
                                if ((iWidthNew > iWidthOld) || (iHeightNew > iHeightOld))
                                {
                                    R_TXTR Res = new R_TXTR(LotPack, IPFD);
                                    if (Res.Deletable(uInst))
                                        LotPack.Remove(IPFD);
                                }
                                break;
                            }
                        case 0x6B943B43: // 2ARY - 2D Array | LOTG - Lot Terrain Geometry
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 6B943B43 = 2ARY - 2D Array  IPFD Instance = {0:X8}", uInst);
                                if (0x00003B76 == uInst)
                                {
                                    // Terrain elevations
                                    R_2ARY_3B76 Res = new R_2ARY_3B76(LotPack, IPFD);
                                    Debug.Assert(Res.Width == iWidthOld);
                                    Debug.Assert(Res.Height == iHeightOld);
                                    Res.Width = iWidthOld + AddToRight;
                                    Res.Height = iHeightOld + AddToTop;
                                    Res.WidthRev = iWidthNew;
                                    Res.HeightRev = iHeightNew;
                                    Debug.Assert(Res.Width == iWidthNew);
                                    Debug.Assert(Res.Height == iHeightNew);
                                }
                                else if( 0x00005CEE == uInst)
                                {
                                    // Terrain paint control record
                                    R_2ARY_5CEE Res = new R_2ARY_5CEE(LotPack, IPFD);
                                    Debug.Assert(Res.Width == iWidthOld);
                                    Debug.Assert(Res.Height == iHeightOld);
                                    Res.Width = iWidthOld + AddToRight;
                                    Res.Height = iHeightOld + AddToTop;
                                    Res.WidthRev = iWidthNew;
                                    Res.HeightRev = iHeightNew;
                                    Debug.Assert(Res.Width == iWidthNew);
                                    Debug.Assert(Res.Height == iHeightNew);
                                }
                                else // if ( (0x00005CBC <= uInst) && (uInst <= 0x00005CED) )
                                {
                                    // Terrain paints
                                    R_2ARY Res = new R_2ARY(LotPack, IPFD);
                                    Debug.Assert(Res.Width == iWidthOld);
                                    Debug.Assert(Res.Height == iHeightOld);
                                    Res.Width = iWidthOld + AddToRight;
                                    Res.Height = iHeightOld + AddToTop;
                                    Res.WidthRev = iWidthNew;
                                    Res.HeightRev = iHeightNew;
                                    Debug.Assert(Res.Width == iWidthNew);
                                    Debug.Assert(Res.Height == iHeightNew);
                                }
                                // else // Invalid terrain paint instance; must be removed from 0x5CEE and TMAP?
                                //    LotPack.Remove(IPFD);
                                break;
                            }
                        case 0x6C589723: // Lot
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 6C589723 = Lot  IPFD Instance = {0:X8}", uInst);
                                R_LOT Res = new R_LOT(LotPack, IPFD);
                                Debug.Assert(Res.Width == iWidthOld);
                                Debug.Assert(Res.Height == iHeightOld);
                                Res.Width = iWidthNew;
                                Res.Height = iHeightNew;
                                Debug.Assert(Res.Width == iWidthNew);
                                Debug.Assert(Res.Height == iHeightNew);
                                if (ChangeRoads.Checked)
                                    Res.U10 = Lot.U10;
                                // if (LETools.Corrupt)
                                //     Res.LotDesc = "Lot may have been corrupted using the LotCorrupter.  DO NOT SHARE!";
                                break;
                            }
                        case 0x0A284D0B: // WGRA - Wall Graph
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 0A284D0B = WGRA - Wall Graph  IPFD Instance = {0:X8}", uInst);
                                // Can base game handle these instances?
                                // Debug.Assert(uInst != 0x19);    // empty?
                                // Debug.Assert(uInst != 0x1A);    // empty
                                // Debug.Assert(uInst != 0x1C);
                                R_WGRA Res = new R_WGRA(LotPack, IPFD);
                                Res.Width = iWidthNew;
                                Res.Height = iHeightNew;
                                Res.Change(AddToLeft, AddToRight, AddToBottom, AddToTop, iWidthNew, iHeightNew);
                                break;
                            }
                        case 0x4B58975B: // LTTX aka TMAP - Lot or Terrain Texture Map
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 4B58975B = TMAP - Lot or Terrain Texture Map  IPFD Instance = {0:X8}", uInst);
                                R_TMAP Res = new R_TMAP(LotPack, IPFD);
                                Debug.Assert(Res.Width == iWidthOld);
                                Debug.Assert(Res.Height == iHeightOld);
                                Res.Width = iWidthNew;
                                Res.Height = iHeightNew;
                                Debug.Assert(Res.Width == iWidthNew);
                                Debug.Assert(Res.Height == iHeightNew);
                                iNumberOfTerrainTextures = Res.NumberOfTextures;
                                if (MatchHoodTerrain.Checked)
                                    Res.LotTexture = sHoodTerrainType;
                                break;
                            }
                        case 0x584D544F: // XMTO - Material Object Class Dump
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 584D544F = XMTO - Material Object Class Dump  IPFD Instance = {0:X8}", uInst);

                                R_XMTO Res = new R_XMTO(LotPack, IPFD, iMinLevel, iMaxLevel);
                                Res.Change(AddToLeft, AddToRight, AddToBottom, AddToTop, iWidthNew, iHeightNew);
                                break;
                            }
                        case 0x584F424A: // XOBJ - Object Class Dump
                            {
                                // Changes to portals made in Portale class
                                bool bPortal = Ports.IsPortal(uInst);
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 584F424A = XOBJ - Object Class Dump  IPFD Instance = {0:X8} {1}",
                                        uInst, ((bPortal) ? "Portal" : " "));
                                if (bPortal)
                                    break;  // Already handled during portale processing

                                R_XOBJ Res = new R_XOBJ(LotPack, IPFD, iMinLevel, iMaxLevel);
                                Res.Change(AddToLeft, AddToRight, AddToBottom, AddToTop, iWidthNew, iHeightNew);

                                break;
                            }
                        case 0xFA1C39F7: // OBJT - Singular Lot Object
                            {
                                // Changes to portals made in Portale class
                                bool bPortal = Ports.IsPortal(uInst);
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = FA1C39F7 = OBJT - Singular Lot Object  IPFD Instance = {0:X8} {1}",
                                        uInst, ((bPortal) ? "Portal" : ""));
                                if (bPortal)
                                    break;  // Already handled during portale processing

                                R_OBJT Res = new R_OBJT(LotPack, IPFD);
                                Res.Change(AddToLeft, AddToRight, AddToBottom, AddToTop, iWidthNew, iHeightNew);
                                break;
                            }
                        case 0xAB9406AA: // Roof
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = AB9406AA = Roof  IPFD Instance = {0:X8}", uInst);
                                R_ROOF Res = new R_ROOF(LotPack, IPFD);
                                Res.Change(AddToLeft, AddToRight, AddToBottom, AddToTop, iWidthNew, iHeightNew);
                                break;
                            }
                        case 0xAB4BA572: // FPL - Fence Post Layer
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = AB4BA572 = FPL - Fence Post Layer  IPFD Instance = {0:X8}", uInst);
                                R_FPL Res = new R_FPL(LotPack, IPFD);
                                Res.Change(AddToLeft, AddToRight, AddToBottom, AddToTop, iWidthNew, iHeightNew);
                                break;
                            }
                        case 0x0C900FDB: // POOL - Swimming pool surface?
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 0C900FDB = Swimming Pool Surface?  IPFD Instance = {0:X8}", uInst);
                                R_POOL Res = new R_POOL(LotPack, IPFD);
                                Res.Change(AddToLeft, AddToRight, AddToBottom, AddToTop, iWidthNew, iHeightNew);
                                break;
                            }
                        case 0x50455253: // PERS - Sim Personal Information
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 50455253 = PERS - Sim Personal Information  IPFD Instance = {0:X8}", uInst);
                                if (OBJTHandled(aPeople, uInst))
                                    break;  // Already handled during OBJT processing

                                R_PERS Res = new R_PERS(LotPack, IPFD);
                                Res.Change(AddToLeft, AddToRight, AddToBottom, AddToTop, iWidthNew, iHeightNew);
                                break;
                            }
                        case 0xCB4387A1: // VERT - Vertext
                            {
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = CB4387A1 = VERT - Vertext  IPFD Instance = {0:X8}", uInst);
                                try
                                {
                                    R_VERT Res = new R_VERT(LotPack, IPFD);
                                    Res.Change(AddToLeft, AddToRight, AddToBottom, AddToTop, iWidthNew, iHeightNew, iMinLevel, iMaxLevel);
                                }
                                catch (ShrinkException e)
                                {
                                    // If we are here, the vertex record may be corrupt and require deletion
                                    VERT_IPFD = IPFD;
                                }
                                break;
                            }

#if DEBUG
                        // No additional changes necessary for any of the following, but can be useful for debugging.
                        case 0x3053CF74: // SCOR - Sim Scores
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 3053CF74 = SCOR - Sim Scores  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0x49596978: // TXMT - Textured Material Definition
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 49596978 = TXMT - Textured Material Definition  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0x4F626A4D: // OBJM - Object Material
                            {
                                // Changes to portals used in Portale class
                                // No other changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 4F626A4D = OBJM - Object Material  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0x53494D49: // SIMI - Sim Information
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 53494D49 = SIMI - Sim Information  IPFD Instance = {0:X8}", uInst);
                                // R_SIMI res = new R_SIMI(LotPack, IPFD);
                                break;
                            }
                        case 0x53545223: // STR# - Text String
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 53494D49 = STR# - Text String  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0x6F626A74: // MOBJT - Main Lot Object
                            {
                                // Changes to portals made in Portale class
                                // No other changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 6F626A74 = MOBJT - Main Lot Object  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0x7BA3838C: // GMND - Geometric Node
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 7BA3838C = GMND - Geometric Node  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0x856DDBAC: // IMG - JPG/TGA/PNG Image
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 856DDBAC = IMG - JPG/TGA/PNG Image  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0x8A84D7B0: // WLL - Wall Layer
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 8A84D7B0 = WLL - Wall Layer  IPFD Instance = {0:X8}", uInst);
                                // R_WLL Res = new R_WLL(LotPack, IPFD);
                                // Res.Change(AddToLeft, AddToRight, AddToBottom, AddToTop, iWidthNew, iHeightNew);
                                break;
                            }
                        case 0xAC4F8687: // GMDC - Geometric Data Container
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = AC4F8687 = GMDC - Geometric Data Container  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0xAC506764: // 3D ID Referencing File
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = AC506764 = 3D ID Referencing File  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0xACE46235: // RTEX aka STXR - Road Texture
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = ACE46235 = STXR - Surface Texture  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0xBA353CE1: // TSSG - TSSG System
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = BA353CE1 = TSSG - TSSG System  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0xCAC4FC40: // SMAP - String Map
                            {
                                // Already handled above
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = CAC4FC40 = SMAP - String Map  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0xE519C933: // CRES - Creation Resource Node
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = E519C933 = CRES - Creation Resource Node  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0xE86B1EEF: // DIR - Directory of Compressed Files
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = E86B1EEF = DIR - Directory of Compressed Files  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0xEC44BDDC: // NHVW - Neighborhood View
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = EC44BDDC = NHVW - Neighborhood View  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0x484F5553: // HOUS - House Descriptor
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = 484F5553 = HOUS - House Descriptor  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0xCC364C2A: // SREL - Sim Relations
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = CC364C2A = SREL - Sim Relations  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0xFC6EB1F7: // SHPE - Shape
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = FC6EB1F7 = SHPE - Shape  IPFD Instance = {0:X8}", uInst);
                                break;
                            }
                        case 0x45585069:
                        case 0x47746162:
                        case 0x6E9C59CF:
                        case 0x8B0C79D6:
                        case 0x8DC0278D:
                        case 0x916DA14D:
                        case 0xB21BE28B:
                        case 0xBC66BAEC:
                        case 0xCD8B6498:
                            {
                                // Apartment Life: new records
                                break;
                            }
                        default:
                            {
                                // No changes necessary
                                if (Test_PrintDebugInfo)
                                    Debug.Print("IPFD Type = {0:X8} = Unknown  IPFD Instance = {1:X8}", uType, uInst);
                                break;
                            }
#endif
                    }
                    Progress.Value += 1;
                }
                #endregion

                if (null != VERT_IPFD)
                {
                    // If we are here, then we know that we have a corrupt vertex record
                    // because there are no objects off-lot, but the vertex record says that there are.
                    // The safest thing to do now is to remove the corrupt vertex record.
                    // A corrupt vertex record will not allow you to place objects, even though there is nothing there.
                    // A missing vertex record will allow you to place objects, even though the location is already occupied.
                    // Since this is similar to the "moveobjects on" cheat, it should not be too serious.
                    // A new record will be generated by the game and all new objects will be added correctly;
                    // old objects will be added if they are moved.
                    LotPack.Remove(VERT_IPFD);
                }

                FixTerrainPaints(LotPack, iNumberOfTerrainTextures);

                // Can only change terrain after examine all WGRA instances:
                // we need to know the minimum level in the WGRA structure,
                // so that we can smooth the correct (ground) layer of the 3D Array.
                int iHeightHood = iHeightNew / iLotTilesPerNeighborhoodTile;
                int iWidthHood = iWidthNew / iLotTilesPerNeighborhoodTile;
                if (0 == (U11 % 2)) // if ((U11_Left == U11) || (U11_Right == U11))
                    Swap(ref iWidthHood, ref iHeightHood);  // Meanings of height and width are swapped
                byte bOrientation = Lot.Orientation;
                if (1 == (bOrientation % 2)) // Orientation_Left || Orientation_Right
                    Swap(ref iWidthHood, ref iHeightHood);  // Meanings of height and width are swapped

                // Neighborhood Terrain Geometry
                // Primarily used to determine what expanded lot terrain should look like
                // Hood terrain will be updated if option is chosen
                IPackedFileDescriptor NHTG = NBPack.FindFile(0xABCB5DA4, 0, 0xFFFFFFFF, 0);
                R_NHTG Terrain = new R_NHTG(NBPack, NHTG);
                float[,] fHoodTerrain = Terrain.GetHoodTerrain(Lot.Left, Lot.Top, iWidthHood, iHeightHood);
                if (Test_PrintDebugInfo)
                {
                    Debug.Print("Hood Terrain");
                    for (int j = 0; j <= iHeightHood; j++)
                    {
                        string s = string.Format("{0}:  ", j);
                        for (int i = 0; i <= iWidthHood; i++)
                        {
                            s = string.Format("{0}  {1:000.0000}", s, fHoodTerrain[i, j]);
                        }
                        Debug.Print(s);
                    }
                    Debug.Print("");
                }
                Progress.Value += 1;

                // Determine neighborhood elevation at the road:
                // ToDo: May want to take an average of several points
                // ToDo: Keep should be default unless Move
                float fHoodRoadElevation = Lot.Elevation;
                if (KeepElevation.Checked)
                { }
                else if (Orientation_Below == bOrientation)
                    fHoodRoadElevation = fHoodTerrain[iWidthHood / 2, 0];
                else if (Orientation_Left == bOrientation)
                    fHoodRoadElevation = fHoodTerrain[iWidthHood, iHeightHood / 2];
                else if (Orientation_Above == bOrientation)
                    fHoodRoadElevation = fHoodTerrain[iWidthHood / 2, iHeightHood];
                else if (Orientation_Right == bOrientation)
                    fHoodRoadElevation = fHoodTerrain[0, iHeightHood / 2];
                else
                    Debug.Fail("Unable to get neighborhood elevation");
                // Debug.Assert(fHoodRoadElevation == Lot.Elevation);
                Progress.Value += 1;

                // Flatten the road(s) in the neighborhood
                if (!BumpyRoads.Checked)
                {
                    // For each road on the lot:
                    for (byte U10 = Lot.U10; U10 > 0; )
                    {
                        int x = 0;  // We don't need the width and height values from NextRoad
                        byte U10Log2 = NextRoad(ref U10, ref x, ref x, ref x, ref x);
                        if (0xFF == U10Log2)
                            break;
                        int iRotation = (4 + bOrientation - U11 + U10Log2) % 4;

                        int iWidthLow = 0;
                        int iWidthHigh = iWidthHood;
                        int iHeightLow = 0;
                        int iHeightHigh = iHeightHood;

                        if (0 == iRotation)
                            iHeightHigh = 1;
                        else if (1 == iRotation)
                            iWidthLow = iWidthHood - 1;
                        else if (2 == iRotation)
                            iHeightLow = iHeightHood - 1;
                        else if (3 == iRotation)
                            iWidthHigh = 1;
                        for (int i = iWidthLow; i <= iWidthHigh; i++)
                        {
                            for (int j = iHeightLow; j <= iHeightHigh; j++)
                            {
                                // Debug.Assert(fHoodTerrain[i, j] == fHoodRoadElevation);
                                fHoodTerrain[i, j] = fHoodRoadElevation;
                            }
                        }
                    }
                    if (Test_PrintDebugInfo)
                    {
                        Debug.Print("Hood Terrain with flat roads");
                        for (int j = 0; j <= iHeightHood; j++)
                        {
                            string s = string.Format("{0}:  ", j);
                            for (int i = 0; i <= iWidthHood; i++)
                            {
                                s = string.Format("{0}  {1:000.0000}", s, fHoodTerrain[i, j]);
                            }
                            Debug.Print(s);
                        }
                    }
                }
                Progress.Value += 1;

                PFD = LotPack.FindFile(0x2A51171B, 0, 0xFFFFFFFF, 1);
                R_3ARY_01 Res3D = new R_3ARY_01(LotPack, PFD);
                Res3D.BeginUpdate();
                Progress.Value += 1;

                // Determine lot elevation at the road:
                float fLotRoadElevation = 0;
                if (0 == Lot.U10)   // No road on the lot
                { }
                else if (U11_Left == U11)
                    fLotRoadElevation = Res3D.Elevation(-iMinLevel, 0, iHeightNew / 2);
                else if (U11_Top == U11)
                    fLotRoadElevation = Res3D.Elevation(-iMinLevel, iWidthNew / 2, iHeightNew);
                else if (U11_Right == U11)
                    fLotRoadElevation = Res3D.Elevation(-iMinLevel, iWidthNew, iHeightNew / 2);
                else if (U11_Bottom == U11)
                    fLotRoadElevation = Res3D.Elevation(-iMinLevel, iWidthNew / 2, 0);
                else
                    Debug.Fail("Unable to get lot elevation");
                if (0 != fLotRoadElevation)
                {
                    // ToDo: need better error message
                    if (MessageBox.Show(this, RME.GetString("MessageRoadElevation"), RME.GetString("TitleRoadElevation"),
                        MessageBoxButtons.YesNo, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2
                        ) == DialogResult.No)
                    {
                        bAbort = true;
                        bUserAbort = true;
                    }
                }
                // fLotRoadElevation = 0;
                Progress.Value += 1;

                if (!KeepElevation.Checked)
                    Lot.Elevation = fHoodRoadElevation - fLotRoadElevation;

                // Now that we've got the correct lot elevation,
                // we can get the neighborhood terrain, sized and rotated for the lot
                // and relative to the lot elevation
                float[,] fLocalHoodTerrain = Lot.GetLocalHoodTerrain(fHoodTerrain);

                PFD = LotPack.FindFile(0x6B943B43, 0, 0xFFFFFFFF, 0x3B76);
                R_2ARY_3B76 Res2D = new R_2ARY_3B76(LotPack, PFD);
                Res2D.BeginUpdate();
                Progress.Value += 1;

                // These values are all relative to the lot elevation:
                float fLandElevation = fLotRoadElevation;   // Standard ground level is road?
                float fWaterElevation = -0.5F;              // Water is .5 underneath the ground
                float fWaterTable = Terrain.WaterTable - Lot.Elevation;

                // Replace the expanded lot terrain with the neighborhood terrain
                // Note that there is some duplication of effort here, but the logic is less complex.
                // Otherwise, we would have to do the corners separately.
                if (AddToLeft > 0)
                    Res3D.ReplaceTerrain(-iMinLevel, fLocalHoodTerrain, 0, AddToLeft, 0, iHeightNew, ref Res2D, fWaterElevation);
                if (AddToRight > 0)
                    Res3D.ReplaceTerrain(-iMinLevel, fLocalHoodTerrain, iWidthNew - AddToRight, iWidthNew, 0, iHeightNew, ref Res2D, fWaterElevation);
                if (AddToBottom > 0)
                    Res3D.ReplaceTerrain(-iMinLevel, fLocalHoodTerrain, 0, iWidthNew, 0, AddToBottom, ref Res2D, fWaterElevation);
                if (AddToTop > 0)
                    Res3D.ReplaceTerrain(-iMinLevel, fLocalHoodTerrain, 0, iWidthNew, iHeightNew - AddToTop, iHeightNew, ref Res2D, fWaterElevation);
                Progress.Value += 1;

                // Compare terrain (Res3D) and water (Res2D)
                /* {
                    float[, ,] fT = Res3D.GetArray();
                    float[,] fW = Res2D.GetArray();
                    int iMismatch = 0;

                    for (int i = 0; i <= iWidthNew; i++)
                    {
                        for (int j = 0; j <= iHeightNew; j++)
                        {
                            if (! LETools.FloatEqual( fT[-iMinLevel, i, j], (fW[j, i] - fWaterElevation)))
                            {
                                Debug.Print("Terrain[{0}, {1}] {2} - {3} = {4} != .5F",
                                    i, j, fT[-iMinLevel, i, j], (fW[j, i]), fT[-iMinLevel, i, j] - (fW[j, i]));
                                iMismatch++;
                            }
                        }
                    }
                    Debug.Print("{0}", iMismatch);
                } */


                // Adjust the lot edges
                if ("Flatten" == (string)(LotEdges.SelectedItem))
                {
                    Res3D.FlattenEdges(-iMinLevel, fLandElevation);
                    Res2D.FlattenEdges(fWaterTable, fLandElevation + fWaterElevation);
                }
                else if ("Neighborhood" == (string)(LotEdges.SelectedItem))
                {
                    Res3D.ReplaceEdges(-iMinLevel, fLocalHoodTerrain);
                    Res2D.ReplaceEdges(fWaterTable, fWaterElevation, fLocalHoodTerrain);
                }
                Progress.Value += 1;

                // Pave the road(s) on the lot
                if (PaveRoads.Checked)
                {
                    PFD = LotPack.FindFile(0x2A51171B, 0, 0xFFFFFFFF, 0);
                    R_3ARY_00 Res3D00 = new R_3ARY_00(LotPack, PFD);
                    Res3D00.BeginUpdate();

                    // For each road on the lot:
                    for (byte U10 = Lot.U10; U10 > 0; )
                    {
                        int iWidthLow = 0;
                        int iWidthHigh = iWidthNew;
                        int iHeightLow = 0;
                        int iHeightHigh = iHeightNew;

                        byte U10Log2 = NextRoad(ref U10,
                            ref iWidthLow, ref iWidthHigh, ref iHeightLow, ref iHeightHigh);
                        if (0xFF == U10Log2)
                            break;

                        ushort[] RoadTiles = new ushort[iLotTilesPerNeighborhoodTile];
                        int iRoadDirection = (U10Log2 % 2);
                        for (int i = 0; i < iLotTilesPerNeighborhoodTile; i++)
                        {
                            string s = RoadStrings[iRoadDirection][i];
                            if( 0 == s.Length)
                                RoadTiles[i] = 0;   // blank tiles between sidewalk and road
                            else
                                RoadTiles[i] = ResFloorMap.FindString(s);
                        }

                        // For now, ignore corners
                        SkipCorners(Lot.U10, U10Log2,
                            ref iWidthLow, ref iWidthHigh, ref iHeightLow, ref iHeightHigh);

                        if (0 == iRoadDirection)
                        {
                            for (int j = iHeightLow; j < iHeightHigh; j++)
                            {
                                Res3D00.PutRoad(-iMinLevel, RoadTiles, iWidthLow, iWidthHigh, j, j + 1);
                            }
                        }
                        else
                        {
                            for (int j = iWidthLow; j < iWidthHigh; j++)
                            {
                                Res3D00.PutRoad(-iMinLevel, RoadTiles, j, j + 1, iHeightLow, iHeightHigh);
                            }
                        }
                    }
                    Res3D00.EndUpdate(true);
                }
                Progress.Value += 1;

                // Flatten the road(s) on the lot
                if (!BumpyRoads.Checked)
                {
                    // For each road on the lot:
                    for (byte U10 = Lot.U10; U10 > 0; )
                    {
                        int iWidthLow = 0;
                        int iWidthHigh = iWidthNew;
                        int iHeightLow = 0;
                        int iHeightHigh = iHeightNew;

                        byte U10Log2 = NextRoad(ref U10,
                            ref iWidthLow, ref iWidthHigh, ref iHeightLow, ref iHeightHigh);
                        if (0xFF == U10Log2)
                            break;
                        Res3D.FlattenTerrain(-iMinLevel, fLandElevation, iWidthLow, iWidthHigh, iHeightLow, iHeightHigh);
                        Res2D.FlattenTerrain(fWaterTable, fLandElevation + fWaterElevation, iWidthLow, iWidthHigh, iHeightLow, iHeightHigh);
                    }
                    // Change the elevation of the portals
                    Ports.ChangeElevation(fLotRoadElevation);
                }
                Progress.Value += 1;

                if (UpdateHoodTerrain.Checked)
                {
                    // Fix the neighborhood terrain array
                    // But first, we need to adjust it with the new lot edges
                    Res3D.FixHoodTerrain(-iMinLevel, fLocalHoodTerrain);
                    // Now we need to rotate back counterclockwise.
                    int iRotateBack = (4 + Lot.Orientation - Lot.U11) % 4;
                    fHoodTerrain = Lot.Rotate(fLocalHoodTerrain, iWidthHood + 1, iHeightHood + 1, iRotateBack, Lot.Elevation);
                    Terrain.ReplaceHoodTerrain(fHoodTerrain, Lot.Left, Lot.Top, iWidthHood, iHeightHood);

                    // This logic is unnecessary; just compares the terrain array in DESC with the one in NHTG
                    /* {
                        // We need to know the minimum level in the WGRA structure,
                        // to determine the ground layer of the 3D Array.

                        // Neighborhood Terrain Geometry
                        IPackedFileDescriptor N = NBPack.FindFile(0xABCB5DA4, 0, 0xFFFFFFFF, 0);
                        R_NHTG T = new R_NHTG(NBPack, N);
                        float[,] fHT = T.GetHoodTerrain(Lot.Left, Lot.Top, iWidthHood, iHeightHood);
                        // float[,] fLT = T.GetLotTerrain(fHT, Lot);
                        Lot.CheckLotTerrain(fHT);
                    } */
                    /* 
                    // Replace the local hood terrain in the DESC record:
                    Lot.ReplaceLocalHoodTerrain(fLocalHoodTerrain);
                    // After fixing the lot terrain, the lot record is no longer valid
                    Lot = null;
                     */
                }

                // Smooth the lot edges
                Res3D.SmoothEdges(-iMinLevel);
                Res2D.SmoothEdges();
                Progress.Value += 1;

                Res3D.EndUpdate();
                Res2D.EndUpdate();
                Progress.Value += 1;
            }
#if !DEBUG
            catch (ShrinkException e)
            {
                bAbort = true;
                bShrinkAbort = true;
                sAbort = e.Message;
            }
            catch (EmptyLotException e)
            {
                bAbort = true;
                bEmptyAbort = true;
                sAbort = e.Message;
            }
            catch( Exception e)
            {
                // There was an unhandled exception.
                // Prevent unsavvy users from continuing.
                bAbort = true;
                sAbort = e.Message;
            }
#endif

            if (Test_PrintDebugInfo)
                Debug.Print("No more records");
            // if (LETools.Corrupt)
            //     Lot.LotDesc = "Lot may have been corrupted using the LotCorrupter.  DO NOT SHARE!";

            if (bAbort || Test_AlwaysAbort)
            {
                // Lot expansion is transactional:
                // Do not change the Neighborhood file if the Lot file is not changed.
                LotPack.ForgetUpdate();
                LotPack.Close();
                if (!Test_Run)
                    LotPack = null;

                NBPack.ForgetUpdate();
                NBPack.Close();
                if (!Test_Run)
                    NBPack = null;

                if (bUserAbort)
                {
                    Title.Text = RME.GetString("Abort");
                    Explanation.Text = RME.GetString("ExplainUserAbort");
                }
                else if (bEmptyAbort)
                {
                    Title.Text = "Empty Lot";
                    Explanation.Text = sAbort;
                }
                else if (bShrinkAbort)
                {
                    Title.Text = "Area Not Empty";
                    Explanation.Text = sAbort;
                }
                else
                {
                    // Will setting focus ensure that active form is not null? No.
                    /*
                    this.Width = ScreenWidthExplain;
                    SunLocation.Visible = false;
                    Title.Text = sAbort;
                    LongExpl.Visible = true;
                    LongExpl.BringToFront();
                    LongExpl.Text = RME.GetString("ExplainAbort");
                     */
                    Explanation.Text = RME.GetString("ExplainAbort");
                }
                Title.ForeColor = Color.Red;
                NextButton.Enabled = false;
            }
            else
            {
                string sExt = ".bkp";
                uint uMaxBKP = 0;
                if (bBackupVersioning)
                {
                    uMaxBKP = MaxBKP(NBPack.FileName, sExt, uMaxBKP);
                    uMaxBKP = MaxBKP(LotPack.FileName, sExt, uMaxBKP);
                }
                PackageSave(LotPack, sExt, bBackupVersioning, uMaxBKP);
                PackageSave(NBPack, sExt, bBackupVersioning, uMaxBKP);

                // Title.Text = RME.GetString("Erfolg");
                if (LETools.Corrupt)
                    Explanation.Text = RME.GetString("Corrupted");
                else if ((AddToLeft == 0) && (AddToBottom == 0) && (AddToRight == 0) && (AddToTop == 0)
                       && (iMoveBack == 0) && (iMoveLeft == 0))
                    Explanation.Text = RME.GetString("FinalExplUnlock");
                else if (KeepStreet.Checked)
                    Explanation.Text = RME.GetString("FinalExplNoMove");
                else
                {
                    /*
                    this.Width = ScreenWidthExplain;
                    SunLocation.Visible = false;
                    LongExpl.Visible = true;
                    LongExpl.BringToFront();
                    LongExpl.Text = RME.GetString("FinalExplWithMove");
                     */
                    Explanation.Text = RME.GetString("FinalExplWithMove");

                }
            }
            LotProperties.Visible = false;
            Progress.Visible = false;
            AdvancedButton.Visible = false;
            NextButton.Visible = true;
            NextButton.Text = RME.GetString("Restart");
            BackButton.Visible = true;
            BackButton.Text = RMF.GetString("BackButton.Text");
            BackButton.Focus();
            Screen = Screen_Final;
        }

        private bool TerrainPaintsAreValid(GeneratableFile LotPack)
        {
            IPackedFileDescriptor pTerrainNames = LotPack.FindFile(0x4B58975B, 0, 0xFFFFFFFF, 0);
            R_TMAP rTerrainNames = new R_TMAP(LotPack, pTerrainNames);
            int iNumberOfTerrainTextures = rTerrainNames.NumberOfTextures;
            string[] sTerrainNames = rTerrainNames.GetTerrainNames();
            foreach (string s in sTerrainNames)
            {
                if ((s == null) || (s == ""))
                    return false;
            }

            IPackedFileDescriptor pControl = LotPack.FindFile(0x6B943B43, 0, 0xFFFFFFFF, 0x5CEE);
            R_2ARY_5CEE rControl = new R_2ARY_5CEE(LotPack, pControl);
            int[] iControlUsage = rControl.GetUsage(iNumberOfTerrainTextures);
            if (iControlUsage.Length != iNumberOfTerrainTextures)
                return false;

            int[] iTerrainUsage = new int[iNumberOfTerrainTextures];
            IPackedFileDescriptor[] pTerrains = LotPack.FindFiles(0x6B943B43);
            foreach (IPackedFileDescriptor pTerrain in pTerrains)
            {
                uint iTerrainInstance = pTerrain.Instance;
                int i = (int)(iTerrainInstance - 0x5CBC);

                if (0x3B76 == iTerrainInstance)
                    continue;   // Ignore water elevation array.

                if (0x5CEE == iTerrainInstance)
                    continue;   // Ignore terrain paint control record.

                R_2ARY rTerrain = new R_2ARY(LotPack, pTerrain);
                if (i >= iNumberOfTerrainTextures)
                    return false;
                iTerrainUsage[i] = rTerrain.Usage;
            }

            for (int i = 0; i < iNumberOfTerrainTextures; i++)
            {
                if (i == (0x5CEE - 0x5CBC + 1))
                    continue;   // Ignore terrain paint control record.
                if (iTerrainUsage[i] != iControlUsage[i])
                    return false;
            }
            return true;
        }

        private void FixTerrainPaints(GeneratableFile LotPack, int iNumberOfTerrainTextures)
        {
            if (RemoveTerrainPaints.Checked)
            { /* no need for further tests */ }
            else if (iNumberOfTerrainTextures <= (0x5CEE - 0x5CBC))
                return;  // No need to remove unused terrain paints
            else if (MessageBox.Show(this,
                RME.GetString("MessageTooManyTerrainPaints"), RME.GetString("TitleTooManyTerrainPaints"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question
                ) == DialogResult.No)
                return;  // User doesn't want to remove unused terrain paints

            Debug.Assert(TerrainPaintsAreValid(LotPack));

            // Remove unused terrain paints.
            IPackedFileDescriptor pTerrainNames = LotPack.FindFile(0x4B58975B, 0, 0xFFFFFFFF, 0);
            R_TMAP rTerrainNames = new R_TMAP(LotPack, pTerrainNames);

            IPackedFileDescriptor pControl = LotPack.FindFile(0x6B943B43, 0, 0xFFFFFFFF, 0x5CEE);
            R_2ARY_5CEE rControl = new R_2ARY_5CEE(LotPack, pControl);

            // The first 4 LOTG records are required, regardless of whether they are unused.
            // Do in reverse order so that we can delete them and keep the indexes in sync.
            for (uint i = (uint)(iNumberOfTerrainTextures - 1); i > 3; i--)
            {
                uint iTerrainInstance = 0x5CBC + (uint)i;

                int iPaintFound = 0;
                int iNameFound = 0;
                if (0x5CEE == iTerrainInstance)
                {
                    // We don't want to delete the control record,
                    // but we want to stop using it.
                    Debug.Print("Remove LOTG {0} instance {1:X8}", i, iTerrainInstance);
                    iPaintFound = rControl.RemoveTerrainPaint((byte)i);
                    Debug.Assert(iPaintFound == 0);
                    iNameFound = rTerrainNames.RemoveTerrainPaint(i);
                    Debug.Assert(iNameFound == 1);
                    continue;
                }

                IPackedFileDescriptor pTerrain = LotPack.FindFile(0x6B943B43, 0, 0xFFFFFFFF, iTerrainInstance);
                if (null == pTerrain)
                {
                    Debug.Fail(string.Format("Missing LOTG {0} instance {1:X8}", i, iTerrainInstance));
                    continue;
                }

                R_2ARY rTerrain = new R_2ARY(LotPack, pTerrain);
                if (rTerrain.Usage > 0)
                    continue;

                Debug.Print("Remove LOTG {0} instance {1:X8}", i, iTerrainInstance);
                iPaintFound = rControl.RemoveTerrainPaint((byte)i);
                Debug.Assert(iPaintFound == 0);
                iNameFound = rTerrainNames.RemoveTerrainPaint(i);
                Debug.Assert(iNameFound == 1);
                LotPack.Remove(pTerrain);
            }
            // Renumber remaining LOTG records (except 0x5CEE)
            Queue<uint> Available = new Queue<uint>();
            for (uint iInstance = 0x5CC0; iInstance < 0x5CBC + iNumberOfTerrainTextures; iInstance++)
            {
                if (0x5CEE == iInstance)
                    continue;   // Skip the control record

                IPackedFileDescriptor pTerrain = LotPack.FindFile(0x6B943B43, 0, 0xFFFFFFFF, iInstance);
                if (null == pTerrain)
                {
                    Available.Enqueue(iInstance);
                    continue;
                }
                try
                {
                    uint iNextInstance = Available.Dequeue();
                    Available.Enqueue(iInstance);
                    pTerrain.Instance = iNextInstance;
                }
                catch
                {
                    // Don't bother to renumber unless there are instances free.
                }
            }
            Debug.Assert(TerrainPaintsAreValid(LotPack));
            if (rTerrainNames.NumberOfTextures <= (0x5CEE - 0x5CBC))
                return;
            MessageBox.Show(this,
                RME.GetString("MessageStillTooManyTerrainPaints"), RME.GetString("TitleTooManyTerrainPaints"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static string[] RoadStringsEven = new string[iLotTilesPerNeighborhoodTile]
        {
            "sidewalk",
            "",
            "road_sidelines_01",
            "road_asphalt",
            "road_whitelines_01",
            "road_whitelines_03",
            "road_asphalt",
            "road_sidelines_03",
            "",
            "sidewalk"
        };
        private static string[] RoadStringsOdd = new string[iLotTilesPerNeighborhoodTile]
        {
            "sidewalk",
            "",
            "road_sidelines_00",
            "road_asphalt",
            "road_whitelines_00",
            "road_whitelines_02",
            "road_asphalt",
            "road_sidelines_02",
            "",
            "sidewalk"
        };
        private string[][] RoadStrings = new string[2][]
        {
            RoadStringsEven,
            RoadStringsOdd
        };

        private uint MaxBKP(string sPath, string sExt, uint uMaxBKP)
        {
            string sDir = Path.GetDirectoryName(sPath);
            string sName = Path.GetFileNameWithoutExtension(sPath);

            string sBKP = string.Concat(sName, "_*");
            sBKP = Path.ChangeExtension(sBKP, sExt);

            // Find unique package file number for backup
            string[] sNames = System.IO.Directory.GetFiles(sDir, sBKP);
            for (int i = 0; i < sNames.Length; i++)
            {
                string s1 = Path.GetFileNameWithoutExtension(sNames[i]);
                string s2 = s1.Substring(sName.Length + 1);
                uint uCurrent = 0;
                try
                {
                    uCurrent = uint.Parse(s2);
                }
                catch(FormatException)
                {
                    continue;
                }
                if (uMaxBKP <= uCurrent)
                    uMaxBKP = uCurrent + 1;
            }
            return uMaxBKP;
        }

        private void ExpandValueChanged(object sender, EventArgs e)
        {
            int iWidthOld = Int32.Parse(WidthOld.Text);
            int iHeightOld = Int32.Parse(HeightOld.Text);
            int iWidthNew = iWidthOld;
            int iHeightNew = iHeightOld;
            int iWidthMax = LETools.MaxLotSize - iWidthOld;
            int iHeightMax = LETools.MaxLotSize - iHeightOld;

            int iNumberOfRoadsWide = ((LeftRoad.Checked) ? 1 : 0) + ((RightRoad.Checked) ? 1 : 0);
            int iMinWidthRoads = (iNumberOfRoadsWide + 1) * iLotTilesPerNeighborhoodTile;

            int iNumberOfRoadsDeep = ((FrontRoad.Checked) ? 1 : 0) + ((BackRoad.Checked) ? 1 : 0);
            int iMinHeightRoads = (iNumberOfRoadsDeep + 1) * iLotTilesPerNeighborhoodTile;

            // Lot must be a integer number of neighborhood tiles
            bool bCorrectSize = true;
            if (((int)(FrontYard.Value + BackYard.Value) % iLotTilesPerNeighborhoodTile) == 0)
                HeightNew.ForeColor = SystemColors.WindowText;
            else
            {
                HeightNew.ForeColor = Color.Red;
                bCorrectSize = false;
            }
            if (((int)(LeftYard.Value + RightYard.Value) % iLotTilesPerNeighborhoodTile) == 0)
                WidthNew.ForeColor = SystemColors.WindowText;
            else
            {
                WidthNew.ForeColor = Color.Red;
                bCorrectSize = false;
            }
            if (!bCorrectSize)
            {
                SizeError.Text = "Final sizes must be a multiple of ten (10)";
                SizeError.ForeColor = Color.Red;
            }
            else if (KeepStreet.Checked)
            {
                SizeError.Text = "Expansion over the road must be in multiples of ten (10)";
                SizeError.ForeColor = SystemColors.WindowText;
            }
            else
                SizeError.Text = "";

            if (ChangeRoads.Checked)
            {
                if (iHeightOld + FrontYard.Value + BackYard.Value < iMinHeightRoads)
                    NextButton.Enabled = false;
                else if (iWidthOld + LeftYard.Value + RightYard.Value < iMinWidthRoads)
                    NextButton.Enabled = false;
                else if (!bCorrectSize)
                    NextButton.Enabled = false;
                else
                    NextButton.Enabled = true;
            }
            else if (!bCorrectSize)
                NextButton.Enabled = false;
            else
                NextButton.Enabled = true;

            int iWidthMin = (AllowShrink.Checked) ? iMinWidthRoads - iWidthOld : 0;
            int iHeightMin = (AllowShrink.Checked) ? iMinHeightRoads - iHeightOld : 0;

            iWidthNew += (int)RightYard.Value + (int)LeftYard.Value;
            iHeightNew += (int)FrontYard.Value + (int)BackYard.Value;

            RightYard.Maximum = iWidthMax - LeftYard.Value;
            LeftYard.Maximum = iWidthMax - RightYard.Value;
            FrontYard.Maximum = iHeightMax - BackYard.Value;
            BackYard.Maximum = iHeightMax - FrontYard.Value;

            RightYard.Minimum = (AllowShrink.Checked) ? iWidthMin - LeftYard.Value : 0;
            if (RightYard.Minimum > 0)
                RightYard.Minimum = 0;
            LeftYard.Minimum = (AllowShrink.Checked) ? iWidthMin - RightYard.Value : 0;
            if (LeftYard.Minimum > 0)
                LeftYard.Minimum = 0;
            FrontYard.Minimum = (AllowShrink.Checked) ? iHeightMin - BackYard.Value : 0;
            if (FrontYard.Minimum > 0)
                FrontYard.Minimum = 0;
            BackYard.Minimum = (AllowShrink.Checked) ? iHeightMin - FrontYard.Value : 0;
            if (BackYard.Minimum > 0)
                BackYard.Minimum = 0;

            WidthNew.Text = iWidthNew.ToString();
            HeightNew.Text = iHeightNew.ToString();
        }

        private string BackupFileName(GeneratableFile Package, string sExt, bool bVersion, uint uVersion)
        {
            string sPath = Package.FileName;
            if (bVersion)
            {
                string sDir = Path.GetDirectoryName(sPath);
                string sName = Path.GetFileNameWithoutExtension(sPath);

                sPath = string.Concat(sDir, Path.DirectorySeparatorChar, sName);
                sPath = string.Concat(sPath, "_", uVersion.ToString());
                sPath = Path.ChangeExtension(sPath, sExt);  // add in backup extension
            }
            else
                sPath = Path.ChangeExtension(sPath, sExt);
            return sPath;
        }

        private void PackageSave(GeneratableFile Package, string sExt, bool bVersion, uint uVersion)
        {
            // This method exists only because SimPE GeneratableFile.Save() does not work here!
            string sPath = BackupFileName(Package, sExt, bVersion, uVersion);
            System.IO.File.Copy(Package.FileName, sPath, true);
            MemoryStream MS = Package.Build();
            Package.Close();
            FileStream FS = new FileStream(Package.FileName, FileMode.Create, FileAccess.ReadWrite);
            FS.Seek(0, SeekOrigin.Begin);
            FS.SetLength(0);
            byte[] B = MS.ToArray();
            FS.Write(B, 0, B.Length);
            FS.Close();
        }

        private void LotExpander_Load(object sender, EventArgs e)
        {
            Explanation.Text = string.Format(RMF.GetString("Explanation.Text"), sVersionStrings[uVersionNumber]);
            LotEdges.SelectedItem = "Neighborhood";

            // Remove the original default trace listener.
            // Debug.Listeners.RemoveAt(0);

            // Create a listener that outputs to the console screen, and add it to the debug listeners.
            // TextWriterTraceListener myWriter = new TextWriterTraceListener(System.Console.Out);
            // Debug.Listeners.Add(myWriter);

            try
            {
                sBackupConfigFile = Application.ExecutablePath;
                sBackupConfigFile = Path.GetDirectoryName(sBackupConfigFile);
                sBackupConfigFile = string.Concat(sBackupConfigFile, Path.DirectorySeparatorChar, "LABKPVER.TXT");
                bBackupVersioning = System.IO.File.Exists(sBackupConfigFile);
            }
            catch
            {
            }

            // Run the program independently (without user input).
            if (Test_Run)
                RunTests();
        }

        private void LotExpander_Shown(object sender, EventArgs e)
        {
            // Depending upon people's settings, the screen widths need to be more flexible.
            ScreenWidthStandard = AdvancedFeatures.Location.X + 15;
            ScreenWidthExplain  = LongExpl.Location.X + LongExpl.Size.Width + 10;
            ScreenWidthAdvanced = AdvancedFeatures.Location.X + AdvancedFeatures.Size.Width + 10;
            this.Width = ScreenWidthStandard;
            if (LETools.Corrupt)
                InitialScreen();
            NextButton.Focus();
            if (null == ValueHandler)
                ValueHandler = new System.EventHandler(this.ExpandValueChanged);
            this.LeftYard.ValueChanged += ValueHandler;
            this.RightYard.ValueChanged += ValueHandler;
            this.BackYard.ValueChanged += ValueHandler;
            this.FrontYard.ValueChanged += ValueHandler;
        }

        private void LotExpander_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (null != sBackupConfigFile)
            {
                try
                {
                    if (!bBackupVersioning)
                        System.IO.File.Delete(sBackupConfigFile);
                    else if (!System.IO.File.Exists(sBackupConfigFile))
                        System.IO.File.Create(sBackupConfigFile);
                }
                catch
                {
                }
            }
        }

        private void Test_Setup(int iWidthOld, int iHeightOld)
        {
            bool bShrink = false;
            bool bExpand = true;

            if (0 == (U11 % 2)) // if ((U11_Left == U11) || (U11_Right == U11))
                Swap(ref iWidthOld, ref iHeightOld);    // Meanings of height and width are swapped
            WidthOld.Text = iWidthOld.ToString();
            HeightOld.Text = iHeightOld.ToString();

            int iWidthNew = 0;
            int iHeightNew = 0;

            if (bShrink)
            {
                // Shrinking one in each direction maximizes code coverage
                LeftYard.Value = (iWidthOld == 1 * iLotTilesPerNeighborhoodTile) ? 0 : -1 * iLotTilesPerNeighborhoodTile;
                RightYard.Value = (iWidthOld <= 2 * iLotTilesPerNeighborhoodTile) ? 0 : -1 * iLotTilesPerNeighborhoodTile;
                FrontYard.Value = (iHeightOld == 1 * iLotTilesPerNeighborhoodTile) ? 0 : -1 * iLotTilesPerNeighborhoodTile;
                BackYard.Value = (iHeightOld <= 2 * iLotTilesPerNeighborhoodTile) ? 0 : -1 * iLotTilesPerNeighborhoodTile;
            }
            else if (bExpand)
            {
                // Expanding one in each direction maximizes code coverage
                LeftYard.Value = (iWidthOld == LETools.MaxLotSize) ? 0 : 4;
                RightYard.Value = (iWidthOld == LETools.MaxLotSize) ? 0 : 6;
                FrontYard.Value = (iHeightOld == LETools.MaxLotSize) ? 0 : 3;
                BackYard.Value = (iHeightOld == LETools.MaxLotSize) ? 0 : 7;
            }

            iWidthNew = iWidthOld + (int)LeftYard.Value + (int)RightYard.Value;
            iHeightNew = iHeightOld + (int)FrontYard.Value + (int)BackYard.Value;

            WidthNew.Text = iWidthNew.ToString();
            HeightNew.Text = iHeightNew.ToString();

            LotEdges.SelectedItem = "Neighborhood";
        }

        // Run Tests: Run the program independently (without user input).
        private void RunTests()
        {
            // Allow us to change Front Back Left Right without resetting Maximum...
            if (null == ValueHandler)
                ValueHandler = new System.EventHandler(this.ExpandValueChanged);
            this.FrontYard.ValueChanged -= ValueHandler;
            this.BackYard.ValueChanged -= ValueHandler;
            this.LeftYard.ValueChanged -= ValueHandler;
            this.RightYard.ValueChanged -= ValueHandler;

            string sPath =
                Path.Combine(Path.Combine(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EA Games"),
                Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Software\\EA Games\\The Sims 2").GetValue("DisplayName").ToString()),
                "Neighborhoods");

            // Select neighborhood directory, or use default
            // sPath = "C:\\My Documents\\EA Games\\The Sims 2\\Neighborhoods";

            // Select specific tests, or set to null for all
            string sTestOnlyNeighborhood = 
                null;
                // "C:\\My Documents\\EA Games\\The Sims 2\\Neighborhoods\\N001";
            string sTestOnlySubNeighborhood =
                null;
                // "C:\\My Documents\\EA Games\\The Sims 2\\Neighborhoods\\N001\\N001_Neighborhood.package";
            string sTestOnlyLot =
                null;
                // "165 Sim Lane";

            if (NBPack != null)
            {
                NBPack.Close(true);
            }


//          string[] dirs = { "E*", "F*", "G*", "N*" };
//          string[] dirs = { "A*", "B*", "D*", "E*", "F*", "G*", "M*", "N*", "T*", "U*" };
//          string[] dirs = { "????" };
            string[] dirs = { "*" };
            for (int h = 0; h < dirs.Length; h++)
            {
                // For every neighborhood
                string[] NBOrdner = Directory.GetDirectories(sPath, dirs[h]);
                Array.Sort(NBOrdner);
                for (int i = 0; i < NBOrdner.Length; i++)
                {
                    string sHoodName = Path.GetFileName(NBOrdner[i]);
                    if (0 == string.Compare("Tutorial", sHoodName))
                        continue;

                    // Test only one neighborhood?
                    if ((sTestOnlyNeighborhood != null) && (sTestOnlyNeighborhood != NBOrdner[i]))
                        continue;

                    // For every subneighborhood
                    string[] AlleNBinOrdner = Directory.GetFiles(NBOrdner[i], "*.package");
                    Debug.Print(NBOrdner[i]);
                    for (int j = 0; j < AlleNBinOrdner.Length; j++)
                    {
                        // Test only one subneighborhood?
                        if ((sTestOnlySubNeighborhood != null) && (sTestOnlySubNeighborhood != AlleNBinOrdner[j]))
                            continue;
                        
                        NBPack = SimPe.Packages.File.LoadFromFile(AlleNBinOrdner[j]);
                        Debug.Print(AlleNBinOrdner[j]);
                        GetFamilies();
                        LotDescription = NBPack.FindFiles(0x0BF999E7);

                        // Skip hidden and empty neighborhoods, like Pets and Weather (Seasons)
                        if (LotDescription.Length == 0)
                            continue;

                        // For every lot
                        ICollection<uint> uLotInstances = LotFamily.Keys;
                        foreach (IPackedFileDescriptor LotDescriptor in LotDescription)
                        {
                            R_DESC Lot = new R_DESC(NBPack, LotDescriptor);

                            // Skip empty lots
                            string sDirectory = Path.Combine(Path.GetDirectoryName(NBPack.FileName), "Lots");
                            string sFileName = Path.GetFileName(NBPack.FileName);
                            int iUnderscore = sFileName.LastIndexOf('_');
                            string sPrefix = sFileName.Substring(0, iUnderscore);
                            string LotPackage = Path.Combine( sDirectory,
                                sPrefix + "_Lot" + LotDescriptor.Instance.ToString() + ".package");
                            GeneratableFile LotPack = SimPe.Packages.File.LoadFromFile(LotPackage);
                            IPackedFileDescriptor MOBJT = LotPack.FindFile(0x6F626A74, 0, 0xFFFFFFFF, 0);
                            if (MOBJT == null)
                                continue;

                            if (uLotInstances.Contains(Lot.Instance))
                            {
                                string sFamilyName = LotFamily[Lot.Instance];
                                Lot.FamilyName = sFamilyName;
                            }

                            U11 = Lot.U11;
                            int iWidthOld = Lot.Width * iLotTilesPerNeighborhoodTile;
                            int iHeightOld = Lot.Height * iLotTilesPerNeighborhoodTile;

                            Liste.BeginUpdate();
                            Liste.Items.Clear();
                            string sTestLotDescriptor = Lot.LotName;
                            Liste.Items.Add(Lot);
                            Liste.SelectedItem = Lot;
                            Liste.EndUpdate();
                            Lot = null;

                            // Test only one lot?
                            if ((sTestOnlyLot != null) && (sTestOnlyLot != sTestLotDescriptor))
                                continue;
                            Debug.Print("Lot{0:D} {1}", LotDescriptor.Instance.ToString(), sTestLotDescriptor);

                            // Try to expand lots on all sides
                            Test_Setup(iWidthOld, iHeightOld);
                            FinalScreen();
                        }
                    }
                }
            }
            this.Close();
            // Application.Exit();
        }
    }
}


