USE [Identity]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClaimUserValues]') AND type in (N'U'))
    DROP TABLE [dbo].[ClaimUserValues]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClaimScopes]') AND type in (N'U'))
    DROP TABLE [dbo].[ClaimScopes]
GO

CREATE TABLE [dbo].[ClaimScopes] (
    [ClaimScopeId]  UNIQUEIDENTIFIER  NOT NULL    DEFAULT NEWID(),
    [Name]          NVARCHAR (50)     NOT NULL,
    [Key]           NVARCHAR (50)     NULL, 
    [Description]   VARCHAR (MAX)     NULL
);

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ClaimTypes]') AND type in (N'U'))
    DROP TABLE [dbo].[ClaimTypes]
GO

CREATE TABLE [dbo].[ClaimTypes] (
    [ClaimTypeId]  INT              IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (MAX)   NOT NULL,
    [Namespace]    NVARCHAR (MAX)   NOT NULL,
    [Description]  NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_ClaimType] PRIMARY KEY CLUSTERED ([ClaimTypeId] ASC)
);

CREATE TABLE [dbo].[ClaimUserValues] (
    [ClaimUserValueId]   INT                IDENTITY (1, 1) NOT NULL,
    [ClaimScopeId]       UNIQUEIDENTIFIER   NOT NULL,
    [UserId]             NVARCHAR (450)     NOT NULL,
    [ClaimTypeId]        INT                NOT NULL,
    [Value]              NVARCHAR (MAX)     NOT NULL,
    CONSTRAINT [PK_ClaimUserValue] PRIMARY KEY CLUSTERED ([ClaimUserValueId] ASC),
    CONSTRAINT [FK_HasClaim] FOREIGN KEY ([ClaimTypeId]) REFERENCES [dbo].[ClaimTypes] ([ClaimTypeId]),
    CONSTRAINT [FK_HasUser] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);


IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RoleTypes]') AND type in (N'U'))
    DROP TABLE [dbo].[RoleTypes]
GO

CREATE TABLE [dbo].[RoleTypes] (
    [RoleTypeId]       INT                 IDENTITY (1, 1) NOT NULL,
    [ClaimScopeId]     UNIQUEIDENTIFIER    NOT NULL,
    [Value]            NVARCHAR (MAX)      NOT NULL,
    [Description]      NVARCHAR (MAX)      NULL,
    CONSTRAINT [PK_RoleType] PRIMARY KEY CLUSTERED ([RoleTypeId] ASC)
);

INSERT INTO [dbo].[ClaimScopes]([Name], [Key], [Description])
VALUES
    ('Dashboard', 'dashboard', 'Claims scoped to the login dashboard'),
    ('Application Global', 'application-global', 'Claims scoped to all applications'),
    ('SPA Application', NULL, 'Claims scoped to example SPA application')

INSERT INTO [dbo].[ClaimTypes]([Name], [Namespace])
VALUES
    ('Country',         'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/country'),
    ('DateOfBirth',     'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/dateofbirth'),
    ('Gender',          'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/gender'),
    ('GivenName',       'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname'),
    ('HomePhone',       'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone'),
    ('MobilePhone',     'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone'),
    ('OtherPhone',      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/otherphone'),
    ('PostalCode',      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/postalcode'),
    ('StateOrProvince', 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/stateorprovince'),
    ('StreetAddress',   'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/streetaddress'),
    ('Role',            'http://schemas.microsoft.com/ws/2008/06/identity/claims/role')
