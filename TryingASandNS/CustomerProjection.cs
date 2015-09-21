using Paramol;
using Paramol.SqlClient;
using Projac;

namespace TryingASandNS
{
        public class CustomerProjection: SqlProjection
        {
            public CustomerProjection() 
            {
                When<CustomerCreated>(@event =>
                    TSql.NonQueryStatement(
                        "INSERT INTO [Customer] ([Id], [Name]) VALUES (@P1, @P2)",
                        new {P1 = TSql.UniqueIdentifier(@event.Id), P2 = TSql.NVarChar(@event.CustomerName, 40)}
                        ));
                When<CustomerNameChanged>(@event =>
                    TSql.NonQueryStatement(
                        "UPDATE [Customer] SET [Name] = @P2 WHERE [Id] = @P1",
                        new {P1 = TSql.UniqueIdentifier(@event.Id), P2 = TSql.NVarChar(@event.NewCustomerName, 40)}
                        ));
                When<NewUserAdded>(@event =>
                    TSql.NonQueryStatement(
                        "INSERT INTO [CustomerUser] ([Id], [CustomerId], [Name]) VALUES (@P1, @P2, @P3)",
                        new {P1 = TSql.UniqueIdentifier(@event.UserId), 
                            P2 = TSql.UniqueIdentifier(@event.CustomerId), 
                            P3 = TSql.NVarChar(@event.UserName, 40)}
                        ));
                When<UserNameChanged>(@event =>
                    TSql.NonQueryStatement(
                        "UPDATE [CustomerUser] SET [Name] = @P2 WHERE [Id] = @P1",
                        new {P1 = TSql.UniqueIdentifier(@event.UserId), P2 = TSql.NVarChar(@event.NewUserName, 40)}
                        ));
                When<CreateSchema>(_ =>
                    TSql.NonQueryStatement(
                        @"IF NOT EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='Customer' AND XTYPE='U')
                        BEGIN
                            CREATE TABLE [Customer] (
                                [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Customer PRIMARY KEY, 
                                [Name] NVARCHAR(MAX) NOT NULL)
                        END
                        IF NOT EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='CustomerUser' AND XTYPE='U')
                        BEGIN
                            CREATE TABLE [CustomerUser] (
                                [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerUser PRIMARY KEY, 
                                [CustomerId] UNIQUEIDENTIFIER NOT NULL,
                                [Name] NVARCHAR(MAX) NOT NULL)
                        END"));
                When<DropSchema>(_ => new[] {
                    TSql.NonQueryStatement(
                        @"IF EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='Customer' AND XTYPE='U')
                        DROP TABLE [Customer]"),
                    TSql.NonQueryStatement(
                        @"IF EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='CustomerUser' AND XTYPE='U')
                        DROP TABLE [CustomerUser]")});
                When<DeleteData>(_ => new [] {
                    TSql.NonQueryStatement(
                        @"IF EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='Customer' AND XTYPE='U')
                        DELETE FROM [Customer]"),
                    TSql.NonQueryStatement(
                        @"IF EXISTS (SELECT * FROM SYSOBJECTS WHERE NAME='CustomerUser' AND XTYPE='U')
                        DELETE FROM [CustomerUser]")
                });
            }
        }
}