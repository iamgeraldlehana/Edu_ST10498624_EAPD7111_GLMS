using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models;

public class ServiceRequest
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Contract")]
    public int ContractId { get; set; }
    public Contract? Contract { get; set; }

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 100000000)]
    [Display(Name = "Cost (USD)")]
    public decimal CostUsd { get; set; }

    [Display(Name = "USD to ZAR rate")]
    public decimal CurrencyRate { get; set; }

    [Display(Name = "Cost (ZAR)")]
    public decimal CostZar { get; set; }

    public RequestStatus Status { get; set; } = RequestStatus.New;
}
