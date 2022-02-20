using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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
                adapter = new SqlDataAdapter("Select * from products;Select * from units", sqlConnection);
                ds = new DataSet();
                adapter.Fill(ds);
                fillBoxes();
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
                } catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    sqlConnection.Close();
                    this.textBox1.Clear();
                    this.textBox2.Clear();
                    this.textBox3.Clear();
                }

            }
        }

        private void changeBtn_Click(object sender, EventArgs e)
        {
            string productId = this.textBox1.Text;
            string productName = this.textBox2.Text;
            selectedUnit = (string)this.comboBox1.SelectedItem;
            int price = 0;
            int unitId = 0;
            bool priceIsNumber = false;

            if (productId == "")
            {
                MessageBox.Show("Укажите ID продукта, который вы хотите изменить");
            }
            else if (this.textBox3.Text != "")
            {
                priceIsNumber = int.TryParse(this.textBox3.Text, out price);
                if (!priceIsNumber)
                {
                    MessageBox.Show("Цена должна быть числом");
                }
            }
            else if (productName == "" && price == 0 && selectedUnit == null)
            {
                MessageBox.Show("Заполните хотя бы одно поле,которое вы хотите изменить");
            }
            else{
                Console.WriteLine("else ");
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
                try
                {
                   
                    sqlConnection.Open();
                    DataRow[] productsRows = productsTable.Select($"product_id = {productId}");
                    if (productsRows.Length == 0)
                    {
                        MessageBox.Show("Продукта с таким ID не существует");
                    }
                    else
                    {
                        DataRow product = productsRows[0];
                        product["product_name"] = productName == "" ? product["product_name"] : productName;
                        product["product_price"] = price > 0 ? price : product["product_price"];
                        product["unit_id"] = selectedUnit == null ? product["unit_id"] : unitId;
                        updateDataSet();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    sqlConnection.Close();
                    this.textBox1.Clear();
                    this.textBox2.Clear();
                    this.textBox3.Clear();
                }
            }
        }
        private void deleteBtn_Click(object sender, EventArgs e)
        {
            string productId = this.textBox1.Text;
            if(productId == "")
            {
                MessageBox.Show("Укажите ID продукта чтобы удалить его");
            }
            else
            {
                try
                {
                    sqlConnection.Open();
                    DataRowCollection productsCollection = productsTable.Rows;
                    DataRow[] productRows = productsTable.Select($"product_id = {productId}");
                    if (productRows.Length == 0)
                    {
                        MessageBox.Show("Элемента с таким Id не существует");
                    }
                    else
                    {
                        productRows[0].Delete();
                        updateDataSet();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    sqlConnection.Close();
                    this.textBox1.Clear();
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
            fillBoxes();
        }


        private void fillBoxes()
        {
            productsTable = ds.Tables[0];
            unitsTable = ds.Tables[1];
            foreach (DataRow row in unitsTable.Rows)
            {
                this.comboBox1.Items.Add(row["unit_name"]);
            }
            this.dataGridView1.DataSource = productsTable;
        }
    }
}

