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

       public  SqlConnection sqlConnection = new SqlConnection(connectionString);

        private DataSet ds;
        private SqlDataAdapter adapter;

        private SqlCommandBuilder commandBuilder;

        public DataTable productsTable;
        public DataTable recipesTable;
        public DataTable dishesTable;
        private DataTable unitsTable;
        private DataTable dataGridViewTable;

        private string selectedDish;
        private int selectedRecipeId = -1;

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
                adapter = new SqlDataAdapter("Select * from recipes; Select dish_id,dish_name,dish_price from dishes order by dish_name; Select product_id,product_name,product_price,unit_id from products order by product_name;select * from units", sqlConnection);
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
            if(this.comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Для добавления новых ингредиентов сначала выберите блюдо");
            }
            else if(this.checkedListBox1.CheckedItems.Count == 0)
            {
                MessageBox.Show("Для добавления новых ингредиентов сначала выберите нужные продукты");
            }
            else
            {

                    selectedDish = this.comboBox1.SelectedItem.ToString();
                    CheckedListBox.CheckedItemCollection checkedProducts = this.checkedListBox1.CheckedItems;
                    RecipeAddForm recipeAddForm = new RecipeAddForm(this, selectedDish, checkedProducts);
                    recipeAddForm.Show();
                    selectedRecipeId = -1;
            }
        }

        private void changeBtn_Click(object sender, EventArgs e)
        {
            if(selectedRecipeId == -1)
            {
                MessageBox.Show("Для изменения количества ингредиента сначала выберите нужный ингредиент");
            }
            else
            {
                decimal productAmount = 0;
                bool productAmountIsDecimal = decimal.TryParse(this.textBox1.Text, out productAmount);
                if(!productAmountIsDecimal || productAmount <= 0)
                {
                    MessageBox.Show("Количество ингредиента должно быть дробным или целым числом и больше нуля");
                }
                else
                {
                    DataRow recipeRow = recipesTable.Select().Where(r => (int)r["recipe_id"] == selectedRecipeId).ToArray()[0];
                    recipeRow["product_amount"] = productAmount;
                    try
                    {
                        sqlConnection.Open();
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
                        selectedRecipeId = -1;
                    }
                }
            }
        }

        private void deleteBtn_Click(object sender, EventArgs e)
        {
            DataRow recipe = null;
            if(selectedRecipeId == -1)
            {
                MessageBox.Show("Для удаления рецепта сначала выберите нужное поле");
            }
            else
            {
                DataRow[] recipes = recipesTable.Select($"recipe_id = {selectedRecipeId}");
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
                        this.comboBox1.Text = "";
                        selectedRecipeId = -1;
                    }
                }
            }
        }

        private void moreInfoBtn_Click(object sender, EventArgs e)
        {

        }

        public void updateDataSet()
        {
            commandBuilder = new SqlCommandBuilder(adapter);
            adapter.Update(ds);
            this.comboBox1.Items.Clear();
            this.checkedListBox1.Items.Clear();
            ds.Clear();
            adapter.Fill(ds);
            fillTables();
        }
        private void fillTables()
        {
            recipesTable = ds.Tables[0];
            dishesTable = ds.Tables[1];
            productsTable = ds.Tables[2];
            unitsTable = ds.Tables[3];
            dataGridViewTable = updateData();
            foreach (DataRow row in dishesTable.Rows)
            {
                this.comboBox1.Items.Add(row["dish_name"]);
            }

            var collection = from t1 in productsTable.AsEnumerable()
                             join t2 in unitsTable.AsEnumerable()
                             on t1["unit_Id"] equals t2["unit_id"]
                             select new { productName = t1["product_name"] + "," + t2["unit_name"] };
            foreach(var item in collection)
            {
                this.checkedListBox1.Items.Add(item.productName);
            }

            this.dataGridView1.DataSource = dataGridViewTable;
            this.dataGridView1.Columns["recipe_id"].Visible = false;
            this.dataGridView1.Columns["unit_name"].Visible = false;
        }

        private DataTable updateData()
        {
            DataTable table = new DataTable();
            var collection = from t1 in recipesTable.AsEnumerable()
                             join t2 in productsTable.AsEnumerable()
                              on t1["product_id"] equals t2["product_id"]
                             join t3 in dishesTable.AsEnumerable()
                               on t1["dish_id"] equals t3["dish_id"]
                             join t4 in unitsTable.AsEnumerable()
                             on t2["unit_id"] equals t4["unit_id"]
                               orderby t3["dish_name"],t2["product_name"]
                             select new { recipeId = t1["recipe_id"], productAmount = t1["product_amount"], productName = t2["product_name"], dishName = t3["dish_name"],unitName = t4["unit_name"]};
            table.Columns.Add("recipe_id", typeof(int));
            table.Columns.Add("product_amount", typeof(decimal));
            table.Columns.Add("product_name", typeof(string));
            table.Columns.Add("dish_name", typeof(string));
            table.Columns.Add("unit_name", typeof(string));
            foreach (var item in collection)
            {
                table.Rows.Add(item.recipeId, Math.Round((decimal)item.productAmount,2), item.productName, item.dishName,item.unitName);
            }
            return table;

        }
     

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                if (row.Cells[0].Value.ToString() == "")
                {
                    selectedRecipeId = -1;
                }
                else
                {
                    selectedRecipeId = (int)row.Cells[0].Value;
                }
            }
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                if (row.Cells[0].Value.ToString() == "")
                {
                    selectedRecipeId = -1;
                }
                else
                {
                    this.textBox1.Text = row.Cells[1].Value.ToString();
                    selectedRecipeId = (int)row.Cells[0].Value;
                }
            }
        }

       
    }
}
