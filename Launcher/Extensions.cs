using System.Threading.Tasks;
using DialogHostAvalonia;
using Launcher.ViewModels;
using Launcher.ViewModels.Dialogs;

namespace Launcher;

public static class Extensions
{
    public static Task<ResultEventArgs<TResult>?> ShowDialogE<TModel, TResult>(this TModel model)
        where TModel : DialogModelBase<TResult>
    {
        model.RequestClose += (_, args) => { DialogHost.Close(null, args); };
        return DialogHost.Show(model).ContinueWith(t => t.Result as ResultEventArgs<TResult>);
    }
}