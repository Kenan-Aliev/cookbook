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

        private int updateUnitId = -1;
        private int deleteUnitId = -1;

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
                adapter = new SqlDataAdapter("Select * from units order by unit_name", sqlConnection);
                dataTable = new DataTable();
                adapter.Fill(dataTable);
                this.dataGridView1.DataSource = dataTable;
                this.dataGridView1.Columns["unit_id"].Visible = false;
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
                        updateUnitId = -1;
                        deleteUnitId = -1;
                    }
                }
            }
        }

        private void changeButton_Click(object sender, EventArgs e)
        {
            string unitName = this.textBox2.Text;
            if(updateUnitId == -1)
            {
                MessageBox.Show("Для изменения единиицы измерения два раза нажмите по нужному полю");
            }
            else if(unitName == "")
            {
                MessageBox.Show("Чтобы изменить данные заполните поле Название");
            }
            else
            {
                DataRow[] unitRows = dataTable.Select($"unit_id = {updateUnitId}");
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
                        this.textBox2.Clear();
                        updateUnitId = -1;
                        deleteUnitId = -1;
                    }
                }
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (deleteUnitId == -1)
            {
                MessageBox.Show("Чтобы удалить единицу измерения сначала выберите нужное поле одним нажатием на него");
            }
            else
            {
                DataRow[] unitRows = dataTable.Select($"unit_id = {deleteUnitId}");
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
                        deleteUnitId = -1;
                        updateUnitId = -1;
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

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                if (row.Cells[0].Value.ToString() == "")
                {
                    updateUnitId = -1;
                }
                else
                {
                    this.textBox2.Text = (string)row.Cells[1].Value;
                    updateUnitId = (int)row.Cells[0].Value;
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
                    deleteUnitId = -1;
                }
                else
                {
                    deleteUnitId = (int)row.Cells[0].Value;
                }
            }
        }
    }
}
