#region Using directives
using System;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NetLogic;
using FTOptix.Core;
using FTOptix.UI;
using FTOptix.WebUI;
#endregion

public class LoginButtonLogic1 : BaseNetLogic
{
    public override void Start()
    {
        ComboBox comboBox = Owner.Owner.Get<ComboBox>("Username");
        if (Project.Current.AuthenticationMode == AuthenticationMode.ModelOnly)
        {
            comboBox.Mode = ComboBoxMode.Normal;
        }
        else
        {
            comboBox.Mode = ComboBoxMode.Editable;
        }
    }

    public override void Stop()
    {
        
    }

    [ExportMethod]
    public void PerformLogin(string username, string password)
    {
        var usersAlias = LogicObject.GetAlias("Users");
        if (usersAlias == null || usersAlias.NodeId == NodeId.Empty)
        {
            Log.Error("LoginButtonLogic", "Missing Users alias");
            return;
        }

        var passwordExpiredDialogType = LogicObject.GetAlias("PasswordExpiredDialogType") as DialogType;
        if (passwordExpiredDialogType == null)
        {
            Log.Error("LoginButtonLogic", "Missing PasswordExpiredDialogType alias");
            return;
        }

        Button loginButton = (Button)Owner;
        loginButton.Enabled = false;

        try
        {
            var outputMessageLabel = Owner.Owner.GetObject("LoginFormOutputMessage");
            var outputMessageLabelResultCode = outputMessageLabel.GetVariable("LoginResultCode");

            var loginResult = Session.Login(username, password);
            outputMessageLabelResultCode.Value = (int)loginResult.ResultCode;

            switch (loginResult.ResultCode)
            {
                case ChangeUserResultCode.Success:
                    // Success
                    break;
                case ChangeUserResultCode.WrongPassword:
                    // WrongPassword
                    break;
                case ChangeUserResultCode.PasswordExpired:
                    loginButton.Enabled = true;
                    var user = usersAlias.Get<User>(username);
                    var ownerButton = (Button)Owner;
                    ownerButton.OpenDialog(passwordExpiredDialogType, user.NodeId);
                    return;
                case ChangeUserResultCode.UserNotFound:
                    loginButton.Enabled = true;
                    Log.Error("LoginButtonLogic", "Could not find user " + username);
                    return;
                case ChangeUserResultCode.UnableToCreateUser:
                    loginButton.Enabled = true;
                    Log.Error("LoginButtonLogic", "Unable to create user " + username);
                    return;
            }

            string outputMessage = outputMessageLabel.GetVariable("Message").Value;
            var outputMessageLogic = outputMessageLabel.GetObject("LoginFormOutputMessageLogic");
            outputMessageLogic.ExecuteMethod("SetOutputMessage", new object[] { outputMessage });
        }
        catch (Exception e)
        {
            Log.Error("LoginButtonLogic", e.Message);
        }

        loginButton.Enabled = true;
    }
}
