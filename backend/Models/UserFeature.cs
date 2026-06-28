// Domain layer — optional per-user capability grants on top of the role system
namespace ProductApi.Models;

[Flags]
public enum UserFeature
{
    None              = 0,
    CanExportExcel    = 1,
    CanViewAuditLog   = 2,
    CanManageInvoices = 4,
}
