#nullable enable
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;

namespace n8nCRM.Module.BusinessObjects;

[DefaultClassOptions]
[DefaultProperty(nameof(InvoiceNumber))]
public class Invoice : BaseObject
{
    [RuleRequiredField]
    public virtual string InvoiceNumber { get; set; } = default!;

    public virtual DateTime InvoiceDate { get; set; }

    public virtual InvoiceStatus Status { get; set; }

    public virtual decimal TotalAmount { get; set; }

    public virtual string? Notes { get; set; }

    public virtual Guid? OrderID { get; set; }

    // Explicit FK: Invoice is the dependent side of the Order <-> Invoice one-to-one.
    // EF Core 10 no longer infers it when both navigations are optional.
    [RuleRequiredField]
    [ForeignKey(nameof(OrderID))]
    public virtual Order? Order { get; set; }
}
