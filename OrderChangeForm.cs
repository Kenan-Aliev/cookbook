using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab1_RKP
{
    public partial class OrderChangeForm : Form
    {
        FormsSettings formsSettings;
        static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        private SqlConnection sqlConnection = new SqlConnection(connectionString);
        private DataSet ds;
        private SqlDataAdapter adapter;
        private SqlCommandBuilder commandBuilder;

        private int orderId = -1;

        private DataTable orderedDishesTable;
        private DataTable dishesTable;
        private DataTable dataGridViewTable;

        public OrderChangeForm(int orderId)
        {
            InitializeComponent();
            FormsSettings formsSettings = new FormsSettings("Форма изменения заказа");
            this.Text = formsSettings.Text;
            this.BackColor = formsSettings.BackColor;
            this.Location = formsSettings.Location;
            this.orderId = orderId;
            this.Load += OrderChangeForm_Load;
        }

        private void OrderChangeForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Открываем подключение
                sqlConnection.Open();
                adapter = new SqlDataAdapter($"Select * from ordered_dishes where order_id = {orderId};Select * from dishes;", sqlConnection);
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
            if(this.textBox2.Text == "" || this.comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Для добавления нового блюда в заказ заполните все поля, кроме ID");
            }
            else
            {
                string selectedDish = (string)this.comboBox1.SelectedItem;
                int dishCount = 0;
                bool dishCountIsNumber = int.TryParse(this.textBox2.Text, out dishCount);
                if(!dishCountIsNumber || dishCount <= 0)
                {
                    MessageBox.Show("Количество блюда должно быть целым числом и больше нуля");
                }
                else
                {
                    DataRow dishRow = dishesTable.Select().Where(r=>(string)r["dish_name"] == selectedDish).ToArray()[0];
                    DataRow[] orderedDishRows = orderedDishesTable.Select().Where(r => (int)r["dish_id"] == (int)dishRow["dish_id"]).ToArray();
                    if(orderedDishRows.Length != 0)
                    {
                        DialogResult result = MessageBox.Show("Блюдо,которое вы хотите добавить уже есть в списке заказанных,хотите ли вы заменить существующий на новый заказ", "Сообщение",
                             MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                        if(result == DialogResult.No)
                        {
                            return;
                        }
                        else
                        {
                            orderedDishRows[0]["dish_count"] = dishCount;
                        }
                    }
                    else
                    {
                        DataRow newRow = orderedDishesTable.NewRow();
                        newRow["order_id"] = orderId;
                        newRow["dish_id"] = (int)dishRow["dish_id"];
                        newRow["dish_count"] = dishCount;
                        orderedDishesTable.Rows.Add(newRow);
                    }
                    try
                    {
                        sqlConnection.Open();
                        updateDataSet();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        sqlConnection.Close();
                        this.textBox1.Clear();
                        this.textBox2.Clear();
                        this.comboBox1.Text = "";
                    }
                }
            }
        }

        private void changeBtn_Click(object sender, EventArgs e)
        {
            if(this.textBox1.Text == "")
            {
                MessageBox.Show("Для изменения заказанного блюда заполните поле ID");
            }
            else if(this.textBox2.Text == "" && this.comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Для изменения заказанного блюда заполните хотя бы одно из полей");
            }
            else
            {
                int orderedDishId = 0;
                bool orderedDishIdIsNumber = int.TryParse(this.textBox1.Text, out orderedDishId);
                if (!orderedDishIdIsNumber)
                {
                    MessageBox.Show("ID должен быть целым числом");
                }
                else
                {
                    int dishCount = 0;
                    string selectedDish = "";
                    DataRow dishRow = null;
                    if (this.textBox2.Text != "")
                    {
                        bool dishCountIsNumber = int.TryParse(this.textBox2.Text, out dishCount);
                        if (!dishCountIsNumber || dishCount <= 0)
                        {
                            MessageBox.Show("Количество блюда должно быть целым числом и больше нуля");
                            return;
                        }
                    }
                    if (this.comboBox1.SelectedItem != null)
                    {
                        selectedDish = (string)this.comboBox1.SelectedItem;
                    }

                    if (selectedDish != "")
                    {
                        dishRow = dishesTable.Select().Where(r => (string)r["dish_name"] == selectedDish).ToArray()[0];
                        DataRow[] orderedDishRows = orderedDishesTable.Select().Where(r => (int)r["dish_id"] == (int)dishRow["dish_id"]).ToArray();
                        if (orderedDishRows.Length != 0)
                        {
                            MessageBox.Show("Блюдо,на которое вы хотите поменять уже есть в списке заказанных,выберите другое блюдо");
                            return;
                        }
                    }
                    DataRow[] orderedDishRow = orderedDishesTable.Select().Where(r => (int)r["orderedDish_id"] == orderedDishId).ToArray();
                    if(orderedDishRow.Length == 0)
                    {
                        MessageBox.Show("Заказа с заданым ID не найдено");
                    }
                    else
                    {
                        orderedDishRow[0]["dish_id"] = selectedDish == "" ? orderedDishRow[0]["dish_id"] : dishRow["dish_id"];
                        orderedDishRow[0]["dish_count"] = dishCount > 0 ? dishCount: orderedDishRow[0]["dish_count"];
                        try
                        {
                            sqlConnection.Open();
                            updateDataSet();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            sqlConnection.Close();
                            this.textBox1.Clear();
                            this.textBox2.Clear();
                            this.comboBox1.Text = "";
                        }
                    }
                }
            }
               
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
           if(this.textBox1.Text == "")
            {
                MessageBox.Show("Для удаления блюда из списка заказа заполните поле ID");
            }
            else
            {
                int orderedDishId = 0;
                bool orderedDishIdIsnumber = int.TryParse(this.textBox1.Text, out orderedDishId);
                if (!orderedDishIdIsnumber)
                {
                    MessageBox.Show("ID должен быть целым числом");
                }
                else
                {
                    DataRow[] orderedDishRows = orderedDishesTable.Select().Where(r => (int)r["orderedDish_id"] == (int)orderedDishId).ToArray();
                    if(orderedDishRows.Length == 0)
                    {
                        MessageBox.Show("Такого заказа не существует");
                    }
                    else
                    {
                        orderedDishRows[0].Delete();
                        try
                        {
                            sqlConnection.Open();
                            updateDataSet();
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            sqlConnection.Close();
                            this.textBox1.Clear();
                            this.textBox2.Clear();
                            this.comboBox1.Text = "";
                        }
                    }
                }
            }
        }

        private void moreInfoBtn_Click(object sender, EventArgs e)
        {

        }

        private void updateDataSet()
        {
            commandBuilder = new SqlCommandBuilder(adapter);
            adapter.Update(ds);
            ds.Clear();
            adapter.Fill(ds);
            fillTables();
        }


        private void fillTables()
        {
            orderedDishesTable = ds.Tables[0];
            dishesTable = ds.Tables[1];
            dataGridViewTable = updateData();

            this.comboBox1.Items.Clear();
            foreach (DataRow row in dishesTable.Rows)
            {
                this.comboBox1.Items.Add(row["dish_name"]);
            }
            this.dataGridView1.DataSource = dataGridViewTable;
        }

        private DataTable updateData()
        {
            DataTable table = new DataTable();
            var collection = from t1 in orderedDishesTable.AsEnumerable()
                             join t2 in dishesTable.AsEnumerable()
                                on t1["dish_id"] equals t2["dish_id"]
                             select new { orderedDish_id = t1["orderedDish_id"], orderId = orderId, dishName = t2["dish_name"], dishCount = t1["dish_count"] };
            table.Columns.Add("orderedDish_id", typeof(int));
            table.Columns.Add("order_id", typeof(int));
            table.Columns.Add("dish_name", typeof(string));
            table.Columns.Add("dish_count", typeof(int));
            foreach (var item in collection)
            {
                table.Rows.Add(item.orderedDish_id, item.orderId, item.dishName, item.dishCount);
            }
            return table;

        }

    }
}
