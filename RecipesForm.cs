using System;
using System.Collections;
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
    public partial class RecipesForm : Form
    {
        FormsSettings formsSettings;
        static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        SqlConnection sqlConnection = new SqlConnection(connectionString);

        private DataSet ds;
        private SqlDataAdapter adapter;

        private SqlCommandBuilder commandBuilder;

        private DataTable productsTable;
        private DataTable recipesTable;
        private DataTable dishesTable;
        private DataTable dataGridViewTable;

        private string selectedProduct;
        private string selectedDish;

        public RecipesForm()
        {
            InitializeComponent();
            formsSettings = new FormsSettings("Рецепты");
            Text = formsSettings.Text;
            this.BackColor = formsSettings.BackColor;
            this.Location = formsSettings.Location;
            this.Load += recipesForm_Load;
        }


        private void recipesForm_Load(object sender,EventArgs e)
        {
            try
            {
                // Открываем подключение
                sqlConnection.Open();
                adapter = new SqlDataAdapter("Select * from recipes; Select dish_id,dish_name,dish_price from dishes; Select product_id,product_name,product_price from products", sqlConnection);
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
            decimal productAmount;
            selectedDish = (string)this.comboBox1.SelectedItem;
            selectedProduct = (string)this.comboBox2.SelectedItem;

            bool productAmountIsNumber = decimal.TryParse(this.textBox2.Text, out productAmount);

            if (this.textBox2.Text == "" || selectedDish == null || selectedProduct == null)
            {
                MessageBox.Show("Для добавления нового рецепта заполните все поля, кроме ID");
            }
            else if (!productAmountIsNumber || productAmount <= 0)
            {
                MessageBox.Show("Количество продукта может быть как целым,так и дробным числом и должно быть больше нуля");
            }
            else
            {
                bool recipeIsHave = false;
                var recipeCandidate = from r in recipesTable.AsEnumerable()
                                      join d in dishesTable.AsEnumerable()
                                      on r["dish_id"] equals d["dish_id"]
                                      join p in productsTable.AsEnumerable() on r["product_id"] equals p["product_id"]
                                      select new { productName = p["product_name"], dishName = d["dish_name"] };
                foreach (var r in recipeCandidate)
                {
                    if (r.dishName == selectedDish && r.productName == selectedProduct)
                    {
                        recipeIsHave = true;
                        break;
                    }
                }
                if (recipeIsHave)
                {
                    MessageBox.Show($"Блюдо: {selectedDish} с ингредиентом: {selectedProduct} уже существует");
                }
                else
                {
                    try
                    {
                        sqlConnection.Open();
                        DataRow newRow = recipesTable.NewRow();
                        newRow["product_amount"] = productAmount;
                        int dish_id = 0;
                        int product_id = 0;

                        DataRow dish = dishesTable.AsEnumerable().Where(r => r["dish_name"] == selectedDish).ToList()[0];
                        DataRow product = productsTable.AsEnumerable().Where(r => r["product_name"] == selectedProduct).ToList()[0];


                        decimal dishPrice = dish["dish_price"].ToString() != "" ? (decimal)dish["dish_price"] : 0;
                        decimal recipePrice = (int)product["product_price"] * productAmount;
                        dishPrice = recipePrice + dishPrice;
                        dish["dish_price"] = dishPrice;

                        dish_id = (int)dish["dish_id"];
                        product_id = (int)product["product_id"];

                        newRow["dish_id"] = dish_id;
                        newRow["product_id"] = product_id;
                        recipesTable.Rows.Add(newRow);

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
                        this.comboBox2.Text = "";
                    }
                }
            }
        }

        private void changeBtn_Click(object sender, EventArgs e)
        {
            int recipeId = 0;

            DataRow product = null;
            DataRow recipe = null;
            selectedDish = (string)this.comboBox1.SelectedItem;
            selectedProduct = (string)this.comboBox2.SelectedItem;
            decimal productAmount = 0;

            bool recipeIdIsNumber = int.TryParse(this.textBox1.Text, out recipeId);


            if (this.textBox1.Text == "" || this.comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Для изменения рецепта блюда укажите ID рецепта и название блюда");
            }
            else if (this.textBox1.Text != "" && !recipeIdIsNumber)
            {
                MessageBox.Show("ID должно быть числом");
            }
            else if (this.textBox2.Text == "" && this.comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Для изменения рецепта блюда также заполните поля  'Количество продукта' и 'Название продукта' или одно из этих полей");
            }
            else
            {
                int productId = 0;
                if (this.comboBox2.SelectedItem != null)
                {
                    product = productsTable.AsEnumerable().Where(r => r["product_name"] == selectedProduct).ToList()[0];
                    productId = (int)product["product_id"];
                }
                if (this.textBox2.Text != "")
                {
                    bool productAmountIsNumber = decimal.TryParse(this.textBox2.Text, out productAmount);
                    if (!productAmountIsNumber || productAmount <= 0)
                    {
                        MessageBox.Show("Количество продукта должно быть либо целым либо дробным числом и больше нуля");
                        return;
                    }
                }
                DataRow[] recipes = recipesTable.Select($"recipe_id = {recipeId}");
                if (recipes.Length == 0)
                {
                    MessageBox.Show("Рецепта с таким ID не существует");
                }
                else
                {
                    recipe = recipes[0];
                    bool recipeIsHave = false;
                    var recipeCandidate = from r in recipesTable.AsEnumerable()
                                          join d in dishesTable.AsEnumerable()
                                          on r["dish_id"] equals d["dish_id"]
                                          join p in productsTable.AsEnumerable() on r["product_id"] equals p["product_id"]
                                          select new { productName = p["product_name"], dishName = d["dish_name"] };
                    foreach (var r in recipeCandidate)
                    {
                        if (r.dishName == selectedDish && r.productName == selectedProduct)
                        {
                            recipeIsHave = true;
                            break;
                        }
                    }
                    if (recipeIsHave)
                    {
                        MessageBox.Show($"Блюдо: {selectedDish} с ингредиентом: {selectedProduct} уже существует");
                    }
                    else
                    {
                        try
                        {
                            sqlConnection.Open();
                            recipe["product_amount"] = productAmount > 0 ? productAmount : recipe["product_amount"];
                            recipe["product_id"] = productId > 0 ? productId : recipe["product_id"];
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
                            this.comboBox2.Text = "";
                        }
                    }
                }
            }

        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            int recipeId = 0;
            bool recipeIdIsNumber = false;
            DataRow recipe = null;
            recipeIdIsNumber = int.TryParse(this.textBox1.Text, out recipeId);

            if(this.textBox1.Text == "")
            {
                MessageBox.Show("Для удаления рецепта укажите его ID");
            }
            else if(this.textBox1.Text != "" && !recipeIdIsNumber)
            {
                MessageBox.Show("ID должно быть числом");
            }
            else
            {
                DataRow[] recipes = recipesTable.Select($"recipe_id = {recipeId}");
                if(recipes.Length == 0)
                {
                    MessageBox.Show("Рецепта с таким ID не существует");
                }
                else
                {
                    recipe = recipes[0];
                    try
                    {
                        sqlConnection.Open();
                        recipe.Delete();
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
                        this.comboBox2.Text = "";
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
            this.comboBox1.Items.Clear();
            this.comboBox2.Items.Clear();
            ds.Clear();
            adapter.Fill(ds);
            fillTables();
        }
        private void fillTables()
        {
            recipesTable = ds.Tables[0];
            dishesTable = ds.Tables[1];
            productsTable = ds.Tables[2];
            dataGridViewTable = updateData();
            foreach (DataRow row in dishesTable.Rows)
            {
                this.comboBox1.Items.Add(row["dish_name"]);
            }

            foreach (DataRow row in productsTable.Rows)
            {
                this.comboBox2.Items.Add(row["product_name"]);
            }

            this.dataGridView1.DataSource = dataGridViewTable;
        }

        private DataTable updateData()
        {
            DataTable table = new DataTable();
            var collection = from t1 in recipesTable.AsEnumerable()
                             join t2 in productsTable.AsEnumerable()
                              on t1["product_id"] equals t2["product_id"]
                             join t3 in dishesTable.AsEnumerable()
                               on t1["dish_id"] equals t3["dish_id"]
                             select new { recipeId = t1["recipe_id"], productAmount = t1["product_amount"], productName = t2["product_name"], dishName = t3["dish_name"] };
            table.Columns.Add("recipe_id", typeof(int));
            table.Columns.Add("product_amount", typeof(decimal));
            table.Columns.Add("product_name", typeof(string));
            table.Columns.Add("dish_name", typeof(string));
            foreach (var item in collection)
            {
                table.Rows.Add(item.recipeId, Math.Round((decimal)item.productAmount,2), item.productName, item.dishName);
            }
            return table;

        }
    }
}
