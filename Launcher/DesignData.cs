using System;
using Launcher.DataModels;
using Launcher.Services;
using Launcher.Services.DefaultImplementations;
using Launcher.Services.Dummies;
using Launcher.ViewModels;
using Launcher.ViewModels.Dialogs;
using Launcher.Views;
using Version = LibGit2Sharp.Version;

namespace Launcher;

public class DesignDialog : DialogBase
{
    public DesignDialog()
    {
        PageName = "Design Dialog";
    }
}

public static class DesignData
{
    public static DesignDialog DesignDialogBase = new DesignDialog()
    {
        // PageName = "Design Dialog Base"
    };

    public static readonly ProjectsPageViewModel DesignProjectsPageViewMode =
        new(new DummyEngineManager(), new DummyProjectManager(), new JsonPreferencesManager(), new DummyFileService());

    public static readonly MainViewModel DesignMainViewModel = new(
        new DummyEngineDownloader(),
        new DownloadManager(),
        new JsonPreferencesManager(),
        new DummyFileService(),
        names => DesignProjectsPageViewMode)
    {
        ActiveDownloads =
        [
            new DownloadEntryViewModel(new DownloadEntry { CurrentAction = "Downloading" }),
            new DownloadEntryViewModel(new DownloadEntry { CurrentAction = "Extracting" })
        ]
    };

    public static readonly AuthenticationDialogModel DesignAuthenticationDialogModel = new(new DeviceCodeResponse()
    {
        DeviceCode = "ADW2G1",
        UserCode = "ADW2G1",
        ExpiresIn = 10,
        VerificationUri = new Uri("https://www.gofuck.youself")
    }, new GithubAuthenticator(), new DummyFileService());

    public static readonly ProjectConfigurationDialogModel DesignProjectConfigDialogModel =
        new(new Project("BipBop", "somepath", "somepath.flaxproj"),
            [new Engine { Version = new NormalVersion(System.Version.Parse("1.8")) }]);
}