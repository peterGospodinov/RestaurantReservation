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
        ClientName NVARCHAR(255),
        ClientPhone NVARCHAR(50),
        TableNumber SMALLINT,
        DateOfReservation DATE,
        Status TINYINT CHECK (Status IN (0, 1)), -- 0 not valid, 1 for valid
        Raw NVARCHAR(MAX)
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
    @clientName NVARCHAR(255),
    @clientPhone NVARCHAR(50),
    @TableNumber SMALLINT,
    @DateOfReservation DATE,
    @Status TINYINT,
    @Raw NVARCHAR(MAX),
    @Result TINYINT OUTPUT,
    @ResultText NVARCHAR(MAX) OUTPUT
AS
BEGIN
    BEGIN TRY
        -- Insert the record into RequestResults table
        INSERT INTO RequestResults (clientName, clientPhone, TableNumber, DateOfReservation, Status, Raw)
        VALUES (@clientName, @clientPhone, @TableNumber, @DateOfReservation, @Status, @Raw);

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
