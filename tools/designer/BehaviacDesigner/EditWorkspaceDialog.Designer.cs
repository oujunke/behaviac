////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2009, Daniel Kollmann
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, are permitted
// provided that the following conditions are met:
//
// - Redistributions of source code must retain the above copyright notice, this list of conditions
//   and the following disclaimer.
//
// - Redistributions in binary form must reproduce the above copyright notice, this list of
//   conditions and the following disclaimer in the documentation and/or other materials provided
//   with the distribution.
//
// - Neither the name of Daniel Kollmann nor the names of its contributors may be used to endorse
//   or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
// IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
// WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY
// WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// The above software in this distribution may have been modified by THL A29 Limited ("Tencent Modifications").
//
// All Tencent Modifications are Copyright (C) 2015-2017 THL A29 Limited.
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Behaviac.Design
{
    partial class EditWorkspaceDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditWorkspaceDialog));
            doneButton = new System.Windows.Forms.Button();
            cancelButton = new System.Windows.Forms.Button();
            nameLabel = new System.Windows.Forms.Label();
            nameTextBox = new System.Windows.Forms.TextBox();
            XMLButton = new System.Windows.Forms.Button();
            XMLTextBox = new System.Windows.Forms.TextBox();
            workspaceButton = new System.Windows.Forms.Button();
            workspaceTextBox = new System.Windows.Forms.TextBox();
            locationLabel = new System.Windows.Forms.Label();
            sourceTextBox = new System.Windows.Forms.TextBox();
            sourceLabel = new System.Windows.Forms.Label();
            sourceButton = new System.Windows.Forms.Button();
            exportTextBox = new System.Windows.Forms.TextBox();
            exportLabel = new System.Windows.Forms.Label();
            exportButton = new System.Windows.Forms.Button();
            typesExportTextBox = new System.Windows.Forms.TextBox();
            generateLabel = new System.Windows.Forms.Label();
            typesExportButton = new System.Windows.Forms.Button();
            languageLabel = new System.Windows.Forms.Label();
            languageComboBox = new System.Windows.Forms.ComboBox();
            metaFileLabel = new System.Windows.Forms.Label();
            useIntValueCheckBox = new System.Windows.Forms.CheckBox();
            folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            includeTextBox = new System.Windows.Forms.TextBox();
            includeLabel = new System.Windows.Forms.Label();
            includeButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // doneButton
            // 
            resources.ApplyResources(doneButton, "doneButton");
            doneButton.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            doneButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            doneButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGray;
            doneButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            doneButton.Name = "doneButton";
            doneButton.UseVisualStyleBackColor = false;
            doneButton.Click += doneButton_Click;
            // 
            // cancelButton
            // 
            resources.ApplyResources(cancelButton, "cancelButton");
            cancelButton.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGray;
            cancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            cancelButton.Name = "cancelButton";
            cancelButton.UseVisualStyleBackColor = false;
            cancelButton.Click += cancelButton_Click;
            // 
            // nameLabel
            // 
            resources.ApplyResources(nameLabel, "nameLabel");
            nameLabel.Name = "nameLabel";
            // 
            // nameTextBox
            // 
            resources.ApplyResources(nameTextBox, "nameTextBox");
            nameTextBox.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            nameTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            nameTextBox.ForeColor = System.Drawing.Color.LightGray;
            nameTextBox.Name = "nameTextBox";
            nameTextBox.TextChanged += nameTextBox_TextChanged;
            // 
            // XMLButton
            // 
            resources.ApplyResources(XMLButton, "XMLButton");
            XMLButton.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            XMLButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGray;
            XMLButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            XMLButton.Name = "XMLButton";
            XMLButton.UseVisualStyleBackColor = false;
            XMLButton.Click += XMLButton_Click;
            // 
            // XMLTextBox
            // 
            resources.ApplyResources(XMLTextBox, "XMLTextBox");
            XMLTextBox.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            XMLTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            XMLTextBox.ForeColor = System.Drawing.Color.LightGray;
            XMLTextBox.Name = "XMLTextBox";
            // 
            // workspaceButton
            // 
            resources.ApplyResources(workspaceButton, "workspaceButton");
            workspaceButton.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            workspaceButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGray;
            workspaceButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            workspaceButton.Name = "workspaceButton";
            workspaceButton.UseVisualStyleBackColor = false;
            workspaceButton.Click += workspaceButton_Click;
            // 
            // workspaceTextBox
            // 
            resources.ApplyResources(workspaceTextBox, "workspaceTextBox");
            workspaceTextBox.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            workspaceTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            workspaceTextBox.ForeColor = System.Drawing.Color.LightGray;
            workspaceTextBox.Name = "workspaceTextBox";
            // 
            // locationLabel
            // 
            resources.ApplyResources(locationLabel, "locationLabel");
            locationLabel.Name = "locationLabel";
            locationLabel.DoubleClick += locationLabel_DoubleClick;
            // 
            // sourceTextBox
            // 
            resources.ApplyResources(sourceTextBox, "sourceTextBox");
            sourceTextBox.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            sourceTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            sourceTextBox.ForeColor = System.Drawing.Color.LightGray;
            sourceTextBox.Name = "sourceTextBox";
            // 
            // sourceLabel
            // 
            resources.ApplyResources(sourceLabel, "sourceLabel");
            sourceLabel.Name = "sourceLabel";
            sourceLabel.DoubleClick += sourceLabel_DoubleClick;
            // 
            // sourceButton
            // 
            resources.ApplyResources(sourceButton, "sourceButton");
            sourceButton.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            sourceButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGray;
            sourceButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            sourceButton.Name = "sourceButton";
            sourceButton.UseVisualStyleBackColor = false;
            sourceButton.Click += buttonSource_Click;
            // 
            // exportTextBox
            // 
            resources.ApplyResources(exportTextBox, "exportTextBox");
            exportTextBox.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            exportTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            exportTextBox.ForeColor = System.Drawing.Color.LightGray;
            exportTextBox.Name = "exportTextBox";
            // 
            // exportLabel
            // 
            resources.ApplyResources(exportLabel, "exportLabel");
            exportLabel.Name = "exportLabel";
            exportLabel.DoubleClick += exportLabel_DoubleClick;
            // 
            // exportButton
            // 
            resources.ApplyResources(exportButton, "exportButton");
            exportButton.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            exportButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGray;
            exportButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            exportButton.Name = "exportButton";
            exportButton.UseVisualStyleBackColor = false;
            exportButton.Click += buttonExport_Click;
            // 
            // typesExportTextBox
            // 
            resources.ApplyResources(typesExportTextBox, "typesExportTextBox");
            typesExportTextBox.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            typesExportTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            typesExportTextBox.ForeColor = System.Drawing.Color.LightGray;
            typesExportTextBox.Name = "typesExportTextBox";
            // 
            // generateLabel
            // 
            resources.ApplyResources(generateLabel, "generateLabel");
            generateLabel.Name = "generateLabel";
            generateLabel.DoubleClick += generateLabel_DoubleClick;
            // 
            // typesExportButton
            // 
            resources.ApplyResources(typesExportButton, "typesExportButton");
            typesExportButton.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            typesExportButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGray;
            typesExportButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            typesExportButton.Name = "typesExportButton";
            typesExportButton.UseVisualStyleBackColor = false;
            typesExportButton.Click += typesExportButton_Click;
            // 
            // languageLabel
            // 
            resources.ApplyResources(languageLabel, "languageLabel");
            languageLabel.Name = "languageLabel";
            // 
            // languageComboBox
            // 
            resources.ApplyResources(languageComboBox, "languageComboBox");
            languageComboBox.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            languageComboBox.ForeColor = System.Drawing.Color.LightGray;
            languageComboBox.FormattingEnabled = true;
            languageComboBox.Items.AddRange(new object[] { resources.GetString("languageComboBox.Items"), resources.GetString("languageComboBox.Items1") });
            languageComboBox.Name = "languageComboBox";
            // 
            // metaFileLabel
            // 
            resources.ApplyResources(metaFileLabel, "metaFileLabel");
            metaFileLabel.Name = "metaFileLabel";
            metaFileLabel.DoubleClick += metaFileLabel_DoubleClick;
            // 
            // useIntValueCheckBox
            // 
            resources.ApplyResources(useIntValueCheckBox, "useIntValueCheckBox");
            useIntValueCheckBox.BackColor = System.Drawing.Color.FromArgb(56, 56, 56);
            useIntValueCheckBox.Name = "useIntValueCheckBox";
            useIntValueCheckBox.UseVisualStyleBackColor = false;
            useIntValueCheckBox.CheckedChanged += useIntValueCheckBox_CheckedChanged;
            useIntValueCheckBox.MouseEnter += useIntValueCheckBox_MouseEnter;
            useIntValueCheckBox.MouseLeave += useIntValueCheckBox_MouseLeave;
            // 
            // includeTextBox
            // 
            resources.ApplyResources(includeTextBox, "includeTextBox");
            includeTextBox.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            includeTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            includeTextBox.ForeColor = System.Drawing.Color.LightGray;
            includeTextBox.Name = "includeTextBox";
            // 
            // includeLabel
            // 
            resources.ApplyResources(includeLabel, "includeLabel");
            includeLabel.Name = "includeLabel";
            // 
            // includeButton
            // 
            resources.ApplyResources(includeButton, "includeButton");
            includeButton.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            includeButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGray;
            includeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            includeButton.Name = "includeButton";
            includeButton.UseVisualStyleBackColor = false;
            // 
            // EditWorkspaceDialog
            // 
            AcceptButton = doneButton;
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(56, 56, 56);
            CancelButton = cancelButton;
            Controls.Add(includeTextBox);
            Controls.Add(includeLabel);
            Controls.Add(includeButton);
            Controls.Add(useIntValueCheckBox);
            Controls.Add(metaFileLabel);
            Controls.Add(languageComboBox);
            Controls.Add(languageLabel);
            Controls.Add(typesExportTextBox);
            Controls.Add(generateLabel);
            Controls.Add(typesExportButton);
            Controls.Add(exportTextBox);
            Controls.Add(exportLabel);
            Controls.Add(exportButton);
            Controls.Add(sourceTextBox);
            Controls.Add(sourceLabel);
            Controls.Add(sourceButton);
            Controls.Add(workspaceTextBox);
            Controls.Add(locationLabel);
            Controls.Add(workspaceButton);
            Controls.Add(XMLTextBox);
            Controls.Add(XMLButton);
            Controls.Add(nameTextBox);
            Controls.Add(nameLabel);
            Controls.Add(cancelButton);
            Controls.Add(doneButton);
            ForeColor = System.Drawing.Color.LightGray;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EditWorkspaceDialog";
            ShowInTaskbar = false;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button doneButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Button XMLButton;
        private System.Windows.Forms.TextBox XMLTextBox;
        private System.Windows.Forms.Button workspaceButton;
        private System.Windows.Forms.TextBox workspaceTextBox;
        private System.Windows.Forms.Label locationLabel;
        private System.Windows.Forms.TextBox sourceTextBox;
        private System.Windows.Forms.Label sourceLabel;
        private System.Windows.Forms.Button sourceButton;
        private System.Windows.Forms.TextBox exportTextBox;
        private System.Windows.Forms.Label exportLabel;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.TextBox typesExportTextBox;
        private System.Windows.Forms.Label generateLabel;
        private System.Windows.Forms.Button typesExportButton;
        private System.Windows.Forms.Label languageLabel;
        private System.Windows.Forms.ComboBox languageComboBox;
        private System.Windows.Forms.Label metaFileLabel;
        private System.Windows.Forms.CheckBox useIntValueCheckBox;
        private System.Windows.Forms.TextBox includeTextBox;
        private System.Windows.Forms.Label includeLabel;
        private System.Windows.Forms.Button includeButton;
    }
}