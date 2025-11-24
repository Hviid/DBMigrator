-- Add index on Email column
CREATE NONCLUSTERED INDEX [IX_Users_Email] 
ON [dbo].[Users] ([Email]);

PRINT 'Email index created successfully';
