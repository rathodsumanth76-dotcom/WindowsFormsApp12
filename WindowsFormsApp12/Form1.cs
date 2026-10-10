
using System;
using System.Data;
using System.Windows.Forms;
using Oracle.ManagedDataAccess.Client;

namespace WindowsFormsApp12
{
    public partial class Form1 : Form
    {
        private readonly string connectionString =
            "User Id=system;" +
            "Password=1234;" +
            "Data Source=localhost:1521/XE;";

        public Form1()
        {
            InitializeComponent();

            // Configure the Category ComboBox.
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new object[]
            {
                "Electronics",
                "Clothing",
                "Grocery",
                "Furniture",
                "Books",
                "Accessories",
                "Other"
            });

            // Configure Quantity.
            numericUpDown1.Minimum = 0;
            numericUpDown1.Maximum = 1000000;

            // Connect button clicks to their methods.
            // Do not also connect these events in the Designer.
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnDelete.Click += btnDelete_Click;
            btnDelete.Click += btnClear_Click;
        }

        // ADD: Save a new product in Oracle.
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateProduct(out int id, out decimal price,
                                 out int quantity))
                return;

            try
            {
                using (OracleConnection con =
                       new OracleConnection(connectionString))
                {
                    con.Open();

                    string sql = @"
                        INSERT INTO RETAIL_PRODUCTS
                        (PRODUCT_ID, PRODUCT_NAME, CATEGORY,
                         PRICE, QUANTITY)
                        VALUES
                        (:id, :name, :category, :price, :quantity)";

                    using (OracleCommand cmd =
                           new OracleCommand(sql, con))
                    {
                        cmd.BindByName = true;

                        cmd.Parameters.Add("id", OracleDbType.Int32)
                            .Value = id;
                        cmd.Parameters.Add("name", OracleDbType.Varchar2)
                            .Value = txtProductName.Text.Trim();
                        cmd.Parameters.Add("category", OracleDbType.Varchar2)
                            .Value = comboBox1.Text;
                        cmd.Parameters.Add("price", OracleDbType.Decimal)
                            .Value = price;
                        cmd.Parameters.Add("quantity", OracleDbType.Int32)
                            .Value = quantity;

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show(
                    "Product saved successfully in Oracle Database!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                ClearFields();
            }
            catch (OracleException ex)
            {
                MessageBox.Show(
                    "Oracle database error:\n" + ex.Message,
                    "Database Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // UPDATE: Change an existing product using its Product ID.
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateProduct(out int id, out decimal price,
                                 out int quantity))
                return;

            try
            {
                using (OracleConnection con =
                       new OracleConnection(connectionString))
                {
                    con.Open();

                    string sql = @"
                        UPDATE RETAIL_PRODUCTS
                        SET PRODUCT_NAME = :name,
                            CATEGORY = :category,
                            PRICE = :price,
                            QUANTITY = :quantity
                        WHERE PRODUCT_ID = :id";

                    using (OracleCommand cmd =
                           new OracleCommand(sql, con))
                    {
                        cmd.BindByName = true;

                        cmd.Parameters.Add("name", OracleDbType.Varchar2)
                            .Value = txtProductName.Text.Trim();
                        cmd.Parameters.Add("category", OracleDbType.Varchar2)
                            .Value = comboBox1.Text;
                        cmd.Parameters.Add("price", OracleDbType.Decimal)
                            .Value = price;
                        cmd.Parameters.Add("quantity", OracleDbType.Int32)
                            .Value = quantity;
                        cmd.Parameters.Add("id", OracleDbType.Int32)
                            .Value = id;

                        int rows = cmd.ExecuteNonQuery();

                        MessageBox.Show(rows > 0
                            ? "Product updated successfully!"
                            : "Product ID not found.");
                    }
                }
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Oracle error:\n" + ex.Message);
            }
        }

        // DELETE: Remove a product using its Product ID.
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtProductID.Text.Trim(), out int id)
                || id <= 0)
            {
                MessageBox.Show("Enter a valid Product ID.");
                txtProductID.Focus();
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Do you want to delete this product?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
                return;

            try
            {
                using (OracleConnection con =
                       new OracleConnection(connectionString))
                {
                    con.Open();

                    string sql =
                        "DELETE FROM RETAIL_PRODUCTS " +
                        "WHERE PRODUCT_ID = :id";

                    using (OracleCommand cmd =
                           new OracleCommand(sql, con))
                    {
                        cmd.BindByName = true;
                        cmd.Parameters.Add("id", OracleDbType.Int32)
                            .Value = id;

                        int rows = cmd.ExecuteNonQuery();

                        MessageBox.Show(rows > 0
                            ? "Product deleted successfully!"
                            : "Product ID not found.");
                    }
                }

                ClearFields();
            }
            catch (OracleException ex)
            {
                MessageBox.Show("Oracle error:\n" + ex.Message);
            }
        }

        // CLEAR: Empty all form fields.
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            txtProductID.Clear();
            txtProductName.Clear();
            txtPrice.Clear();
            comboBox1.SelectedIndex = -1;
            numericUpDown1.Value = 0;

            txtProductID.Focus();
        }

        // Validate the input before saving or updating.
        private bool ValidateProduct(
            out int id, out decimal price, out int quantity)
        {
            id = 0;
            price = 0;
            quantity = (int)numericUpDown1.Value;

            if (!int.TryParse(txtProductID.Text.Trim(), out id)
                || id <= 0)
            {
                MessageBox.Show("Enter a valid positive Product ID.");
                txtProductID.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Enter Product Name.");
                txtProductName.Focus();
                return false;
            }

            if (comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show("Select a Category.");
                comboBox1.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(), out price)
                || price < 0 || price > 99999999.99m)
            {
                MessageBox.Show("Enter a valid Price.");
                txtPrice.Focus();
                return false;
            }

            return true;
        }
    }
}