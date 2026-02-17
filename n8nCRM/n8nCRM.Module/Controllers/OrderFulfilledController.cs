#nullable enable
using System.Net.Http;
using System.Text;
using System.Text.Json;
using DevExpress.ExpressApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using n8nCRM.Module.BusinessObjects;

namespace n8nCRM.Module.Controllers;

public class OrderFulfilledController : ViewController<DetailView>
{
    private readonly HashSet<Guid> _fulfilledOrderIds = new();

    public OrderFulfilledController()
    {
        TargetObjectType = typeof(Order);
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
        ObjectSpace.Committed += ObjectSpace_Committed;
    }

    protected override void OnDeactivated()
    {
        ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
        ObjectSpace.Committed -= ObjectSpace_Committed;
        _fulfilledOrderIds.Clear();
        base.OnDeactivated();
    }

    private void ObjectSpace_ObjectChanged(object? sender, ObjectChangedEventArgs e)
    {
        if (e.Object is Order order
            && e.PropertyName == nameof(Order.Status)
            && e.NewValue is OrderStatus newStatus
            && newStatus == OrderStatus.Fulfilled)
        {
            _fulfilledOrderIds.Add(order.ID);
        }
    }

    private void ObjectSpace_Committed(object? sender, EventArgs e)
    {
        if (_fulfilledOrderIds.Count == 0)
            return;

        var configuration = Application.ServiceProvider.GetRequiredService<IConfiguration>();
        var webhookUrl = configuration["Webhooks:OrderFulfilled"];

        if (string.IsNullOrEmpty(webhookUrl))
        {
            _fulfilledOrderIds.Clear();
            return;
        }

        var order = (Order?)View.CurrentObject;
        if (order == null || !_fulfilledOrderIds.Contains(order.ID))
        {
            _fulfilledOrderIds.Clear();
            return;
        }

        var payload = new
        {
            OrderId = order.ID,
            OrderNumber = order.OrderNumber,
            CustomerName = order.Customer?.Name ?? string.Empty,
            TotalAmount = order.TotalAmount,
            FulfilledAt = DateTime.UtcNow.ToString("o")
        };

        var json = JsonSerializer.Serialize(payload);
        var url = webhookUrl;

        _fulfilledOrderIds.Clear();

        _ = Task.Run(async () =>
        {
            try
            {
                using var httpClient = new HttpClient();
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    $"OrderFulfilledController: Failed to send webhook to {url}. Error: {ex.Message}");
            }
        });
    }
}
