namespace LotExpander
{
    partial class PrimaryForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrimaryForm));
            this.Title = new System.Windows.Forms.Label();
            this.Liste = new System.Windows.Forms.ListBox();
            this.NextButton = new System.Windows.Forms.Button();
            this.BackButton = new System.Windows.Forms.Button();
            this.LotProperties = new System.Windows.Forms.GroupBox();
            this.LabelRoad = new System.Windows.Forms.Label();
            this.LabelLeft = new System.Windows.Forms.Label();
            this.LeftYard = new System.Windows.Forms.NumericUpDown();
            this.LeftRoad = new System.Windows.Forms.CheckBox();
            this.LabelRight = new System.Windows.Forms.Label();
            this.RightYard = new System.Windows.Forms.NumericUpDown();
            this.RightRoad = new System.Windows.Forms.CheckBox();
            this.LabelFront = new System.Windows.Forms.Label();
            this.FrontYard = new System.Windows.Forms.NumericUpDown();
            this.FrontRoad = new System.Windows.Forms.CheckBox();
            this.LabelBack = new System.Windows.Forms.Label();
            this.BackYard = new System.Windows.Forms.NumericUpDown();
            this.BackRoad = new System.Windows.Forms.CheckBox();
            this.LabelWidth = new System.Windows.Forms.Label();
            this.LabelMax = new System.Windows.Forms.Label();
            this.WidthMax = new System.Windows.Forms.Label();
            this.LabelX0 = new System.Windows.Forms.Label();
            this.HeightMax = new System.Windows.Forms.Label();
            this.LabelOld = new System.Windows.Forms.Label();
            this.WidthOld = new System.Windows.Forms.Label();
            this.LabelX1 = new System.Windows.Forms.Label();
            this.HeightOld = new System.Windows.Forms.Label();
            this.LabelNew = new System.Windows.Forms.Label();
            this.WidthNew = new System.Windows.Forms.Label();
            this.LabelX2 = new System.Windows.Forms.Label();
            this.HeightNew = new System.Windows.Forms.Label();
            this.SizeError = new System.Windows.Forms.TextBox();
            this.LabelSize = new System.Windows.Forms.Label();
            this.Progress = new System.Windows.Forms.ProgressBar();
            this.KeepStreet = new System.Windows.Forms.CheckBox();
            this.Explanation = new System.Windows.Forms.TextBox();
            this.PictureLogo = new System.Windows.Forms.PictureBox();
            this.AdvancedButton = new System.Windows.Forms.Button();
            this.AdvancedFeatures = new System.Windows.Forms.GroupBox();
            this.MoveLot = new System.Windows.Forms.CheckBox();
            this.LabelMoveLeft = new System.Windows.Forms.Label();
            this.MoveLeft = new System.Windows.Forms.Label();
            this.LabelMoveBack = new System.Windows.Forms.Label();
            this.MoveBack = new System.Windows.Forms.Label();
            this.PictureBack = new System.Windows.Forms.PictureBox();
            this.PictureLeft = new System.Windows.Forms.PictureBox();
            this.MoveResetLabel = new System.Windows.Forms.Label();
            this.MoveReset = new System.Windows.Forms.TextBox();
            this.PictureRight = new System.Windows.Forms.PictureBox();
            this.PictureForward = new System.Windows.Forms.PictureBox();
            this.ChangeRoads = new System.Windows.Forms.CheckBox();
            this.AllowShrink = new System.Windows.Forms.CheckBox();
            this.RemoveFurniture = new System.Windows.Forms.CheckBox();
            this.Hidden = new System.Windows.Forms.CheckBox();
            this.BeachLot = new System.Windows.Forms.CheckBox();
            this.RemoveTerrainPaints = new System.Windows.Forms.CheckBox();
            this.MatchHoodTerrain = new System.Windows.Forms.CheckBox();
            this.LeavePortals = new System.Windows.Forms.CheckBox();
            this.PaveRoads = new System.Windows.Forms.CheckBox();
            this.BumpyRoads = new System.Windows.Forms.CheckBox();
            this.KeepElevation = new System.Windows.Forms.CheckBox();
            this.UpdateHoodTerrain = new System.Windows.Forms.CheckBox();
            this.LabelEdges = new System.Windows.Forms.Label();
            this.LotEdges = new System.Windows.Forms.ComboBox();
            this.ClassValuePanel = new System.Windows.Forms.Panel();
            this.ClassOverride = new System.Windows.Forms.CheckBox();
            this.ClassValueDisplay = new System.Windows.Forms.Label();
            this.ClassValueChange = new System.Windows.Forms.NumericUpDown();
            this.MultiBackup = new System.Windows.Forms.CheckBox();
            this.AdvancedExpl = new System.Windows.Forms.TextBox();
            this.LongExpl = new System.Windows.Forms.TextBox();
            this.Defaults = new System.Windows.Forms.Button();
            this.SunLocation = new System.Windows.Forms.TextBox();
            this.LotProperties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LeftYard)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RightYard)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FrontYard)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BackYard)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureLogo)).BeginInit();
            this.AdvancedFeatures.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureRight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureForward)).BeginInit();
            this.ClassValuePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ClassValueChange)).BeginInit();
            this.SuspendLayout();
            // 
            // Title
            // 
            resources.ApplyResources(this.Title, "Title");
            this.Title.Name = "Title";
            // 
            // Liste
            // 
            resources.ApplyResources(this.Liste, "Liste");
            this.Liste.FormattingEnabled = true;
            this.Liste.Name = "Liste";
            this.Liste.DoubleClick += new System.EventHandler(this.Liste_DoubleClick);
            this.Liste.SelectedIndexChanged += new System.EventHandler(this.Liste_SelectedIndexChanged);
            this.Liste.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Liste_KeyDown);
            // 
            // NextButton
            // 
            resources.ApplyResources(this.NextButton, "NextButton");
            this.NextButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.NextButton.Name = "NextButton";
            this.NextButton.UseVisualStyleBackColor = true;
            this.NextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // BackButton
            // 
            resources.ApplyResources(this.BackButton, "BackButton");
            this.BackButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BackButton.Name = "BackButton";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // LotProperties
            // 
            resources.ApplyResources(this.LotProperties, "LotProperties");
            this.LotProperties.Controls.Add(this.LabelRoad);
            this.LotProperties.Controls.Add(this.LabelLeft);
            this.LotProperties.Controls.Add(this.LeftYard);
            this.LotProperties.Controls.Add(this.LeftRoad);
            this.LotProperties.Controls.Add(this.LabelRight);
            this.LotProperties.Controls.Add(this.RightYard);
            this.LotProperties.Controls.Add(this.RightRoad);
            this.LotProperties.Controls.Add(this.LabelFront);
            this.LotProperties.Controls.Add(this.FrontYard);
            this.LotProperties.Controls.Add(this.FrontRoad);
            this.LotProperties.Controls.Add(this.LabelBack);
            this.LotProperties.Controls.Add(this.BackYard);
            this.LotProperties.Controls.Add(this.BackRoad);
            this.LotProperties.Controls.Add(this.LabelWidth);
            this.LotProperties.Controls.Add(this.LabelMax);
            this.LotProperties.Controls.Add(this.WidthMax);
            this.LotProperties.Controls.Add(this.LabelX0);
            this.LotProperties.Controls.Add(this.HeightMax);
            this.LotProperties.Controls.Add(this.LabelOld);
            this.LotProperties.Controls.Add(this.WidthOld);
            this.LotProperties.Controls.Add(this.LabelX1);
            this.LotProperties.Controls.Add(this.HeightOld);
            this.LotProperties.Controls.Add(this.LabelNew);
            this.LotProperties.Controls.Add(this.WidthNew);
            this.LotProperties.Controls.Add(this.LabelX2);
            this.LotProperties.Controls.Add(this.HeightNew);
            this.LotProperties.Controls.Add(this.SizeError);
            this.LotProperties.Controls.Add(this.LabelSize);
            this.LotProperties.Controls.Add(this.Progress);
            this.LotProperties.Name = "LotProperties";
            this.LotProperties.TabStop = false;
            // 
            // LabelRoad
            // 
            resources.ApplyResources(this.LabelRoad, "LabelRoad");
            this.LabelRoad.Name = "LabelRoad";
            // 
            // LabelLeft
            // 
            resources.ApplyResources(this.LabelLeft, "LabelLeft");
            this.LabelLeft.Name = "LabelLeft";
            // 
            // LeftYard
            // 
            resources.ApplyResources(this.LeftYard, "LeftYard");
            this.LeftYard.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.LeftYard.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            -2147483648});
            this.LeftYard.Name = "LeftYard";
            // 
            // LeftRoad
            // 
            resources.ApplyResources(this.LeftRoad, "LeftRoad");
            this.LeftRoad.Name = "LeftRoad";
            this.LeftRoad.UseVisualStyleBackColor = true;
            this.LeftRoad.Enter += new System.EventHandler(this.Road_Enter);
            this.LeftRoad.Leave += new System.EventHandler(this.Road_Leave);
            this.LeftRoad.CheckStateChanged += new System.EventHandler(this.LeftRoad_CheckStateChanged);
            // 
            // LabelRight
            // 
            resources.ApplyResources(this.LabelRight, "LabelRight");
            this.LabelRight.Name = "LabelRight";
            // 
            // RightYard
            // 
            resources.ApplyResources(this.RightYard, "RightYard");
            this.RightYard.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.RightYard.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            -2147483648});
            this.RightYard.Name = "RightYard";
            // 
            // RightRoad
            // 
            resources.ApplyResources(this.RightRoad, "RightRoad");
            this.RightRoad.Name = "RightRoad";
            this.RightRoad.UseVisualStyleBackColor = true;
            this.RightRoad.Enter += new System.EventHandler(this.Road_Enter);
            this.RightRoad.Leave += new System.EventHandler(this.Road_Leave);
            this.RightRoad.CheckStateChanged += new System.EventHandler(this.RightRoad_CheckStateChanged);
            // 
            // LabelFront
            // 
            resources.ApplyResources(this.LabelFront, "LabelFront");
            this.LabelFront.Name = "LabelFront";
            // 
            // FrontYard
            // 
            resources.ApplyResources(this.FrontYard, "FrontYard");
            this.FrontYard.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.FrontYard.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            -2147483648});
            this.FrontYard.Name = "FrontYard";
            // 
            // FrontRoad
            // 
            resources.ApplyResources(this.FrontRoad, "FrontRoad");
            this.FrontRoad.Name = "FrontRoad";
            this.FrontRoad.UseVisualStyleBackColor = true;
            this.FrontRoad.Enter += new System.EventHandler(this.Road_Enter);
            this.FrontRoad.Leave += new System.EventHandler(this.Road_Leave);
            this.FrontRoad.CheckStateChanged += new System.EventHandler(this.FrontRoad_CheckStateChanged);
            // 
            // LabelBack
            // 
            resources.ApplyResources(this.LabelBack, "LabelBack");
            this.LabelBack.Name = "LabelBack";
            // 
            // BackYard
            // 
            resources.ApplyResources(this.BackYard, "BackYard");
            this.BackYard.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.BackYard.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            -2147483648});
            this.BackYard.Name = "BackYard";
            // 
            // BackRoad
            // 
            resources.ApplyResources(this.BackRoad, "BackRoad");
            this.BackRoad.Name = "BackRoad";
            this.BackRoad.UseVisualStyleBackColor = true;
            this.BackRoad.Enter += new System.EventHandler(this.Road_Enter);
            this.BackRoad.Leave += new System.EventHandler(this.Road_Leave);
            this.BackRoad.CheckStateChanged += new System.EventHandler(this.BackRoad_CheckStateChanged);
            // 
            // LabelWidth
            // 
            resources.ApplyResources(this.LabelWidth, "LabelWidth");
            this.LabelWidth.Name = "LabelWidth";
            // 
            // LabelMax
            // 
            resources.ApplyResources(this.LabelMax, "LabelMax");
            this.LabelMax.Name = "LabelMax";
            // 
            // WidthMax
            // 
            resources.ApplyResources(this.WidthMax, "WidthMax");
            this.WidthMax.Name = "WidthMax";
            // 
            // LabelX0
            // 
            resources.ApplyResources(this.LabelX0, "LabelX0");
            this.LabelX0.Name = "LabelX0";
            // 
            // HeightMax
            // 
            resources.ApplyResources(this.HeightMax, "HeightMax");
            this.HeightMax.Name = "HeightMax";
            // 
            // LabelOld
            // 
            resources.ApplyResources(this.LabelOld, "LabelOld");
            this.LabelOld.Name = "LabelOld";
            // 
            // WidthOld
            // 
            resources.ApplyResources(this.WidthOld, "WidthOld");
            this.WidthOld.Name = "WidthOld";
            // 
            // LabelX1
            // 
            resources.ApplyResources(this.LabelX1, "LabelX1");
            this.LabelX1.Name = "LabelX1";
            // 
            // HeightOld
            // 
            resources.ApplyResources(this.HeightOld, "HeightOld");
            this.HeightOld.Name = "HeightOld";
            // 
            // LabelNew
            // 
            resources.ApplyResources(this.LabelNew, "LabelNew");
            this.LabelNew.Name = "LabelNew";
            // 
            // WidthNew
            // 
            resources.ApplyResources(this.WidthNew, "WidthNew");
            this.WidthNew.Name = "WidthNew";
            // 
            // LabelX2
            // 
            resources.ApplyResources(this.LabelX2, "LabelX2");
            this.LabelX2.Name = "LabelX2";
            // 
            // HeightNew
            // 
            resources.ApplyResources(this.HeightNew, "HeightNew");
            this.HeightNew.Name = "HeightNew";
            // 
            // SizeError
            // 
            this.SizeError.BackColor = System.Drawing.SystemColors.Control;
            this.SizeError.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SizeError.Cursor = System.Windows.Forms.Cursors.No;
            this.SizeError.ForeColor = System.Drawing.SystemColors.WindowText;
            resources.ApplyResources(this.SizeError, "SizeError");
            this.SizeError.Name = "SizeError";
            this.SizeError.ReadOnly = true;
            this.SizeError.TabStop = false;
            // 
            // LabelSize
            // 
            resources.ApplyResources(this.LabelSize, "LabelSize");
            this.LabelSize.Name = "LabelSize";
            // 
            // Progress
            // 
            resources.ApplyResources(this.Progress, "Progress");
            this.Progress.Name = "Progress";
            this.Progress.Step = 1;
            // 
            // KeepStreet
            // 
            resources.ApplyResources(this.KeepStreet, "KeepStreet");
            this.KeepStreet.Name = "KeepStreet";
            this.KeepStreet.UseVisualStyleBackColor = true;
            this.KeepStreet.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.KeepStreet.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.KeepStreet.CheckedChanged += new System.EventHandler(this.KeepStreet_CheckedChanged);
            this.KeepStreet.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // Explanation
            // 
            resources.ApplyResources(this.Explanation, "Explanation");
            this.Explanation.BackColor = System.Drawing.SystemColors.Control;
            this.Explanation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Explanation.Name = "Explanation";
            this.Explanation.ReadOnly = true;
            this.Explanation.TabStop = false;
            // 
            // PictureLogo
            // 
            this.PictureLogo.Image = global::LotExpander.Properties.Resources.LotAdjuster;
            resources.ApplyResources(this.PictureLogo, "PictureLogo");
            this.PictureLogo.Name = "PictureLogo";
            this.PictureLogo.TabStop = false;
            // 
            // AdvancedButton
            // 
            resources.ApplyResources(this.AdvancedButton, "AdvancedButton");
            this.AdvancedButton.Name = "AdvancedButton";
            this.AdvancedButton.UseVisualStyleBackColor = true;
            this.AdvancedButton.Click += new System.EventHandler(this.AdvancedButton_Click);
            // 
            // AdvancedFeatures
            // 
            resources.ApplyResources(this.AdvancedFeatures, "AdvancedFeatures");
            this.AdvancedFeatures.Controls.Add(this.KeepStreet);
            this.AdvancedFeatures.Controls.Add(this.MoveLot);
            this.AdvancedFeatures.Controls.Add(this.LabelMoveLeft);
            this.AdvancedFeatures.Controls.Add(this.MoveLeft);
            this.AdvancedFeatures.Controls.Add(this.LabelMoveBack);
            this.AdvancedFeatures.Controls.Add(this.MoveBack);
            this.AdvancedFeatures.Controls.Add(this.PictureBack);
            this.AdvancedFeatures.Controls.Add(this.PictureLeft);
            this.AdvancedFeatures.Controls.Add(this.MoveResetLabel);
            this.AdvancedFeatures.Controls.Add(this.MoveReset);
            this.AdvancedFeatures.Controls.Add(this.PictureRight);
            this.AdvancedFeatures.Controls.Add(this.PictureForward);
            this.AdvancedFeatures.Controls.Add(this.ChangeRoads);
            this.AdvancedFeatures.Controls.Add(this.AllowShrink);
            this.AdvancedFeatures.Controls.Add(this.RemoveFurniture);
            this.AdvancedFeatures.Controls.Add(this.Hidden);
            this.AdvancedFeatures.Controls.Add(this.BeachLot);
            this.AdvancedFeatures.Controls.Add(this.RemoveTerrainPaints);
            this.AdvancedFeatures.Controls.Add(this.MatchHoodTerrain);
            this.AdvancedFeatures.Controls.Add(this.LeavePortals);
            this.AdvancedFeatures.Controls.Add(this.PaveRoads);
            this.AdvancedFeatures.Controls.Add(this.BumpyRoads);
            this.AdvancedFeatures.Controls.Add(this.KeepElevation);
            this.AdvancedFeatures.Controls.Add(this.UpdateHoodTerrain);
            this.AdvancedFeatures.Controls.Add(this.LabelEdges);
            this.AdvancedFeatures.Controls.Add(this.LotEdges);
            this.AdvancedFeatures.Controls.Add(this.ClassValuePanel);
            this.AdvancedFeatures.Controls.Add(this.MultiBackup);
            this.AdvancedFeatures.Controls.Add(this.AdvancedExpl);
            this.AdvancedFeatures.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.AdvancedFeatures.Name = "AdvancedFeatures";
            this.AdvancedFeatures.TabStop = false;
            this.AdvancedFeatures.MouseHover += new System.EventHandler(this.AdvancedMouseLeave);
            // 
            // MoveLot
            // 
            resources.ApplyResources(this.MoveLot, "MoveLot");
            this.MoveLot.Name = "MoveLot";
            this.MoveLot.UseVisualStyleBackColor = true;
            this.MoveLot.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.MoveLot.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.MoveLot.CheckedChanged += new System.EventHandler(this.MoveLot_CheckedChanged);
            this.MoveLot.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // LabelMoveLeft
            // 
            resources.ApplyResources(this.LabelMoveLeft, "LabelMoveLeft");
            this.LabelMoveLeft.Name = "LabelMoveLeft";
            this.LabelMoveLeft.Enter += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // MoveLeft
            // 
            resources.ApplyResources(this.MoveLeft, "MoveLeft");
            this.MoveLeft.Name = "MoveLeft";
            this.MoveLeft.Enter += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // LabelMoveBack
            // 
            resources.ApplyResources(this.LabelMoveBack, "LabelMoveBack");
            this.LabelMoveBack.Name = "LabelMoveBack";
            this.LabelMoveBack.Enter += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // MoveBack
            // 
            resources.ApplyResources(this.MoveBack, "MoveBack");
            this.MoveBack.Name = "MoveBack";
            this.MoveBack.Enter += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // PictureBack
            // 
            this.PictureBack.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureBack.Image = global::LotExpander.Properties.Resources.MoveBack;
            resources.ApplyResources(this.PictureBack, "PictureBack");
            this.PictureBack.Name = "PictureBack";
            this.PictureBack.TabStop = false;
            this.PictureBack.Click += new System.EventHandler(this.PictureBack_Click);
            // 
            // PictureLeft
            // 
            this.PictureLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureLeft.Image = global::LotExpander.Properties.Resources.MoveLeft;
            resources.ApplyResources(this.PictureLeft, "PictureLeft");
            this.PictureLeft.Name = "PictureLeft";
            this.PictureLeft.TabStop = false;
            this.PictureLeft.Click += new System.EventHandler(this.PictureLeft_Click);
            // 
            // MoveResetLabel
            // 
            resources.ApplyResources(this.MoveResetLabel, "MoveResetLabel");
            this.MoveResetLabel.Name = "MoveResetLabel";
            this.MoveResetLabel.Enter += new System.EventHandler(this.MoveReset_Click);
            this.MoveResetLabel.Click += new System.EventHandler(this.MoveReset_Click);
            // 
            // MoveReset
            // 
            this.MoveReset.BackColor = System.Drawing.SystemColors.Control;
            this.MoveReset.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MoveReset.Cursor = System.Windows.Forms.Cursors.Arrow;
            resources.ApplyResources(this.MoveReset, "MoveReset");
            this.MoveReset.Name = "MoveReset";
            this.MoveReset.ReadOnly = true;
            this.MoveReset.Enter += new System.EventHandler(this.MoveReset_Click);
            this.MoveReset.Click += new System.EventHandler(this.MoveReset_Click);
            // 
            // PictureRight
            // 
            this.PictureRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureRight.Image = global::LotExpander.Properties.Resources.MoveRight;
            resources.ApplyResources(this.PictureRight, "PictureRight");
            this.PictureRight.Name = "PictureRight";
            this.PictureRight.TabStop = false;
            this.PictureRight.Click += new System.EventHandler(this.PictureRight_Click);
            // 
            // PictureForward
            // 
            this.PictureForward.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PictureForward.Image = global::LotExpander.Properties.Resources.MoveFront;
            resources.ApplyResources(this.PictureForward, "PictureForward");
            this.PictureForward.Name = "PictureForward";
            this.PictureForward.TabStop = false;
            this.PictureForward.Click += new System.EventHandler(this.PictureForward_Click);
            // 
            // ChangeRoads
            // 
            resources.ApplyResources(this.ChangeRoads, "ChangeRoads");
            this.ChangeRoads.Name = "ChangeRoads";
            this.ChangeRoads.UseVisualStyleBackColor = true;
            this.ChangeRoads.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.ChangeRoads.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.ChangeRoads.CheckedChanged += new System.EventHandler(this.ChangeRoads_CheckedChanged);
            this.ChangeRoads.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // AllowShrink
            // 
            resources.ApplyResources(this.AllowShrink, "AllowShrink");
            this.AllowShrink.Name = "AllowShrink";
            this.AllowShrink.UseVisualStyleBackColor = true;
            this.AllowShrink.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.AllowShrink.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.AllowShrink.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            this.AllowShrink.CheckStateChanged += new System.EventHandler(this.AllowShrink_CheckStateChanged);
            // 
            // RemoveFurniture
            // 
            resources.ApplyResources(this.RemoveFurniture, "RemoveFurniture");
            this.RemoveFurniture.Name = "RemoveFurniture";
            this.RemoveFurniture.UseVisualStyleBackColor = true;
            this.RemoveFurniture.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.RemoveFurniture.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.RemoveFurniture.CheckedChanged += new System.EventHandler(this.AdvancedForeColor);
            this.RemoveFurniture.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // Hidden
            // 
            resources.ApplyResources(this.Hidden, "Hidden");
            this.Hidden.Name = "Hidden";
            this.Hidden.UseVisualStyleBackColor = true;
            this.Hidden.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.Hidden.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.Hidden.CheckedChanged += new System.EventHandler(this.AdvancedForeColor);
            this.Hidden.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // BeachLot
            // 
            resources.ApplyResources(this.BeachLot, "BeachLot");
            this.BeachLot.Name = "BeachLot";
            this.BeachLot.UseVisualStyleBackColor = true;
            this.BeachLot.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.BeachLot.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.BeachLot.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // RemoveTerrainPaints
            // 
            resources.ApplyResources(this.RemoveTerrainPaints, "RemoveTerrainPaints");
            this.RemoveTerrainPaints.Name = "RemoveTerrainPaints";
            this.RemoveTerrainPaints.UseVisualStyleBackColor = true;
            this.RemoveTerrainPaints.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.RemoveTerrainPaints.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.RemoveTerrainPaints.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // MatchHoodTerrain
            // 
            resources.ApplyResources(this.MatchHoodTerrain, "MatchHoodTerrain");
            this.MatchHoodTerrain.Name = "MatchHoodTerrain";
            this.MatchHoodTerrain.UseVisualStyleBackColor = true;
            this.MatchHoodTerrain.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.MatchHoodTerrain.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.MatchHoodTerrain.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // LeavePortals
            // 
            resources.ApplyResources(this.LeavePortals, "LeavePortals");
            this.LeavePortals.Name = "LeavePortals";
            this.LeavePortals.UseVisualStyleBackColor = true;
            this.LeavePortals.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.LeavePortals.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.LeavePortals.CheckedChanged += new System.EventHandler(this.AdvancedForeColor);
            this.LeavePortals.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // PaveRoads
            // 
            resources.ApplyResources(this.PaveRoads, "PaveRoads");
            this.PaveRoads.Name = "PaveRoads";
            this.PaveRoads.UseVisualStyleBackColor = true;
            this.PaveRoads.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.PaveRoads.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.PaveRoads.CheckedChanged += new System.EventHandler(this.PaveRoads_CheckedChanged);
            this.PaveRoads.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // BumpyRoads
            // 
            resources.ApplyResources(this.BumpyRoads, "BumpyRoads");
            this.BumpyRoads.Name = "BumpyRoads";
            this.BumpyRoads.UseVisualStyleBackColor = true;
            this.BumpyRoads.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.BumpyRoads.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.BumpyRoads.CheckedChanged += new System.EventHandler(this.AdvancedForeColor);
            this.BumpyRoads.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // KeepElevation
            // 
            resources.ApplyResources(this.KeepElevation, "KeepElevation");
            this.KeepElevation.Checked = true;
            this.KeepElevation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.KeepElevation.Name = "KeepElevation";
            this.KeepElevation.UseVisualStyleBackColor = true;
            this.KeepElevation.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.KeepElevation.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.KeepElevation.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // UpdateHoodTerrain
            // 
            resources.ApplyResources(this.UpdateHoodTerrain, "UpdateHoodTerrain");
            this.UpdateHoodTerrain.Name = "UpdateHoodTerrain";
            this.UpdateHoodTerrain.UseVisualStyleBackColor = true;
            this.UpdateHoodTerrain.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.UpdateHoodTerrain.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.UpdateHoodTerrain.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // LabelEdges
            // 
            resources.ApplyResources(this.LabelEdges, "LabelEdges");
            this.LabelEdges.Name = "LabelEdges";
            this.LabelEdges.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.LabelEdges.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.LabelEdges.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // LotEdges
            // 
            resources.ApplyResources(this.LotEdges, "LotEdges");
            this.LotEdges.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LotEdges.FormattingEnabled = true;
            this.LotEdges.Items.AddRange(new object[] {
            resources.GetString("LotEdges.Items"),
            resources.GetString("LotEdges.Items1"),
            resources.GetString("LotEdges.Items2")});
            this.LotEdges.Name = "LotEdges";
            this.LotEdges.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.LotEdges.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.LotEdges.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            this.LotEdges.SelectedIndexChanged += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // ClassValuePanel
            // 
            resources.ApplyResources(this.ClassValuePanel, "ClassValuePanel");
            this.ClassValuePanel.Controls.Add(this.ClassOverride);
            this.ClassValuePanel.Controls.Add(this.ClassValueDisplay);
            this.ClassValuePanel.Controls.Add(this.ClassValueChange);
            this.ClassValuePanel.Name = "ClassValuePanel";
            // 
            // ClassOverride
            // 
            resources.ApplyResources(this.ClassOverride, "ClassOverride");
            this.ClassOverride.Name = "ClassOverride";
            this.ClassOverride.UseVisualStyleBackColor = true;
            this.ClassOverride.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.ClassOverride.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.ClassOverride.CheckedChanged += new System.EventHandler(this.ClassOverride_CheckedChanged);
            this.ClassOverride.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // ClassValueDisplay
            // 
            resources.ApplyResources(this.ClassValueDisplay, "ClassValueDisplay");
            this.ClassValueDisplay.Name = "ClassValueDisplay";
            this.ClassValueDisplay.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.ClassValueDisplay.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.ClassValueDisplay.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // ClassValueChange
            // 
            resources.ApplyResources(this.ClassValueChange, "ClassValueChange");
            this.ClassValueChange.Maximum = new decimal(new int[] {
            -1,
            0,
            0,
            0});
            this.ClassValueChange.Name = "ClassValueChange";
            this.ClassValueChange.Value = new decimal(new int[] {
            -1,
            0,
            0,
            0});
            this.ClassValueChange.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.ClassValueChange.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.ClassValueChange.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // MultiBackup
            // 
            resources.ApplyResources(this.MultiBackup, "MultiBackup");
            this.MultiBackup.Name = "MultiBackup";
            this.MultiBackup.UseVisualStyleBackColor = true;
            this.MultiBackup.Enter += new System.EventHandler(this.AdvancedMouseHover);
            this.MultiBackup.Leave += new System.EventHandler(this.AdvancedMouseLeave);
            this.MultiBackup.CheckedChanged += new System.EventHandler(this.MultiBackup_CheckedChanged);
            this.MultiBackup.MouseHover += new System.EventHandler(this.AdvancedMouseHover);
            // 
            // AdvancedExpl
            // 
            resources.ApplyResources(this.AdvancedExpl, "AdvancedExpl");
            this.AdvancedExpl.BackColor = System.Drawing.SystemColors.Control;
            this.AdvancedExpl.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.AdvancedExpl.ForeColor = System.Drawing.SystemColors.WindowText;
            this.AdvancedExpl.Name = "AdvancedExpl";
            this.AdvancedExpl.ReadOnly = true;
            this.AdvancedExpl.TabStop = false;
            // 
            // LongExpl
            // 
            resources.ApplyResources(this.LongExpl, "LongExpl");
            this.LongExpl.BackColor = System.Drawing.SystemColors.Control;
            this.LongExpl.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LongExpl.Name = "LongExpl";
            this.LongExpl.ReadOnly = true;
            // 
            // Defaults
            // 
            resources.ApplyResources(this.Defaults, "Defaults");
            this.Defaults.Name = "Defaults";
            this.Defaults.UseVisualStyleBackColor = true;
            this.Defaults.Click += new System.EventHandler(this.Defaults_Click);
            // 
            // SunLocation
            // 
            resources.ApplyResources(this.SunLocation, "SunLocation");
            this.SunLocation.BackColor = System.Drawing.SystemColors.Control;
            this.SunLocation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SunLocation.Name = "SunLocation";
            this.SunLocation.ReadOnly = true;
            this.SunLocation.TabStop = false;
            // 
            // PrimaryForm
            // 
            this.AcceptButton = this.NextButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BackButton;
            this.Controls.Add(this.PictureLogo);
            this.Controls.Add(this.SunLocation);
            this.Controls.Add(this.Title);
            this.Controls.Add(this.LotProperties);
            this.Controls.Add(this.AdvancedFeatures);
            this.Controls.Add(this.Explanation);
            this.Controls.Add(this.LongExpl);
            this.Controls.Add(this.Liste);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.AdvancedButton);
            this.Controls.Add(this.NextButton);
            this.Controls.Add(this.Defaults);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "PrimaryForm";
            this.Shown += new System.EventHandler(this.LotExpander_Shown);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LotExpander_FormClosing);
            this.Load += new System.EventHandler(this.LotExpander_Load);
            this.LotProperties.ResumeLayout(false);
            this.LotProperties.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LeftYard)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RightYard)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FrontYard)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BackYard)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureLogo)).EndInit();
            this.AdvancedFeatures.ResumeLayout(false);
            this.AdvancedFeatures.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureRight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PictureForward)).EndInit();
            this.ClassValuePanel.ResumeLayout(false);
            this.ClassValuePanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ClassValueChange)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Title;
        private System.Windows.Forms.ListBox Liste;
        private System.Windows.Forms.Button NextButton;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.GroupBox LotProperties;
        private System.Windows.Forms.TextBox Explanation;
        private System.Windows.Forms.Label HeightOld;
        private System.Windows.Forms.Label WidthOld;
        private System.Windows.Forms.Label WidthNew;
        private System.Windows.Forms.NumericUpDown LeftYard;
        private System.Windows.Forms.NumericUpDown RightYard;
        private System.Windows.Forms.NumericUpDown BackYard;
        private System.Windows.Forms.NumericUpDown FrontYard;
        private System.Windows.Forms.Label LabelOld;
        private System.Windows.Forms.Label LabelNew;
        private System.Windows.Forms.Label HeightNew;
        private System.Windows.Forms.Label LabelFront;
        private System.Windows.Forms.Label LabelBack;
        private System.Windows.Forms.Label LabelRight;
        private System.Windows.Forms.Label LabelLeft;
        private System.Windows.Forms.CheckBox KeepStreet;
        private System.Windows.Forms.PictureBox PictureLogo;
        private System.Windows.Forms.Button AdvancedButton;
        private System.Windows.Forms.GroupBox AdvancedFeatures;
        private System.Windows.Forms.CheckBox AllowShrink;
        private System.Windows.Forms.CheckBox LeavePortals;
        private System.Windows.Forms.CheckBox BumpyRoads;
        private System.Windows.Forms.ProgressBar Progress;
        private System.Windows.Forms.CheckBox MoveLot;
        private System.Windows.Forms.Label LabelMoveLeft;
        private System.Windows.Forms.Label LabelX1;
        private System.Windows.Forms.Label LabelX2;
        private System.Windows.Forms.CheckBox BackRoad;
        private System.Windows.Forms.CheckBox FrontRoad;
        private System.Windows.Forms.Label LabelWidth;
        private System.Windows.Forms.Label LabelRoad;
        private System.Windows.Forms.CheckBox RightRoad;
        private System.Windows.Forms.CheckBox LeftRoad;
        private System.Windows.Forms.Label LabelMax;
        private System.Windows.Forms.Label LabelSize;
        private System.Windows.Forms.Label LabelMoveBack;
        private System.Windows.Forms.PictureBox PictureBack;
        private System.Windows.Forms.PictureBox PictureForward;
        private System.Windows.Forms.PictureBox PictureRight;
        private System.Windows.Forms.PictureBox PictureLeft;
        private System.Windows.Forms.CheckBox ChangeRoads;
        private System.Windows.Forms.Label MoveBack;
        private System.Windows.Forms.Label MoveLeft;
        private System.Windows.Forms.TextBox AdvancedExpl;
        private System.Windows.Forms.TextBox LongExpl;
        private System.Windows.Forms.Label MoveResetLabel;
        private System.Windows.Forms.TextBox MoveReset;
        private System.Windows.Forms.Label LabelEdges;
        private System.Windows.Forms.ComboBox LotEdges;
        private System.Windows.Forms.Button Defaults;
        private System.Windows.Forms.CheckBox KeepElevation;
        private System.Windows.Forms.Panel ClassValuePanel;
        private System.Windows.Forms.Label ClassValueDisplay;
        private System.Windows.Forms.NumericUpDown ClassValueChange;
        private System.Windows.Forms.CheckBox ClassOverride;
        private System.Windows.Forms.CheckBox MultiBackup;
        private System.Windows.Forms.CheckBox UpdateHoodTerrain;
        private System.Windows.Forms.Label LabelX0;
        private System.Windows.Forms.Label HeightMax;
        private System.Windows.Forms.Label WidthMax;
        private System.Windows.Forms.TextBox SizeError;
        private System.Windows.Forms.CheckBox PaveRoads;
        private System.Windows.Forms.TextBox SunLocation;
        private System.Windows.Forms.CheckBox RemoveFurniture;
        private System.Windows.Forms.CheckBox BeachLot;
        private System.Windows.Forms.CheckBox Hidden;
        private System.Windows.Forms.CheckBox RemoveTerrainPaints;
        private System.Windows.Forms.CheckBox MatchHoodTerrain;
    }
}

