CREATE TABLE IF NOT EXISTS "Users" (
    "Id" SERIAL PRIMARY KEY,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "Email" VARCHAR(255) NOT NULL,
    "IsActive" BOOLEAN NOT NULL,
    "CreatedOn" VARCHAR(50),
    "ModifiedOn" VARCHAR(50)
);