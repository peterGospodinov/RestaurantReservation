IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Reservations')
BEGIN
    CREATE DATABASE Reservations;
END;
GO

USE Reservations;

-- Create the RequestResults table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RequestResults')
BEGIN
   CREATE TABLE RequestResults (
        Id INT IDENTITY(1,1) PRIMARY KEY,    
        Raw VARCHAR(MAX),
        DT DATETIME2,
        ValidationResult  INT -- 0:Fail -9:OK       
    );
END;
GO

USE Reservations;
-- Create the insertion stored procedure
IF OBJECT_ID('sp_InsertRequestResult', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE sp_InsertRequestResult;
END;
GO

CREATE PROCEDURE sp_InsertRequestResult
    @Raw VARCHAR(MAX),
    @DT DATETIME2,
    @ValidationResult INT,
    @Result TINYINT OUTPUT,
    @ResultText NVARCHAR(MAX) OUTPUT
AS
BEGIN
    BEGIN TRY
        -- Insert the record into RequestResults table
        INSERT INTO RequestResults (Raw, DT, ValidationResult)
        VALUES (@Raw, @DT, @ValidationResult);

        -- Set output parameters for success
        SET @Result = 1;
        SET @ResultText = N'Record inserted successfully.';
    END TRY
    BEGIN CATCH
        -- Set output parameters for error
        SET @Result = 0;
        SET @ResultText = ERROR_MESSAGE();
    END CATCH
END;
GO
