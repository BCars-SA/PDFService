using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace API.Models;

public class User : IUser
{
    [Key]
    [Column("username")]
    public string? Username { get; set; }

    [Column("password")]
    [JsonIgnore]
    public string? Password { get; set; }

    [Column("vars")]
    public JsonDocument? Vars { get; set; }

    [Column("createdate")]
    public DateTime CreateDate { get; set; }

    [Column("modifydate")]
    public DateTime ModifyDate { get; set; }

    [Column("status", TypeName = "text")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserStatus Status { get; set; }
}

public enum UserStatus
{
    Active,
    Deleted
}
