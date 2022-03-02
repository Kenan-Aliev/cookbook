using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Lab1_RKP
{
    public partial class ProductsForm : Form
    {

        FormsSettings formsSettings;
        static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        SqlConnection sqlConnection = new SqlConnection(connectionString);

        private DataSet ds;
        private SqlDataAdapter adapter;

        private SqlCommandBuilder commandBuilder;

        private DataTable productsTable;
        private DataTable unitsTable;
        private DataTable dataGridViewTable;


        private int updateProductId = -1;
        private int deleteProductId = -1;
        private string selectedUnit;
        public ProductsForm()
        {
            InitializeComponent();
            formsSettings = new FormsSettings("Продукты");
            Text = formsSettings.Text;
            this.BackColor = formsSettings.BackColor;
            this.Location = formsSettings.Location;
            this.Load += productsForm_Load;
        }

        private void productsForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Открываем подключение
                sqlConnection.Open();
                adapter = new SqlDataAdapter("Select * from products order by product_name;Select * from units order by unit_name", sqlConnection);
                ds = new DataSet();
                adapter.Fill(ds);
                fillTables();
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // закрываем подключение
                sqlConnection.Close();
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            string productName = this.textBox2.Text;
            int price;
            bool priceIsNumber = int.TryParse(this.textBox3.Text, out price);
            if (productName == "" || this.textBox3.Text == "" || !priceIsNumber || this.comboBox1.SelectedItem == null || price <= 0)
            {
                MessageBox.Show("Прочитайте внимательно правила для добавления продукта в базу");
            }
            else
            {

                DataRow[] productCandidate = productsTable.Select().Where(r => r["product_name"].ToString().ToLower() == productName.ToLower()).ToArray();
                if (productCandidate.Length > 0)
                {
                    MessageBox.Show("Такой продукт уже существует");
                }
                else
                {
                    try
                    {
                        selectedUnit = (string)this.comboBox1.SelectedItem;
                        sqlConnection.Open();
                        DataRow newRow = productsTable.NewRow();
                        newRow["product_name"] = productName;
                        newRow["product_price"] = price;
                        int unitId = 0;
                        foreach (DataRow row in unitsTable.Rows)
                        {
                            if (row["unit_name"] == selectedUnit)
                            {
                                unitId = (int)row["unit_id"];
                            }
                        }
                        newRow["unit_id"] = unitId;
                        productsTable.Rows.Add(newRow);
                        updateDataSet();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        sqlConnection.Close();
                        this.textBox2.Clear();
                        this.textBox3.Clear();
                        this.comboBox1.Text = "";
                        updateProductId = -1;
                        deleteProductId = -1;
                    }
                }
            }
        }

        private void changeBtn_Click(object sender, EventArgs e)
        {
            string productName = this.textBox2.Text;
            selectedUnit = (string)this.comboBox1.SelectedItem;
            int price = 0;
            int unitId = 0;
            bool priceIsNumber = false;
            DataRow[] productsRows = productsTable.Select($"product_id = {updateProductId}");

           
            if (updateProductId == -1)
            {
                MessageBox.Show("Сначала выберите поле,которое хотите изменить");
            }

            else if (productName == "" && this.textBox3.Text == "" && selectedUnit == null)
            {
                MessageBox.Show("Заполните хотя бы одно поле,которое вы хотите изменить");
            }
            else if (productsRows.Length == 0)
            {
                MessageBox.Show("Продукта с таким ID не существует");
            }
            else 
            { 
                if (selectedUnit != null)
                {
                    foreach (DataRow row in unitsTable.Rows)
                    {
                        if (row["unit_name"] == selectedUnit)
                        {
                            unitId = (int)row["unit_id"];
                        }
                    }
                }
                if (this.textBox3.Text != "")
                {
                    priceIsNumber = int.TryParse(this.textBox3.Text, out price);
                    if (!priceIsNumber || price <= 0)
                    {
                        MessageBox.Show("Цена должна быть числом и больше нуля");
                        return;
                    }
                }

                if (productName != "")
                {
                    DataRow[] productCandidate = productsTable.Select().Where(r => r["product_name"].ToString().ToLower() == productName.ToLower()).ToArray();
                    if (productCandidate.Length > 0)
                    {
                        MessageBox.Show("Такой продукт уже существует");
                        return;
                    }
                }
                try
                {
                    Console.WriteLine($"{productName} {price} {selectedUnit}");
                    sqlConnection.Open();
                    DataRow product = productsRows[0];
                    product["product_name"] = productName == "" ? product["product_name"] : productName;
                    product["product_price"] = price > 0 ? price : product["product_price"];
                    product["unit_id"] = selectedUnit == null ? product["unit_id"] : unitId;
                    updateDataSet();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    sqlConnection.Close();
                    this.textBox2.Clear();
                    this.textBox3.Clear();
                    this.comboBox1.Text = "";
                    updateProductId = -1;
                    deleteProductId = -1;
                }
                
            }
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            if (deleteProductId == -1)
            {
                MessageBox.Show("Чтобы удалить продукт,сначала выберите нужное поле");
            }
            else
            {
                DataRow[] productRows = productsTable.Select($"product_id = {deleteProductId}");
                if (productRows.Length == 0)
                {
                    MessageBox.Show("Элемента с таким Id не существует");
                }
                else
                {
                    try
                    {
                        sqlConnection.Open();
                        productRows[0].Delete();
                        updateDataSet();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        sqlConnection.Close();
                        this.textBox2.Clear();
                        this.textBox3.Clear();
                        this.comboBox1.Text = "";
                        updateProductId = -1;
                        deleteProductId = -1;
                    }
                }
            }
        }

        private void moreInfoBtn_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("* Для добавления продукта заполните все поля кроме ID, цена должна быть числом и больше нуля\n");
            sb.AppendLine("* Для изменения продукта обязательно заполните поле ID и одно или несколько полей,которые вы хотите изменить\n");
            sb.AppendLine("* Для удаления продукта заполните поле ID");
            MessageBox.Show(sb.ToString());
        }

        private void updateDataSet()
        {
            commandBuilder = new SqlCommandBuilder(adapter);
            adapter.Update(ds);
            this.comboBox1.Items.Clear();
            ds.Clear();
            adapter.Fill(ds);
            fillTables();
        }


        private void fillTables()
        {
            productsTable = ds.Tables[0];
            unitsTable = ds.Tables[1];
            dataGridViewTable = updateData();

            foreach (DataRow row in unitsTable.Rows)
            {
                this.comboBox1.Items.Add(row["unit_name"]);
            }
            this.dataGridView1.DataSource = dataGridViewTable;
            this.dataGridView1.Columns["product_id"].Visible = false;
        }

        private DataTable updateData()
        {
            DataTable table = new DataTable();
            var collection = from t1 in productsTable.AsEnumerable()
                             join t2 in unitsTable.AsEnumerable()
                                on t1["unit_id"] equals t2["unit_id"]
                             select new { productId = t1["product_id"], productName = t1["product_name"], productPrice = t1["product_price"], unitName = t2["unit_name"] };
            table.Columns.Add("product_id", typeof(int));
            table.Columns.Add("product_name", typeof(string));
            table.Columns.Add("product_price", typeof(int));
            table.Columns.Add("unit_name", typeof(string));
            foreach(var item in collection)
            {
                table.Rows.Add(item.productId, item.productName, item.productPrice, item.unitName);
            }
            return table;

        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                if (row.Cells[0].Value.ToString() == "")
                {
                    updateProductId = -1;
                }
                else
                {
                    this.textBox2.Text = (string)row.Cells[1].Value;
                    this.comboBox1.Text = (string)row.Cells[3].Value;
                    this.textBox3.Text = row.Cells[2].Value.ToString();
                    updateProductId = (int)row.Cells[0].Value;
                }
                Console.WriteLine(updateProductId);
            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                if (row.Cells[0].Value.ToString() == "")
                {
                    deleteProductId = -1;
                }
                else
                {
                    deleteProductId = (int)row.Cells[0].Value;
                }
                Console.WriteLine(updateProductId);
            }
        }
    }
}

