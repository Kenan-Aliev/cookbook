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
    public partial class ShowCheckForm : Form
    {
        FormsSettings formsSettings;

        static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        private SqlConnection sqlConnection = new SqlConnection(connectionString);
        private DataTable dt;
        private SqlDataAdapter adapter;

        private SqlCommandBuilder commandBuilder;

        private OrdersForm ordersForm;

        public ShowCheckForm(OrdersForm ordersForm)
        {
            InitializeComponent();
            this.ordersForm = ordersForm;
            formsSettings = new FormsSettings("Форма выдачи чека");
            this.Text = formsSettings.Text;
            this.BackColor = formsSettings.BackColor;
            this.Location = formsSettings.Location;
            this.Load += ShowCheckForm_Load_1;
        }


        private void ShowCheckForm_Load_1(object sender,EventArgs e)
        {
            try
            {
                sqlConnection.Open();
                adapter = new SqlDataAdapter($"Select dishes.dish_name,ordered_dishes.dish_count,dishes.dish_price from orders join ordered_dishes on orders.order_id = ordered_dishes.order_id join dishes on ordered_dishes.dish_id = dishes.dish_id where orders.order_id = {ordersForm.selectedOrderId}",
                    sqlConnection);
                dt = new DataTable();
                adapter.Fill(dt);
                int y = 117;
                decimal sum = 0;
                foreach(DataRow row in dt.Rows)
                {
                    Label nameLabel = new Label();
                    nameLabel.Text = (string)row["dish_name"];
                    nameLabel.Location = new Point(40, y);
                    nameLabel.ForeColor = Color.Black;

                    Label dishCountLabel = new Label();
                    dishCountLabel.Text = row["dish_count"].ToString();
                    dishCountLabel.Location = new Point(315, y);
                    dishCountLabel.ForeColor = Color.Black;


                    Label dishPriceLabel = new Label();
                    dishPriceLabel.Text = Math.Round(((decimal)row["dish_price"] * (int)row["dish_count"])).ToString();
                    dishPriceLabel.Location = new Point(485, y);
                    dishPriceLabel.ForeColor = Color.Black;

                    sum += (decimal)row["dish_price"] * (int)row["dish_count"];

                    this.Controls.Add(nameLabel);
                    this.Controls.Add(dishCountLabel);
                    this.Controls.Add(dishPriceLabel);

                    y = y + 25;
                }
              
                Label sumLabel = new Label();
                sumLabel.Text = Math.Round(sum).ToString() + " сом";
                sumLabel.Location = new Point(485, 300);
                sumLabel.ForeColor = Color.Black;
                this.Controls.Add(sumLabel);


                DataRow orderRow = ordersForm.ordersTable.Select().Where(r => (int)r["order_id"] == ordersForm.selectedOrderId).ToArray()[0];
                orderRow["order_price"] = sum;
                ordersForm.updateDataSet();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
            }
          
        }

      
    }
}
