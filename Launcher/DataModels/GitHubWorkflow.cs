using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Launcher.DataModels;

/// <summary>
/// Represents a CI workflow. Contains all the produced artifacts and other relevant information.
/// </summary>
public class GitHubWorkflow
{
    /// <summary>
    /// The id of this workflow.
    /// </summary>
    [JsonPropertyName("id")]
    public ulong Id { get; set; }

    /// <summary>
    /// The branch that was used to run the CI.
    /// </summary>
    [JsonPropertyName("head_branch")]
    public string Branch { get; set; } = string.Empty;

    /// <summary>
    /// The commit that was used for building all the artifacts.
    /// </summary>
    [JsonPropertyName("head_sha")]
    public string CommitHash { get; set; } = string.Empty;

    /// <summary>
    /// Returns only the first 6 characters of the commit hash.
    /// </summary>
    [JsonIgnore]
    public string ShortCommitHash => CommitHash[..6];

    /// <summary>
    /// The status of the run.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// The conclusion of the run.
    /// </summary>
    [JsonPropertyName("conclusion")]
    public string Conclusion { get; set; } = string.Empty;

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("run_number")]
    public int RunNumber { get; set; }

    /// <summary>
    /// When was this workflow created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets the editor artifact for the current operating system.
    /// </summary>
    [JsonIgnore]
    public GitHubArtifact EditorArtifact => Artifacts.First(x => x.IsEditor && x.IsForThisPlatform());

    /// <summary>
    /// The list of all the artifacts produces by this workflow.
    /// </summary>
    [JsonIgnore]
    public List<GitHubArtifact> Artifacts { get; set; } = [];

    /// <summary>
    /// Is this workflow safe to download from?
    /// </summary>
    public bool IsValid => Branch == "master" && Status == "completed" && Conclusion == "success";

    /// <summary>
    /// Get the supported platform tools artifacts for this platform.
    /// </summary>
    public List<GitHubArtifact> SupportedPlatformTools =>
        Artifacts.FindAll(x => !x.IsEditor && x.SupportsThisPlatform());

    /// <summary>
    /// Compare two workflows based on their creation time.
    /// </summary>
    /// <param name="workflow">The workflow to compare with.</param>
    public int CompareTo(GitHubWorkflow workflow)
    {
        return DateTime.Compare(CreatedAt, workflow.CreatedAt);
    }
}

[JsonSerializable(typeof(GitHubWorkflow))]
internal partial class GitHubWorkflowGenerationContext : JsonSerializerContext;