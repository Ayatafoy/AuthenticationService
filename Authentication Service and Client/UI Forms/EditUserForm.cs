﻿using InternshipAuthenticationService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using InternshipAuthenticationService.Client.AuthenticationService;
using InternshipAuthenticationService.Models.ServiceModels;
using InternshipAuthenticationService.Models.OperationResult;
using System.ServiceModel;
using System.Web.UI.WebControls;

namespace InternshipAuthenticationService.Client.UIForms
{
    public partial class EditUserForm : Form
    {
        private User user;
        private User _yourUser;
        public EditUserForm(User user, User yourUser)
        {
            InitializeComponent();
            this.user = user;
            _yourUser = yourUser;
            textBoxLogin.Text = user.Login;
            textBoxFullName.Text = user.FullName;
            LoadRoles();
            if (user.Id == _yourUser.Id)
            {
                comboBoxRole.Visible = false;
                labelRole.Visible = false;
            }

        }

        private void LoadRoles()
        {
            AuthenticationServiceClient client = new AuthenticationServiceClient();
            Task<Role[]> rolesTask = client.GetAllRolesAsync();
            rolesTask.ContinueWith(task =>
            {
                Invoke(new Action(() =>
                {
                    try
                    {
                        comboBoxRole.Items.Clear();
                        foreach (string role in task.Result.Select(r => r.RoleName))
                        {
                            comboBoxRole.Items.Add(role);
                        }
                       Enabled = true;
                        comboBoxRole.Text = user.Roles.First<Role>().RoleName;
                        if (user.Id == _yourUser.Id)
                            comboBoxRole.DropDownStyle = ComboBoxStyle.DropDown;
                    }
                    catch (FaultException<Models.Faults.InvalidRoleFault> exc)
                    {
                        MessageBox.Show(exc.Message);
                    }
                    catch (FaultException exc)
                    {
                        MessageBox.Show(exc.Message);
                    }
                }));
            });
        }
        private void buttonChangePassword_Click(object sender, EventArgs e)
        {
            ChangePasswordForm changePasswordForm = new ChangePasswordForm(user);
            changePasswordForm.Owner = this;
            changePasswordForm.ShowDialog();
        }

        private void buttonEditUser_Click(object sender, EventArgs e)
        {
            ClearErrorProvidres();
            BuildUser();
            try
            {
                AuthenticationServiceClient client = new AuthenticationServiceClient();
            OperationResult serviceResult = client.UpdateUser(user);
            if (CheckServiceResult(serviceResult))
            {
                Close();
            }
            }
            catch (FaultException<Models.Faults.InvalidRoleFault> exc)
            {
                MessageBox.Show(exc.Message);
            }
            catch (FaultException exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private bool CheckServiceResult(OperationResult serviceResult)
        {
            if (!serviceResult.Success)
            {
                if (serviceResult.Errors.Contains(OperationErrors.LoginErr))
                {
                    errorProviderLogin.SetError(textBoxLogin, "Login is not valid!");
                }
                if (serviceResult.Errors.Contains(OperationErrors.FullNameErr))
                {
                    errorProviderFullName.SetError(textBoxFullName, "Full name is not valid!");
                }
                if (serviceResult.Errors.Contains(OperationErrors.RoleErr))
                {
                    errorProviderRole.SetError(comboBoxRole, "Role is not valid!");
                }
            }
            return serviceResult.Success;
        }

        private void BuildUser()
        {
            user.Login = textBoxLogin.Text;
            user.FullName = textBoxFullName.Text;
            List<Role> roles = new List<Role>();
            roles.Add(new Role(comboBoxRole.Text));
            user.Roles = roles;
        }

        private void ClearErrorProvidres()
        {
            errorProviderLogin.Clear();
            errorProviderFullName.Clear();
            errorProviderRole.Clear();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
