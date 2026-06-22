using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JpgToJpegPortable;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new RenamerForm());
    }
}

internal sealed class RenamerForm : Form
{
    private readonly TextBox folderTextBox;
    private readonly CheckBox recursiveCheckBox;
    private readonly CheckBox previewCheckBox;
    private readonly ProgressBar progressBar;
    private readonly TextBox logTextBox;
    private readonly Button runButton;

    public RenamerForm()
    {
        Text = "JPG -> JPEG Smart Renamer v1.0 Portable";
        Size = new Size(620, 300);
        MinimumSize = new Size(620, 300);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        folderTextBox = new TextBox
        {
            Location = new Point(20, 20),
            Size = new Size(460, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        var browseButton = new Button
        {
            Text = "Browse",
            Location = new Point(490, 18),
            Size = new Size(90, 30),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        browseButton.Click += BrowseButton_Click;

        recursiveCheckBox = new CheckBox
        {
            Text = "Include subfolders",
            Checked = true,
            AutoSize = true,
            Location = new Point(20, 60)
        };

        previewCheckBox = new CheckBox
        {
            Text = "Preview only",
            AutoSize = true,
            Location = new Point(200, 60)
        };

        progressBar = new ProgressBar
        {
            Location = new Point(20, 95),
            Size = new Size(560, 22),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };

        logTextBox = new TextBox
        {
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Location = new Point(20, 125),
            Size = new Size(560, 80),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true
        };

        runButton = new Button
        {
            Text = "Rename",
            Location = new Point(250, 215),
            Size = new Size(100, 30),
            Anchor = AnchorStyles.Bottom
        };
        runButton.Click += RunButton_Click;

        Controls.Add(folderTextBox);
        Controls.Add(browseButton);
        Controls.Add(recursiveCheckBox);
        Controls.Add(previewCheckBox);
        Controls.Add(progressBar);
        Controls.Add(logTextBox);
        Controls.Add(runButton);
    }

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Choose the folder that contains .jpg files",
            SelectedPath = Directory.Exists(folderTextBox.Text) ? folderTextBox.Text : string.Empty
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            folderTextBox.Text = dialog.SelectedPath;
        }
    }

    private async void RunButton_Click(object? sender, EventArgs e)
    {
        var folder = folderTextBox.Text.Trim();
        if (!Directory.Exists(folder))
        {
            MessageBox.Show(this, "Please choose a valid folder.", "Folder not found",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        runButton.Enabled = false;
        logTextBox.Clear();
        progressBar.Value = 0;

        try
        {
            var options = recursiveCheckBox.Checked ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.EnumerateFiles(folder, "*", options)
                .Where(path => string.Equals(Path.GetExtension(path), ".jpg", StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList();

            progressBar.Maximum = Math.Max(1, files.Count);

            if (files.Count == 0)
            {
                MessageBox.Show(this, "No .jpg files were found.", "Nothing to rename",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var previewOnly = previewCheckBox.Checked;
            var logFile = Path.Combine(folder, "rename_log.txt");
            if (!previewOnly)
            {
                File.WriteAllText(logFile, string.Empty);
            }

            var lines = await Task.Run(() => RenameFiles(files, previewOnly, logFile));

            foreach (var line in lines)
            {
                AppendLog(line);
                if (progressBar.Value < progressBar.Maximum)
                {
                    progressBar.Value++;
                }
            }

            MessageBox.Show(this, "Complete", "JPG to JPEG Renamer",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Rename failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            runButton.Enabled = true;
        }
    }

    private static List<string> RenameFiles(IEnumerable<string> files, bool previewOnly, string logFile)
    {
        var lines = new List<string>();

        foreach (var file in files)
        {
            var directory = Path.GetDirectoryName(file) ?? string.Empty;
            var baseName = Path.GetFileNameWithoutExtension(file);
            var target = GetUniqueName(directory, baseName);

            if (previewOnly)
            {
                lines.Add($"Would rename {Path.GetFileName(file)} -> {Path.GetFileName(target)}");
                continue;
            }

            File.Move(file, target);
            var line = $"{file} -> {target}";
            File.AppendAllText(logFile, line + Environment.NewLine);
            lines.Add(line);
        }

        return lines;
    }

    private static string GetUniqueName(string directory, string baseName)
    {
        var candidate = Path.Combine(directory, $"{baseName}.jpeg");
        var index = 1;

        while (File.Exists(candidate))
        {
            candidate = Path.Combine(directory, $"{baseName}({index}).jpeg");
            index++;
        }

        return candidate;
    }

    private void AppendLog(string line)
    {
        logTextBox.AppendText(line + Environment.NewLine);
    }
}
