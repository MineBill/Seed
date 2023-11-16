using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using ReactiveUI;
using Seed.Models;
using System;

namespace Seed.ViewModels;

public class DownloadWorkflowsViewModel : ViewModelBase
{
    private WorkflowViewModel? _selectedWorkflow;

    public WorkflowViewModel? SelectedWorkflow
    {
        get => _selectedWorkflow;
        set => this.RaiseAndSetIfChanged(ref _selectedWorkflow, value);
    }

    public ObservableCollection<WorkflowViewModel> AvailableWorkflows { get; } = new();
    public ReactiveCommand<Unit, DownloadDialogResult<Workflow, Artifact>?> DownloadCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; }

    public DownloadWorkflowsViewModel(List<Workflow> workflows)
    {
        DownloadCommand = ReactiveCommand.Create<DownloadDialogResult<Workflow, Artifact>?>(() =>
        {
            var tools = SelectedWorkflow!.Artifacts.FindAll(x => x.IsChecked);

            return new DownloadDialogResult<Workflow, Artifact>(SelectedWorkflow.Workflow,
                tools.ConvertAll(x => x.Artifact));
        });
        CloseWindowCommand = ReactiveCommand.Create(() => { });

        workflows.RemoveAll(x => x.SupportedPlatformTools.Count <= 0);
        workflows.Sort((a, b) => b.CompareTo(a));
        foreach (var workflow in workflows)
        {
            AvailableWorkflows.Add(new WorkflowViewModel(workflow));
        }

        if (AvailableWorkflows.Count > 0)
            SelectedWorkflow = AvailableWorkflows[0];

        this.WhenAnyValue(x => x.SelectedWorkflow)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(OnSelectedVersionChanged);
    }

    private void OnSelectedVersionChanged(WorkflowViewModel? viewModel)
    {
        if (viewModel!.Artifacts.Count == 0)
        {
            // No platform packages == no editor too.
        }

        foreach (var packageVm in viewModel.Artifacts)
        {
            if (packageVm.IsCurrentPlatform)
            {
                packageVm.IsChecked = true;
                break;
            }
        }
    }
}