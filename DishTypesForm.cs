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
    public partial class DishTypesForm : Form
    {

        FormsSettings formsSettings;
        static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        SqlConnection sqlConnection = new SqlConnection(connectionString);

        private DataTable dataTable;
        private SqlDataAdapter adapter;

        private SqlCommandBuilder commandBuilder;

        public DishTypesForm()
        {
            InitializeComponent();
            formsSettings = new FormsSettings("Типы блюд");
            Text = formsSettings.Text;
            this.BackColor = formsSettings.BackColor;
            this.Location = formsSettings.Location;
            this.Load += DishTypesFrom_Load;
        }

        private void DishTypesFrom_Load(object sender,EventArgs e)
        {
            try
            {
                // Открываем подключение
                sqlConnection.Open();
                adapter = new SqlDataAdapter("Select * from dishTypes", sqlConnection);
                dataTable = new DataTable();
                adapter.Fill(dataTable);
                this.dataGridView1.DataSource = dataTable;
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
            string dishTypeName = this.textBox2.Text;
            if (dishTypeName == "")
            {
                MessageBox.Show("Укажите название типа блюд");
            }
            else
            {
                DataRow[] dishTypeCandidate = dataTable.Select().Where(r => r["dishType_name"].ToString().ToLower() == dishTypeName.ToLower()).ToArray();
                if (dishTypeCandidate.Length > 0)
                {
                    MessageBox.Show("Такой вид блюда уже существует");
                }
                else
                {
                    try
                    {
                        sqlConnection.Open();
                        DataRow newRow = dataTable.NewRow();
                        newRow["dishType_name"] = dishTypeName;
                        dataTable.Rows.Add(newRow);
                        updateTable();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        sqlConnection.Close();
                        this.textBox2.Clear();
                    }
                }
            }
        }

        private void changeBtn_Click(object sender, EventArgs e)
        {
            string dishTypeName = this.textBox2.Text;
            string dishTypeId = this.textBox1.Text;
            if (dishTypeName == "" || dishTypeId == "")
            {
                MessageBox.Show("Укажите ID и новое название типа блюд, чтобы изменить данные в базе");
            }
            else
            {
                DataRow[] dishTypeRows = dataTable.Select($"dishType_id = {dishTypeId}");
                DataRow[] dishTypeCandidate = dataTable.Select().Where(r => r["dishType_name"].ToString().ToLower() == dishTypeName.ToLower()).ToArray();
                if (dishTypeRows.Length == 0)
                {
                    MessageBox.Show("Элемента с таким Id не существует");
                }
                else if (dishTypeCandidate.Length > 0)
                {
                    MessageBox.Show("Такой вид блюда уже существует");
                }
                else
                {
                    try
                    {
                        sqlConnection.Open();
                        dishTypeRows[0]["dishType_name"] = dishTypeName;
                        updateTable();
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
                    }
                }
               
            }
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            string dishTypeId = this.textBox1.Text;
            if (dishTypeId == "")
            {
                MessageBox.Show("Укажите ID типа блюд, который вы хотите удалить");
            }
            else
            {
                DataRow[] dishTypeRows = dataTable.Select($"dishType_id = {dishTypeId}");
                if (dishTypeRows.Length == 0)
                {
                    MessageBox.Show("Элемента с таким Id не существует");
                }
                else
                {
                    try
                    {
                        sqlConnection.Open();
                        dishTypeRows[0].Delete();
                        updateTable();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        sqlConnection.Close();
                        this.textBox1.Clear();
                    }
                }
               
            }
        }

        private void moreInfoBtn_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("* Для добавления нового поля заполните поле с названием типа блюд\n");
            sb.AppendLine("* Для изменения данных заполните поле ID и поле Название\n");
            sb.AppendLine("* Для удаления данных заполните поле ID");
            MessageBox.Show(sb.ToString());
        }

        private void updateTable()
        {
            commandBuilder = new SqlCommandBuilder(adapter);
            adapter.Update(dataTable);
            dataTable.Clear();
            adapter.Fill(dataTable);
            this.dataGridView1.DataSource = dataTable;
        }
    }
}
