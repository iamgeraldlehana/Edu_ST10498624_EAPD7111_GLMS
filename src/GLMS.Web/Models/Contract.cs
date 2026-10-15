using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models;

public class Contract
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Client")]
    public int ClientId { get; set; }
    public Client? Client { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1);

    public ContractStatus Status { get; set; } = ContractStatus.Draft;

    [Required, StringLength(50)]
    [Display(Name = "Service level")]
    public string ServiceLevel { get; set; } = string.Empty;

    public string? SignedAgreementPath { get; set; }
    public List<ServiceRequest> ServiceRequests { get; set; } = [];
}
