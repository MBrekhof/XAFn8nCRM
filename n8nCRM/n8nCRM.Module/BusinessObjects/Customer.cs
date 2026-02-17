#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;

namespace n8nCRM.Module.BusinessObjects;

[DefaultClassOptions]
[DefaultProperty(nameof(Name))]
public class Customer : BaseObject
{
    [RuleRequiredField]
    public virtual string Name { get; set; } = default!;

    public virtual string? Email { get; set; }

    public virtual string? Phone { get; set; }

    public virtual string? Address { get; set; }

    public virtual string? Notes { get; set; }

    public virtual IList<Order> Orders { get; set; } = new ObservableCollection<Order>();
}
