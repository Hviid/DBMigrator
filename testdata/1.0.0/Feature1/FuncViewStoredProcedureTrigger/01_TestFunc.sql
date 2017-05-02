CREATE FUNCTION TestFunc (@LastName varchar(255)) 
RETURNS varchar(255)   
AS BEGIN 
	RETURN @LastName + (SELECT TOP(1) LastName FROM Tester)
END