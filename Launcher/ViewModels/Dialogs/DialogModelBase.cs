using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace Launcher.ViewModels.Dialogs;

public partial class DialogModelBase<TResult> : ViewModelBase, IDialogModel<TResult>
{
    public event EventHandler? RequestClose;

    [RelayCommand]
    protected void CloseDialog()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    protected void CloseDialogWithParam(TResult result)
    {
        RequestClose?.Invoke(this, new ResultEventArgs<TResult>(result));
    }

    public Task<ResultEventArgs<TResult>?> ShowDialog()
    {
        RequestClose += (_, args) => { DialogHost.Close(null, args); };
        return DialogHost.Show(this).ContinueWith(t => t.Result as ResultEventArgs<TResult>);
    }
}