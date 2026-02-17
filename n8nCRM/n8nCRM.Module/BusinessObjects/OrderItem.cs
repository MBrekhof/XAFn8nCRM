#nullable enable
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;

namespace n8nCRM.Module.BusinessObjects;

[DefaultClassOptions]
[DefaultProperty(nameof(ProductName))]
public class OrderItem : BaseObject
{
    [RuleRequiredField]
    public virtual string ProductName { get; set; } = default!;

    public virtual int Quantity { get; set; }

    public virtual decimal UnitPrice { get; set; }

    [NotMapped]
    public decimal LineTotal => Quantity * UnitPrice;

    [RuleRequiredField]
    public virtual Order? Order { get; set; }
}
