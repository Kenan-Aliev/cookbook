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
using static System.Windows.Forms.CheckedListBox;

namespace Lab1_RKP
{
    public partial class AddOrderForm : Form
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        private FormsSettings formsSettings;

        SqlConnection sqlConnection = new SqlConnection(connectionString);
        private DataTable orderedDishesTable;
        private DataTable ordersTable;
        private SqlDataAdapter adapter;
        private SqlCommandBuilder commandBuilder;


        private OrdersForm ordersForm;

        private CheckedItemCollection checkedDishes;

        private Dictionary<Label, TextBox> textboxes;
        private Dictionary<Button, Label> btns;

       

        public AddOrderForm(OrdersForm ordersForm, CheckedItemCollection checkedDishes)
        {
            InitializeComponent();
            formsSettings = new FormsSettings("Добавление количества выбранных блюд");
            this.Text = formsSettings.Text;
            this.BackColor = formsSettings.BackColor;
            this.Location = formsSettings.Location;
            this.ordersForm = ordersForm;
            this.checkedDishes = checkedDishes;
            textboxes = new Dictionary<Label, TextBox>();
            btns = new Dictionary<Button, Label>();
            this.Load += AddOrderFrom_Load;
        }

        private void AddOrderFrom_Load(object sender,EventArgs e)
        {
            try
            {
                // Открываем подключение
                sqlConnection.Open();
                adapter = new SqlDataAdapter("Select * from ordered_dishes;", sqlConnection);
                orderedDishesTable = new DataTable();
                adapter.Fill(orderedDishesTable);
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                // закрываем подключение
                sqlConnection.Close();
            }


            int y = 0;
            foreach(var item in checkedDishes)
            {
                Label label = new Label();
                label.Text = item.ToString();
                label.ForeColor = Color.Red;
                label.Location = new Point(100, 50 + y);

                TextBox textBox = new TextBox();
                textBox.Location = new Point(200, 50 + y);
                textBox.Size = new Size(100, 22);

                Button btn = new Button();
                btn.Text = "Удалить блюдо из заказа";
                btn.ForeColor = Color.White;
                btn.BackColor = Color.Red;
                btn.Size = new Size(114, 42);
                btn.Location = new Point(350, 40 + y);
                btn.Click += deleteBtnClick;

                btns.Add(btn, label);
                textboxes.Add(label, textBox);

                this.Controls.Add(label);
                this.Controls.Add(btn);
                this.Controls.Add(textBox);

                y = y + 50;
            }
        }


        private void deleteBtnClick(object sender,EventArgs e)
        {
            Button btn = sender as Button;
            Label label = btns[btn];
            TextBox textBox = textboxes[label];

            this.Controls.Remove(btn);
            this.Controls.Remove(label);
            this.Controls.Remove(textBox);

            this.btns.Remove(btn);
            this.textboxes.Remove(label);
        }

        private void addBtnClick(object sender, EventArgs e)
        {
            if (textboxes.Count == 0)
            {
                MessageBox.Show("Сначала выберите блюда, чтобы сделать заказ");
                this.Dispose();
            }
            else
            {
                int[] textBoxesValues = new int[textboxes.Count];
                string[] dishesName = new string[textboxes.Count];
                int i = 0;
                foreach (var textBox in textboxes)
                {
                    string textBoxValue = textBox.Value.Text;
                    bool textBoxValueIsNumber = int.TryParse(textBoxValue, out textBoxesValues[i]);

                    if (textBoxValue == "")
                    {
                        MessageBox.Show($"У блюда: {textBox.Key.Text} отсутствует значение количества этого блюда");
                        return;
                    }
                    else if (!textBoxValueIsNumber || textBoxesValues[i] <= 0)
                    {
                        MessageBox.Show($"У блюда: {textBox.Key.Text} количество должно быть целым числом и больше нуля");
                        return;
                    }
                    dishesName[i] = textBox.Key.Text;
                    i++;
                }
                i = 0;

                DataRow newRow = ordersForm.ordersTable.NewRow();
                newRow["order_date"] = DateTime.Now.Date;
                try
                {
                    ordersForm.sqlConnection.Open();
                    ordersForm.ordersTable.Rows.Add(newRow);
                    ordersForm.updateDataSet();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
                finally
                {
                    ordersForm.sqlConnection.Close();
                }

                int orderId = getLastOrderId();
                Console.WriteLine(orderId);
                try
                {
                    sqlConnection.Open();
                    foreach (int textBoxValue in textBoxesValues)
                    {
                        DataRow dishRow = null;

                        foreach (DataRow dish in ordersForm.dishesTable.Rows)
                        {
                            if ((string)dish["dish_name"] == dishesName[i])
                            {
                                dishRow = dish;
                                break;
                            }
                        }

                        DataRow newOrderedDishRow = orderedDishesTable.NewRow();
                        newOrderedDishRow["order_id"] = orderId;
                        newOrderedDishRow["dish_id"] = (int)dishRow["dish_id"];
                        newOrderedDishRow["dish_count"] = textBoxValue;

                        orderedDishesTable.Rows.Add(newOrderedDishRow);
                        i++;
                    }
                    updateDataTable();
                    MessageBox.Show("Вы успешно сделали заказ");
                    this.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    sqlConnection.Close();
                }
            }
        }

        private void updateDataTable()
        {
            commandBuilder = new SqlCommandBuilder(adapter);
            adapter.Update(orderedDishesTable);
            orderedDishesTable.Clear();
            adapter.Fill(orderedDishesTable);
        }


        private int getLastOrderId()
        {
            string sqlExpression = "LastInsertedOrderId";
            int orderId = default;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.CommandType = CommandType.StoredProcedure;

                

                // определяем первый выходной параметр
                SqlParameter orderIdParam = new SqlParameter
                {
                    ParameterName = "@orderId",
                    SqlDbType = SqlDbType.Int // тип параметра
                };
                // указываем, что параметр будет выходным
                orderIdParam.Direction = ParameterDirection.Output;
                command.Parameters.Add(orderIdParam);

                command.ExecuteNonQuery();

                orderId = (int)command.Parameters["@orderId"].Value;
            }
            return orderId;
        }
     
    }
}
