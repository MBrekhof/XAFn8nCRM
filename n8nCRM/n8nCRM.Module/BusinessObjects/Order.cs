#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;

namespace n8nCRM.Module.BusinessObjects;

[DefaultClassOptions]
[DefaultProperty(nameof(OrderNumber))]
public class Order : BaseObject
{
    [RuleRequiredField]
    public virtual string OrderNumber { get; set; } = default!;

    public virtual DateTime OrderDate { get; set; }

    public virtual OrderStatus Status { get; set; }

    [RuleRequiredField]
    public virtual Customer? Customer { get; set; }

    [Aggregated]
    public virtual IList<OrderItem> OrderItems { get; set; } = new ObservableCollection<OrderItem>();

    [NotMapped]
    public decimal TotalAmount => OrderItems?.Sum(i => i.LineTotal) ?? 0m;

    public virtual Invoice? Invoice { get; set; }
}
