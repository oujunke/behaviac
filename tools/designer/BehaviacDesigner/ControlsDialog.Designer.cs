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
    partial class ControlsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlsDialog));
            webBrowser = new System.Windows.Forms.WebBrowser();
            buttonClose = new System.Windows.Forms.Button();
            checkBoxNext = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            // 
            // webBrowser
            // 
            resources.ApplyResources(webBrowser, "webBrowser");
            webBrowser.Name = "webBrowser";
            webBrowser.Url = new System.Uri("about:blank", System.UriKind.Absolute);
            webBrowser.PreviewKeyDown += webBrowser_PreviewKeyDown;
            // 
            // buttonClose
            // 
            resources.ApplyResources(buttonClose, "buttonClose");
            buttonClose.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            buttonClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.DarkGray;
            buttonClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.DarkGray;
            buttonClose.Name = "buttonClose";
            buttonClose.UseVisualStyleBackColor = false;
            buttonClose.Click += buttonClose_Click;
            // 
            // checkBoxNext
            // 
            resources.ApplyResources(checkBoxNext, "checkBoxNext");
            checkBoxNext.BackColor = System.Drawing.Color.FromArgb(65, 65, 65);
            checkBoxNext.Name = "checkBoxNext";
            checkBoxNext.UseVisualStyleBackColor = false;
            // 
            // ControlsDialog
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(56, 56, 56);
            Controls.Add(checkBoxNext);
            Controls.Add(buttonClose);
            Controls.Add(webBrowser);
            ForeColor = System.Drawing.Color.LightGray;
            KeyPreview = true;
            Name = "ControlsDialog";
            FormClosed += ControlsDialog_FormClosed;
            Load += ControlsDialog_Load;
            KeyDown += ControlsDialog_KeyDown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.CheckBox checkBoxNext;
    }
}