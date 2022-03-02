using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Objects.SqlClient;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab1_RKP
{
    public partial class DishesForm : Form
    {

        FormsSettings formsSettings;
        static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        SqlConnection sqlConnection = new SqlConnection(connectionString);

        private DataSet ds;
        private SqlDataAdapter adapter;

        private SqlCommandBuilder commandBuilder;

        private DataTable dishesTable;
        private DataTable dishTypeTable;
        private DataTable dataGridViewTable;

        private string selectedDishType;
        private int updateDishId = -1;
        private int deleteDishId = -1;

        public DishesForm()
        {
            InitializeComponent();
            formsSettings = new FormsSettings("Блюда");
            Text = formsSettings.Text;
            this.BackColor = formsSettings.BackColor;
            this.Location = formsSettings.Location;
            this.Load += dishesForm_Load;
        }

        private void dishesForm_Load(object sender,EventArgs e)
        {
            try
            {
                // Открываем подключение
                sqlConnection.Open();
                adapter = new SqlDataAdapter("Select * from dishes order by dish_name;Select * from dishTypes order by dishType_name", sqlConnection);
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
            string dishName = this.textBox2.Text;
            decimal dishPrice = 0;
            bool dishPriceIsnumber = false;
            if (dishName == "" || this.comboBox1.SelectedItem == null || this.textBox3.Text == "")
            {
                MessageBox.Show("Для добавления блюда все поля должны быть заполнены,кроме ID");
            }
            else
            {
                dishPriceIsnumber = decimal.TryParse(this.textBox3.Text, out dishPrice);
                if (!dishPriceIsnumber || dishPrice <= 0)
                {
                    MessageBox.Show("Цена может быть целым или дробным числом и должна быть больше нуля");
                }
                else
                {
                    
                    DataRow[] dishCandidate = dishesTable.Select().Where(r=>r["dish_name"].ToString().ToLower() == dishName.ToLower()).ToArray();
                    if (dishCandidate.Length > 0)
                    {
                        MessageBox.Show("Такое блюдо уже существует");
                    }
                    else
                    {
                        try
                        {
                            selectedDishType = (string)this.comboBox1.SelectedItem;
                            sqlConnection.Open();
                            DataRow newRow = dishesTable.NewRow();
                            newRow["dish_name"] = dishName;
                            newRow["dish_price"] = dishPrice;
                            int dishTypeId = 0;
                            foreach (DataRow row in dishTypeTable.Rows)
                            {
                                if (row["dishType_name"] == selectedDishType)
                                {
                                    dishTypeId = (int)row["dishType_id"];
                                }
                            }
                            newRow["dishType_id"] = dishTypeId;
                            dishesTable.Rows.Add(newRow);
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
                            updateDishId = -1;
                            deleteDishId = -1;
                        }
                    }
                }
            }
        }

        private void changeBtn_Click(object sender, EventArgs e)
        {
            string dishName = this.textBox2.Text;
            decimal dishPrice = 0;
            bool dishPriceIsNumber = false;
            selectedDishType = (string)this.comboBox1.SelectedItem;
            int dishTypeId = 0;

            if (updateDishId == -1)
            {
                MessageBox.Show("Сначала выберите поле, которое вы хотите изменить");
            }

            else if (dishName == "" && selectedDishType == null && this.textBox3.Text == "")
            {
                MessageBox.Show("Заполните хотя бы одно поле,которое вы хотите изменить");
            }
            else
            {
                if (selectedDishType != null)
                {
                    foreach (DataRow row in dishTypeTable.Rows)
                    {
                        if (row["dishType_name"] == selectedDishType)
                        {
                            dishTypeId = (int)row["dishType_id"];
                        }
                    }
                }
                if(dishName != "")
                {
                    DataRow[] dishCandidate = dishesTable.Select().Where(r => r["dish_name"].ToString().ToLower() == dishName.ToLower()).ToArray();
                    if (dishCandidate.Length > 0)
                    {
                        MessageBox.Show("Такое блюдо уже существует");
                        return;
                    }
                }
                if (this.textBox3.Text != "")
                {
                    dishPriceIsNumber = decimal.TryParse(this.textBox3.Text, out dishPrice);
                    if (!dishPriceIsNumber || dishPrice <= 0)
                    {
                        MessageBox.Show("Цена блюда должна быть больше нуля и числом");
                        return;
                    }
                }
                DataRow[] dishesRows = dishesTable.Select($"dish_id = {updateDishId}");
                if (dishesRows.Length == 0)
                {
                    MessageBox.Show("Блюда с таким ID не существует");
                }
                else
                {
                    try
                    {
                        sqlConnection.Open();
                        DataRow dish = dishesRows[0];
                        dish["dish_name"] = dishName == "" ? dish["dish_name"] : dishName;
                        dish["dishType_id"] = selectedDishType == null ? dish["dishType_id"] : dishTypeId;
                        dish["dish_price"] = dishPrice > 0 ? dishPrice : dish["dish_price"];
                        updateDataSet();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        sqlConnection.Close();
                        this.textBox2.Clear();
                        this.textBox3.Clear();
                        this.comboBox1.Text = "";
                        updateDishId = -1;
                        deleteDishId = -1;
                    }
                }
            }
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            if (deleteDishId == -1)
            {
                MessageBox.Show("Сначала выберите поле,которое вы хотите удалить");
            }
            else
            {
                DataRow[] dishRows = dishesTable.Select($"dish_id = {deleteDishId}");
                if (dishRows.Length == 0)
                {
                    MessageBox.Show("Блюда с таким Id не существует");
                }
                else
                {
                    try
                    {
                        sqlConnection.Open();
                        dishRows[0].Delete();
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
                        updateDishId = -1;
                        deleteDishId = -1;
                    }
                }
            }
        }

        private void moreInfoBtn_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("* Для добавления блюда заполните все поля кроме ID\n");
            sb.AppendLine("* Для изменения блюда обязательно заполните поле ID и одно или несколько полей,которые вы хотите изменить\n");
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
            dishesTable = ds.Tables[0];
            dishTypeTable = ds.Tables[1];
            dataGridViewTable = updateData();
            foreach (DataRow row in dishTypeTable.Rows)
            {
                this.comboBox1.Items.Add(row["dishType_name"]);
            }
            this.dataGridView1.DataSource = dataGridViewTable;
            this.dataGridView1.Columns["dish_id"].Visible = false;
        }

        private DataTable updateData()
        {
            DataTable table = new DataTable();
            var collection = from t1 in dishesTable.AsEnumerable()
                             join t2 in dishTypeTable.AsEnumerable()
                                on t1["dishType_id"] equals t2["dishType_id"]
                                orderby t1["dish_name"],t1["dish_price"],t2["dishType_name"]
                             select new { dishId = t1["dish_id"], dishName = t1["dish_name"], dishPrice = t1["dish_price"], dishTypeName = t2["dishType_name"] };
            table.Columns.Add("dish_id", typeof(int));
            table.Columns.Add("dish_name", typeof(string));
            table.Columns.Add("dish_price", typeof(decimal));
            table.Columns.Add("dishType_name", typeof(string));
            foreach (var item in collection)
            {
                table.Rows.Add(item.dishId, item.dishName, Math.Round((decimal)item.dishPrice), item.dishTypeName);
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
                    updateDishId = -1;
                }
                else
                {
                    this.textBox2.Text = (string)row.Cells[1].Value;
                    this.comboBox1.Text = row.Cells[3].Value.ToString();
                    this.textBox3.Text = row.Cells[2].Value.ToString();
                    updateDishId = (int)row.Cells[0].Value;
                }
            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                if (row.Cells[0].Value.ToString() == "")
                {
                    deleteDishId = -1;
                }
                else
                {
                    deleteDishId = (int)row.Cells[0].Value;
                }
            }
        }
    }
}
