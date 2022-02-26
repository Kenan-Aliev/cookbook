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
            decimal productsSumForDay;
            decimal productsSumForWeek;
            decimal productsSumForMonth;

            decimal orderedDishesSumForDay;
            decimal orderedDishesSumForWeek;
            decimal orderedDishesSumForMonth;

            try
            {
                productsSumForDay = getUsedProductsSumForDate("day");
                productsSumForWeek = getUsedProductsSumForDate("week");
                productsSumForMonth = getUsedProductsSumForDate("month");

                orderedDishesSumForDay = getOrderedDishesSumForDate("day");
                orderedDishesSumForWeek = getOrderedDishesSumForDate("week");
                orderedDishesSumForMonth = getOrderedDishesSumForDate("month");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            Label label1 = new Label();
            label1.Text = productsSumForDay.ToString() + " сом";
            label1.Location = new Point(228, 120);
            label1.ForeColor = Color.Black;
            this.Controls.Add(label1);


            Label label2 = new Label();
            label2.Text = orderedDishesSumForDay.ToString() + " сом";
            label2.Location = new Point(390, 120);
            label2.ForeColor = Color.Black;
            this.Controls.Add(label2);


            Label label3 = new Label();
            label3.Text = (orderedDishesSumForDay - productsSumForDay).ToString() + " сом";
            label3.Location = new Point(515, 120);
            label3.ForeColor = Color.Black;
            this.Controls.Add(label3);



            Label label4 = new Label();
            label4.Text = productsSumForWeek.ToString() + " сом";
            label4.Location = new Point(228, 175);
            label4.ForeColor = Color.Black;
            this.Controls.Add(label4);


            Label label5 = new Label();
            label5.Text = orderedDishesSumForWeek.ToString() + " сом";
            label5.Location = new Point(390, 175);
            label5.ForeColor = Color.Black;
            this.Controls.Add(label5);


            Label label6 = new Label();
            label6.Text = (orderedDishesSumForWeek - productsSumForWeek).ToString() + " сом";
            label6.Location = new Point(515, 175);
            label6.ForeColor = Color.Black;
            this.Controls.Add(label6);



            Label label7 = new Label();
            label7.Text = productsSumForMonth.ToString() + " сом";
            label7.Location = new Point(228, 225);
            label7.ForeColor = Color.Black;
            this.Controls.Add(label7);


            Label label8 = new Label();
            label8.Text = orderedDishesSumForMonth.ToString() + " сом";
            label8.Location = new Point(390, 225);
            label8.ForeColor = Color.Black;
            this.Controls.Add(label8);


            Label label9 = new Label();
            label9.Text = (orderedDishesSumForMonth - productsSumForMonth).ToString() + " сом";
            label9.Location = new Point(515, 225);
            label9.ForeColor = Color.Black;
            this.Controls.Add(label9);

        }

        private decimal getUsedProductsSumForDate(string date)
        {
             string sqlExpression = "";
            switch (date)
            {
                case "day":
                    sqlExpression = "getUsedProductsSumForToday";
                    break;
                case "week":
                    sqlExpression = "getUsedProductsSumForWeek";
                    break;
                case "month":
                    sqlExpression = "getUsedProductsSumForMonth";
                    break;
            }
           
            try
            {
                sqlConnection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, sqlConnection);
                // указываем, что команда представляет хранимую процедуру
                command.CommandType = CommandType.StoredProcedure;
                // параметр для ввода имени
                SqlParameter sumParam = new SqlParameter
                {
                    ParameterName = "@sum",
                    SqlDbType = SqlDbType.Money,
                    Direction = ParameterDirection.Output
                };
                // добавляем параметр
                command.Parameters.Add(sumParam);

                command.ExecuteNonQuery();
                return Math.Round((decimal)command.Parameters["@sum"].Value, 2);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                sqlConnection.Close();
            }
        }
    
        private decimal getOrderedDishesSumForDate(string date)
        {
            string sqlExpression = "";
            switch (date)
            {
                case "day":
                    sqlExpression = "getOrderedDishesSumForToday";
                    break;
                case "week":
                    sqlExpression = "getOrderedDishesSumForWeek";
                    break;
                case "month":
                    sqlExpression = "getOrderedDishesSumForMonth";
                    break;
            }

            try
            {
                sqlConnection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, sqlConnection);
                // указываем, что команда представляет хранимую процедуру
                command.CommandType = CommandType.StoredProcedure;
                // параметр для ввода имени
                SqlParameter sumParam = new SqlParameter
                {
                    ParameterName = "@sum",
                    SqlDbType = SqlDbType.Money,
                    Direction = ParameterDirection.Output
                };
                // добавляем параметр
                command.Parameters.Add(sumParam);

                command.ExecuteNonQuery();
                return Math.Round((decimal)command.Parameters["@sum"].Value, 2);
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
