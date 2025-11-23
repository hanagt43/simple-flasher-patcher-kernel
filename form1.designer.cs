// File: MainForm.Designer.cs
namespace SimpleFlasherPatcher
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblFemmy;
        private System.Windows.Forms.TextBox txtBootPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtImagePath;
        private System.Windows.Forms.Button btnSelectImage;
        private System.Windows.Forms.Button btnPickZip;
        private System.Windows.Forms.Button btnPatch;
        private System.Windows.Forms.Button btnFlash;
        private System.Windows.Forms.ComboBox cmbPartition;
        private System.Windows.Forms.CheckBox chkAutoDelete;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Timer deviceTimer;
        private System.Windows.Forms.Label lblDeviceStatus;
        private System.Windows.Forms.Button btnFastbootReboot;
        private System.Windows.Forms.Button btnFastbootRebootBootloader;
        private System.Windows.Forms.Button btnAdbRebootBootloader;
        private System.Windows.Forms.Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblFemmy = new System.Windows.Forms.Label();
            this.txtBootPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtImagePath = new System.Windows.Forms.TextBox();
            this.btnSelectImage = new System.Windows.Forms.Button();
            this.btnPickZip = new System.Windows.Forms.Button();
            this.btnPatch = new System.Windows.Forms.Button();
            this.btnFlash = new System.Windows.Forms.Button();
            this.cmbPartition = new System.Windows.Forms.ComboBox();
            this.chkAutoDelete = new System.Windows.Forms.CheckBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.deviceTimer = new System.Windows.Forms.Timer(this.components);
            this.lblDeviceStatus = new System.Windows.Forms.Label();
            this.btnFastbootReboot = new System.Windows.Forms.Button();
            this.btnFastbootRebootBootloader = new System.Windows.Forms.Button();
            this.btnAdbRebootBootloader = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();

            // Form properties
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(880, 600);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "Simple Flasher Patcher kernel by @hanagt43";
            this.BackColor = System.Drawing.Color.FromArgb(28, 47, 86);

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Text = "Simple Flasher Patcher";

            // lblFemmy (branding)
            this.lblFemmy.AutoSize = true;
            this.lblFemmy.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblFemmy.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblFemmy.Location = new System.Drawing.Point(680, 16);
            this.lblFemmy.Text = "by @hanagt43";

            // txtBootPath
            this.txtBootPath.Location = new System.Drawing.Point(20, 56);
            this.txtBootPath.Size = new System.Drawing.Size(600, 23);

            // btnBrowse
            this.btnBrowse.Location = new System.Drawing.Point(632, 54);
            this.btnBrowse.Size = new System.Drawing.Size(100, 26);
            this.btnBrowse.Text = "Browse boot.img";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);

            // txtImagePath
            this.txtImagePath.Location = new System.Drawing.Point(20, 92);
            this.txtImagePath.Size = new System.Drawing.Size(600, 23);
            this.txtImagePath.ReadOnly = true;
            this.txtImagePath.PlaceholderText = "Pilih file 'image' (atau ekstrak dari anykernel.zip)";

            // btnSelectImage
            this.btnSelectImage.Location = new System.Drawing.Point(632, 90);
            this.btnSelectImage.Size = new System.Drawing.Size(110, 26);
            this.btnSelectImage.Text = "Pilih file image";
            this.btnSelectImage.Click += new System.EventHandler(this.btnSelectImage_Click);

            // btnPickZip
            this.btnPickZip.Location = new System.Drawing.Point(748, 90);
            this.btnPickZip.Size = new System.Drawing.Size(110, 26);
            this.btnPickZip.Text = "Pilih anykernel.zip";
            this.btnPickZip.Click += new System.EventHandler(this.btnPickZip_Click);

            // btnPatch
            this.btnPatch.Location = new System.Drawing.Point(20, 130);
            this.btnPatch.Size = new System.Drawing.Size(180, 38);
            this.btnPatch.Text = "Patch & Repack";
            this.btnPatch.BackColor = System.Drawing.Color.DodgerBlue;
            this.btnPatch.ForeColor = System.Drawing.Color.White;
            this.btnPatch.Click += new System.EventHandler(this.btnPatch_Click);

            // btnFlash
            this.btnFlash.Location = new System.Drawing.Point(212, 130);
            this.btnFlash.Size = new System.Drawing.Size(180, 38);
            this.btnFlash.Text = "Flash patched";
            this.btnFlash.BackColor = System.Drawing.Color.SeaGreen;
            this.btnFlash.ForeColor = System.Drawing.Color.White;
            this.btnFlash.Enabled = false;
            this.btnFlash.Click += new System.EventHandler(this.btnFlash_Click);

            // cmbPartition
            this.cmbPartition.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPartition.Location = new System.Drawing.Point(404, 138);
            this.cmbPartition.Size = new System.Drawing.Size(120, 23);

            // chkAutoDelete
            this.chkAutoDelete.AutoSize = true;
            this.chkAutoDelete.ForeColor = System.Drawing.Color.White;
            this.chkAutoDelete.Location = new System.Drawing.Point(540, 140);
            this.chkAutoDelete.Text = "Auto delete temp";

            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(660, 130);
            this.btnCancel.Size = new System.Drawing.Size(120, 38);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            this.btnCancel.Enabled = false;

            // progressBar
            this.progressBar.Location = new System.Drawing.Point(20, 178);
            this.progressBar.Size = new System.Drawing.Size(838, 18);

            // txtLog
            this.txtLog.Location = new System.Drawing.Point(20, 206);
            this.txtLog.Size = new System.Drawing.Size(838, 340);
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.ReadOnly = true;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F);

            // lblDeviceStatus
            this.lblDeviceStatus.Location = new System.Drawing.Point(20, 556);
            this.lblDeviceStatus.Size = new System.Drawing.Size(300, 22);
            this.lblDeviceStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDeviceStatus.ForeColor = System.Drawing.Color.White;
            this.lblDeviceStatus.Text = "Device: checking...";

            // btnFastbootReboot
            this.btnFastbootReboot.Location = new System.Drawing.Point(340, 552);
            this.btnFastbootReboot.Size = new System.Drawing.Size(140, 26);
            this.btnFastbootReboot.Text = "fastboot reboot";
            this.btnFastbootReboot.Click += new System.EventHandler(this.btnFastbootReboot_Click);

            // btnFastbootRebootBootloader
            this.btnFastbootRebootBootloader.Location = new System.Drawing.Point(488, 552);
            this.btnFastbootRebootBootloader.Size = new System.Drawing.Size(200, 26);
            this.btnFastbootRebootBootloader.Text = "fastboot reboot bootloader";
            this.btnFastbootRebootBootloader.Click += new System.EventHandler(this.btnFastbootRebootBootloader_Click);

            // btnAdbRebootBootloader
            this.btnAdbRebootBootloader.Location = new System.Drawing.Point(700, 552);
            this.btnAdbRebootBootloader.Size = new System.Drawing.Size(160, 26);
            this.btnAdbRebootBootloader.Text = "adb reboot bootloader";
            this.btnAdbRebootBootloader.Click += new System.EventHandler(this.btnAdbRebootBootloader_Click);

            // Add controls
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblFemmy);
            this.Controls.Add(this.txtBootPath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtImagePath);
            this.Controls.Add(this.btnSelectImage);
            this.Controls.Add(this.btnPickZip);
            this.Controls.Add(this.btnPatch);
            this.Controls.Add(this.btnFlash);
            this.Controls.Add(this.cmbPartition);
            this.Controls.Add(this.chkAutoDelete);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lblDeviceStatus);
            this.Controls.Add(this.btnFastbootReboot);
            this.Controls.Add(this.btnFastbootRebootBootloader);
            this.Controls.Add(this.btnAdbRebootBootloader);

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
