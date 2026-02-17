#nullable enable
using System.ComponentModel;
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

    [RuleRequiredField]
    public virtual Order? Order { get; set; }
}
