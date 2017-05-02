CREATE PROCEDURE dbo.TestProc   
    @LastName nvarchar(50)
AS   

    SET NOCOUNT ON;  
    INSERT INTO dbo.Tester (PersonID, Lastname) VALUES (1, @LastName) 
	SELECT * FROM dbo.Tester
GO