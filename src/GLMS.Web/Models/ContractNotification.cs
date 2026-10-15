using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models;

public class ContractNotification
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public Contract? Contract { get; set; }
    [MaxLength(30)]
    public string Department { get; set; } = string.Empty;
    [MaxLength(250)]
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
