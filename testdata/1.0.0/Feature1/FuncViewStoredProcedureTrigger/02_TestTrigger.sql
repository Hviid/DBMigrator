CREATE TRIGGER reminder1  
ON dbo.Tester  
AFTER INSERT
AS
	UPDATE dbo.Tester SET Tester.Lastname = inserted.Lastname + 'trigger' FROM dbo.Tester INNER JOIN inserted ON inserted.PersonID = Tester.PersonID
GO 