using System.Reactive;
using System.Windows.Input;
using ReactiveUI;
using Seed.Models;

namespace Seed.ViewModels;

public class CommandLineOptionsViewModel: ViewModelBase
{
    public ReactiveCommand<Unit, string> SaveCommand { get; set; }
    private string _arguments = string.Empty;

    public string Arguments
    {
        get => _arguments;
        set => this.RaiseAndSetIfChanged(ref _arguments, value);
    }

    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public CommandLineOptionsViewModel(Project project)
    {
        Title = $"Editing arguments for {project.Name}";
        Arguments = project.ProjectArguments ?? string.Empty;
        SaveCommand = ReactiveCommand.Create(() => Arguments);
    }
}