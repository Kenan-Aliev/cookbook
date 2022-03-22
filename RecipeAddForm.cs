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
    public partial class RecipeAddForm : Form
    {
        private RecipesForm recipesForm;
        private CheckedListBox.CheckedItemCollection checkedProducts;
        private string selectedDish;

        Dictionary<Label, TextBox> textboxes = null;
        Dictionary<Button, Label> btns = null;

        FormsSettings formsSettings;
        static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        SqlConnection sqlConnection = new SqlConnection(connectionString);

        private DataSet ds;
        private SqlDataAdapter adapter;

        private SqlCommandBuilder commandBuilder;

        public RecipeAddForm(RecipesForm recipesForm, string selectedDish,CheckedListBox.CheckedItemCollection checkedProducts)
        {
            InitializeComponent();
            this.recipesForm = recipesForm;
            this.checkedProducts = checkedProducts;
            this.selectedDish = selectedDish;
            btns = new Dictionary<Button, Label>();
            textboxes = new Dictionary<Label, TextBox>();
            formsSettings = new FormsSettings("Форма для добавления количества выбранных ингредиентов");
            this.Text = formsSettings.Text;
            this.BackColor = formsSettings.BackColor;
            this.Location = formsSettings.Location;
            this.Load += RecipeAddForm_Load;
        }

        private void RecipeAddForm_Load(object sender, EventArgs e)
        {
            int y = 0;

           foreach(var item in checkedProducts)
            {
                Console.WriteLine(item);
                Label label = new Label();
                label.Text = item.ToString();
                label.ForeColor = Color.Red;
                label.Location = new Point(50, 50 + y);

                TextBox textBox = new TextBox();
                textBox.Location = new Point(200, 50 + y);
                textBox.Size = new Size(100, 22);

                Button btn = new Button();
                btn.Text = "Удалить ингредиент из списка";
                btn.ForeColor = Color.White;
                btn.BackColor = Color.Red;
                btn.Size = new Size(130, 35);
                btn.Location = new Point(400, 40 + y);
                btn.Click += deleteBtnClick;

                btns.Add(btn, label);
                textboxes.Add(label, textBox);

                this.Controls.Add(label);
                this.Controls.Add(btn);
                this.Controls.Add(textBox);

                y = y + 35;
            }
        }

        private void addBtn_Click(object sender, EventArgs e)
        {

            if (textboxes.Count == 0)
            {
                MessageBox.Show("Для добавления ингредиентов сначала выберите нужные ингредиенты");
                this.Dispose();
            }
            else
            {
                foreach (var item in textboxes)
                {
                    decimal recipeCount = 0;
                    bool recipeCountIsDecimal = false;
                    recipeCountIsDecimal = decimal.TryParse(item.Value.Text, out recipeCount);
                    if (!recipeCountIsDecimal || recipeCount <= 0)
                    {
                        MessageBox.Show($"Количество у ингредиента:{item.Key.Text} должно быть дробным или целым числом и больше нуля");
                        return;
                    }
                    bool productIsHave = false;
                    DataRow newRow = recipesForm.recipesTable.NewRow();
                    string productName = item.Key.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0];
                    DataRow productRow = recipesForm.productsTable.Select().Where(r => r["product_name"].ToString() == productName).ToArray()[0];
                    DataRow dishRow = recipesForm.dishesTable.Select().Where(r => r["dish_name"].ToString() == selectedDish).ToArray()[0];

                    foreach (DataRow row in recipesForm.recipesTable.Rows)
                    {
                        if (row["product_id"].ToString() == productRow["product_id"].ToString() && row["dish_id"].ToString() == dishRow["dish_id"].ToString())
                        {
                            productIsHave = true;
                            break;
                        }
                    }
                    if (productIsHave)
                    {
                        MessageBox.Show($"Блюдо: {selectedDish} с ингредиентом: {productName} уже существует");
                        return;
                    }
                    newRow["product_amount"] = recipeCount;
                    newRow["product_id"] = productRow["product_id"];
                    newRow["dish_id"] = dishRow["dish_id"];
                    recipesForm.recipesTable.Rows.Add(newRow);
                }
                try
                {
                    recipesForm.sqlConnection.Open();
                    recipesForm.updateDataSet();
                    MessageBox.Show("Вы успешно добавили ингредиенты");
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    recipesForm.sqlConnection.Close();
                    this.Dispose();
                }

            }
        }



        private void deleteBtnClick(object sender, EventArgs e)
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
    }
}
