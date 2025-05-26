using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace ECD_WPF
{
    public partial class MainWindow : Window
    {
        private string connectionString;

        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["ECD_DatabaseConnection"].ConnectionString;
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadDataIntoDataGrid();
        }

        private void LoadDataIntoDataGrid()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT CommercialInvoiceNo, ShipDate, Forwarder, BLNo, PO, Line, PartNo, VendorCode, Qty, UnitPrice, Amount, Currency, FinanceDate, CustomerName FROM Customer";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dataGridCustomers.ItemsSource = dt.DefaultView;

                }
                catch (SqlException ex)
                {
                    MessageBox.Show("Lỗi khi tải dữ liệu từ Database: " + ex.Message, "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Đã xảy ra lỗi không mong muốn khi tải dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            ClearInputFields();
        }

        private void ClearInputFields()
        {
            txt_CIN.Text = "";
            txt_forwarder.Text = "";
            txt_blno.Text = "";
            txt_PO.Text = "";
            txt_line.Text = "";
            txt_part.Text = "";
            txt_vendorcode.Text = "";
            txt_qty.Text = "";
            txt_unit.Text = "";
            txt_Amount.Text = "";
            txt_currency.Text = "";
            txt_客户.Text = "";
            dtp_shipdate.SelectedDate = null;
            dtb_财务.SelectedDate = null;
        }

        private void CalculateAndDisplayAmount()
        {
            if (int.TryParse(txt_qty.Text, out int qty) &&
                decimal.TryParse(txt_unit.Text, out decimal unitPrice))
            {
                decimal amount = qty * unitPrice;
                txt_Amount.Text = amount.ToString("N2");
            }
            else
            {
                txt_Amount.Text = "";
            }
        }

        private void txt_qty_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateAndDisplayAmount();
        }

        private void txt_unit_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalculateAndDisplayAmount();
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            string commercialInvoiceNo = txt_CIN.Text.Trim();
            DateTime? shipDate = dtp_shipdate.SelectedDate;

            string forwarder = txt_forwarder.Text.Trim();
            string blNo = txt_blno.Text.Trim();
            string po = txt_PO.Text.Trim();
            string line = txt_line.Text.Trim();
            string partNo = txt_part.Text.Trim();
            string vendorCode = txt_vendorcode.Text.Trim();

            int qty;
            if (!int.TryParse(txt_qty.Text, out qty))
            {
                MessageBox.Show("Số lượng (Qty) phải là một số nguyên hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                txt_qty.Focus();
                return;
            }

            decimal unitPrice;
            if (!decimal.TryParse(txt_unit.Text, out unitPrice))
            {
                // Dòng này đã được sửa
                MessageBox.Show("Giá đơn vị (Unit price) phải là một số hợp lệ.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                txt_unit.Focus();
                return;
            }

            decimal amount = qty * unitPrice;

            string currency = txt_currency.Text.Trim();
            DateTime? financeDate = dtb_财务.SelectedDate;
            string customerName = txt_客户.Text.Trim();

            if (string.IsNullOrWhiteSpace(commercialInvoiceNo))
            {
                MessageBox.Show("Commercial Invoice No. không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                txt_CIN.Focus();
                return;
            }

            if (!shipDate.HasValue)
            {
                MessageBox.Show("Ngày giao hàng không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                dtp_shipdate.Focus();
                return;
            }
            if (!financeDate.HasValue)
            {
                MessageBox.Show("Ngày tài chính không được để trống.", "Lỗi nhập liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                dtb_财务.Focus();
                return;
            }


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Customer (CommercialInvoiceNo, ShipDate, Forwarder, BLNo, PO, Line, PartNo, VendorCode, Qty, UnitPrice, Amount, Currency, FinanceDate, CustomerName)
                                 VALUES (@CommercialInvoiceNo, @ShipDate, @Forwarder, @BLNo, @PO, @Line, @PartNo, @VendorCode, @Qty, @UnitPrice, @Amount, @Currency, @FinanceDate, @CustomerName)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CommercialInvoiceNo", commercialInvoiceNo);
                    command.Parameters.AddWithValue("@ShipDate", shipDate.Value);

                    command.Parameters.AddWithValue("@Forwarder", string.IsNullOrEmpty(forwarder) ? (object)DBNull.Value : forwarder);
                    command.Parameters.AddWithValue("@BLNo", string.IsNullOrEmpty(blNo) ? (object)DBNull.Value : blNo);

                    command.Parameters.AddWithValue("@PO", po);
                    command.Parameters.AddWithValue("@Line", line);
                    command.Parameters.AddWithValue("@PartNo", partNo);
                    command.Parameters.AddWithValue("@VendorCode", vendorCode);
                    command.Parameters.AddWithValue("@Qty", qty);
                    command.Parameters.AddWithValue("@UnitPrice", unitPrice);
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@Currency", currency);

                    command.Parameters.AddWithValue("@FinanceDate", financeDate.Value);
                    command.Parameters.AddWithValue("@CustomerName", string.IsNullOrEmpty(customerName) ? (object)DBNull.Value : customerName);

                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Dữ liệu đã được lưu thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                            ClearInputFields();
                            LoadDataIntoDataGrid();
                        }
                        else
                        {
                            MessageBox.Show("Lưu dữ liệu không thành công. Không có dòng nào bị ảnh hưởng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Lỗi Database: " + ex.Message + "\nSource: " + ex.Source, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Đã xảy ra lỗi: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void dataGridCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridCustomers.SelectedItem is DataRowView selectedRow)
            {
                txt_CIN.Text = selectedRow["CommercialInvoiceNo"]?.ToString();

                dtp_shipdate.SelectedDate = (selectedRow["ShipDate"] != DBNull.Value) ? Convert.ToDateTime(selectedRow["ShipDate"]) : (DateTime?)null;

                txt_forwarder.Text = selectedRow["Forwarder"]?.ToString();
                txt_blno.Text = selectedRow["BLNo"]?.ToString();
                txt_PO.Text = selectedRow["PO"]?.ToString();
                txt_line.Text = selectedRow["Line"]?.ToString();
                txt_part.Text = selectedRow["PartNo"]?.ToString();
                txt_vendorcode.Text = selectedRow["VendorCode"]?.ToString();
                txt_qty.Text = selectedRow["Qty"]?.ToString();
                txt_unit.Text = selectedRow["UnitPrice"]?.ToString();
                txt_Amount.Text = selectedRow["Amount"]?.ToString();
                txt_currency.Text = selectedRow["Currency"]?.ToString();

                dtb_财务.SelectedDate = (selectedRow["FinanceDate"] != DBNull.Value) ? Convert.ToDateTime(selectedRow["FinanceDate"]) : (DateTime?)null;

                txt_客户.Text = selectedRow["CustomerName"]?.ToString();
            }
        }
    }
}