using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EF;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Microsoft.Extensions.DependencyInjection;
using n8nCRM.Module.BusinessObjects;

namespace n8nCRM.Module.DatabaseUpdate
{
    // For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
    public class Updater : ModuleUpdater
    {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion)
        {
        }
        public override void UpdateDatabaseAfterUpdateSchema()
        {
            base.UpdateDatabaseAfterUpdateSchema();
            //string name = "MyName";
            //EntityObject1 theObject = ObjectSpace.FirstOrDefault<EntityObject1>(u => u.Name == name);
            //if(theObject == null) {
            //    theObject = ObjectSpace.CreateObject<EntityObject1>();
            //    theObject.Name = name;
            //}

            // The code below creates users and roles for testing purposes only.
            // In production code, you can create users and assign roles to them automatically, as described in the following help topic:
            // https://docs.devexpress.com/eXpressAppFramework/119064/data-security-and-safety/security-system/authentication
#if !RELEASE
            // If a role doesn't exist in the database, create this role
            var defaultRole = CreateDefaultRole();
            var adminRole = CreateAdminRole();

            ObjectSpace.CommitChanges(); //This line persists created object(s).

            UserManager userManager = ObjectSpace.ServiceProvider.GetRequiredService<UserManager>();

            // If a user named 'User' doesn't exist in the database, create this user
            if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "User") == null)
            {
                // Set a password if the standard authentication type is used
                string EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "User", EmptyPassword, (user) =>
                {
                    // Add the Users role to the user
                    user.Roles.Add(defaultRole);
                });
            }

            // If a user named 'Admin' doesn't exist in the database, create this user
            if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "Admin") == null)
            {
                // Set a password if the standard authentication type is used
                string EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "Admin", EmptyPassword, (user) =>
                {
                    // Add the Administrators role to the user
                    user.Roles.Add(adminRole);
                });
            }

            ObjectSpace.CommitChanges(); //This line persists created object(s).

            SeedCrmData();
            ObjectSpace.CommitChanges();
#endif
        }
        void SeedCrmData()
        {
            // Seed customers
            var acme = ObjectSpace.FirstOrDefault<Customer>(c => c.Name == "Acme Corp");
            if (acme == null)
            {
                acme = ObjectSpace.CreateObject<Customer>();
                acme.Name = "Acme Corp";
                acme.Email = "info@acmecorp.com";
                acme.Phone = "+1-555-0100";
                acme.Address = "123 Main St, Springfield, IL 62701";
            }

            var globex = ObjectSpace.FirstOrDefault<Customer>(c => c.Name == "Globex Inc");
            if (globex == null)
            {
                globex = ObjectSpace.CreateObject<Customer>();
                globex.Name = "Globex Inc";
                globex.Email = "contact@globex.com";
                globex.Phone = "+1-555-0200";
                globex.Address = "456 Oak Ave, Shelbyville, IL 62565";
            }

            var initech = ObjectSpace.FirstOrDefault<Customer>(c => c.Name == "Initech");
            if (initech == null)
            {
                initech = ObjectSpace.CreateObject<Customer>();
                initech.Name = "Initech";
                initech.Email = "hello@initech.com";
                initech.Phone = "+1-555-0300";
                initech.Address = "789 Corporate Blvd, Austin, TX 73301";
            }

            // Seed orders (Draft and Confirmed only — Fulfilled is for testing the n8n workflow)
            if (ObjectSpace.FirstOrDefault<Order>(o => o.OrderNumber == "ORD-001") == null)
            {
                var order1 = ObjectSpace.CreateObject<Order>();
                order1.OrderNumber = "ORD-001";
                order1.OrderDate = DateTime.Today.AddDays(-5);
                order1.Status = OrderStatus.Confirmed;
                order1.Customer = acme;

                var item1a = ObjectSpace.CreateObject<OrderItem>();
                item1a.ProductName = "Widget Pro";
                item1a.Quantity = 10;
                item1a.UnitPrice = 29.99m;
                item1a.Order = order1;

                var item1b = ObjectSpace.CreateObject<OrderItem>();
                item1b.ProductName = "Gadget Standard";
                item1b.Quantity = 5;
                item1b.UnitPrice = 49.99m;
                item1b.Order = order1;
            }

            if (ObjectSpace.FirstOrDefault<Order>(o => o.OrderNumber == "ORD-002") == null)
            {
                var order2 = ObjectSpace.CreateObject<Order>();
                order2.OrderNumber = "ORD-002";
                order2.OrderDate = DateTime.Today.AddDays(-2);
                order2.Status = OrderStatus.Draft;
                order2.Customer = globex;

                var item2a = ObjectSpace.CreateObject<OrderItem>();
                item2a.ProductName = "Premium Service Package";
                item2a.Quantity = 1;
                item2a.UnitPrice = 499.00m;
                item2a.Order = order2;
            }

            if (ObjectSpace.FirstOrDefault<Order>(o => o.OrderNumber == "ORD-003") == null)
            {
                var order3 = ObjectSpace.CreateObject<Order>();
                order3.OrderNumber = "ORD-003";
                order3.OrderDate = DateTime.Today;
                order3.Status = OrderStatus.Confirmed;
                order3.Customer = initech;

                var item3a = ObjectSpace.CreateObject<OrderItem>();
                item3a.ProductName = "Enterprise License";
                item3a.Quantity = 3;
                item3a.UnitPrice = 199.99m;
                item3a.Order = order3;

                var item3b = ObjectSpace.CreateObject<OrderItem>();
                item3b.ProductName = "Support Plan (Annual)";
                item3b.Quantity = 3;
                item3b.UnitPrice = 79.99m;
                item3b.Order = order3;
            }
        }
        // Grant CRM entity permissions to the Default role
        void GrantCrmPermissions(PermissionPolicyRole role)
        {
            role.AddTypePermissionsRecursively<Customer>(SecurityOperations.CRUDAccess, SecurityPermissionState.Allow);
            role.AddTypePermissionsRecursively<Order>(SecurityOperations.CRUDAccess, SecurityPermissionState.Allow);
            role.AddTypePermissionsRecursively<OrderItem>(SecurityOperations.CRUDAccess, SecurityPermissionState.Allow);
            role.AddTypePermissionsRecursively<Invoice>(SecurityOperations.CRUDAccess, SecurityPermissionState.Allow);
        }
        public override void UpdateDatabaseBeforeUpdateSchema()
        {
            base.UpdateDatabaseBeforeUpdateSchema();
        }
        PermissionPolicyRole CreateAdminRole()
        {
            PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
            if (adminRole == null)
            {
                adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                adminRole.Name = "Administrators";
                adminRole.IsAdministrative = true;
            }
            return adminRole;
        }
        PermissionPolicyRole CreateDefaultRole()
        {
            PermissionPolicyRole defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
            if (defaultRole == null)
            {
                defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                defaultRole.Name = "Default";

                defaultRole.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
                defaultRole.AddObjectPermission<ModelDifference>(SecurityOperations.ReadWriteAccess, "UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
                defaultRole.AddObjectPermission<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, "Owner.UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);
                GrantCrmPermissions(defaultRole);
            }
            return defaultRole;
        }
    }
}
