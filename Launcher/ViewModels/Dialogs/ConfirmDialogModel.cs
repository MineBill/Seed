using CommunityToolkit.Mvvm.Input;

namespace Launcher.ViewModels.Dialogs;

public enum ConfirmAction
{
    No,
    Yes
}

public partial class ConfirmDialogModel(string message) : DialogModelBase<ConfirmAction>
{
    public string Message => message;

    public ConfirmDialogModel() : this("Are you sure?")
    {
    }

    [RelayCommand]
    private void Confirm(ConfirmAction param)
    {
        CloseDialogWithParam(param);
    }
}