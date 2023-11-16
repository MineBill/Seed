using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ReactiveUI;
using Seed.Models;

namespace Seed.ViewModels;

public class WorkflowViewModel : ViewModelBase
{
    public Workflow Workflow { get; }

    public List<ArtifactViewModel> Artifacts { get; } = new();

    public string Name => $"{Workflow.CreatedAt.Day} - {Workflow.CommitHash}";

    public WorkflowViewModel(Workflow workflow)
    {
        Workflow = workflow;

        foreach (var artifact in Workflow.SupportedPlatformTools)
        {
            Artifacts.Add(new ArtifactViewModel(artifact));
        }
    }
}

public class ArtifactViewModel : ViewModelBase
{
    public Artifact Artifact { get; }

    private bool _isChecked;

    public bool IsChecked
    {
        get => _isChecked;
        set => this.RaiseAndSetIfChanged(ref _isChecked, value);
    }

    public string Name => Artifact.Name;

    public bool IsEditorPackage => Artifact.IsEditor;

    public bool IsCurrentPlatform => Artifact.IsForThisPlatform();

    public ArtifactViewModel(Artifact artifact)
    {
        Artifact = artifact;
        IsChecked = false;
    }
}