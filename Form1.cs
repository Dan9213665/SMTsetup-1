using FastMember;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using DataTable = System.Data.DataTable;
using System.Runtime.InteropServices;
using ProgressBar = System.Windows.Forms.ProgressBar;
using Font = System.Drawing.Font;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace SMTsetup
{
    
    public partial class SMTSetupMain : Form
    {

        public int progressCounter
        {
            set { countItems = value; }
        }

        List<BomItem> items = new List<BomItem>();
        List<BomItem> Availableitems = new List<BomItem>();
        List<BomItem> Founditems = new List<BomItem>();
        System.Data.DataTable Atable = new DataTable();
        DataTable Ftable = new DataTable();
        public int countItems = 0;
        string m = string.Empty;
        string loadedDirNameCSPS = string.Empty;
        public SMTSetupMain()
        {
            InitializeComponent();
            progressBar1.Minimum= 0;
            progressBar2.Minimum= 0;
            progressBar2.Value = 0;
            textBox1.Enabled = false;
            comboBox1.SelectedIndex= 0;
            DateTime fileModifiedDate = File.GetLastWriteTime(@"SMTsetup.exe");
            this.Text = "SMT setup Updated " + fileModifiedDate.ToString(); ;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            //dataGridView1.DataSource = null;
            //dataGridView2.DataSource = null;
            items.Clear();
            Availableitems.Clear();
            Founditems.Clear();
            Atable.Clear();
            Ftable.Clear();
            groupBox5.ResetText();
            groupBox3.ResetText();



            //int size = -1;
            folderBrowserDialog1.InitialDirectory = "\\\\dbr1\\Data\\SMT\\SETUP";
            //openFileDialog1.InitialDirectory = "\\\\dbr1\\Data\\SMT\\SETUP";
            //openFileDialog1.Filter = "Excel files(*.xlsx,*.xls) | *.xlsx;*.xls";
            //DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            DialogResult result = folderBrowserDialog1.ShowDialog();
            string folderPath = folderBrowserDialog1.SelectedPath;
            //MessageBox.Show(folderPath.ToString());
           
            if (result == DialogResult.OK && Directory.EnumerateFiles(folderPath, "*.xls").Count() > 0) // Test result.
            {
                label1.Text = "";
                groupBox2.Text = "";
                frmLoadingScreen ls = new frmLoadingScreen();
                ls.Show();
                loadedDirNameCSPS = folderPath.ToString();



                foreach (string file in Directory.EnumerateFiles(folderPath, "*.xls"))
                {

                    //MessageBox.Show(folderPath.ToString());
                    string contents = File.ReadAllText(file);

                    //MessageBox.Show(file.ToString());
                    //string file = openFileDialog1.FileName;

                    string thesheetName = (System.IO.Path.GetFileNameWithoutExtension(file)).ToString();
                    //MessageBox.Show(thesheetName);
                    m = thesheetName.Substring(thesheetName.Length - 1);
                    label1.Text += file+"\n";
                    groupBox2.Text += file+" ";
                    try
                    {
                        //string text = File.ReadAllText(file);
                        // size = text.Length;
                        //List<string> values = new List<string>();
                        string constr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + file + "; Extended Properties=\"Excel 12.0 Xml;HDR=NO;\"";
                        //MessageBox.Show(constr.ToString());
                        using (OleDbConnection conn = new OleDbConnection(constr))
                        {
                            conn.Open();
                            //OleDbCommand command = new OleDbCommand("Select * from [Sheet1$]", conn);
                            OleDbCommand command = new OleDbCommand("Select * from [" + thesheetName + "$]", conn);
                            OleDbDataReader reader = command.ExecuteReader();
                            if (reader.HasRows)
                            {
                                int i = 0;
                                
                                while (reader.Read())
                                {
                                    i += 1;
                                    
                                    // this assumes just one column, and the value is text
                                    //string value = reader[0].ToString();
                                    BomItem abc = new BomItem
                                    {
                                        SetNo = "M"+m + "-" + reader[0].ToString(),
                                        CompName = reader[1].ToString(),
                                        Comments = reader[2].ToString(),
                                        FdrType = reader[3].ToString(),
                                        PitchIndex = reader[4].ToString(),
                                        FoundTheItem=false
                                     
                                       
                                    };
                                    if (i == 4)
                                    {
                                        //groupBox2.Text += reader[7].ToString() + ".....Machine (" + m + ")";
                                    }
                                    if (i > 4 && reader[0].ToString() != "")
                                    {
                                        items.Add(abc);
                                        //countItems++;
                                    }

                                    //values.Add(value);

                                }
                            }
                            conn.Close();
                        }
                    }
                    catch (IOException)
                    {
                    }
                }
                ls.Hide();
                countItems = items.Count();
                progressBar1.Maximum = items.Count();
                progressBar2.Maximum = items.Count();

                progressBar1.Value = items.Count();
               
                Availableitems = items;
                RepopulateAvailableTable();
                textBox1.Enabled = true;
                textBox1.Focus();
                
            }
            
            else
            {
                 if (result == DialogResult.OK && Directory.EnumerateFiles(folderPath, "*.xlsx").Count() > 0)
                {

                    label1.Text = "";
                    groupBox2.Text = "";
                    frmLoadingScreen ls = new frmLoadingScreen();
                    ls.Show();


                    foreach (string file in Directory.EnumerateFiles(folderPath, "*.xlsx"))
                    {

                        //MessageBox.Show(folderPath.ToString());
                        string contents = File.ReadAllText(file);

                        //MessageBox.Show(file.ToString());
                        //string file = openFileDialog1.FileName;

                        string thesheetName = (System.IO.Path.GetFileNameWithoutExtension(file)).ToString();
                        //MessageBox.Show(thesheetName);
                        m = thesheetName.Substring(thesheetName.Length - 1);
                        label1.Text += file + "\n";
                        groupBox2.Text += file + " ";
                        try
                        {
                            //string text = File.ReadAllText(file);
                            // size = text.Length;
                            //List<string> values = new List<string>();
                            string constr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + file + "; Extended Properties=\"Excel 12.0 Xml;HDR=NO;\"";
                            //MessageBox.Show(constr.ToString());
                            using (OleDbConnection conn = new OleDbConnection(constr))
                            {
                                conn.Open();
                                //OleDbCommand command = new OleDbCommand("Select * from [Sheet1$]", conn);
                                OleDbCommand command = new OleDbCommand("Select * from [" + thesheetName + "$]", conn);
                                OleDbDataReader reader = command.ExecuteReader();
                                if (reader.HasRows)
                                {
                                    int i = 0;

                                    while (reader.Read())
                                    {
                                        i += 1;

                                        // this assumes just one column, and the value is text
                                        //string value = reader[0].ToString();
                                        BomItem abc = new BomItem
                                        {
                                            SetNo = "M" + m + "-" + reader[0].ToString(),
                                            CompName = reader[1].ToString(),
                                            Comments = reader[2].ToString(),
                                            FdrType = reader[3].ToString(),
                                            PitchIndex = reader[4].ToString(),
                                            FoundTheItem = false
                                        };
                                        if (i == 4)
                                        {
                                            //groupBox2.Text += reader[7].ToString() + ".....Machine (" + m + ")";
                                        }
                                        if (i > 4 && reader[0].ToString() != "")
                                        {
                                            items.Add(abc);
                                            //countItems++;
                                        }

                                        //values.Add(value);

                                    }
                                }
                                conn.Close();
                            }
                        }
                        catch (IOException)
                        {
                        }
                    }
                    ls.Hide();
                    countItems = items.Count();
                    progressBar1.Maximum = items.Count();
                    progressBar2.Maximum = items.Count();

                    progressBar1.Value = items.Count();

                    Availableitems = items;
                    RepopulateAvailableTable();
                    textBox1.Enabled = true;
                    textBox1.Focus();

                }
                else
                {
                    MessageBox.Show("Select folder with *.XLS/*.XLSX files !");
                    button3.PerformClick();
                }
            }
        }
       
        private void RepopulateAvailableTable()
        {
            IEnumerable<BomItem> data = Availableitems;
            Atable.Clear();
            using (var reader = ObjectReader.Create(data))
            {
                Atable.Load(reader);
            }
            dataGridView1.DataSource = Atable.DefaultView;
            groupBox3.Text = "Avaliable items : " + Availableitems.Count.ToString() + "/" + countItems.ToString();
            styleFormatter(dataGridView1);
            progressBar1.RightToLeftLayout = true;
            progressBar1.Style = ProgressBarStyle.Blocks;
            if(Availableitems.Count>=0)
            {
                progressBar1.Value = Availableitems.Count;
            }
            else
            {
                progressBar1.Value = 0;
            }
            dataGridView1.Update();
        }
       
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //MoveItemFromAvaliableToFound(e.RowIndex);

            }
            catch (Exception)
            {
                //throw;
            }
        }
        private void RepopulateFoundTable()
        {
            Ftable.Clear();
            IEnumerable<BomItem> data = Founditems;
            using (var reader = ObjectReader.Create(data))
            {
                Ftable.Load(reader);
            }
            dataGridView2.DataSource = Ftable.DefaultView;
            groupBox5.Text = "Found items : " + Founditems.Count.ToString() + "/" + countItems.ToString();
            styleFormatter(dataGridView2);
            progressBar2.Value = Founditems.Count;
        }
       
        private void MoveItemFromAvaliableToFound(int index)
        {
            try
            {
                BomItem b = new BomItem
                {
                    SetNo = dataGridView1.Rows[index].Cells[dataGridView1.Columns["SetNo"].DisplayIndex].Value.ToString(),
                    CompName = dataGridView1.Rows[index].Cells[dataGridView1.Columns["CompName"].DisplayIndex].Value.ToString(),
                    Comments = dataGridView1.Rows[index].Cells[dataGridView1.Columns["Comments"].DisplayIndex].Value.ToString(),
                    FdrType = dataGridView1.Rows[index].Cells[dataGridView1.Columns["FdrType"].DisplayIndex].Value.ToString(),
                    PitchIndex = dataGridView1.Rows[index].Cells[dataGridView1.Columns["PitchIndex"].DisplayIndex].Value.ToString(),
                    FoundTheItem = true
                };
                Founditems.Add(b);
                var itemToRemove = Availableitems.Single(r => r.CompName == dataGridView1.Rows[index].Cells[dataGridView1.Columns["CompName"].DisplayIndex].Value.ToString());
                Availableitems.Remove(itemToRemove);
                SendToPrint(itemToRemove);
                RepopulateFoundTable();
                RepopulateAvailableTable();
                textBox1.Clear();
            }
            catch (Exception)
            {
                //throw;
            }
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            if(comboBox1.Text=="ENE_")
            {
                FilterAvaliableGW(comboBox1.Text + textBox1.Text);
            }
            else if(comboBox1.Text == "---_" && textBox1.Text.Length>14)
            {
                FilterAvaliableGW(textBox1.Text.Substring(4));
            }
            else
            {
                FilterAvaliableGW(textBox1.Text);
            }
        }

        private void FilterAvaliableGW(string searchString)
        {
            DataView dv = Atable.DefaultView;
            dv.RowFilter = "CompName LIKE '%" + searchString + "%'";
            dataGridView1.DataSource = dv;
            
            styleFormatter(dataGridView1);
        }

        private void textBox1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && textBox1.Text != string.Empty)
            {
                dataGridView2.ClearSelection();
                try
                {
                    MoveItemFromAvaliableToFound(dataGridView1.CurrentCell.RowIndex);
                }
                catch (Exception)
                {
                    MessageBox.Show(textBox1.Text + " Not found in AVALIABLE ITEMS list");
                    if (comboBox1.Text == "ENE_")
                    {
                        AlreadyFoundLogic(comboBox1.Text + textBox1.Text);
                    }
                    else if (comboBox1.Text == "---_" && textBox1.Text.Length > 14)
                    {
                        AlreadyFoundLogic(textBox1.Text.Substring(4));
                    }
                    else
                    {
                        AlreadyFoundLogic(textBox1.Text);
                    }
                    textBox1.Clear();
                    DataView dv = Atable.DefaultView;
                    dataGridView1.DataSource = dv;
                    dataGridView1.Update();
                    styleFormatter(dataGridView1);
                }
            }
        }

        private void AlreadyFoundLogic(string searchValue)
        {
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            try
            {
                foreach (DataGridViewRow row in dataGridView2.Rows)
                {
                    if (row.Cells[dataGridView1.Columns["CompName"].DisplayIndex].Value.ToString().Equals(searchValue))
                    {
                        MessageBox.Show(searchValue + " already exists in the FOUND ITEMS list !");
                        row.Selected = true;
                        dataGridView2.CurrentCell = dataGridView2.Rows[row.Index].Cells[dataGridView1.Columns["CompName"].DisplayIndex];
                        string pr = dataGridView2.Rows[row.Index].Cells[dataGridView1.Columns["SetNo"].DisplayIndex].Value.ToString();

                        PrintDocument p = new PrintDocument();
                        p.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
                        {
                            Margins margins = new Margins(0, 0, 0, 0);
                            p.DefaultPageSettings.Margins = margins;
                            e1.Graphics.DrawString(pr, new Font("Arial", 14, FontStyle.Bold), new SolidBrush(Color.Black), new RectangleF(5, 5, 170, 90));
                        };
                        try
                        {
                            p.Print();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Exception Occured While Printing", ex);
                        }

                        break;
                    }
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            RepopulateAvailableTable();
        }

        private void SendToPrint(BomItem itemToRemove)
        {
            string s = itemToRemove.CompName.ToString() + " " + itemToRemove.SetNo.ToString();
            string pr = itemToRemove.SetNo.ToString();
            textBox2.Text= s+" "+ DateTime.Now.ToString("HH:mm:ss");
            if (itemToRemove.SetNo.StartsWith("M1-"))
                {
                textBox2.BackColor= Color.LightGreen;
            }
            else
            {
                textBox2.BackColor = Color.PaleVioletRed;
            }
            PrintDocument p = new PrintDocument();
            p.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
            {
                Margins margins = new Margins(0, 0, 0, 0);
                p.DefaultPageSettings.Margins = margins;
                e1.Graphics.DrawString(pr, new Font("Arial", 14, FontStyle.Bold), new SolidBrush(Color.Black), new RectangleF(5, 5, 170, 90));
            };
            try
            {
                //p.Print();
                addToXML();
            }
            catch (Exception ex)
            {
                throw new Exception("Exception Occured While Printing", ex);
            }
        }

    

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }
        private void styleFormatter(DataGridView dgw)
        {
            dgw.Columns["CompName"].DisplayIndex = 1;
            dgw.Columns["Comments"].DisplayIndex = 2;
            dgw.Columns["FdrType"].DisplayIndex = 3;
            dgw.Columns["PitchIndex"].DisplayIndex =4;
            dgw.Columns["SetNo"].DisplayIndex =5;
            dgw.Columns["FoundTheItem"].Visible = false;

            dgw.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.AutoResizeColumns();

            int setNoColIndex = dgw.Columns["SetNo"].DisplayIndex;

            foreach (DataGridViewRow r in dgw.Rows)
            {
                //MessageBox.Show(r.Cells[4].Value.ToString());
                if (r.Cells[setNoColIndex].Value.ToString().StartsWith("M1"))
                {
                    dgw.Rows[r.Index].Cells[setNoColIndex].Style.BackColor =  Color.LightGreen;
                }
                else if (r.Cells[setNoColIndex].Value.ToString().StartsWith("M2"))
                {
                    dgw.Rows[r.Index].Cells[setNoColIndex].Style.BackColor = Color.PaleVioletRed;
                }
            }
        }
        private void addToXML()
        {
            List<BomItem> allData = new List<BomItem>();
            allData.AddRange(Founditems);
            allData.AddRange(Availableitems);
            string s= SerializeToXml(allData);
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(s);
            string theLogFileName = loadedDirNameCSPS + DateTime.Now.ToString("_yyMMddHHmm")+ ".log";
            xdoc.Save(theLogFileName);
        }
        private void loadFromXML()
        {

        }
        public string SerializeToXml(object input)
        {
            XmlSerializer ser = new XmlSerializer(input.GetType(), "http://schemas.yournamespace.com");
            string result = string.Empty;

            using (MemoryStream memStm = new MemoryStream())
            {
                ser.Serialize(memStm, input);

                memStm.Position = 0;
                result = new StreamReader(memStm).ReadToEnd();
            }

            return result;
        }
        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
          
        }

        private void button1_Click(object sender, EventArgs e)
        {
            items.Clear();
            Availableitems.Clear();
            Founditems.Clear();
            Atable.Clear();
            Ftable.Clear();
            groupBox5.ResetText();
            groupBox3.ResetText();



            //int size = -1;
           openFileDialog2.InitialDirectory = "\\\\dbr1\\Data\\SMT\\SETUP";
            //openFileDialog1.InitialDirectory = "\\\\dbr1\\Data\\SMT\\SETUP";
             openFileDialog2.Filter = "LOG files(*.log) | *.log";
            //DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            DialogResult result = openFileDialog2.ShowDialog();
            openFileDialog2.Multiselect = false;
            string foldefileName = openFileDialog2.FileName;
           
            MessageBox.Show(foldefileName.ToString());

            label1.Text += foldefileName.ToString() + "\n";
            groupBox2.Text += foldefileName.ToString() + " ";



            //if (result == DialogResult.OK && Directory.EnumerateFiles(folderPath, "*.log").Count() > 0) // Test result.
            //{
            //    label1.Text = "";
            //    groupBox2.Text = "";
            //    frmLoadingScreen ls = new frmLoadingScreen();
            //    ls.Show();
            //    loadedDirNameCSPS = folderPath.ToString();
            //}
        }

    }
}