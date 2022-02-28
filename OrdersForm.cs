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
    public partial class OrdersForm : Form
    {

        FormsSettings formsSettings;
        static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public SqlConnection sqlConnection = new SqlConnection(connectionString);
        private DataSet ds;
        private SqlDataAdapter adapter;

        private SqlCommandBuilder commandBuilder;

        public DataTable ordersTable;
        public DataTable dishesTable;

        public int selectedOrderId = -1;

        public OrdersForm()
        {
            InitializeComponent();
            formsSettings = new FormsSettings("Заказы");
            Text = formsSettings.Text;
            this.BackColor = formsSettings.BackColor;
            this.Location = formsSettings.Location;
            this.Load += ordersForm_Load;
        }


        private void ordersForm_Load(object sender,EventArgs e)
        {
            try
            {
                // Открываем подключение
                sqlConnection.Open();
                adapter = new SqlDataAdapter("Select * from orders;Select * from dishes;", sqlConnection);
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
            if(this.checkedListBox1.CheckedItems.Count == 0)
            {
                MessageBox.Show("Чтобы сделать заказ выберите блюда");
            }
            else
            {
                
                    AddOrderForm addOrderForm = new AddOrderForm(this, this.checkedListBox1.CheckedItems);
                    addOrderForm.ShowDialog();
                    addOrderForm.Disposed += clearSelection;
            }
        }

        private void changeBtn_Click(object sender, EventArgs e)
        {
            if(selectedOrderId == -1)
            {
                MessageBox.Show("Сначала выберите заказ,который хотите изменить");
            }
            else
            {
                try
                {
                    OrderChangeForm orderChangeForm = new OrderChangeForm(selectedOrderId);
                    orderChangeForm.ShowDialog();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    selectedOrderId = -1;
                }
                
            }
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            if(selectedOrderId == -1)
            {
                MessageBox.Show("Чтобы удалить заказ,сначала выберите нужное поле");
            }
            else
            {
                DataRow order = ordersTable.Select().Where(r => (int)r["order_id"] == selectedOrderId).ToArray()[0];
                order.Delete();
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
                    selectedOrderId = -1;
                }
            }
        }

        private void moreInfoBtn_Click(object sender, EventArgs e)
        {

        }

        private void showCheck_Click(object sender, EventArgs e)
        {
            if(selectedOrderId == -1)
            {
                MessageBox.Show("Сначала выберите нужный заказ,чтобы выдать чек");
            }
            else
            {
                try
                {
                    ShowCheckForm showCheckForm = new ShowCheckForm(this);
                    showCheckForm.ShowDialog();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    selectedOrderId = -1;
                }
            }
        }

        private void clearSelection(object sender,EventArgs e)
        {
            this.checkedListBox1.ClearSelected();
        }
        public void updateDataSet()
        {
            commandBuilder = new SqlCommandBuilder(adapter);
            adapter.Update(ds);
            ds.Clear();
            adapter.Fill(ds);
            fillBoxes();
        }

        private void fillBoxes()
        {
            ordersTable = ds.Tables[0];
            dishesTable = ds.Tables[1];
            this.checkedListBox1.Items.Clear();
            foreach (DataRow dish in dishesTable.Rows)
            {
                this.checkedListBox1.Items.Add(dish["dish_name"]);
            }
            this.dataGridView1.DataSource = ordersTable;
            this.dataGridView1.Columns["order_id"].Visible = false;
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {

            if(e.RowIndex > -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                if (row.Cells[0].Value.ToString() == "")
                {
                    selectedOrderId = -1;
                }
                else
                {
                    selectedOrderId = (int)row.Cells[0].Value;
                }
            }
        }

        
    }
}
