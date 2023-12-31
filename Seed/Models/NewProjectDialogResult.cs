namespace Seed.Models;

/// <summary>
/// The result of a new project dialog.
/// </summary>
/// <param name="NewProject">The new project to be created.</param>
/// <param name="Template">The template project. Can be an avares uri.</param>
public record NewProjectDialogResult(Project NewProject, ProjectTemplate Template);