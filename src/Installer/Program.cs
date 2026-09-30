using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using RO3.Installer;

namespace RO3.Installer.UI;

static class Program
{
    [STAThread] static void Main() { ApplicationConfiguration.Initialize(); Application.Run(new MainForm()); }
}
sealed class MainForm : Form
{
    readonly TextBox location = new() { ReadOnly = true, Width = 530 };
    readonly Button chooseFolder = new() { Text = "เลือกโฟลเดอร์เกม", AutoSize = true };
    readonly Button chooseExe = new() { Text = "เลือกไฟล์ ro3.exe", AutoSize = true };
    readonly Button install = new() { Text = "ติดตั้ง", Enabled = false, AutoSize = true };
    readonly ProgressBar bar = new() { Width = 530 };
    readonly Label status = new() { AutoSize = true, MaximumSize = new Size(530, 0), Text = "รุ่นทดสอบหน้าจอเท่านั้น ยังไม่มี BepInEx/runtime payload และยังติดตั้งภาษาไทยไม่ได้ เลือก Client เพื่อทดสอบตรวจตำแหน่งเกม โดยไม่มีการค้นหาอัตโนมัติ" };
    string? selected;
    bool busy;
    public MainForm()
    {
        Text = "RO3 Thai Patch — UI Preview (ยังติดตั้งแพตช์ไม่ได้)"; Width = 590; Height = 350;
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(20), AutoScroll = true };
        var buttons = new FlowLayoutPanel { Width = 530, Height = 45 };
        buttons.Controls.AddRange([chooseFolder, chooseExe]);
        panel.Controls.AddRange([new Label { Text = "ตำแหน่งเกม", AutoSize = true }, location, buttons, install, bar, status]); Controls.Add(panel);
        chooseFolder.Click += (_, _) =>
        {
            using var picker = new FolderBrowserDialog { Description = "เลือกโฟลเดอร์ Client ที่มี ro3.exe", UseDescriptionForTitle = true };
            if (picker.ShowDialog(this) == DialogResult.OK) Check(picker.SelectedPath, false);
        };
        chooseExe.Click += (_, _) =>
        {
            using var picker = new OpenFileDialog { Title = "เลือกไฟล์ ro3.exe", Filter = "RO3 game|ro3.exe", CheckFileExists = true, Multiselect = false };
            if (picker.ShowDialog(this) == DialogResult.OK) Check(picker.FileName, true);
        };
        install.Click += async (_, _) => await Install();
        FormClosing += (_, e) => { if (busy) { e.Cancel = true; status.Text = "กรุณารอให้ติดตั้งหรือ rollback เสร็จ ก่อนปิดโปรแกรม"; } };
    }
    static Stream? Resource(string suffix)
    {
        var asm = Assembly.GetExecutingAssembly();
        string? name = asm.GetManifestResourceNames().SingleOrDefault(n => n.EndsWith(suffix, StringComparison.Ordinal));
        return name == null ? null : asm.GetManifestResourceStream(name);
    }
    static PayloadManifest Manifest()
    {
        using var stream = Resource("payload-manifest.json") ?? throw new InvalidDataException("รุ่นพัฒนานี้ยังไม่มี payload ที่พร้อมติดตั้ง");
        return JsonSerializer.Deserialize<PayloadManifest>(stream) ?? throw new InvalidDataException("Invalid payload manifest");
    }
    void Check(string path, bool isExe)
    {
        selected = null; install.Enabled = false; location.Text = path;
        try
        {
            string valid = isExe ? GameSelection.FromExe(path) : GameSelection.ValidateDirectory(path);
            selected = valid; location.Text = valid;
            var manifest = Manifest();
            if (!manifest.ReadyForInstallation || manifest.Files.Length == 0) throw new InvalidDataException("คำแปลและ runtime ยังไม่พร้อมติดตั้ง");
            using var payload = Resource("payload.zip") ?? throw new InvalidDataException("ไม่มี payload");
            install.Enabled = true; status.Text = "โฟลเดอร์ถูกต้อง กรุณาปิดเกมและ Launcher ก่อนติดตั้ง";
        }
        catch (Exception error) { status.Text = error.Message; }
    }
    async Task Install()
    {
        if (selected == null || busy) return;
        if (MessageBox.Show(this, "ปิดเกมและ RO3AsiaLauncher แล้วใช่ไหม?", "ก่อนติดตั้ง", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        busy = true; install.Enabled = chooseFolder.Enabled = chooseExe.Enabled = false;
        string staging = Path.Combine(Path.GetTempPath(), "ro3-thai-" + Guid.NewGuid().ToString("N"));
        try
        {
            var manifest = Manifest();
            var progress = new Progress<InstallProgress>(p => { bar.Maximum = p.Total; bar.Value = p.Completed; status.Text = $"กำลังติดตั้ง {p.Completed}/{p.Total}: {p.Path}"; });
            string explicitSelection = selected;
            await Task.Run(() =>
            {
                Directory.CreateDirectory(staging);
                using var stream = Resource("payload.zip") ?? throw new InvalidDataException("ไม่มี payload");
                using var zip = new ZipArchive(stream, ZipArchiveMode.Read);
                var allowed = manifest.Files.ToDictionary(f => f.Path, StringComparer.OrdinalIgnoreCase);
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                long size = 0;
                foreach (var entry in zip.Entries)
                {
                    if (!allowed.ContainsKey(entry.FullName) || !seen.Add(entry.FullName)) throw new InvalidDataException("Unexpected payload entry");
                    size += entry.Length; if (size > 256 * 1024 * 1024) throw new InvalidDataException("Payload size limit exceeded");
                    string output = InstallEngine.ResolveSafe(staging, entry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(output)!); entry.ExtractToFile(output, false);
                }
                InstallEngine.Install(explicitSelection, staging, manifest, progress);
            });
            status.Text = "ติดตั้งสำเร็จ: ตรวจไฟล์และบันทึกข้อมูลการติดตั้งแล้ว";
        }
        catch (Exception error) { status.Text = "ติดตั้งไม่สำเร็จ: " + error.Message; MessageBox.Show(this, error.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally
        {
            try { if (Directory.Exists(staging)) Directory.Delete(staging, true); } catch { /* Do not mask installation errors. */ }
            busy = false; chooseFolder.Enabled = chooseExe.Enabled = true;
            install.Enabled = false; // Revalidate through explicit user selection before another attempt.
        }
    }
}
