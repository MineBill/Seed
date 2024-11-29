using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Launcher.Services;
using NLog;

namespace Launcher.ViewModels.Dialogs;

public partial class AuthenticationDialogModel : DialogModelBase<string>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private readonly DeviceCodeResponse _code;
    private readonly IFilesService _filesService;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public string Url => _code.VerificationUri.AbsolutePath;
    public string UserCode => _code.UserCode;

    private CancellationTokenSource CancellationTokenSource { get; set; } = new();

    public AuthenticationDialogModel(
        DeviceCodeResponse code,
        GithubAuthenticator authenticator,
        IFilesService filesService)
    {
        _code = code;
        _filesService = filesService;

        Task.Run(async () =>
        {
            try
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource = new CancellationTokenSource();
                Logger.Debug("Before authenticate");
                var result = await authenticator.Authenticate(_code, CancellationTokenSource.Token);
                Logger.Debug("Checking error: {Error}", result.Error);
                ErrorMessage = result.Error switch
                {
                    AuthenticationError.Ok => string.Empty,
                    AuthenticationError.Expired => "Code expired, please restart the authentication process.",
                    AuthenticationError.AccessDenied => """
                                                        You denied access to generate the token.
                                                        If you want to manually generate a token, please follow the
                                                        instructions at <insert-url-here>.
                                                        """,
                    AuthenticationError.Pending => "",
                    AuthenticationError.IncorrectCredentials => "The client id was incorrect, please open a new issue.",
                    AuthenticationError.IncorrectDeviceCode =>
                        "The device code was incorrect, please open a new issue.",
                    AuthenticationError.UnsupportedGrantType => "Unsupported grant type, please open a new issue",
                    AuthenticationError.DeviceFlowDisabled => "The developer really fucked up, please open a new issue",
                    AuthenticationError.Failed => "",
                    _ => throw new ArgumentOutOfRangeException()
                };

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (result.AccessToken is not null)
                    {
                        CloseDialogWithParam(result.AccessToken);
                    }
                    else
                    {
                        CloseDialog();
                    }
                });
            }
            catch (Exception e) when (e is TaskCanceledException or OperationCanceledException)
            {
                Logger.Debug("Operation canceled");
            }
        });
    }

    [RelayCommand]
    private void CancelAndClose()
    {
        CancellationTokenSource.Cancel();
        CloseDialog();
    }

    [RelayCommand]
    private void OpenAuthPage()
    {
        _filesService.OpenUri(_code.VerificationUri);
    }
}