CREATE DATABASE Reservations;

\c Reservations

-- Create the RequestResults table
CREATE TABLE IF NOT EXISTS RequestResults (
    Id SERIAL PRIMARY KEY,
	Raw TEXT,
	Dt TIMESTAMP,
	ValidationResult INT -- 0:Fail, -9:OK
);

CREATE OR REPLACE PROCEDURE sp_InsertRequestResult(   
    IN Raw TEXT,
    Dt TIMESTAMP,
    ValidationResult INT,
    OUT Result SMALLINT,
    OUT ResultText TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Attempt to insert the data
    BEGIN
        INSERT INTO RequestResults (Raw,Dt,ValidationResult)
        VALUES (Raw,Dt,ValidationResult);

        -- Set output parameters for success
        Result := 1;
        ResultText := 'Record inserted successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            -- Set output parameters for error
            Result := 0;
            ResultText := SQLERRM;
    END;
END;
$$;