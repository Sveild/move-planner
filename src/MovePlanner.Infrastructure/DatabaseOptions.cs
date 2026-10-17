using System.ComponentModel.DataAnnotations;

namespace MovePlanner.Infrastructure;

/// <summary>Connection settings for the PostgreSQL database.</summary>
public sealed class DatabaseOptions
{
    /// <summary>The configuration section the options are bound from.</summary>
    public const string SectionName = "Database";

    /// <summary>Gets or sets the connection string used to reach PostgreSQL.</summary>
    [Required(AllowEmptyStrings = false)]
    public string ConnectionString { get; set; } = string.Empty;
}
