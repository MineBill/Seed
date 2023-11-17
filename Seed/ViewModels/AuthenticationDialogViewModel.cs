using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsBox.Avalonia;
using NLog;
using ReactiveUI;
using Seed.Services;
using Seed.Services.Implementations;

namespace Seed.ViewModels;

public class AuthenticationDialogViewModel : ViewModelBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private DeviceCodeResponse _response;

    public string Url => _response.VerificationUri.ToString();
    public string UserCode => _response.UserCode;

    private bool _authenticationComplete;

    public bool AuthenticationComplete
    {
        get => _authenticationComplete;
        set => this.RaiseAndSetIfChanged(ref _authenticationComplete, value);
    }

    public CancellationTokenSource CancellationTokenSource { get; set; } = new();

    public ICommand OpenInBrowserCommand { get; }

    public ReactiveCommand<string?, string?> AuthProcessFinished { get; }
    public event EventHandler<string?>? AuthProcessFinishedEvent;

    public AuthenticationDialogViewModel(DeviceCodeResponse response)
    {
        _response = response;
        OpenInBrowserCommand = ReactiveCommand.Create(() =>
        {
            var filesService = App.Current.Services.GetService<IFilesService>()!;
            filesService.OpenUri(_response.VerificationUri);
        });

        AuthProcessFinished = ReactiveCommand.Create<string?, string?>(token => { return token; });

        var authenticator = App.Current.Services.GetService<GithubAuthenticator>()!;
        Task.Run(async () =>
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource = new CancellationTokenSource();
            var token = CancellationTokenSource.Token;
            try
            {
                var result = await authenticator.Authenticate(response, token);
                var message = result.Error switch
                {
                    AuthenticationError.Ok => string.Empty,
                    AuthenticationError.Expired => "Code expired, please restart the authentication process.",
                    AuthenticationError.AccessDenied => """
                                                        You denied access to generate the token.
                                                        If you want to manually generate a token, please follow the
                                                        instructions at <>
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
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    if (message != string.Empty)
                    {
                        var box = MessageBoxManager.GetMessageBoxStandard(
                            "Authentication Error",
                            message,
                            icon: MsBox.Avalonia.Enums.Icon.Error);
                        await box.ShowWindowDialogAsync(App.Current.MainWindow);
                    }

                    AuthProcessFinishedEvent?.Invoke(this, result.AccessToken);
                });
            }catch (Exception e) when (e is TaskCanceledException or OperationCanceledException)
            {
                Logger.Debug("Canceled active github download.");
            }
        });
    }
}