using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Controlls_GridView : System.Web.UI.Page
{
    private string connectionString = ConfigurationManager.ConnectionStrings["YourConnectionString"].ConnectionString;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindGridView();
        }
    }

    // Bind data to the GridView
    private void BindGridView()
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = "GetAllEmployees"; // Your table name
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    sda.Fill(dt);
                    GridView1.DataSource = dt;
                    GridView1.DataBind();
                }
            }
        }
    }

    // Handle the RowEditing event
    protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
    {
        GridView1.EditIndex = e.NewEditIndex;
        BindGridView();
    }

    // Handle the RowUpdating event
    protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        // Get the EmployeeID from the DataKeys collection
        int employeeID = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Value);

        // Retrieve the new values from the edit controls
        TextBox txtFirstName = (TextBox)GridView1.Rows[e.RowIndex].FindControl("txtFirstName");
        TextBox txtLastName = (TextBox)GridView1.Rows[e.RowIndex].FindControl("txtLastName");
        TextBox txtEmail = (TextBox)GridView1.Rows[e.RowIndex].FindControl("txtEmail");
        TextBox txtJobTitle = (TextBox)GridView1.Rows[e.RowIndex].FindControl("txtJobTitle");
        TextBox txtSalary = (TextBox)GridView1.Rows[e.RowIndex].FindControl("txtSalary");

        // Check if any of the controls are null before proceeding
        if (txtFirstName != null && txtLastName != null && txtEmail != null && txtJobTitle != null && txtSalary != null)
        {
            UpdateEmployee(employeeID, txtFirstName.Text, txtLastName.Text, txtEmail.Text, txtJobTitle.Text, Convert.ToDecimal(txtSalary.Text));
        }

        // Exit edit mode
        GridView1.EditIndex = -1;

        // Rebind the GridView to refresh the data
        BindGridView();
    }
    protected void btnInsert_Click(object sender, EventArgs e)
    {
        // Insert logic as previously defined...
        string firstName = txtFirstName.Text;
        string lastName = txtLastName.Text;
        string email = txtEmail.Text;
        string jobTitle = txtJobTitle.Text;
        decimal salary;

        if (decimal.TryParse(txtSalary.Text, out salary))
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                
                using (SqlCommand cmd = new SqlCommand("AddEmployee", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FirstName", firstName);
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@HireDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@JobTitle", jobTitle);
                    cmd.Parameters.AddWithValue("@Salary", salary);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            // Clear input fields
            ClearInputFields();

            // Rebind the GridView
            BindGridView();
        }
    }

    private void ClearInputFields()
    {
        txtFirstName.Text = string.Empty;
        txtLastName.Text = string.Empty;
        txtEmail.Text = string.Empty;
        txtJobTitle.Text = string.Empty;
        txtSalary.Text = string.Empty;
        hdnEmployeeID.Value = string.Empty; // Clear hidden field
    }

    private void UpdateEmployee(int employeeID, string firstName, string lastName, string email, string jobTitle, decimal salary)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            
            using (SqlCommand cmd = new SqlCommand("UpdateEmployee", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@JobTitle", jobTitle);
                cmd.Parameters.AddWithValue("@Salary", salary);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }

    // Handle the RowCancelingEdit event
    protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {
        GridView1.EditIndex = -1;
        BindGridView();
    }

    // Handle the RowDeleting event
    protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        int employeeID = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Value);
        DeleteEmployee(employeeID);
        BindGridView();
    }

    private void DeleteEmployee(int employeeID)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {           
            using (SqlCommand cmd = new SqlCommand("DeleteEmployee", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@EmployeeID", employeeID);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}