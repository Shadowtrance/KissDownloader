using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace KissDownloader_GUI
{
    //=======================================================
    // config.ini example
    /*
           [Login]
           config login=true
           username=user
           password=pass

           [show]
           anime=http://kissanime.com/Anime/Prison-School/
           title=Prison School
           season=1
           episodemin=1
           episodemax=1
           destination=C:\download\
           quality=1280x720.mp4
    */
    //=======================================================

    public partial class MainForm : Form
    {
        string nl = Environment.NewLine;
        IniFile MyIni = new IniFile("config.ini");

        FolderBrowserDialog brwsr = new FolderBrowserDialog();

        public MainForm()
        {
            if (!File.Exists("config.ini"))
            {
                CreateIni();
            }
            if (!File.Exists("KissDownloader.py"))
            {
                MessageBox.Show("KissDownloader.py not found! Please place it in this folder and restart the program.", "ERROR File Missing!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

            InitializeComponent();

            //Disable Write Config button and Download button until Quality is selected
            button1.Enabled = false;
            button2.Enabled = false;

            //Read config.ini values to textboxes
            textBox1.Text = MyIni.Read("username", "Login");
            textBox2.Text = MyIni.Read("password", "Login");
            textBox3.Text = MyIni.Read("anime", "show");
            textBox4.Text = MyIni.Read("title", "show");
            textBox5.Text = MyIni.Read("season", "show");
            textBox6.Text = MyIni.Read("episodemin", "show");
            textBox7.Text = MyIni.Read("episodemax", "show");
            textBox8.Text = MyIni.Read("destination", "show");

            //backgroundworker
            var worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            //default quality selection - 1280x720
            comboBox1.SelectedIndex = 1;
        }

        //Exit button
        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //Worker DO WORK!
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            runCommand(worker);
        }

        //Worker finished
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show("Finished");
            button1.Enabled = true;
        }

        //Run our process
        public void runCommand(BackgroundWorker worker)
        {
            Process proc;
            proc = new Process();
            proc.StartInfo.FileName = @"cmd";

            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardError = true;

            proc.StartInfo.Arguments = "/C" + "KissDownloader.py";

            proc.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
            proc.Start();
            AppendTextBox("Worker has started...");

            proc.BeginOutputReadLine();

            proc.WaitForExit();
            proc.Close();
        }

        //handle console redirection to the textbox and invoke to prevent cross-thread errors
        void SortOutputHandler(object sender, DataReceivedEventArgs e)
        {
            Trace.WriteLine(e.Data);
            BeginInvoke(new MethodInvoker(() =>
            {
                AppendTextBox(e.Data + Environment.NewLine ?? string.Empty);
            }));
        }

        //same as above
        public void AppendTextBox(string value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendTextBox), new object[] { value });
                return;
            }
            richTextBox1.Text += value;
        }

        //Console output text scroll to bottom
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        //Write details to config.ini with the write config button.
        private void button1_Click(object sender, EventArgs e)
        {
            MyIni.Write("username", textBox1.Text, "Login");
            MyIni.Write("password", textBox2.Text, "Login");

            MyIni.Write("anime", textBox3.Text, "show");
            MyIni.Write("title", textBox4.Text, "show");
            MyIni.Write("season", textBox5.Text, "show");
            MyIni.Write("episodemin", textBox6.Text, "show");
            MyIni.Write("episodemax", textBox7.Text, "show");
            MyIni.Write("destination", textBox8.Text, "show");
            MyIni.Write("quality", comboBox1.Text, "show");

            //And read it out to the textbox
            richTextBox1.Text = "Username: " + MyIni.Read("username", "Login") + nl;
            richTextBox1.AppendText("Password: " + MyIni.Read("password", "Login") + nl + nl);
            richTextBox1.AppendText("Show Link: " + MyIni.Read("anime", "show") + nl);
            richTextBox1.AppendText("Show Title: " + MyIni.Read("title", "show") + nl);
            richTextBox1.AppendText("Season: " + MyIni.Read("season", "show") + nl);
            richTextBox1.AppendText("Episode Min: " + MyIni.Read("episodemin", "show") + nl);
            richTextBox1.AppendText("Episode Max: " + MyIni.Read("episodemax", "show") + nl);
            richTextBox1.AppendText("Save Destination: " + MyIni.Read("destination", "show") + nl);
            richTextBox1.AppendText("Quality: " + MyIni.Read("quality", "show"));

            //enable the download button
            button2.Enabled = true;
            button2.BackColor = Color.Lime;
        }

        //Download button
        private void button2_Click(object sender, EventArgs e)
        {
            if (worker.IsBusy != true)
            {
                richTextBox1.Text = "";
                worker.RunWorkerAsync();
                button1.Enabled = false;
            }
            else
            {
                MessageBox.Show("Worker is busy...");
            }
        }

        //Create config.ini if it is missing
        public void CreateIni()
        {
            if (!File.Exists("config.ini"))
            {
                try
                {
                    WriteIni();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex, "Error!", MessageBoxButtons.OK);
                }
            }
        }

        //Write Sections to config.ini
        public void WriteIni()
        {
            if (!File.Exists("config.ini"))
            {
                try
                {
                    MyIni.Write("config login", "true", "Login");
                    MyIni.Write("username", "", "Login");
                    MyIni.Write("password", "" + nl, "Login");

                    MyIni.Write("anime", "", "show");
                    MyIni.Write("title", "", "show");
                    MyIni.Write("season", "", "show");
                    MyIni.Write("episodemin", "", "show");
                    MyIni.Write("episodemax", "", "show");
                    MyIni.Write("destination", "", "show");
                    MyIni.Write("quality", "", "show");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex, "Error!", MessageBoxButtons.OK);
                }
            }
        }

        //Quality
        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            //enable Write Config button and change the backcolor of the download button to yellow - nearly ready
            button1.Enabled = true;
            button2.BackColor = Color.Yellow;
        }

        //Save Destination Dialog
        private void textBox8_Click(object sender, EventArgs e)
        {
            if (brwsr.ShowDialog() == DialogResult.Cancel)
            {
                 return;
            }
            else
            {
                 textBox8.Text = brwsr.SelectedPath + "\\";
            }
        }
        //===========================================================================================
    }
}
