# n8n CRM Integration Workflows

This directory contains importable n8n workflow JSON files that integrate with the XAF CRM application via its OData API.

## Workflows

### 1. Order Fulfilled - Create Invoice (`order-fulfilled-create-invoice.json`)

This workflow automatically creates a Draft invoice in the CRM whenever an order is marked as fulfilled. It is triggered by a webhook POST request and performs the following steps:

1. Receives a POST request at `/webhook/order-fulfilled` with order details.
2. Authenticates against the XAF CRM API to obtain a JWT token.
3. Prepares the invoice payload (auto-generates an invoice number, sets status to "Draft", and links the invoice to the originating order).
4. Creates the invoice via the OData `Invoice` endpoint.
5. Returns a JSON success response to the webhook caller.

### 2. Daily Orders Report (`daily-orders-report.json`)

This workflow runs on a daily schedule at 8:00 AM and sends an HTML email report summarizing recent orders. It performs the following steps:

1. Triggers automatically every day at 8:00 AM.
2. Authenticates against the XAF CRM API to obtain a JWT token.
3. Calculates yesterday's date and queries all orders from yesterday onward (with Customer and OrderItems expanded).
4. Formats the results into an HTML email with a summary table showing Order #, Customer, Date, Status, and Total, plus a summary row with the order count and total amount.
5. Sends the report via SMTP email.

## How to Import into n8n

1. Open your n8n instance in a web browser.
2. Go to **Workflows** in the left sidebar.
3. Click **Add Workflow** (the `+` button).
4. In the new workflow editor, click the **three-dot menu** (top-right) and select **Import from File...**.
5. Select the desired `.json` file from this directory.
6. The workflow will be loaded with all nodes and connections pre-configured.
7. Click **Save** to keep the workflow.

Repeat for each workflow file.

## Configuration Required

### For Both Workflows

- **XAF CRM API URL**: The workflows are configured to reach the CRM at `http://xafapp:8080`. If your CRM is hosted at a different address, update the URLs in all HTTP Request nodes accordingly.
- **Authentication Credentials**: The workflows authenticate with `userName: "Admin"` and an empty password. Update the Authenticate node's JSON body if your credentials differ.

### Invoice Creation Workflow

- **Webhook URL**: Once imported and activated, n8n will expose the webhook at `https://<your-n8n-host>/webhook/order-fulfilled`. Configure your XAF application or external system to POST to this URL when an order is fulfilled.
- The expected POST payload is:
  ```json
  {
    "OrderId": "guid-string",
    "OrderNumber": "ORD-001",
    "CustomerName": "Acme Corp",
    "TotalAmount": 549.85,
    "FulfilledAt": "2026-02-17T14:30:00Z"
  }
  ```

### Daily Orders Report Workflow

- **SMTP Credentials**: You must configure SMTP credentials in n8n before this workflow can send emails.
  1. Go to **Settings > Credentials** in n8n.
  2. Create a new **SMTP** credential with your mail server details.
  3. Open the Daily Orders Report workflow, click the **Send Email** node, and select your SMTP credential.
- **Email Addresses**: Update the `toEmail` and `fromEmail` fields in the Send Email node to match your desired recipients and sender address.
- **Schedule**: The workflow is set to run daily at 8:00 AM (server time). Adjust the Schedule Trigger node's cron expression if you need a different time.

## Testing the Invoice Creation Workflow

You can manually test the invoice creation workflow using `curl` after activating it in n8n.

### 1. Activate the Workflow

In the n8n editor, open the "Order Fulfilled - Create Invoice" workflow and toggle it to **Active**.

### 2. Send a Test Request

Run the following `curl` command (replace `<your-n8n-host>` with your actual n8n instance URL):

```bash
curl -X POST https://<your-n8n-host>/webhook/order-fulfilled \
  -H "Content-Type: application/json" \
  -d '{
    "OrderId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "OrderNumber": "ORD-TEST-001",
    "CustomerName": "Test Customer",
    "TotalAmount": 299.99,
    "FulfilledAt": "2026-02-17T14:30:00Z"
  }'
```

### 3. Expected Response

If everything is configured correctly, you should receive a JSON response like:

```json
{
  "success": true,
  "message": "Invoice created successfully",
  "invoice": {
    "ID": "...",
    "InvoiceNumber": "INV-a1b2c3d4",
    "InvoiceDate": "2026-02-17T...",
    "Status": "Draft",
    "TotalAmount": 299.99
  }
}
```

### 4. Verify in the CRM

Log into the XAF CRM application and navigate to the Invoices list to confirm the new Draft invoice was created and linked to the correct order.

## Troubleshooting

- **Authentication errors**: Verify the CRM is reachable at the configured URL and the credentials are correct.
- **Webhook not responding**: Make sure the workflow is set to Active in n8n. Inactive workflows do not listen for webhooks.
- **Email not sending**: Confirm SMTP credentials are properly configured and the mail server is reachable from the n8n instance.
- **OData errors**: Check that the XAF CRM application is running and the OData endpoints are enabled. The API should be accessible at `http://xafapp:8080/api/odata/`.
