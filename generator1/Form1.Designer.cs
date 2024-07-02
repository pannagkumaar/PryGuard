namespace generator1
{
    partial class Generator
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblRandomEmail;
        private System.Windows.Forms.Button btnGenerateFingerprint;
        private System.Windows.Forms.Button btnGenerateRandomEmail;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblRandomEmail = new System.Windows.Forms.Label();
            this.btnGenerateFingerprint = new System.Windows.Forms.Button();
            this.btnGenerateRandomEmail = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblRandomEmail
            // 
            this.lblRandomEmail.AutoSize = true;
            this.lblRandomEmail.Location = new System.Drawing.Point(12, 9);
            this.lblRandomEmail.Name = "lblRandomEmail";
            this.lblRandomEmail.Size = new System.Drawing.Size(91, 13);
            this.lblRandomEmail.TabIndex = 0;
            this.lblRandomEmail.Text = "Generated Random Email: ";
            // 
            // btnGenerateFingerprint
            // 
            this.btnGenerateFingerprint.Location = new System.Drawing.Point(12, 40);
            this.btnGenerateFingerprint.Name = "btnGenerateFingerprint";
            this.btnGenerateFingerprint.Size = new System.Drawing.Size(200, 23);
            this.btnGenerateFingerprint.TabIndex = 1;
            this.btnGenerateFingerprint.Text = "Generate Fingerprint";
            this.btnGenerateFingerprint.UseVisualStyleBackColor = true;
            this.btnGenerateFingerprint.Click += new System.EventHandler(this.btnGenerateFingerprint_Click);
            // 
            // btnGenerateRandomEmail
            // 
            this.btnGenerateRandomEmail.Location = new System.Drawing.Point(12, 70);
            this.btnGenerateRandomEmail.Name = "btnGenerateRandomEmail";
            this.btnGenerateRandomEmail.Size = new System.Drawing.Size(200, 23);
            this.btnGenerateRandomEmail.TabIndex = 2;
            this.btnGenerateRandomEmail.Text = "Generate Random Email";
            this.btnGenerateRandomEmail.UseVisualStyleBackColor = true;
            this.btnGenerateRandomEmail.Click += new System.EventHandler(this.btnGenerateRandomEmail_Click);
            // 
            // Generator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.btnGenerateRandomEmail);
            this.Controls.Add(this.btnGenerateFingerprint);
            this.Controls.Add(this.lblRandomEmail);
            this.Name = "Generator";
            this.Text = "Fingerprint Generator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
