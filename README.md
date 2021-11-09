# APSIM.Registration

The apsim registration website and REST api

## Initialisation

- The database needs to be created in advance:

```
CREATE DATABASE APSIM_Registrations;
```

- The user specified in the connection string needs all privileges on the database specified in the connection string:

```
GRANT ALL PRIVILEGES ON APSIM_Registrations.* TO 'rego'@'localhost';
```

## Deploy Changes to the website

Run the deploy script at the root of the repository.
