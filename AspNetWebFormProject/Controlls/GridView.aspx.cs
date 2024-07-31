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
    private string connectionString = ConfigurationManager.ConnectionStrings["Employeeconnectionstring"].ConnectionString;

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindGridView();
            BindJobTitles();
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

        BindJobTitlesToEditDropdown();
    }

    private void BindJobTitlesToEditDropdown()
    {
        // Loop through each row in the GridView
        foreach (GridViewRow row in GridView1.Rows)
        {
            if (row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlJobTitle = (DropDownList)row.FindControl("ddlJobTitle");
                if (ddlJobTitle != null)
                {
                    ddlJobTitle.DataSource = GetJobTitles();
                    ddlJobTitle.DataTextField = "Title"; // Field to display
                    ddlJobTitle.DataValueField = "JobTitleID"; // Field to submit
                    ddlJobTitle.DataBind();

                    // Set the selected value based on current data
                    HiddenField lblJobTitle = (HiddenField)row.FindControl("hnJobTitle");
                    if (lblJobTitle != null)
                    {
                        foreach (ListItem item in ddlJobTitle.Items)
                        {
                            if (item.Text == lblJobTitle.Value)
                            {
                                ddlJobTitle.SelectedValue = item.Value;
                            }
                        }
                    }
                }
            }
        }
    }

    private DataTable GetJobTitles()
    {
        DataTable dt = new DataTable();
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("SELECT JobTitleID, Title FROM JobTitle", conn))
            {
                conn.Open();
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(dt);
            }
        }
        return dt;
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
        DropDownList ddlJobTitle = (DropDownList)GridView1.Rows[e.RowIndex].FindControl("ddlJobTitle");
        TextBox txtSalary = (TextBox)GridView1.Rows[e.RowIndex].FindControl("txtSalary");

        // Check if any of the controls are null before proceeding
        if (txtFirstName != null && txtLastName != null && txtEmail != null && Convert.ToInt32(ddlJobTitle.SelectedValue) != 0 && txtSalary != null)
        {
            UpdateEmployee(employeeID, txtFirstName.Text, txtLastName.Text, txtEmail.Text, Convert.ToInt32(ddlJobTitle.SelectedValue), Convert.ToDecimal(txtSalary.Text));
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
                    cmd.Parameters.AddWithValue("@JobTitleID", ddlJobTitles.SelectedItem.Value);
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

    private void BindJobTitles()
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            using (SqlCommand cmd = new SqlCommand("SELECT JobTitleID, Title FROM JobTitle", conn))
            {
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                ddlJobTitles.DataSource = reader;
                ddlJobTitles.DataTextField = "Title";  // The text to display in the dropdown
                ddlJobTitles.DataValueField = "JobTitleID";  // The value to be submitted
                ddlJobTitles.DataBind();
            }
        }

        // Optionally add a default item
        ddlJobTitles.Items.Insert(0, new ListItem("Select Job Title", "0"));
    }

    private void ClearInputFields()
    {
        txtFirstName.Text = string.Empty;
        txtLastName.Text = string.Empty;
        txtEmail.Text = string.Empty;
        ddlJobTitles.ClearSelection();
        txtSalary.Text = string.Empty;
        hdnEmployeeID.Value = string.Empty; // Clear hidden field
    }

    private void UpdateEmployee(int employeeID, string firstName, string lastName, string email, int JobTitleID, decimal salary)
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
                cmd.Parameters.AddWithValue("@JobTitleID", JobTitleID);
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