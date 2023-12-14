using FastMember;
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
using System.Reflection.Emit;
using System.Reflection.Metadata;
using Label = System.Windows.Forms.Label;
using WH_Panel;
using GroupBox = System.Windows.Forms.GroupBox;
using System.Collections;
using System.ComponentModel;
using TextBox = System.Windows.Forms.TextBox;
using ComboBox = System.Windows.Forms.ComboBox;
using Button = System.Windows.Forms.Button;

namespace SMTsetup
{
    public partial class SMTSetupMain : Form
    {
        public List<ClientWarehouse> WarehouseList { get; set; }
        public int progressCounter
        {
            set { countItems = value; }
        }
        public List<WHitem> stockItems = new List<WHitem>();
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
            progressBar1.Minimum = 0;
            progressBar2.Minimum = 0;
            progressBar2.Value = 0;
            textBox1.Enabled = false;
            comboBox1.SelectedIndex = 0;
            DateTime fileModifiedDate = File.GetLastWriteTime(@"SMTsetup.exe");
            this.Text = "SMT setup Updated " + fileModifiedDate.ToString(); ;
            UpdateControlColors(this);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            WarehouseList = PopulateWarehouses();
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        private async void Blink()
        {
            //while (true)
            for (int i = 0; i < 5; i++)
            {
                await Task.Delay(500);
                label2.ForeColor = Color.Black;
                label2.BackColor = Color.Yellow;
                await Task.Delay(500);
                label2.ForeColor = Color.White;
                label2.BackColor = Color.Red;
            }
        }
        public List<ClientWarehouse> PopulateWarehouses()
        {
            string directoryPath = "\\\\dbr1\\Data\\WareHouse\\STOCK_CUSTOMERS";
            List<ClientWarehouse> warehouses = new List<ClientWarehouse>();
            // Get all subdirectories under the specified directory
            string[] subDirectories = Directory.GetDirectories(directoryPath);
            foreach (string subDir in subDirectories)
            {
                string clName = new DirectoryInfo(subDir).Name;
                string clPrefix = GetPrefixFromFile(Path.Combine(subDir, "prefix.txt"));
                string clAvlFile = Directory.GetFiles(subDir, "*_AVL.XLSM").FirstOrDefault();
                string clStockFile = Directory.GetFiles(subDir, "*_STOCK.XLSM").FirstOrDefault();
                string clLogoFile = Directory.GetFiles(subDir, "logo.png").FirstOrDefault();
                if (!string.IsNullOrEmpty(clAvlFile) && !string.IsNullOrEmpty(clStockFile))
                {
                    ClientWarehouse warehouse = new ClientWarehouse
                    {
                        clName = clName,
                        clPrefix = clPrefix,
                        clAvlFile = clAvlFile,
                        clStockFile = clStockFile,
                        clLogo = clLogoFile
                    };
                    warehouses.Add(warehouse);
                }
            }
            return warehouses;
        }
        private string GetPrefixFromFile(string prefixFilePath)
        {
            if (File.Exists(prefixFilePath))
            {
                try
                {
                    return File.ReadAllText(prefixFilePath).Trim();
                }
                catch (Exception)
                {
                    // Handle any exceptions that may occur while reading the prefix file
                }
            }
            // Return a default value if the prefix file is missing or invalid
            return string.Empty;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //dataGridView1.DataSource = null;
            //dataGridView2.DataSource = null;
            label2.Text = string.Empty;
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
                                    }
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
                        string contents = File.ReadAllText(file);
                        string thesheetName = (System.IO.Path.GetFileNameWithoutExtension(file)).ToString();
                        m = thesheetName.Substring(thesheetName.Length - 1);
                        label1.Text += file + "\n";
                        groupBox2.Text += file + " ";
                        try
                        {
                            string constr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + file + "; Extended Properties=\"Excel 12.0 Xml;HDR=NO;\"";
                            using (OleDbConnection conn = new OleDbConnection(constr))
                            {
                                conn.Open();
                                OleDbCommand command = new OleDbCommand("Select * from [" + thesheetName + "$]", conn);
                                OleDbDataReader reader = command.ExecuteReader();
                                if (reader.HasRows)
                                {
                                    int i = 0;
                                    while (reader.Read())
                                    {
                                        i += 1;
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
                                        }
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
            groupBox3.Text = "Avaliable items : " + Availableitems.Count.ToString() + "/" + (Availableitems.Count + Founditems.Count).ToString();
            styleFormatter(dataGridView1);
            progressBar1.RightToLeftLayout = true;
            progressBar1.Style = ProgressBarStyle.Blocks;
            if (Availableitems.Count >= 0)
            {
                progressBar1.Value = Availableitems.Count;
            }
            else
            {
                progressBar1.Value = 0;
            }
            dataGridView1.Update();

            SetTheClientLogo();

        }
        private void SetTheClientLogo()
        {
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.Image = null;

            if (dataGridView1 != null)
            {
                foreach (ClientWarehouse w in WarehouseList)
                {
                    if (w != null && dataGridView1.Rows[0].Cells[1].Value.ToString().StartsWith(w.clPrefix))
                    {
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        pictureBox1.Image = Image.FromFile(w.clLogo);
                        break;
                    }
                }
            }
            else
            {
                foreach (ClientWarehouse w in WarehouseList)
                {
                    if (w != null && dataGridView2.Rows[0].Cells[1].Value.ToString().StartsWith(w.clPrefix))
                    {
                        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                        pictureBox1.Image = Image.FromFile(w.clLogo);
                        break;
                    }
                }
            }



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
            progressBar2.Maximum = Availableitems.Count + Founditems.Count;
            Ftable.Clear();
            IEnumerable<BomItem> data = Founditems;
            using (var reader = ObjectReader.Create(data))
            {
                Ftable.Load(reader);
            }
            dataGridView2.DataSource = Ftable.DefaultView;
            groupBox5.Text = "Found items : " + Founditems.Count.ToString() + "/" + (Availableitems.Count + Founditems.Count).ToString();
            styleFormatter(dataGridView2);
            progressBar2.Value = Founditems.Count;
        }
        private void MoveItemFromAvaliableToFound(int index)
        {
            try
            {
                BomItem b = new BomItem
                {
                    SetNo = dataGridView1.Rows[index].Cells[dataGridView1.Columns["SetNo"].DisplayIndex + 1].Value.ToString(),
                    CompName = dataGridView1.Rows[index].Cells[dataGridView1.Columns["CompName"].DisplayIndex + 1].Value.ToString(),
                    Comments = dataGridView1.Rows[index].Cells[dataGridView1.Columns["Comments"].DisplayIndex + 1].Value.ToString(),
                    FdrType = dataGridView1.Rows[index].Cells[dataGridView1.Columns["FdrType"].DisplayIndex + 1].Value.ToString(),
                    PitchIndex = dataGridView1.Rows[index].Cells[dataGridView1.Columns["PitchIndex"].DisplayIndex + 1].Value.ToString(),
                    FoundTheItem = true
                };
                //MessageBox.Show(dataGridView1.Rows[index].Cells[dataGridView1.Columns["SetNo"].DisplayIndex+1].Value.ToString());
                Founditems.Add(b);
                var itemToRemove = Availableitems.Single(r => r.CompName == dataGridView1.Rows[index].Cells[dataGridView1.Columns["CompName"].DisplayIndex + 1].Value.ToString());
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

            if (comboBox1.Text == "---_" && textBox1.Text.Length > 14)
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
            if (e.KeyCode == Keys.Enter && textBox1.Text != "")
            {
                dataGridView2.ClearSelection();
                try
                {
                    MoveItemFromAvaliableToFound(dataGridView1.CurrentCell.RowIndex);
                }
                catch (Exception)
                {
                    label2.Text = textBox1.Text + " Not found in AVALIABLE ITEMS list";
                    label2.BackColor = Color.Red;
                    Blink();
                    //if (comboBox1.Text == "ENE_")
                    //{
                    //    AlreadyFoundLogic(comboBox1.Text + textBox1.Text);
                    //}
                    if (comboBox1.Text == "---_" && textBox1.Text.Length > 14)
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
                    if (row.Cells[dataGridView2.Columns["CompName"].DisplayIndex + 1].Value.ToString().Equals(searchValue))
                    {
                        label2.Text = searchValue + " already exists in the FOUND ITEMS list !";
                        label2.BackColor = Color.Red;
                        Blink();
                        row.Selected = true;
                        dataGridView2.CurrentCell = dataGridView2.Rows[row.Index].Cells[dataGridView1.Columns["CompName"].DisplayIndex + 1];
                        string pr = dataGridView2.Rows[row.Index].Cells[dataGridView1.Columns["SetNo"].DisplayIndex + 1].Value.ToString();
                        PrintDocument p = new PrintDocument();
                        p.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
                        {
                            Margins margins = new Margins(0, 0, 0, 0);
                            p.DefaultPageSettings.Margins = margins;
                            e1.Graphics.DrawString(pr, new Font("Arial", 14, FontStyle.Bold), new SolidBrush(Color.Black), new RectangleF(5, 5, 170, 90));
                        };
                        try
                        {
                            if (Environment.UserName == "lgt")
                            {
                                //
                            }
                            else
                            {
                                p.Print();
                            }
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
            textBox2.Text = s + " " + DateTime.Now.ToString("HH:mm:ss");
            if (itemToRemove.SetNo.StartsWith("M1-"))
            {
                textBox2.BackColor = Color.DarkGreen;
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
                if (Environment.UserName == "lgt")
                {
                    //
                }
                else
                {
                    p.Print();
                }

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
            dgw.Columns["CompName"].DisplayIndex = 0;
            dgw.Columns["Comments"].DisplayIndex = 1;
            dgw.Columns["FdrType"].DisplayIndex = 2;
            dgw.Columns["PitchIndex"].DisplayIndex = 3;
            dgw.Columns["SetNo"].DisplayIndex = 4;
            dgw.Columns["FoundTheItem"].DisplayIndex = 5;
            dgw.Columns["FoundTheItem"].Visible = false;
            dgw.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.AutoResizeColumns();
            int setNoColIndex = dgw.Columns["SetNo"].DisplayIndex + 1;
            foreach (DataGridViewRow r in dgw.Rows)
            {
                if (r.Cells[setNoColIndex].Value.ToString().StartsWith("M1"))
                {
                    dgw.Rows[r.Index].Cells[setNoColIndex].Style.BackColor = Color.DarkGreen;
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
            string s = SerializeToXml(allData);
            XmlDocument xdoc = new XmlDocument();

            string theTimeStamp = DateTime.Now.ToString("_yyMMdd");
            string theLogFileName = loadedDirNameCSPS + theTimeStamp + ".log";
            try
            {
                xdoc.Load(theLogFileName);
                xdoc.LoadXml(s);
                xdoc.Save(theLogFileName);
            }
            catch (Exception)
            {
                xdoc.LoadXml(s);
                xdoc.Save(theLogFileName);
            }




        }
        private void loadFromXML()
        {
            openFileDialog2.InitialDirectory = "\\\\dbr1\\Data\\SMT\\SETUP";
            openFileDialog2.Filter = "LOG files(*.log) | *.log";
            openFileDialog2.Multiselect = false;
            List<BomItem> BomItemS = new List<BomItem>();
            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                string foldefileName = openFileDialog2.FileName;
                label1.Text += foldefileName.ToString() + "\n";
                groupBox2.Text += foldefileName.ToString() + " ";
                XmlSerializer serializer = new XmlSerializer(typeof(List<BomItem>));
                using (StreamReader reader = new StreamReader(openFileDialog2.FileName))
                {
                    BomItemS = (List<BomItem>)serializer.Deserialize(reader);
                }
            }
            if (BomItemS != null && BomItemS.Count > 0)
            {
                for (int i = 0; i < BomItemS.Count; i++)
                {
                    if (BomItemS[i].FoundTheItem == true)
                    {
                        Founditems.Add(BomItemS[i]);
                    }
                    else
                    {
                        Availableitems.Add(BomItemS[i]);
                    }
                }
            }
        }
        public string SerializeToXml(object input)
        {
            XmlSerializer ser = new XmlSerializer(input.GetType(), "");
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
        private void btnLoadFromLogFile_Click(object sender, EventArgs e)
        {
            label2.Text = string.Empty;
            items.Clear();
            Availableitems.Clear();
            Founditems.Clear();
            Atable.Clear();
            Ftable.Clear();
            groupBox5.ResetText();
            groupBox3.ResetText();
            loadFromXML();
            RepopulateAvailableTable();
            RepopulateFoundTable();
            textBox1.Enabled = true;
        }

        public DataTable stockDTable = new DataTable();
        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Get the current cell or row indices
                int currentRow = e.RowIndex;
                int currentColumn = e.ColumnIndex + 1;

                // Check if the click is within the valid cell range
                if (currentRow >= 0 && currentColumn >= 0)
                {
                    // Get the cell value
                    //object cellValue = dataGridView1.Rows[currentRow].Cells[0].Value;

                    // Create and configure the IPNdetails form dynamically
                    Form ipnDetailsForm = new Form();
                    ipnDetailsForm.Text = "IPN Details";
                    //ipnDetailsForm.Size = new Size(500, 500);


                    Label labelCellValue = new Label();

                    if (WarehouseList.Count > 0)
                    {

                        ipnDetailsForm.Text = "IPN Details";

                        // Create a TableLayoutPanel to organize controls
                        TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
                        tableLayoutPanel.Dock = DockStyle.Fill;
                        tableLayoutPanel.RowCount = 2; // Two rows
                        ipnDetailsForm.Controls.Add(tableLayoutPanel);

                        // Create a GroupBox to contain the DataGridView
                        GroupBox groupBoxDetails = new GroupBox();
                        groupBoxDetails.Text = "IPN details";

                        tableLayoutPanel.Controls.Add(groupBoxDetails, 0, 0); // First row
                        groupBoxDetails.Dock = DockStyle.Fill;


                        GroupBox groupBoxWHmovements = new GroupBox();
                        groupBoxWHmovements.Text = "WAREHOUSE movements for IPN";
                        tableLayoutPanel.Controls.Add(groupBoxWHmovements, 0, 1);
                        groupBoxWHmovements.Dock = DockStyle.Fill;

                        DataGridView dataGridViewWarehouseMovements = new DataGridView();
                        dataGridViewWarehouseMovements.Dock = DockStyle.Fill;
                        dataGridViewWarehouseMovements.ReadOnly = true;
                        dataGridViewWarehouseMovements.AllowUserToAddRows = false;
                        groupBoxWHmovements.Controls.Add(dataGridViewWarehouseMovements);


                        // Create a DataGridView to display the row contents
                        DataGridView dataGridViewDetails = new DataGridView();


                        groupBoxDetails.Controls.Add(dataGridViewDetails);
                        dataGridViewDetails.AutoSize = true;
                        dataGridViewDetails.Dock = DockStyle.Fill;
                        dataGridViewDetails.ReadOnly = true;
                        dataGridViewDetails.AllowUserToAddRows = false;



                        // Add columns to the DataGridView (assuming the column names are strings)
                        foreach (DataGridViewColumn column in dataGridView1.Columns)
                        {
                            dataGridViewDetails.Columns.Add(column.Name, column.HeaderText);
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        }


                        // Add a row to the DataGridView with the contents of the clicked row
                        dataGridViewDetails.Rows.Add();
                        for (int i = 0; i < dataGridView1.Columns.Count; i++)
                        {
                            dataGridViewDetails.Rows[0].Cells[i].Value = dataGridView1.Rows[currentRow].Cells[i].Value;

                        }
                        dataGridViewDetails.Columns["CompName"].DisplayIndex = 0;

                        ipnDetailsForm.Text = dataGridViewDetails.Rows[0].Cells[1].Value.ToString();

                        foreach (DataGridViewColumn column in dataGridViewDetails.Columns)
                        {
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        }
                        dataGridViewDetails.AutoResizeColumns();
                        // Add controls to the TableLayoutPanel
                       
                            foreach (ClientWarehouse w in WarehouseList)
                            {
                                if (w != null && dataGridViewDetails.Rows[0].Cells[1].Value.ToString().StartsWith(w.clPrefix))
                                {
                                    dataGridViewWarehouseMovementsDataLoader(w.clStockFile, "STOCK");
                                    break;
                                }
                            }
                        


                        // dataGridViewWarehouseMovements.DataSource = stockItems.Select(x => x.IPN == dataGridViewDetails.Rows[0].Cells[1].Value.ToString()).ToList();
                        // dataGridViewWarehouseMovements.Update();
                        // Subscribe to the CellFormatting event
                        dataGridViewWarehouseMovements.CellFormatting += (sender, e) =>
                        {
                            // Check if the current column is the STOCK column
                            if (e.ColumnIndex == dataGridViewWarehouseMovements.Columns["STOCK"].Index && e.RowIndex >= 0)
                            {
                                // Get the value in the STOCK column
                                int stockValue;
                                if (int.TryParse(e.Value?.ToString(), out stockValue))
                                {
                                    // Apply coloring logic based on the STOCK value
                                    if (stockValue <= 0)
                                    {
                                        e.CellStyle.BackColor = Color.IndianRed;
                                    }
                                    else
                                    {
                                        e.CellStyle.BackColor = Color.DarkGreen;
                                    }
                                }
                            }
                        };

                        IEnumerable<WHitem> data = stockItems;
                        stockDTable.Clear();
                        using (var reader = ObjectReader.Create(data))
                        {
                            stockDTable.Load(reader);
                        }

                        DataView dv = stockDTable.DefaultView;
                        dv.RowFilter = "[IPN] LIKE '%" + dataGridViewDetails.Rows[0].Cells[1].Value.ToString() +
                            "%'";
                        dataGridViewWarehouseMovements.DataSource = dv;
                        foreach (DataGridViewColumn column in dataGridViewWarehouseMovements.Columns)
                        {
                            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                        }
                        SetSTOCKiewColumsOrder(dataGridViewWarehouseMovements);

                        // Calculate the size of the form based on the DataGridView content and the GroupBox
                        //int formWidth = dataGridViewDetails.Width + 20; // Add a margin
                        //int formHeight = dataGridViewDetails.Height + 65; // Add a margin

                        // Set the size of the form
                        ipnDetailsForm.Size = new Size(1200, 900);

                    }
                    else
                    {
                        labelCellValue.Text = "ERROR loading warehouses";
                        labelCellValue.Location = new Point(10, 10);
                        ipnDetailsForm.Controls.Add(labelCellValue);
                    }


                    ipnDetailsForm.WindowState = FormWindowState.Maximized;
                    ipnDetailsForm.StartPosition = FormStartPosition.CenterScreen;
                    // Show the form
                    UpdateControlColors(ipnDetailsForm);
                    ipnDetailsForm.ShowDialog();
                }
            }
        }
        private void SetSTOCKiewColumsOrder(DataGridView dgw)
        {

            dgw.Columns["IPN"].DisplayIndex = 0;
            dgw.Columns["Manufacturer"].DisplayIndex = 1;
            dgw.Columns["MFPN"].DisplayIndex = 2;
            dgw.Columns["Description"].DisplayIndex = 3;
            dgw.Columns["Stock"].DisplayIndex = 4;
            dgw.Columns["UpdatedOn"].DisplayIndex = 5;
            dgw.Columns["ReelBagTrayStick"].DisplayIndex = 6;
            dgw.Columns["SourceRequester"].DisplayIndex = 7;
            dgw.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[5].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[6].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Columns[7].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgw.Sort(dgw.Columns["UpdatedOn"], ListSortDirection.Descending);
        }
        private void dataGridViewWarehouseMovementsDataLoader(string fp, string thesheetName)
        {
            try
            {
                string constr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fp + "; Extended Properties=\"Excel 12.0 Macro;HDR=YES;IMEX=0\"";
                using (OleDbConnection conn = new OleDbConnection(constr))
                {
                    conn.Open();
                    OleDbCommand command = new OleDbCommand("Select * from [" + thesheetName + "$]", conn);
                    OleDbDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                int res = 0;
                                int toStk;
                                bool stk = int.TryParse(reader[4].ToString(), out res);
                                if (stk)
                                {
                                    toStk = res;
                                }
                                else
                                {
                                    toStk = 0;
                                }
                                WHitem abc = new WHitem
                                {
                                    IPN = reader[0].ToString(),
                                    Manufacturer = reader[1].ToString(),
                                    MFPN = reader[2].ToString(),
                                    Description = reader[3].ToString(),
                                    Stock = toStk,
                                    UpdatedOn = reader[5].ToString(),
                                    ReelBagTrayStick = reader[6].ToString(),
                                    SourceRequester = reader[7].ToString()
                                };


                                stockItems.Add(abc);

                            }
                            catch (Exception E)
                            {
                                MessageBox.Show(E.Message);
                                throw;
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Error");
            }
        }
        private void UpdateControlColors(Control parentControl)
        {
            foreach (Control control in parentControl.Controls)
            {
                // Update control colors based on your criteria
                control.BackColor = Color.LightGray;
                control.ForeColor = Color.White;
                // Handle Button controls separately
                if (control is Button button)
                {
                    button.FlatStyle = FlatStyle.Flat; // Set FlatStyle to Flat
                    button.FlatAppearance.BorderColor = Color.DarkGray; // Change border color
                    button.ForeColor = Color.Black;
                }
                // Handle Button controls separately
                if (control is GroupBox groupbox)
                {
                    groupbox.FlatStyle = FlatStyle.Flat; // Set FlatStyle to Flat
                    groupbox.ForeColor = Color.Black;
                }
                // Handle TextBox controls separately
                if (control is TextBox textBox)
                {
                    textBox.BorderStyle = BorderStyle.FixedSingle; // Set border style to FixedSingle
                    textBox.BackColor = Color.LightGray; // Change background color
                    textBox.ForeColor = Color.Black; // Change text color
                }
                // Handle Label controls separately
                if (control is Label label)
                {
                    label.BorderStyle = BorderStyle.FixedSingle; // Set border style to FixedSingle
                    label.BackColor = Color.Gray; // Change background color
                    label.ForeColor = Color.Black; // Change text color
                }
                // Handle TabControl controls separately
                if (control is TabControl tabControl)
                {
                    //tabControl.BackColor = Color.Black; // Change TabControl background color
                    tabControl.ForeColor = Color.Black;
                    // Handle each TabPage within the TabControl
                    foreach (TabPage tabPage in tabControl.TabPages)
                    {
                        tabPage.BackColor = Color.Gray; // Change TabPage background color
                        tabPage.ForeColor = Color.Black; // Change TabPage text color
                    }
                }
                // Handle DataGridView controls separately
                if (control is DataGridView dataGridView)
                {
                    // Update DataGridView styles
                    dataGridView.EnableHeadersVisualStyles = false;
                    dataGridView.BackgroundColor = Color.DarkGray;
                    dataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.Gray;
                    dataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    dataGridView.RowHeadersDefaultCellStyle.BackColor = Color.Gray;
                    dataGridView.DefaultCellStyle.BackColor = Color.Gray;
                    dataGridView.DefaultCellStyle.ForeColor = Color.White;
                    dataGridView.DefaultCellStyle.SelectionBackColor = Color.Green;
                    dataGridView.DefaultCellStyle.SelectionForeColor = Color.White;
                    // Change the header cell styles for each column
                    foreach (DataGridViewColumn column in dataGridView.Columns)
                    {
                        column.HeaderCell.Style.BackColor = Color.DarkGray;
                        column.HeaderCell.Style.ForeColor = Color.Black;
                    }
                }
                // Handle ComboBox controls separately
                if (control is ComboBox comboBox)
                {
                    comboBox.FlatStyle = FlatStyle.Flat; // Set FlatStyle to Flat
                    comboBox.BackColor = Color.DarkGray; // Change ComboBox background color
                    comboBox.ForeColor = Color.Black; // Change ComboBox text color
                }
                // Handle DateTimePicker controls separately
                if (control is DateTimePicker dateTimePicker)
                {
                    // Change DateTimePicker's custom properties here
                    dateTimePicker.BackColor = Color.DarkGray; // Change DateTimePicker background color
                    dateTimePicker.ForeColor = Color.White; // Change DateTimePicker text color
                                                            // Customize other DateTimePicker properties as needed
                }
                // Recursively update controls within containers
                if (control.Controls.Count > 0)
                {
                    UpdateControlColors(control);
                }
            }
        }
    }
}