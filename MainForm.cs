using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab1_RKP
{
    public partial class MainForm : Form
    {
        private List<FormBtnsSettings> mainFormBtnsSettings = new List<FormBtnsSettings>();
        private Color mainFormBtnsBackColor = Color.DarkBlue;
        private Color mainFormBtnsForeColor = Color.White;
        private string[] mainFormBtnsTexts = { "Единицы измерения", "Продукты", "Виды блюд", "Блюда", "Рецепты","Заказы","Отчет по прибыли" };
        private Size mainFormBtnsSize = new Size(100, 50);
        private Point mainFormBtnsLocation = new Point(100, 200);
        private FormsSettings formsSettings;

        public MainForm()
        {
            InitializeComponent();
            formsSettings = new FormsSettings("Кулинарная книга");
            Text = formsSettings.Text;
            this.BackColor = formsSettings.BackColor;
            this.Location = formsSettings.Location;
            this.Size = new Size(900, 500);
            AddSettingsToList();
            this.Load += Load_MainForm;
        }

        private void AddSettingsToList()
        {
            for (int i = 0; i < 7; i++)
            {
                mainFormBtnsLocation.X = 100 + 100 * i;
                mainFormBtnsSettings.Add(new FormBtnsSettings(mainFormBtnsTexts[i], mainFormBtnsBackColor, mainFormBtnsForeColor, mainFormBtnsSize, mainFormBtnsLocation));
            }
        }

        private void Load_MainForm(object sender, EventArgs e)
        {
            int i = 0;
            foreach (FormBtnsSettings item in mainFormBtnsSettings)
            {
                i++;
                Button btn = new Button();
                btn.Text = item.Text;
                btn.BackColor = item.BackColor;
                btn.ForeColor = item.ForeColor;
                btn.Size = item.Size;
                btn.Location = item.Location;
                btn.Name = $"btn{i}";
                btn.Click += AddBtnsClickEvent;
                this.Controls.Add(btn);
            }
        }

        private void AddBtnsClickEvent(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            switch (btn.Name)
            {
                case "btn1":
                    UnitsForm unitsForm = new UnitsForm();
                    unitsForm.Show();
                    break;

                case "btn2":
                    ProductsForm productsForm = new ProductsForm();
                    productsForm.Show();
                    break;

                case "btn3":
                    DishTypesForm dishTypesForm = new DishTypesForm();
                    dishTypesForm.Show();
                    break;

                case "btn4":
                    DishesForm dishesForm = new DishesForm();
                    dishesForm.Show();
                    break;

                case "btn5":
                    RecipesForm recipesForm = new RecipesForm();
                    recipesForm.Show();
                    break;
                case "btn6":
                    OrdersForm ordersFrom = new OrdersForm();
                    ordersFrom.Show();
                    break;
                case "btn7":
                    ProfitReportForm profitReportForm = new ProfitReportForm();
                    profitReportForm.Show();
                    break;
            }
        }
    }
}
