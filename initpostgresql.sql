CREATE DATABASE Reservations;

\c Reservations

-- Create the RequestResults table
CREATE TABLE IF NOT EXISTS FailMessages (
    Id SERIAL PRIMARY KEY,
	Raw TEXT,
	Dt TIMESTAMP,
	ValidationResult INT -- 0:Fail, -9:OK
);

CREATE OR REPLACE PROCEDURE sp_insertfailmessage(   
    in raw TEXT,
    dt TIMESTAMP,
    validationresult INT,
    OUT result SMALLINT,
    OUT resultText TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    -- Attempt to insert the data
    BEGIN
        INSERT INTO FailMessages (Raw,Dt,ValidationResult)
        VALUES (raw,dt,validationresult);

        -- Set output parameters for success
        result := 1;
        resulttext := 'Record inserted successfully.';
    EXCEPTION
        WHEN OTHERS THEN
            -- Set output parameters for error
            result := 0;
            resulttext := SQLERRM;
    END;
END;
$$;