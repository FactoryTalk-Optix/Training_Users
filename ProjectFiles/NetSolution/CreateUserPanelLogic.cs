#region Using directives
using System;
using System.Linq;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NetLogic;
using FTOptix.OPCUAServer;
using FTOptix.UI;
using FTOptix.WebUI;
#endregion

public class CreateUserPanelLogic : BaseNetLogic
{
    [ExportMethod]
    public void CreateUser(string username, string password, string locale, out NodeId result)
    {
		result = NodeId.Empty;
		if (string.IsNullOrEmpty(username))
		{
			ShowMessage("Cannot create user with empty username");
			return;
		}

		result = GenerateUser(username, password, locale);
    }

    private NodeId GenerateUser(string username, string password, string locale)
    {
		var users = GetUsers();
		if (users == null)
		{
			ShowMessage("Unable to get users");
			return NodeId.Empty;
		}
		foreach (var child in users.Children.OfType<FTOptix.Core.User>())
		{
			if (child.BrowseName.Equals(username, StringComparison.OrdinalIgnoreCase))
			{
				ShowMessage("Username '" + username + "' already exists");
				return NodeId.Empty;
			}
		}

		var user = InformationModel.MakeObject<FTOptix.Core.User>(username);
		user.LocaleId = locale;

		users.Add(user);
		var result = Session.ChangePassword(username, password, string.Empty);
		if (result.ResultCode == FTOptix.Core.ChangePasswordResultCode.PasswordTooShort)
		{
			ShowMessage("Password too short");

			users.Remove(user);
			return NodeId.Empty;
		}

		return user.NodeId;
	}

    private IUANode GetUsers()
	{
		var pathResolverResult = LogicObject.Context.ResolvePath(LogicObject, "{Users}");
		if (pathResolverResult == null)
			return null;
		if (pathResolverResult.ResolvedNode == null)
			return null;

		return pathResolverResult.ResolvedNode;
	}

	private void ShowMessage(string message)
    {
		Log.Error("CreateUserPanelLogic", message);
		var errorMessageVariable = LogicObject.GetVariable("ErrorMessage");
		if (errorMessageVariable != null)
			errorMessageVariable.Value = message;
	}
}
