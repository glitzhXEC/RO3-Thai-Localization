using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using RO3.Installer;
using RO3.ThaiLocalization;

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
    readonly Label versionInfo = new() { AutoSize = true, MaximumSize = new Size(530, 0) };
    readonly Label translationInfo = new() { AutoSize = true, MaximumSize = new Size(530, 0) };
    readonly Button install = new() { Text = "ติดตั้งแพตช์", Enabled = false, AutoSize = true };
    readonly Button updateTranslations = new() { Text = "อัปเดตคำแปลจาก GitHub", Enabled = false, AutoSize = true };
    readonly Button uninstall = new() { Text = "ถอนแพตช์นี้", Enabled = false, AutoSize = true };
    readonly ProgressBar bar = new() { Width = 530 };
    readonly Label status = new() { AutoSize = true, MaximumSize = new Size(530, 0), Text = "เลือกโฟลเดอร์ Client เพื่อตรวจรุ่นที่ติดตั้ง ไม่มีการค้นหาเกมอัตโนมัติ" };
    string? selected;
    bool busy;
    public MainForm()
    {
        bool ready = false; try { ready = Manifest().ReadyForInstallation; } catch { }
        Text = ready ? "RO3 Thai Localization — Auto-update Alpha (ยังไม่ทดสอบในเกม)" : "RO3 Thai Patch — UI Preview";
        if (ready) status.Text = "เลือก Client ที่มี ro3.exe แล้วโปรแกรมจะแสดงรุ่นแพตช์และเปิดใช้การอัปเดตหรือถอนติดตั้งได้ รุ่น Alpha สำหรับ Mono x64 ปิดเกมและ Launcher ก่อนดำเนินการ";
        Width = 590; Height = 400;
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(20), AutoScroll = true };
        var buttons = new FlowLayoutPanel { Width = 530, Height = 45 };
        buttons.Controls.AddRange([chooseFolder, chooseExe]);
        panel.Controls.AddRange([new Label { Text = "ตำแหน่งเกม", AutoSize = true }, location, buttons, versionInfo, translationInfo, install, updateTranslations, uninstall, bar, status]); Controls.Add(panel);
        chooseFolder.Click += async (_, _) =>
        {
            using var picker = new FolderBrowserDialog { Description = "เลือกโฟลเดอร์ Client ที่มี ro3.exe", UseDescriptionForTitle = true };
            if (picker.ShowDialog(this) == DialogResult.OK) await Check(picker.SelectedPath, false);
        };
        chooseExe.Click += async (_, _) =>
        {
            using var picker = new OpenFileDialog { Title = "เลือกไฟล์ ro3.exe", Filter = "RO3 game|ro3.exe", CheckFileExists = true, Multiselect = false };
            if (picker.ShowDialog(this) == DialogResult.OK) await Check(picker.FileName, true);
        };
        install.Click += async (_, _) => await Install();
        updateTranslations.Click += async (_, _) => await UpdateTranslations();
        uninstall.Click += async (_, _) => await Remove();
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
    async Task Check(string path, bool isExe)
    {
        selected = null; install.Enabled = updateTranslations.Enabled = uninstall.Enabled = false; install.Text = "ติดตั้งแพตช์"; versionInfo.Text = translationInfo.Text = ""; location.Text = path;
        try
        {
            string valid = isExe ? GameSelection.FromExe(path) : GameSelection.ValidateDirectory(path);
            selected = valid; location.Text = valid;
            var manifest = Manifest();
            if (!manifest.ReadyForInstallation || manifest.Files.Length == 0) throw new InvalidDataException("คำแปลและ runtime ยังไม่พร้อมติดตั้ง");
            using var payload = Resource("payload.zip") ?? throw new InvalidDataException("ไม่มี payload");
            if (manifest.TargetProfile == "ro3-mono-x64") InstallEngine.ValidateMonoX64(valid);
            var installed = InstallEngine.ReadInstallation(valid);
            if (installed != null)
            {
                int comparison = InstallEngine.CompareVersions(installed.Version, manifest.Version);
                versionInfo.Text = $"รุ่น runtime ที่ติดตั้ง: {installed.Version}    รุ่น runtime ใน Installer: {manifest.Version}";
                uninstall.Enabled = true;
                if (comparison < 0) { install.Text = "อัปเดตตัวแพตช์"; install.Enabled = true; }
                else if (comparison == 0) install.Text = "ตัวแพตช์เป็นรุ่นล่าสุด";
                else status.Text = "runtime ที่ติดตั้งใหม่กว่าไฟล์ในตัวติดตั้งนี้; จะไม่ลดรุ่นให้โดยอัตโนมัติ";
                string config = Path.Combine(valid, "BepInEx", "config");
                string localVersion = "";
                try { TranslationUpdater.LoadCache(config, out localVersion); } catch { }
                translationInfo.Text = "เวอร์ชันชุดคำแปลในเครื่อง: " + ShortVersion(localVersion) + "    กำลังตรวจ GitHub…";
                status.Text = comparison < 0 ? "พบ runtime รุ่นเก่า; อัปเดตตัวแพตช์ได้ และตรวจชุดคำแปลแยกจากกัน" : "ตรวจชุดคำแปลแยกจากเวอร์ชันตัวติดตั้ง";
                try
                {
                    string latest = await Task.Run(() => TranslationUpdater.GetLatestVersion());
                    translationInfo.Text = "เวอร์ชันชุดคำแปลในเครื่อง: " + ShortVersion(localVersion) + "    ล่าสุดบน GitHub: " + ShortVersion(latest);
                    updateTranslations.Enabled = latest != localVersion;
                    if (latest == localVersion) status.Text += " — คำแปลเป็นรุ่นล่าสุดแล้ว";
                    else status.Text += " — พบคำแปลรุ่นใหม่ กดอัปเดตคำแปลได้โดยไม่ดาวน์โหลดตัวติดตั้งใหม่";
                }
                catch (Exception error)
                {
                    translationInfo.Text = "เวอร์ชันชุดคำแปลในเครื่อง: " + ShortVersion(localVersion) + "    ตรวจ GitHub ไม่สำเร็จ";
                    updateTranslations.Enabled = true; // Allow a direct retry from the update action.
                    status.Text += " — plugin จะตรวจอัปเดตให้อีกครั้งเมื่อเปิดเกม: " + error.Message;
                }
                return;
            }
            versionInfo.Text = "รุ่นแพตช์ที่จะติดตั้ง: " + manifest.Version;
            if (Directory.Exists(Path.Combine(valid, "BepInEx")) || File.Exists(Path.Combine(valid, "winhttp.dll"))) throw new InvalidDataException("พบ BepInEx/proxy ที่ไม่ได้เป็นของแพตช์นี้ โปรแกรมจะไม่ทับม็อดหรือไฟล์เดิม");
            install.Enabled = true; status.Text = "โฟลเดอร์ถูกต้อง กรุณาปิดเกมและ Launcher ก่อนติดตั้ง";
        }
        catch (Exception error) { status.Text = error.Message; }
    }
    async Task Install()
    {
        if (selected == null || busy) return;
        string action = install.Text == "อัปเดตแพตช์" ? "อัปเดตแพตช์ภาษาไทยเป็นรุ่นล่าสุด" : "ติดตั้งแพตช์ภาษาไทย";
        if (MessageBox.Show(this, "ปิดเกมและ RO3AsiaLauncher แล้วใช่ไหม?\n\n" + action + " โดยไม่สร้าง backup ถาวรชุดใหม่", "ก่อนดำเนินการ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        busy = true; install.Enabled = updateTranslations.Enabled = uninstall.Enabled = chooseFolder.Enabled = chooseExe.Enabled = false;
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
            status.Text = "ติดตั้งหรืออัปเดตสำเร็จ ไฟล์ที่แพตช์เป็นเจ้าของได้รับการตรวจแล้ว";
        }
        catch (Exception error) { status.Text = "ติดตั้งไม่สำเร็จ: " + error.Message; MessageBox.Show(this, error.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally
        {
            try { if (Directory.Exists(staging)) Directory.Delete(staging, true); } catch { /* Do not mask installation errors. */ }
            busy = false; chooseFolder.Enabled = chooseExe.Enabled = true;
            if (selected != null) await Check(selected, false);
        }
    }
    async Task Remove()
    {
        if (selected == null || busy) return;
        if (MessageBox.Show(this, "ปิดเกมและ Launcher แล้วใช่ไหม?\n\nจะลบเฉพาะไฟล์ที่แพตช์นี้ติดตั้ง และคืนไฟล์เดิมที่แพตช์เคยแทนที่ โดยไม่เก็บ backup", "ถอนแพตช์ภาษาไทย", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        busy = true; install.Enabled = updateTranslations.Enabled = uninstall.Enabled = chooseFolder.Enabled = chooseExe.Enabled = false;
        try
        {
            var progress = new Progress<InstallProgress>(p => { bar.Maximum = p.Total; bar.Value = p.Completed; status.Text = $"กำลังถอน {p.Completed}/{p.Total}: {p.Path}"; });
            string root = selected;
            await Task.Run(() => InstallEngine.Uninstall(root, progress));
            status.Text = "ถอนเฉพาะไฟล์แพตช์แล้ว ไม่มี backup คงเหลือ ไฟล์หรือม็อดที่ไม่ได้ติดตั้งโดยแพตช์ยังอยู่";
        }
        catch (Exception error) { status.Text = "ถอนแพตช์ไม่สำเร็จ: " + error.Message; MessageBox.Show(this,error.Message,"ข้อผิดพลาด",MessageBoxButtons.OK,MessageBoxIcon.Error); }
        finally { busy = false; chooseFolder.Enabled = chooseExe.Enabled = true; if (selected != null) await Check(selected, false); }
    }

    static string ShortVersion(string version) => string.IsNullOrEmpty(version) ? "ไม่มีแคช" : version[..Math.Min(12, version.Length)];

    async Task UpdateTranslations()
    {
        if (selected == null || busy) return;
        if (MessageBox.Show(this, "ดาวน์โหลดเฉพาะข้อมูลคำแปลจาก GitHub และตรวจ hash/schema ก่อนเขียนแคช\n\nไม่ดาวน์โหลด Client, Installer หรือ DLL", "อัปเดตคำแปล", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        busy = true; install.Enabled = updateTranslations.Enabled = uninstall.Enabled = chooseFolder.Enabled = chooseExe.Enabled = false;
        try
        {
            string config = Path.Combine(selected, "BepInEx", "config");
            string current = "";
            try { TranslationUpdater.LoadCache(config, out current); } catch { /* A corrupt cache will be replaced only after the new bundle validates. */ }
            string message = "กำลังตรวจเวอร์ชันชุดคำแปลจาก GitHub…";
            await Task.Run(() => TranslationUpdater.Refresh(config, current, text => message = text));
            TranslationUpdater.LoadCache(config, out string updated);
            string latest = await Task.Run(() => TranslationUpdater.GetLatestVersion());
            if (updated != latest) throw new InvalidOperationException(message);
            status.Text = "อัปเดตข้อมูลคำแปลสำเร็จ โดยไม่ดาวน์โหลด Client หรือ Installer";
        }
        catch (Exception error) { status.Text = "อัปเดตคำแปลไม่สำเร็จ; ข้อมูลเดิมยังอยู่: " + error.Message; }
        finally { busy = false; chooseFolder.Enabled = chooseExe.Enabled = true; if (selected != null) await Check(selected, false); }
    }

}
