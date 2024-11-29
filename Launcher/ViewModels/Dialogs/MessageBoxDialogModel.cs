using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Launcher.ViewModels.Dialogs;

[Flags]
public enum MessageDialogActions
{
    Ok,
    No,
    Yes,
    Confirm,
    Cancel
}

public partial class MessageBoxDialogModel : DialogModelBase<MessageDialogActions>
{
    public record UserAction(string Action, ICommand Command);

    public string Message { get; }

    [ObservableProperty]
    private ObservableCollection<UserAction> _userActions = new();

    public MessageBoxDialogModel() : this("I am a simple message", MessageDialogActions.Ok | MessageDialogActions.Cancel | MessageDialogActions.Yes)
    {
    }

    /// <inheritdoc/>
    public MessageBoxDialogModel(string message, MessageDialogActions userActions)
    {
        Message = message;

        if (userActions.HasFlag(MessageDialogActions.Cancel))
        {
            UserActions.Add(new("Cancel", ActionCancelCommand));
        }

        if (userActions.HasFlag(MessageDialogActions.Ok))
        {
            UserActions.Add(new("OK", ActionOkCommand));
        }

        if (userActions.HasFlag(MessageDialogActions.No))
        {
            UserActions.Add(new("No", ActionNoCommand));
        }

        if (userActions.HasFlag(MessageDialogActions.Yes))
        {
            UserActions.Add(new("Yes", ActionYesCommand));
        }

        if (userActions.HasFlag(MessageDialogActions.Confirm))
        {
            UserActions.Add(new("Confirm", ActionConfirmCommand));
        }
    }


    [RelayCommand]
    private void ActionCancel()
    {
        CloseDialogWithParam(MessageDialogActions.Cancel);
    }

    [RelayCommand]
    private void ActionOk()
    {
        CloseDialogWithParam(MessageDialogActions.Ok);
    }

    [RelayCommand]
    private void ActionNo()
    {
        CloseDialogWithParam(MessageDialogActions.No);
    }

    [RelayCommand]
    private void ActionYes()
    {
        CloseDialogWithParam(MessageDialogActions.Yes);
    }

    [RelayCommand]
    private void ActionConfirm()
    {
        CloseDialogWithParam(MessageDialogActions.Confirm);
    }
}