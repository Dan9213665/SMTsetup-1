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
        string m = "";
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
                                        Comment = reader[2].ToString(),
                                        FdrType = reader[3].ToString(),
                                        PitchIndex = reader[4].ToString()
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
                MessageBox.Show("Select folder with *.XLS files !");
                button3.PerformClick();
            }
           

            //if (result == DialogResult.OK) // Test result.
            //{

            //    string file = openFileDialog1.FileName;

            //    string thesheetName = (System.IO.Path.GetFileNameWithoutExtension(openFileDialog1.FileName)).ToString();
            //    //MessageBox.Show(thesheetName);
            //    m = thesheetName.Substring(thesheetName.Length - 1);
            //    label1.Text = file;
            //    groupBox2.Text = file;
            //    try
            //    {
            //        //string text = File.ReadAllText(file);
            //        // size = text.Length;
            //        //List<string> values = new List<string>();
            //        string constr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + file + "; Extended Properties=\"Excel 12.0 Xml;HDR=NO;\"";
            //        //MessageBox.Show(constr.ToString());
            //        using (OleDbConnection conn = new OleDbConnection(constr))
            //        {
            //            conn.Open();
            //            //OleDbCommand command = new OleDbCommand("Select * from [Sheet1$]", conn);
            //            OleDbCommand command = new OleDbCommand("Select * from [" + thesheetName + "$]", conn);
            //            OleDbDataReader reader = command.ExecuteReader();
            //            if (reader.HasRows)
            //            {
            //                int i = 0;
            //                while (reader.Read())
            //                {
            //                    i += 1;
            //                    // this assumes just one column, and the value is text
            //                    //string value = reader[0].ToString();
            //                    BomItem abc = new BomItem
            //                    {
            //                        SetNo = m + "-" + reader[0].ToString(),
            //                        CompName = reader[1].ToString(),
            //                        Comment = reader[2].ToString(),
            //                        FdrType = reader[3].ToString(),
            //                        PitchIndex = reader[4].ToString()
            //                    };
            //                    if (i == 4)
            //                    {
            //                        groupBox2.Text = reader[7].ToString() + ".....Machine (" + m + ")";
            //                    }
            //                    if (i > 4 && reader[0].ToString() != "")
            //                    {
            //                        items.Add(abc);
            //                        //countItems++;
            //                    }

            //                    //values.Add(value);

            //                }
            //            }
            //            conn.Close();
            //        }
            //        countItems = items.Count();
            //        Availableitems = items;
            //        RepopulateAvailableTable();
            //        textBox1.Enabled = true;
            //        textBox1.Focus();
            //    }
            //    catch (IOException)
            //    {
            //    }
            //}
            //Console.WriteLine(size); // <-- Shows file size in debugging mode.
            //Console.WriteLine(result); // <-- For debugging use.


        }
       
        private void RepopulateAvailableTable()
        {
            IEnumerable<BomItem> data = Availableitems;
            Atable.Clear();
            using (var reader = ObjectReader.Create(data))
            {
                Atable.Load(reader);
            }
            dataGridView1.DataSource = Atable;
            groupBox3.Text = "Avaliable items : " + Availableitems.Count.ToString() + "/" + countItems.ToString();


            
            
            this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridView1.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridView1.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
           
            dataGridView1.AutoResizeColumns();
            styleFormatter(dataGridView1);

            progressBar1.RightToLeftLayout = true;
            
            
            progressBar1.Style = ProgressBarStyle.Blocks;
            //progressBar1.SetState(2);
            //progressBar1.ForeColor = Color.Gray;
            //progressBar1.BackColor = Color.PaleVioletRed;
            if(Availableitems.Count>=0)
            {
                progressBar1.Value = Availableitems.Count;
            }
            else
            {
                progressBar1.Value = 0;
                //progressBar1.Refresh();
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
            IEnumerable<BomItem> data = Founditems;
            Ftable.Clear();
            using (var reader = ObjectReader.Create(data))
            {
                Ftable.Load(reader);
            }
            dataGridView2.DataSource = Ftable;

            groupBox5.Text = "Found items : " + Founditems.Count.ToString() + "/" + countItems.ToString();
            this.dataGridView2.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridView2.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridView2.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridView2.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dataGridView2.Columns[4].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView2.AutoResizeColumns();
            styleFormatter(dataGridView2);

            
            progressBar2.Value = Founditems.Count;
           
            
        }
       
        private void MoveItemFromAvaliableToFound(int index)
        {
            try
            {
                //MessageBox.Show(dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString());
                BomItem b = new BomItem
                {

                    SetNo = dataGridView1.Rows[index].Cells[4].Value.ToString(),
                    CompName = dataGridView1.Rows[index].Cells[1].Value.ToString(),
                    Comment = dataGridView1.Rows[index].Cells[0].Value.ToString(),
                    FdrType = dataGridView1.Rows[index].Cells[2].Value.ToString(),
                    PitchIndex = dataGridView1.Rows[index].Cells[3].Value.ToString()
                };
                Founditems.Add(b);
                var itemToRemove = Availableitems.Single(r => r.CompName == dataGridView1.Rows[index].Cells[1].Value.ToString());
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
            
            DataView dv = Atable.DefaultView;
            dv.RowFilter = "CompName LIKE '%" + textBox1.Text + "%'";
            dataGridView1.DataSource = dv;
            styleFormatter(dataGridView1);
        }

        private void textBox1_KeyDown_1(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Enter && textBox1.Text != "")
            {
                dataGridView2.ClearSelection();

                // DataView dv2 = Ftable.DefaultView;
                //dataGridView2.DataSource = dv2;
                //styleFormatter(dataGridView2);

                try
                {
                    MoveItemFromAvaliableToFound(dataGridView1.CurrentCell.RowIndex);
                }
                catch (Exception)
                {

                    MessageBox.Show(textBox1.Text + " Not found in AVALIABLE ITEMS list");

                    //dv2 = Ftable.DefaultView;
                    //dv2.RowFilter = "CompName LIKE '%" + textBox1.Text + "%'";
                    //dataGridView2.DataSource = dv2;
                    //styleFormatter(dataGridView2);
                    string searchValue = comboBox1.SelectedItem.ToString()+ textBox1.Text;

                    //MessageBox.Show(searchValue);

                    dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    try
                    {
                        foreach (DataGridViewRow row in dataGridView2.Rows)
                        {
                            if (row.Cells[1].Value.ToString().Equals(searchValue))
                            {
                                MessageBox.Show(searchValue +" already exists in the FOUND ITEMS list !");
                                row.Selected = true;
                                dataGridView2.CurrentCell = dataGridView2.Rows[row.Index].Cells[0];
                                string pr = dataGridView2.Rows[row.Index].Cells[4].Value.ToString();

                                PrintDocument p = new PrintDocument();
                                p.PrintPage += delegate (object sender1, PrintPageEventArgs e1)
                                {
                                    Margins margins = new Margins(0, 0, 0, 0);
                                    p.DefaultPageSettings.Margins = margins;
                                    e1.Graphics.DrawString(pr, new Font("Times New Roman", 17, FontStyle.Bold), new SolidBrush(Color.Black), new RectangleF(5, 5, 180, 90));
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


                    textBox1.Clear();
                    DataView dv = Atable.DefaultView;
                    dataGridView1.DataSource = dv;
                    styleFormatter(dataGridView1);
                    
                }
            }
        }
        private void SendToPrint(BomItem itemToRemove)
        {
            //MessageBox.Show(itemToRemove.CompName.ToString() + " " + itemToRemove.SetNo.ToString());
            string s = itemToRemove.CompName.ToString() + " " + itemToRemove.SetNo.ToString();
            string pr = itemToRemove.SetNo.ToString();
            //MessageBox.Show(s,"",MessageBoxButtons.OK,MessageBoxIcon.Information);
            
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
                e1.Graphics.DrawString(pr, new Font("Times New Roman", 17,FontStyle.Bold), new SolidBrush(Color.Black), new RectangleF(5, 5, 180, 90));
            };
            try
            {
                p.Print();
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
            foreach (DataGridViewRow r in dgw.Rows)
            {
                //MessageBox.Show(r.Cells[4].Value.ToString());
                if (r.Cells[4].Value.ToString().StartsWith("M1"))
                {
                    dgw.Rows[r.Index].Cells[4].Style.BackColor = Color.LightGreen;
                }
                else if (r.Cells[4].Value.ToString().StartsWith("M2"))
                {
                    dgw.Rows[r.Index].Cells[4].Style.BackColor = Color.PaleVioletRed;
                }

            }
        }
    }
}