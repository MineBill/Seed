using System;

namespace Launcher.ViewModels;

public class ResultEventArgs<TResult>(TResult result) : EventArgs
{
    public TResult Result { get; private set; } = result;
}

public interface IDialogModel<TResult>
{
    /// <summary>
    /// Can be used from the View Model to request for it to be closed.
    /// </summary>
    public event EventHandler RequestClose;
    // public delegate void CloseEvent(TResult result);

    // public CloseEvent RequestClose { get; set; }
}