// File: MainForm.cs
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SimpleFlasherPatcher
{
    public partial class MainForm : Form
    {
        private string patchedImagePath = null;
        private string selectedImagePath = null; // extracted or chosen 'image'
        private CancellationTokenSource processCts = null;
        private readonly string appOutDir;

        public MainForm()
        {
            InitializeComponent();

            // set UI initial
            cmbPartition.Items.AddRange(new[] { "boot_a", "boot_b", "boot" });
            cmbPartition.SelectedIndex = 0;

            deviceTimer.Interval = 3000;
            deviceTimer.Tick += DeviceTimer_Tick;
            deviceTimer.Start();

            appOutDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SimpleFlasherPatcher");
            Directory.CreateDirectory(appOutDir);

            AppendLog("App output folder: " + appOutDir);
            _ = UpdateDeviceStatusAsync();
        }

        // UI-safe logging
        private void AppendLog(string text)
        {
            if (txtLog.InvokeRequired) { txtLog.Invoke(new Action(() => AppendLog(text))); return; }
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        // Browse boot.img
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Boot Image (*.img;*.bin)|*.img;*.bin|All files|*.*", Title = "Pilih boot.img" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtBootPath.Text = ofd.FileName;
                patchedImagePath = null;
                btnFlash.Enabled = false;
                AppendLog("Selected boot.img: " + ofd.FileName);
            }
        }

        // Pick raw image file
        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "All files (*.*)|*.*", Title = "Pilih file 'image' (raw)" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedImagePath = ofd.FileName;
                txtImagePath.Text = selectedImagePath;
                AppendLog("Selected image file: " + selectedImagePath);
            }
        }

        // Pick anykernel.zip and extract entry named 'image' (case-insensitive)
        private void btnPickZip_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog { Filter = "Zip (*.zip)|*.zip|All files|*.*", Title = "Pilih anykernel.zip" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string outPath = ExtractImageFromZip(ofd.FileName);
                    if (outPath != null)
                    {
                        selectedImagePath = outPath;
                        txtImagePath.Text = selectedImagePath;
                        AppendLog("Extracted 'image' from zip -> " + selectedImagePath);
                    }
                    else
                    {
                        AppendLog("Zip tidak mengandung entry bernama 'image'.");
                        MessageBox.Show("Zip tidak berisi file bernama 'image'.", "Not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    AppendLog("Error extracting zip: " + ex.Message);
                    MessageBox.Show("Gagal ekstrak zip: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Extract entry named 'image' (case-insensitive) and return extracted full path or null
        private string ExtractImageFromZip(string zipPath)
        {
            using var z = ZipFile.OpenRead(zipPath);
            var entry = z.Entries.FirstOrDefault(e => string.Equals(Path.GetFileName(e.FullName), "image", StringComparison.OrdinalIgnoreCase));
            if (entry == null) return null;

            string tempDir = Path.Combine(Path.GetTempPath(), "sfp_zip_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            string dest = Path.Combine(tempDir, "image");
            using (var rs = entry.Open())
            using (var ws = File.Create(dest))
                rs.CopyTo(ws);
            return dest;
        }

        // Patch & Repack logic (main)
        private async void btnPatch_Click(object sender, EventArgs e)
        {
            string bootPath = txtBootPath.Text?.Trim();
            if (string.IsNullOrEmpty(bootPath) || !File.Exists(bootPath))
            {
                MessageBox.Show("Pilih file boot.img yang valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnPatch.Enabled = false;
            btnFlash.Enabled = false;
            btnCancel.Enabled = true;
            processCts?.Cancel();
            processCts = new CancellationTokenSource();

            AppendLog("Mulai Patch & Repack...");
            progressBar.Value = 0;

            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string magiskExe = FindExecutable("magiskboot.exe", exeDir);
                if (magiskExe == null)
                {
                    MessageBox.Show("magiskboot.exe tidak ditemukan. Letakkan magiskboot.exe di folder EXE atau PATH.", "Missing magiskboot", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    AppendLog("magiskboot.exe not found.");
                    return;
                }

                string tempDir = Path.Combine(Path.GetTempPath(), "sfp_unpk_" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(tempDir);
                AppendLog("Working folder: " + tempDir);

                string tempBoot = Path.Combine(tempDir, "boot.img");
                File.Copy(bootPath, tempBoot, true);
                AppendLog("Copied boot.img to working folder.");

                AppendLog("Running: magiskboot unpack boot.img");
                var unpackOut = await RunProcessStreamAsync(magiskExe, $"unpack \"{Path.GetFileName(tempBoot)}\"", tempDir, processCts.Token);
                AppendLog(unpackOut);
                progressBar.Value = 20;

                var allFiles = Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories).ToList();
                AppendLog($"Files unpack: {allFiles.Count}");
                foreach (var f in allFiles.Take(10)) AppendLog("  " + Path.GetRelativePath(tempDir, f));

                // delete existing kernel if present
                var kernel = allFiles.FirstOrDefault(f => string.Equals(Path.GetFileName(f), "kernel", StringComparison.OrdinalIgnoreCase));
                if (kernel != null)
                {
                    try { File.Delete(kernel); AppendLog("Deleted existing kernel."); }
                    catch (Exception ex) { AppendLog("Failed delete kernel: " + ex.Message); }
                }
                else AppendLog("No existing kernel found.");

                // Determine source image:
                string sourceImage = null;
                if (!string.IsNullOrEmpty(selectedImagePath) && File.Exists(selectedImagePath))
                {
                    sourceImage = selectedImagePath;
                    AppendLog("Using selected image: " + sourceImage);
                }
                else
                {
                    // find file named 'image' in unpack
                    var imageInUnpack = allFiles.FirstOrDefault(f => string.Equals(Path.GetFileName(f), "image", StringComparison.OrdinalIgnoreCase));
                    if (imageInUnpack != null)
                    {
                        sourceImage = imageInUnpack;
                        AppendLog("Found 'image' inside unpack: " + Path.GetRelativePath(tempDir, sourceImage));
                    }
                }

                if (sourceImage == null)
                {
                    AppendLog("File 'image' tidak ditemukan dan tidak ada file image yang dipilih.");
                    MessageBox.Show("File 'image' tidak ditemukan. Pilih file 'image' dari anykernel.zip atau file terpisah.", "Image not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    // keep temp for inspection
                    return;
                }

                // copy sourceImage -> temp/kernel
                string destKernel = Path.Combine(tempDir, "kernel");
                File.Copy(sourceImage, destKernel, true);
                AppendLog($"Copied '{Path.GetFileName(sourceImage)}' -> kernel");
                progressBar.Value = 60;

                // repack
                AppendLog("Running: magiskboot repack boot.img");
                var repackOut = await RunProcessStreamAsync(magiskExe, $"repack \"{Path.GetFileName(tempBoot)}\"", tempDir, processCts.Token, timeoutMs: 180000);
                AppendLog(repackOut);
                progressBar.Value = 90;

                // find result .img
                string resultImg = Directory.GetFiles(tempDir, "*.img", SearchOption.TopDirectoryOnly).FirstOrDefault()
                                   ?? Directory.GetFiles(tempDir, "*.img", SearchOption.AllDirectories).OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();

                if (resultImg == null)
                {
                    AppendLog("Hasil .img tidak ditemukan setelah repack.");
                    MessageBox.Show("Repack selesai tapi file .img tidak ditemukan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // copy to appOutDir (safe location)
                string destName = Path.GetFileNameWithoutExtension(bootPath) + "-patched" + Path.GetExtension(bootPath);
                string destPath = Path.Combine(appOutDir, destName);
                File.Copy(resultImg, destPath, true);
                patchedImagePath = destPath;
                AppendLog("Patched image saved: " + destPath);
                progressBar.Value = 100;

                if (chkAutoDelete.Checked)
                {
                    try { Directory.Delete(tempDir, true); AppendLog("Temp deleted."); }
                    catch { AppendLog("Failed delete temp."); }
                }
                else AppendLog("Temp kept: " + tempDir);

                MessageBox.Show("Patch & Repack selesai.\nFile: " + patchedImagePath, "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnFlash.Enabled = true;
            }
            catch (OperationCanceledException)
            {
                AppendLog("Patch process cancelled.");
            }
            catch (UnauthorizedAccessException ua)
            {
                AppendLog("Access denied: " + ua.Message);
                MessageBox.Show("Akses ditolak saat menyimpan file. Gunakan folder yang bisa di-tulis atau jalankan sebagai Administrator.", "Permission denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                AppendLog("Error patch: " + ex.ToString());
                MessageBox.Show("Terjadi error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnPatch.Enabled = true;
                btnCancel.Enabled = false;
                processCts?.Dispose();
                processCts = null;
            }
        }

        // Flash patched image via fastboot
        private async void btnFlash_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(patchedImagePath) || !File.Exists(patchedImagePath))
            {
                MessageBox.Show("File patched tidak ditemukan. Lakukan Patch & Repack terlebih dahulu.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnFlash.Enabled = false;
            btnPatch.Enabled = false;
            btnCancel.Enabled = true;
            processCts?.Cancel();
            processCts = new CancellationTokenSource();

            AppendLog("Start flashing...");
            progressBar.Value = 0;

            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string fastbootExe = FindExecutable("fastboot.exe", exeDir);
                if (fastbootExe == null) { MessageBox.Show("fastboot.exe tidak ditemukan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); AppendLog("fastboot.exe not found."); return; }

                AppendLog("Checking fastboot devices...");
                var devicesOut = await RunProcessStreamAsync(fastbootExe, "devices", exeDir, processCts.Token, timeoutMs: 5000);
                AppendLog(devicesOut);
                bool hasDevice = devicesOut.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Any(l => !l.StartsWith("List of devices") && l.Trim().Length > 0);
                if (!hasDevice)
                {
                    AppendLog("No fastboot device detected.");
                    MessageBox.Show("Perangkat fastboot tidak terdeteksi. Pastikan device dalam bootloader.", "No Device", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string partition = cmbPartition.SelectedItem?.ToString() ?? "boot_a";
                AppendLog($"flash {partition} \"{patchedImagePath}\"");
                var flashOut = await RunProcessStreamAsync(fastbootExe, $"flash {partition} \"{patchedImagePath}\"", exeDir, processCts.Token, timeoutMs: 600000);
                AppendLog(flashOut);

                AppendLog("Rebooting device via fastboot...");
                var rebootOut = await RunProcessStreamAsync(fastbootExe, "reboot", exeDir, processCts.Token, timeoutMs: 10000);
                AppendLog(rebootOut);

                AppendLog("Flash completed.");
                MessageBox.Show("Flash selesai.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                progressBar.Value = 100;
            }
            catch (OperationCanceledException)
            {
                AppendLog("Flash cancelled.");
            }
            catch (Exception ex)
            {
                AppendLog("Error flash: " + ex.ToString());
                MessageBox.Show("Error saat flash: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnFlash.Enabled = true;
                btnPatch.Enabled = true;
                btnCancel.Enabled = false;
                processCts?.Dispose();
                processCts = null;
            }
        }

        // fastboot simple commands
        private async void btnFastbootReboot_Click(object sender, EventArgs e) => await RunFastbootSimpleAsync("reboot");
        private async void btnFastbootRebootBootloader_Click(object sender, EventArgs e) => await RunFastbootSimpleAsync("reboot bootloader");

        private async void btnAdbRebootBootloader_Click(object sender, EventArgs e)
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string adbExe = FindExecutable("adb.exe", exeDir);
            if (adbExe == null)
            {
                MessageBox.Show("adb.exe tidak ditemukan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendLog("adb.exe not found.");
                return;
            }
            AppendLog("Running: adb reboot bootloader");
            var outText = await RunProcessStreamAsync(adbExe, "reboot bootloader", exeDir, CancellationToken.None, timeoutMs: 10000);
            AppendLog(outText);
            MessageBox.Show("adb reboot bootloader sent.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task RunFastbootSimpleAsync(string args)
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string fastbootExe = FindExecutable("fastboot.exe", exeDir);
            if (fastbootExe == null) { MessageBox.Show("fastboot.exe tidak ditemukan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); AppendLog("fastboot.exe not found."); return; }
            AppendLog("Running: fastboot " + args);
            var outText = await RunProcessStreamAsync(fastbootExe, args, exeDir, CancellationToken.None, timeoutMs: 10000);
            AppendLog(outText);
            MessageBox.Show("fastboot " + args + " executed.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Cancel current process
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (processCts != null && !processCts.IsCancellationRequested)
            {
                processCts.Cancel();
                AppendLog("Cancellation requested.");
            }
        }

        // device timer
        private async void DeviceTimer_Tick(object sender, EventArgs e) => await UpdateDeviceStatusAsync();

        private async Task UpdateDeviceStatusAsync()
        {
            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string fastbootExe = FindExecutable("fastboot.exe", exeDir);
                if (fastbootExe == null)
                {
                    lblDeviceStatus.Text = "fastboot: NOT FOUND";
                    lblDeviceStatus.ForeColor = System.Drawing.Color.DarkRed;
                    btnFlash.Enabled = false;
                    return;
                }

                var outText = await RunProcessStreamAsync(fastbootExe, "devices", exeDir, CancellationToken.None, timeoutMs: 3000);
                bool hasDevice = outText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Any(l => !l.StartsWith("List of devices") && l.Trim().Length > 0);
                if (hasDevice)
                {
                    lblDeviceStatus.Text = "Device: DETECTED";
                    lblDeviceStatus.ForeColor = System.Drawing.Color.DarkGreen;
                    btnFlash.Enabled = File.Exists(patchedImagePath ?? "");
                }
                else
                {
                    lblDeviceStatus.Text = "Device: NOT DETECTED";
                    lblDeviceStatus.ForeColor = System.Drawing.Color.DarkRed;
                    btnFlash.Enabled = false;
                }
            }
            catch
            {
                lblDeviceStatus.Text = "Device: ERROR";
                lblDeviceStatus.ForeColor = System.Drawing.Color.OrangeRed;
                btnFlash.Enabled = false;
            }
        }

        // stream stdout/stderr line-by-line into log & return combined output
        private Task<string> RunProcessStreamAsync(string exePath, string args, string workingDir, CancellationToken token, int timeoutMs = 120000)
        {
            return Task.Run(() =>
            {
                var sb = new StringBuilder();
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        WorkingDirectory = workingDir
                    };

                    using var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
                    proc.OutputDataReceived += (s, e) => { if (e.Data != null) { AppendLog(e.Data); sb.AppendLine(e.Data); } };
                    proc.ErrorDataReceived += (s, e) => { if (e.Data != null) { AppendLog("[ERR] " + e.Data); sb.AppendLine("[ERR] " + e.Data); } };

                    proc.Start();
                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();

                    int waited = 0;
                    while (!proc.HasExited)
                    {
                        if (token.IsCancellationRequested) { try { proc.Kill(); } catch { } token.ThrowIfCancellationRequested(); }
                        Thread.Sleep(200);
                        waited += 200;
                        if (waited > timeoutMs) { try { proc.Kill(); } catch { } AppendLog("Process timeout."); break; }
                    }

                    proc.WaitForExit(2000);
                    sb.AppendLine($"ExitCode: {proc.ExitCode}");
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { AppendLog("RunProcess failed: " + ex.Message); sb.AppendLine("RunProcess failed: " + ex.Message); }
                return sb.ToString();
            }, token);
        }

        // find exe in app folder then PATH
        private string FindExecutable(string exeName, string exeDir)
        {
            string cand = Path.Combine(exeDir, exeName);
            if (File.Exists(cand)) return cand;
            var paths = (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator);
            foreach (var p in paths)
            {
                try
                {
                    var c = Path.Combine(p.Trim(), exeName);
                    if (File.Exists(c)) return c;
                }
                catch { }
            }
            return null;
        }
    }
}
