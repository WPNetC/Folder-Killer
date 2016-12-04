using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pri.LongPath;

namespace LongPathKiller
{
    public partial class Form1 : Form
    {
        private string _path;

        public Form1()
        {
            InitializeComponent();
        }

        private delegate void SetFilePathDelegate(string path);
        private delegate void SetStatusDelegate(string status);

        private void btnGetFoler_OnClick(object sender, EventArgs e)
        {
            btnKill.Enabled = false;

            if (null == fbdSelectFolder)
                fbdSelectFolder = new FolderBrowserDialog();
            var result = fbdSelectFolder.ShowDialog();

            switch (result)
            {
                case DialogResult.Yes:
                case DialogResult.OK:
                    _path = fbdSelectFolder.SelectedPath;
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(_path))
            {
                SetFilePath(_path);
                btnKill.Enabled = Directory.Exists(_path);
            }
        }

        private void SetFilePath(string path)
        {
            if (lblFilePath.InvokeRequired)
            {
                var del = new SetFilePathDelegate(SetFilePath);
                Invoke(del, new object[] { path });
            }
            else
            {
                lblFilePath.Text = path;
            }
        }

        private void SetStatus(string status)
        {
            if (lblFilePath.InvokeRequired)
            {
                var del = new SetStatusDelegate(SetStatus);
                Invoke(del, new[] { status });
            }
            else
            {
                lblStatus.Text = status;
            }
        }

        private async void btnKill_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_path))
            {
                SetStatus("No folder selected.");
                return;
            }

            SetStatus("Deleting. Please wait...");

            await Task.Run(() =>
              {
                  int deleted = 0;
                  var allFolders = Directory.EnumerateDirectories(_path, "*", System.IO.SearchOption.AllDirectories).Reverse().ToArray();
                  foreach (var folder in allFolders)
                  {
                      SetFilePath(folder);
                      SetStatus("Folders deleted: " + Convert.ToString(++deleted));

                      if (Directory.Exists(folder))
                      {
                          try
                          {
                              Directory.Delete(folder, true);
                          }
                          catch
                          {
                              foreach (var file in Directory.GetFiles(folder))
                              {
                                  var fi = new FileInfo(file);
                                  fi.IsReadOnly = false;
                              }

                              try
                              {
                                  Directory.Delete(folder, true);
                              }
                              catch
                              {
                                  SetStatus("Could not delete: " + folder);
                              }
                          }
                      }
                  }
                  try
                  {
                      if (Directory.Exists(_path))
                          Directory.Delete(_path, true);

                      SetStatus("Folder Deleted.");
                      SetFilePath(string.Empty);
                      return;
                  }
                  catch(Exception ex)
                  {
                      if (Directory.Exists(_path))
                      {
                          SetStatus("Could not delete all folders. You may need to delete the top folder manually.");
                      }
                  }

                  SetFilePath(_path);
              });
        }
    }
}
