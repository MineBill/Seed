using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Seed.Models;

namespace Seed.ViewModels;

/// <summary>
///
/// </summary>
public class Workflow
{
    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("id")]
    public ulong Id { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("head_branch")]
    public string Branch { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("head_sha")]
    public string CommitHash { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("conclusion")]
    public string Conclusion { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("run_number")]
    public int RunNumber { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///
    /// </summary>
    [JsonIgnore]
    public Artifact EditorArtifact => Artifacts.First(x => x.IsEditor && x.IsForThisPlatform());

    /// <summary>
    ///
    /// </summary>
    [JsonIgnore]
    public List<Artifact> Artifacts { get; set; }

    public bool IsValid => Branch == "master" && Status == "completed" && Conclusion == "success";

    public List<Artifact> SupportedPlatformTools => Artifacts.FindAll(x => !x.IsEditor && x.IsForThisPlatform());

    /// <summary>
    /// Compare two workflows based on their creation time.
    /// </summary>
    /// <param name="workflow">The workflow to compare with.</param>
    public int CompareTo(Workflow workflow)
    {
        return DateTime.Compare(CreatedAt, workflow.CreatedAt);
    }
}