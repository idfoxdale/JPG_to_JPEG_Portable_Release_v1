
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

function Get-UniqueName {
 param([string]$Directory,[string]$BaseName)
 $candidate = Join-Path $Directory "$BaseName.jpeg"
 $i=1
 while(Test-Path $candidate){
   $candidate = Join-Path $Directory "$BaseName($i).jpeg"
   $i++
 }
 $candidate
}

$form = New-Object Windows.Forms.Form
$form.Text = "JPG → JPEG Smart Renamer v1.0 Portable"
$form.Size = New-Object Drawing.Size(620,260)
$form.StartPosition="CenterScreen"

$folder = New-Object Windows.Forms.TextBox
$folder.Location="20,20"
$folder.Size="460,30"
$form.Controls.Add($folder)

$browse = New-Object Windows.Forms.Button
$browse.Text="Browse"
$browse.Location="490,18"
$form.Controls.Add($browse)

$recursive = New-Object Windows.Forms.CheckBox
$recursive.Text="Include subfolders"
$recursive.Checked=$true
$recursive.Location="20,60"
$form.Controls.Add($recursive)

$preview = New-Object Windows.Forms.CheckBox
$preview.Text="Preview only"
$preview.Location="200,60"
$form.Controls.Add($preview)

$bar = New-Object Windows.Forms.ProgressBar
$bar.Location="20,95"
$bar.Size="560,22"
$form.Controls.Add($bar)

$log = New-Object Windows.Forms.TextBox
$log.Multiline=$true
$log.ScrollBars="Vertical"
$log.Location="20,125"
$log.Size="560,60"
$form.Controls.Add($log)

$run = New-Object Windows.Forms.Button
$run.Text="Rename"
$run.Location="250,195"
$form.Controls.Add($run)

$browse.Add_Click({
 $d=New-Object Windows.Forms.FolderBrowserDialog
 if($d.ShowDialog() -eq "OK"){ $folder.Text=$d.SelectedPath }
})

$run.Add_Click({
 if(!(Test-Path $folder.Text)){ return }
 $items=Get-ChildItem $folder.Text -Filter *.jpg -Recurse:$recursive.Checked
 $bar.Maximum=[Math]::Max(1,$items.Count)
 $logfile=Join-Path $folder.Text "rename_log.txt"
 "" | Set-Content $logfile

 foreach($f in $items){
   $target=Get-UniqueName $f.DirectoryName $f.BaseName
   if($preview.Checked){
      $line="Would rename $($f.Name)"
   } else {
      Rename-Item $f.FullName -NewName (Split-Path $target -Leaf)
      $line="$($f.FullName) -> $target"
      Add-Content $logfile $line
   }
   $log.AppendText($line+"`r`n")
   if($bar.Value -lt $bar.Maximum){$bar.Value++}
   [Windows.Forms.Application]::DoEvents()
 }
 [Windows.Forms.MessageBox]::Show("Complete")
})

[void]$form.ShowDialog()
