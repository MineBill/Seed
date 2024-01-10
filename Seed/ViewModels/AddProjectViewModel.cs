using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Seed.Models;
using Seed.Services;

namespace Seed.ViewModels;

public class AddProjectViewModel : ViewModelBase
{
    public record ViewableEngineVersion(EngineVersion Version, string Name)
    {
        public override string ToString()
        {
            return $"{Name} - {Version}";
        }
    }

    public ObservableCollection<ViewableEngineVersion> AvailableEngineVersions { get; } = new();

    private string _projectPath = string.Empty;

    public string ProjectPath
    {
        get => _projectPath;
        set => this.RaiseAndSetIfChanged(ref _projectPath, value);
    }

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private ViewableEngineVersion? _selectedVersion;

    public ViewableEngineVersion? SelectedVersion
    {
        get => _selectedVersion;
        set => this.RaiseAndSetIfChanged(ref _selectedVersion, value);
    }

    // TODO: This feels hacky. Maybe an Interaction<> would be better.
    public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; } = ReactiveCommand.Create(() => { });
    public ReactiveCommand<Unit, Project> AddProjectCommand { get; }

    public AddProjectViewModel(IStorageFile file)
    {
        Task.Run(async () =>
        {
            var projectJson = JsonNode.Parse(await file.OpenReadAsync());
            // very fucking hacky, i know
            var name = projectJson?[nameof(Name)]?.ToString()!;
            var versionStr = projectJson?["Version"]?.ToString()!;
            var version = new NormalVersion(Version.Parse(versionStr));
            var projectPath = Path.GetDirectoryName(file.TryGetLocalPath()!)!;

            Name = name;
            ProjectPath = projectPath;

            var locator = App.Current.Services.GetService<IEngineManager>()!;
            foreach (var engine in locator.Engines)
            {
                AvailableEngineVersions.Add(new ViewableEngineVersion(engine.Version, engine.Name));
            }

            SelectedVersion = AvailableEngineVersions.FirstOrDefault(x => x.Version == version);
            if (SelectedVersion is null)
            {
                // No compatible engine version found.
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Error",
                    "No compatible engine found for this project",
                    icon: Icon.Error);
                await box.ShowWindowDialogAsync(App.Current.MainWindow);
                CloseWindowCommand.Execute();
            }
        });

        AddProjectCommand =
            ReactiveCommand.Create(() => new Project(Name, ProjectPath, SelectedVersion!.Version));
    }
}