using System;
using System.Configuration;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Linq;

namespace Lab1_RKP
{
    public partial class UnitsForm : Form
    {
        FormsSettings formsSettings;
        static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        SqlConnection sqlConnection = new SqlConnection(connectionString);

        private DataTable dataTable;
        private SqlDataAdapter adapter;

        private SqlCommandBuilder commandBuilder;

        public UnitsForm()
        {
            InitializeComponent();
            formsSettings = new FormsSettings("Единицы измерения");
            Text = formsSettings.Text;
            this.BackColor = formsSettings.BackColor;
            this.Location = formsSettings.Location;
            this.Load += UnitsForm_Load;
        }


        private void UnitsForm_Load(object sender, EventArgs e)
        {
           
                try
                {
                // Открываем подключение
                sqlConnection.Open();
                adapter = new SqlDataAdapter("Select * from units", sqlConnection);
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

        private void addButton_Click(object sender, EventArgs e)
        {
            string unitName = this.textBox2.Text;
            if(unitName == "")
            {
                MessageBox.Show("Для добавления единицы измерения заполните поле с названием");
            }
            else
            {
                DataRow[] unitCandidate = dataTable.Select().Where(r => r["unit_name"].ToString().ToLower() == unitName.ToLower()).ToArray();
                if(unitCandidate.Length > 0)
                {
                    MessageBox.Show("Такая единица измерения уже существует");
                }
                else
                {
                    try
                    {
                        sqlConnection.Open();
                        DataRow newRow = dataTable.NewRow();
                        newRow["unit_name"] = unitName;
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

        private void changeButton_Click(object sender, EventArgs e)
        {
            string unitName = this.textBox2.Text;
            string unitId = this.textBox1.Text;
            if (unitName == "" || unitId == "")
            {
                MessageBox.Show("Чтобы изменить данные заполните поля ID и Название");
            }
            else
            {
                DataRow[] unitRows = dataTable.Select($"unit_id = {unitId}");
                DataRow[] unitCandidate = dataTable.Select().Where(r => r["unit_name"].ToString().ToLower() == unitName.ToLower()).ToArray();
                if (unitRows.Length == 0)
                {
                    MessageBox.Show("Элемента с таким Id не существует");
                }
                else if (unitCandidate.Length > 0)
                {
                    MessageBox.Show("Такая единица измерения уже существует");
                }
                else
                {
                    try
                    {
                        sqlConnection.Open();
                        unitRows[0]["unit_name"] = unitName;
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

        private void deleteButton_Click(object sender, EventArgs e)
        {
            string unitId = this.textBox1.Text;
            if (this.textBox2.Text != "" || unitId == "")
            {
                MessageBox.Show("Внимательно прочитайте как нужно удалять данные в базе");
            }
            else
            {
                DataRow[] unitRows = dataTable.Select($"unit_id = {unitId}");
                if (unitRows.Length == 0)
                {
                    MessageBox.Show("Элемента с таким Id не существует");
                }
                else
                {
                    try
                    {
                        sqlConnection.Open();
                        unitRows[0].Delete();
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
                    }
                }
               
            }
        }

        private void moreInfobutton_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("* Для добавления нового элемента заполните поле с названием единицы измерения\n");
            sb.AppendLine("* Для изменения существующего элемента заполните поля с ID и названием единицы измерения\n");
            sb.AppendLine("* Для удаления элемента заполните поле с ID единицы измерения");
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
