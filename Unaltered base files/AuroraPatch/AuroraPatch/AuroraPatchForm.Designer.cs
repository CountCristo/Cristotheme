
namespace AuroraPatch
{
    partial class AuroraPatchForm
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
            this.ButtonStart = new System.Windows.Forms.Button();
            this.ListPatches = new System.Windows.Forms.ListBox();
            this.LabelPatches = new System.Windows.Forms.Label();
            this.LabelDescription = new System.Windows.Forms.Label();
            this.ButtonChangeSettings = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ButtonStart
            // 
            this.ButtonStart.Location = new System.Drawing.Point(32, 310);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(203, 62);
            this.ButtonStart.TabIndex = 0;
            this.ButtonStart.Text = "Start Aurora";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // ListPatches
            // 
            this.ListPatches.FormattingEnabled = true;
            this.ListPatches.Location = new System.Drawing.Point(29, 47);
            this.ListPatches.Name = "ListPatches";
            this.ListPatches.Size = new System.Drawing.Size(205, 238);
            this.ListPatches.TabIndex = 1;
            this.ListPatches.SelectedIndexChanged += new System.EventHandler(this.ListPatches_SelectedIndexChanged);
            // 
            // LabelPatches
            // 
            this.LabelPatches.AutoSize = true;
            this.LabelPatches.Location = new System.Drawing.Point(29, 31);
            this.LabelPatches.Name = "LabelPatches";
            this.LabelPatches.Size = new System.Drawing.Size(81, 13);
            this.LabelPatches.TabIndex = 2;
            this.LabelPatches.Text = "Found patches:";
            // 
            // LabelDescription
            // 
            this.LabelDescription.AutoSize = true;
            this.LabelDescription.Location = new System.Drawing.Point(247, 53);
            this.LabelDescription.Name = "LabelDescription";
            this.LabelDescription.Size = new System.Drawing.Size(63, 13);
            this.LabelDescription.TabIndex = 3;
            this.LabelDescription.Text = "Description:";
            // 
            // ButtonChangeSettings
            // 
            this.ButtonChangeSettings.Location = new System.Drawing.Point(251, 91);
            this.ButtonChangeSettings.Name = "ButtonChangeSettings";
            this.ButtonChangeSettings.Size = new System.Drawing.Size(141, 41);
            this.ButtonChangeSettings.TabIndex = 4;
            this.ButtonChangeSettings.Text = "Change settings";
            this.ButtonChangeSettings.UseVisualStyleBackColor = true;
            this.ButtonChangeSettings.Click += new System.EventHandler(this.ButtonChangeSettings_Click);
            // 
            // AuroraPatchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ButtonChangeSettings);
            this.Controls.Add(this.LabelDescription);
            this.Controls.Add(this.LabelPatches);
            this.Controls.Add(this.ListPatches);
            this.Controls.Add(this.ButtonStart);
            this.Name = "AuroraPatchForm";
            this.Text = "AuroraPatch";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AuroraPatchForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.ListBox ListPatches;
        private System.Windows.Forms.Label LabelPatches;
        private System.Windows.Forms.Label LabelDescription;
        private System.Windows.Forms.Button ButtonChangeSettings;
    }
}