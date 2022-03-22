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
    public partial class ProfitReportForm : Form
    {
        FormsSettings formsSettings;
        static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        SqlConnection sqlConnection = new SqlConnection(connectionString);

        private DataTable usedProductsTable;
        private DataTable orderedDishesTable;
        public ProfitReportForm()
        {
            InitializeComponent();
            formsSettings = new FormsSettings("Форма для отчета по прибыли");
            this.Text = formsSettings.Text;
            this.BackColor = formsSettings.BackColor;
            this.Location = formsSettings.Location;
            this.Load += ProfitReportForm_Load;
        }


        private void ProfitReportForm_Load(object sender,EventArgs e)
        {
            usedProductsTable = new DataTable();
            usedProductsTable.Columns.Add("product_name", typeof(string));
            usedProductsTable.Columns.Add("unit_name", typeof(string));
            usedProductsTable.Columns.Add("total_product_amount", typeof(decimal));
            usedProductsTable.Columns.Add("total_product_price", typeof(decimal));

            orderedDishesTable = new DataTable();
            orderedDishesTable.Columns.Add("dish_name", typeof(string));
            orderedDishesTable.Columns.Add("dish_total_count", typeof(int));
            orderedDishesTable.Columns.Add("dish_total_price", typeof(decimal));



            this.textBox1.Enabled = false;
            this.textBox2.Enabled = false;
            this.textBox3.Enabled = false;
            this.dataGridView1.DataSource = usedProductsTable;
            this.dataGridView2.DataSource = orderedDishesTable;
        }

        private void getProfit_Click(object sender, EventArgs e)
        {

            DateTime startDate = this.dateTimePicker1.Value.Date;
            DateTime endDate = this.dateTimePicker2.Value.Date;
            Console.WriteLine(startDate.ToString());
            Console.WriteLine(endDate.ToString());


            int compareDates = startDate.CompareTo(endDate);

            if (compareDates == 1)
            {
                MessageBox.Show("Начальная дата должна быть раньше чем конечная");
            }
            else
            {
                try
                {
                    decimal usedProductsTotalPrice = 0;
                    decimal orderedProductsTotalPrice = 0;
                    decimal totalProfitPrice = 0;

                    setUsedProductsForPeriod(startDate, endDate);
                    setOrderedDishesForPeriod(startDate, endDate);

                    if (usedProductsTable.Rows.Count == 0)
                    {
                        usedProductsTotalPrice = 0;
                        this.textBox1.Text = usedProductsTotalPrice.ToString();
                    }
                    else
                    {
                        foreach (DataRow row in usedProductsTable.Rows)
                        {
                            usedProductsTotalPrice += (decimal)row["total_product_price"];
                        }
                        this.textBox1.Text = usedProductsTotalPrice.ToString() + "  сом";
                    }

                    if (orderedDishesTable.Rows.Count == 0)
                    {
                        orderedProductsTotalPrice = 0;
                        this.textBox2.Text = orderedProductsTotalPrice.ToString();
                    }
                    else
                    {
                        foreach (DataRow row in orderedDishesTable.Rows)
                        {
                            orderedProductsTotalPrice += (decimal)row["dish_total_price"];
                        }
                        this.textBox2.Text = orderedProductsTotalPrice.ToString() + "  сом";
                    }

                    totalProfitPrice = orderedProductsTotalPrice - usedProductsTotalPrice;
                    this.textBox3.Text = totalProfitPrice.ToString() + "  сом";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void setUsedProductsForPeriod(DateTime startdate,DateTime enddate)
        {
            usedProductsTable.Clear();
            string sqlExpression = "getUsedProductsForPeriod";
            try
            {
                sqlConnection.Open();
                SqlCommand command = new SqlCommand(sqlExpression,sqlConnection);
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter startDateParam = new SqlParameter
                {
                    ParameterName = "@startdate",
                    Value = startdate
                };
                // добавляем параметр
                command.Parameters.Add(startDateParam);
                // параметр для ввода возраста
                SqlParameter enddateParam = new SqlParameter
                {
                    ParameterName = "@enddate",
                    Value = enddate
                };
                command.Parameters.Add(enddateParam);

                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string productName = reader.GetString(0);
                        string unitName = reader.GetString(1);
                        decimal totalProductAmount = reader.GetDecimal(2);
                        decimal totalProductPrice = reader.GetDecimal(3);

                        DataRow newRow = usedProductsTable.NewRow();
                        newRow["product_name"] = productName;
                        newRow["unit_name"] = unitName;
                        newRow["total_product_amount"] = Math.Round(totalProductAmount,2);
                        newRow["total_product_price"] = Math.Round(totalProductPrice,2);

                        usedProductsTable.Rows.Add(newRow);
                    }
                }
                else
                {
                    MessageBox.Show("Использованных продуктов за выбранный период времени нет");
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                sqlConnection.Close();
            }
        }


        private void setOrderedDishesForPeriod(DateTime startdate, DateTime enddate)
        {
            orderedDishesTable.Clear();
            string sqlExpression = "getOrderedDishesForPeriod";
            try
            {
                sqlConnection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, sqlConnection);
                command.CommandType = CommandType.StoredProcedure;

                SqlParameter startDateParam = new SqlParameter
                {
                    ParameterName = "@startdate",
                    Value = startdate
                };
                // добавляем параметр
                command.Parameters.Add(startDateParam);
                // параметр для ввода возраста
                SqlParameter enddateParam = new SqlParameter
                {
                    ParameterName = "@enddate",
                    Value = enddate
                };
                command.Parameters.Add(enddateParam);

                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        string dishName = reader.GetString(0);
                        int totalDishesAmount = reader.GetInt32(1);
                        decimal totalDishesPrice = reader.GetDecimal(2);

                        DataRow newRow = orderedDishesTable.NewRow();
                        newRow["dish_name"] = dishName;
                        newRow["dish_total_count"] = totalDishesAmount;
                        newRow["dish_total_price"] = Math.Round(totalDishesPrice, 2);

                        orderedDishesTable.Rows.Add(newRow);
                    }
                }
                else
                {
                    MessageBox.Show("Заказанных блюд за выбранный период времени нет");
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                sqlConnection.Close();
            }
        }

    }
}
