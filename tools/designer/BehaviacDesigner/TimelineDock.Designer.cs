/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Tencent is pleased to support the open source community by making behaviac available.
//
// Copyright (C) 2015-2017 THL A29 Limited, a Tencent company. All rights reserved.
//
// Licensed under the BSD 3-Clause License (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at http://opensource.org/licenses/BSD-3-Clause
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is
// distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Behaviac.Design
{
    partial class TimelineDock
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimelineDock));
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            startButton = new System.Windows.Forms.Button();
            backwardButton = new System.Windows.Forms.Button();
            playButton = new System.Windows.Forms.Button();
            forwardButton = new System.Windows.Forms.Button();
            endButton = new System.Windows.Forms.Button();
            zoomOutButton = new System.Windows.Forms.Button();
            zoomInButton = new System.Windows.Forms.Button();
            comboBoxLogFilter = new System.Windows.Forms.ComboBox();
            numericUpDownFPS = new System.Windows.Forms.NumericUpDown();
            gotoLabel = new System.Windows.Forms.Label();
            gotoNumericUpDown = new System.Windows.Forms.NumericUpDown();
            promptLabel = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            trackBar = new System.Windows.Forms.TrackBar();
            startLabel = new System.Windows.Forms.Label();
            endLabel = new System.Windows.Forms.Label();
            toolTip = new System.Windows.Forms.ToolTip(components);
            effectTimer = new System.Windows.Forms.Timer(components);
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownFPS).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gotoNumericUpDown).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar).BeginInit();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            flowLayoutPanel1.Controls.Add(startButton);
            flowLayoutPanel1.Controls.Add(backwardButton);
            flowLayoutPanel1.Controls.Add(playButton);
            flowLayoutPanel1.Controls.Add(forwardButton);
            flowLayoutPanel1.Controls.Add(endButton);
            flowLayoutPanel1.Controls.Add(zoomOutButton);
            flowLayoutPanel1.Controls.Add(zoomInButton);
            flowLayoutPanel1.Controls.Add(comboBoxLogFilter);
            flowLayoutPanel1.Controls.Add(numericUpDownFPS);
            flowLayoutPanel1.Controls.Add(gotoLabel);
            flowLayoutPanel1.Controls.Add(gotoNumericUpDown);
            flowLayoutPanel1.Controls.Add(promptLabel);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(1114, 34);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // startButton
            // 
            startButton.AutoSize = true;
            startButton.Dock = System.Windows.Forms.DockStyle.Left;
            startButton.Image = (System.Drawing.Image)resources.GetObject("startButton.Image");
            startButton.Location = new System.Drawing.Point(20, 3);
            startButton.Margin = new System.Windows.Forms.Padding(20, 3, 3, 3);
            startButton.Name = "startButton";
            startButton.Size = new System.Drawing.Size(23, 28);
            startButton.TabIndex = 1;
            toolTip.SetToolTip(startButton, "Return to the first frame.");
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += startButton_Click;
            // 
            // backwardButton
            // 
            backwardButton.AutoSize = true;
            backwardButton.Dock = System.Windows.Forms.DockStyle.Left;
            backwardButton.Image = (System.Drawing.Image)resources.GetObject("backwardButton.Image");
            backwardButton.Location = new System.Drawing.Point(49, 3);
            backwardButton.Name = "backwardButton";
            backwardButton.Size = new System.Drawing.Size(22, 28);
            backwardButton.TabIndex = 2;
            toolTip.SetToolTip(backwardButton, "Return to the previous frame.");
            backwardButton.UseVisualStyleBackColor = true;
            backwardButton.Click += backwardButton_Click;
            // 
            // playButton
            // 
            playButton.Dock = System.Windows.Forms.DockStyle.Left;
            playButton.Image = Properties.Resources.Play;
            playButton.Location = new System.Drawing.Point(77, 3);
            playButton.Name = "playButton";
            playButton.Size = new System.Drawing.Size(22, 28);
            playButton.TabIndex = 3;
            toolTip.SetToolTip(playButton, "Continue/Break");
            playButton.UseVisualStyleBackColor = true;
            playButton.Click += playButton_Click;
            // 
            // forwardButton
            // 
            forwardButton.AutoSize = true;
            forwardButton.Dock = System.Windows.Forms.DockStyle.Left;
            forwardButton.Image = (System.Drawing.Image)resources.GetObject("forwardButton.Image");
            forwardButton.Location = new System.Drawing.Point(105, 3);
            forwardButton.Name = "forwardButton";
            forwardButton.Size = new System.Drawing.Size(22, 28);
            forwardButton.TabIndex = 4;
            toolTip.SetToolTip(forwardButton, "Go to the next frame.");
            forwardButton.UseVisualStyleBackColor = true;
            forwardButton.Click += forwardButton_Click;
            // 
            // endButton
            // 
            endButton.AutoSize = true;
            endButton.Dock = System.Windows.Forms.DockStyle.Left;
            endButton.Image = (System.Drawing.Image)resources.GetObject("endButton.Image");
            endButton.Location = new System.Drawing.Point(133, 3);
            endButton.Name = "endButton";
            endButton.Size = new System.Drawing.Size(22, 28);
            endButton.TabIndex = 5;
            toolTip.SetToolTip(endButton, "Go to the last frame.");
            endButton.UseVisualStyleBackColor = true;
            endButton.Click += endButton_Click;
            // 
            // zoomOutButton
            // 
            zoomOutButton.AutoSize = true;
            zoomOutButton.Dock = System.Windows.Forms.DockStyle.Left;
            zoomOutButton.Image = (System.Drawing.Image)resources.GetObject("zoomOutButton.Image");
            zoomOutButton.Location = new System.Drawing.Point(161, 3);
            zoomOutButton.Name = "zoomOutButton";
            zoomOutButton.Size = new System.Drawing.Size(22, 28);
            zoomOutButton.TabIndex = 7;
            toolTip.SetToolTip(zoomOutButton, "Zoom out");
            zoomOutButton.UseVisualStyleBackColor = true;
            zoomOutButton.Click += zoomOutButton_Click;
            // 
            // zoomInButton
            // 
            zoomInButton.AutoSize = true;
            zoomInButton.Dock = System.Windows.Forms.DockStyle.Left;
            zoomInButton.Image = (System.Drawing.Image)resources.GetObject("zoomInButton.Image");
            zoomInButton.Location = new System.Drawing.Point(189, 3);
            zoomInButton.Name = "zoomInButton";
            zoomInButton.Size = new System.Drawing.Size(22, 28);
            zoomInButton.TabIndex = 8;
            toolTip.SetToolTip(zoomInButton, "Zoon in");
            zoomInButton.UseVisualStyleBackColor = true;
            zoomInButton.Click += zoomInButton_Click;
            // 
            // comboBoxLogFilter
            // 
            comboBoxLogFilter.BackColor = System.Drawing.Color.FromArgb(56, 56, 56);
            comboBoxLogFilter.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            comboBoxLogFilter.ForeColor = System.Drawing.Color.LightGray;
            comboBoxLogFilter.FormattingEnabled = true;
            comboBoxLogFilter.Items.AddRange(new object[] { "ALL", "RED", "ORANGE", "YELLOW", "GREEN", "BLUE", "INDIGO", "PURPLE" });
            comboBoxLogFilter.Location = new System.Drawing.Point(217, 3);
            comboBoxLogFilter.Name = "comboBoxLogFilter";
            comboBoxLogFilter.Size = new System.Drawing.Size(94, 25);
            comboBoxLogFilter.TabIndex = 19;
            toolTip.SetToolTip(comboBoxLogFilter, "Log filter");
            comboBoxLogFilter.Visible = false;
            comboBoxLogFilter.SelectedIndexChanged += comboBoxLogFilter_SelectedIndexChanged;
            comboBoxLogFilter.KeyDown += comboBoxLogFilter_KeyDown;
            // 
            // numericUpDownFPS
            // 
            numericUpDownFPS.BackColor = System.Drawing.Color.FromArgb(56, 56, 56);
            numericUpDownFPS.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            numericUpDownFPS.Dock = System.Windows.Forms.DockStyle.Left;
            numericUpDownFPS.ForeColor = System.Drawing.Color.LightGray;
            numericUpDownFPS.Location = new System.Drawing.Point(317, 5);
            numericUpDownFPS.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            numericUpDownFPS.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numericUpDownFPS.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownFPS.Name = "numericUpDownFPS";
            numericUpDownFPS.RightToLeft = System.Windows.Forms.RightToLeft.No;
            numericUpDownFPS.Size = new System.Drawing.Size(75, 23);
            numericUpDownFPS.TabIndex = 17;
            toolTip.SetToolTip(numericUpDownFPS, "Simulating FPS");
            numericUpDownFPS.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // gotoLabel
            // 
            gotoLabel.AutoSize = true;
            gotoLabel.Dock = System.Windows.Forms.DockStyle.Left;
            gotoLabel.Location = new System.Drawing.Point(405, 0);
            gotoLabel.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
            gotoLabel.Name = "gotoLabel";
            gotoLabel.Size = new System.Drawing.Size(48, 34);
            gotoLabel.TabIndex = 13;
            gotoLabel.Text = "Frame";
            gotoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gotoNumericUpDown
            // 
            gotoNumericUpDown.BackColor = System.Drawing.Color.FromArgb(56, 56, 56);
            gotoNumericUpDown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            gotoNumericUpDown.Dock = System.Windows.Forms.DockStyle.Left;
            gotoNumericUpDown.ForeColor = System.Drawing.Color.LightGray;
            gotoNumericUpDown.Location = new System.Drawing.Point(459, 5);
            gotoNumericUpDown.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            gotoNumericUpDown.Name = "gotoNumericUpDown";
            gotoNumericUpDown.RightToLeft = System.Windows.Forms.RightToLeft.No;
            gotoNumericUpDown.Size = new System.Drawing.Size(79, 23);
            gotoNumericUpDown.TabIndex = 14;
            toolTip.SetToolTip(gotoNumericUpDown, "Go to which frame?");
            gotoNumericUpDown.ValueChanged += gotoNumericUpDown_ValueChanged;
            // 
            // promptLabel
            // 
            promptLabel.AutoSize = true;
            promptLabel.Dock = System.Windows.Forms.DockStyle.Left;
            promptLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            promptLabel.ForeColor = System.Drawing.Color.Gold;
            promptLabel.Location = new System.Drawing.Point(551, 0);
            promptLabel.Margin = new System.Windows.Forms.Padding(10, 0, 3, 0);
            promptLabel.Name = "promptLabel";
            promptLabel.Size = new System.Drawing.Size(127, 34);
            promptLabel.TabIndex = 15;
            promptLabel.Text = "break prompt";
            promptLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            promptLabel.Click += promptLabel_Click;
            // 
            // panel1
            // 
            panel1.AutoSize = true;
            panel1.Controls.Add(trackBar);
            panel1.Dock = System.Windows.Forms.DockStyle.Top;
            panel1.Location = new System.Drawing.Point(0, 34);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1114, 56);
            panel1.TabIndex = 0;
            // 
            // trackBar
            // 
            trackBar.BackColor = System.Drawing.SystemColors.Control;
            trackBar.Dock = System.Windows.Forms.DockStyle.Top;
            trackBar.LargeChange = 10;
            trackBar.Location = new System.Drawing.Point(0, 0);
            trackBar.Maximum = 100;
            trackBar.Name = "trackBar";
            trackBar.RightToLeft = System.Windows.Forms.RightToLeft.No;
            trackBar.Size = new System.Drawing.Size(1114, 56);
            trackBar.SmallChange = 5;
            trackBar.TabIndex = 15;
            trackBar.TickFrequency = 10;
            trackBar.TickStyle = System.Windows.Forms.TickStyle.Both;
            trackBar.ValueChanged += trackBar_ValueChanged;
            // 
            // startLabel
            // 
            startLabel.AutoSize = true;
            startLabel.Dock = System.Windows.Forms.DockStyle.Left;
            startLabel.Location = new System.Drawing.Point(0, 90);
            startLabel.Name = "startLabel";
            startLabel.Padding = new System.Windows.Forms.Padding(8, 0, 0, 0);
            startLabel.Size = new System.Drawing.Size(24, 17);
            startLabel.TabIndex = 16;
            startLabel.Text = "0";
            // 
            // endLabel
            // 
            endLabel.AutoSize = true;
            endLabel.Dock = System.Windows.Forms.DockStyle.Right;
            endLabel.Location = new System.Drawing.Point(1082, 90);
            endLabel.Name = "endLabel";
            endLabel.Size = new System.Drawing.Size(32, 17);
            endLabel.TabIndex = 17;
            endLabel.Text = "100";
            // 
            // effectTimer
            // 
            effectTimer.Interval = 250;
            effectTimer.Tick += effectTimer_Tick;
            // 
            // TimelineDock
            // 
            BackColor = System.Drawing.Color.FromArgb(56, 56, 56);
            ClientSize = new System.Drawing.Size(1114, 117);
            Controls.Add(endLabel);
            Controls.Add(startLabel);
            Controls.Add(panel1);
            Controls.Add(flowLayoutPanel1);
            Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            ForeColor = System.Drawing.Color.LightGray;
            Name = "TimelineDock";
            ShowIcon = false;
            TabText = "Timeline";
            Text = "Timeline";
            Load += TimelineDock_Load;
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownFPS).EndInit();
            ((System.ComponentModel.ISupportInitialize)gotoNumericUpDown).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label gotoLabel;
        private System.Windows.Forms.NumericUpDown gotoNumericUpDown;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TrackBar trackBar;
        private System.Windows.Forms.Label startLabel;
        private System.Windows.Forms.Label endLabel;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button backwardButton;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.Button forwardButton;
        private System.Windows.Forms.Button endButton;
        private System.Windows.Forms.Button zoomOutButton;
        private System.Windows.Forms.Button zoomInButton;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.Label promptLabel;
        private System.Windows.Forms.NumericUpDown numericUpDownFPS;
        private System.Windows.Forms.Timer effectTimer;
        private System.Windows.Forms.ComboBox comboBoxLogFilter;
    }
}
