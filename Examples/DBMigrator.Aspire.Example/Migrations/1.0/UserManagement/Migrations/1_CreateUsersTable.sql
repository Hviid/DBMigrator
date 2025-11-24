-- Create a sample table
CREATE TABLE [dbo].[Users] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Username] NVARCHAR(100) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

PRINT 'Users table created successfully';
