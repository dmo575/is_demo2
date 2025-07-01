## Backend API

### Overview
Backend API that exposes some RESTful endpoints we can interact with in order to communicate with an Azure database.

### Endpoints
| Method | Route       | Req. Body   | Res. Body                |
|--------|-------------|-------------|--------------------------|
|GET     | /Users      | N/A         | JSON - List of all users |
|GET     | /Users/{id} | N/A         | JSON - The fetched user  |
|POST    | /Users      | JSON - User | JSON - The created user  |
|PUT     | /Users/{id} | JSON - User | JSON - The updated user  |
|PATCH   | /Users/{id} | JSON - User | JSON - The updated user  |
|DELETE  | /Users/{id} | N/A         | N/A                      |

### JSON's rules
- For a POST:
    - The JSON must contain all the REQUIRED columns: FirstName, LastName, Email
    - All other columns can be ignored (gets assigned a NULL if so), set to a proper value, or set specifically to NULL
- For a PUT:
    - Not allowed to set a REQUIRED column to NULL
    - Skipping any non required column sets it to NULL in the database
    - Setting any non required column to NULL sets it to NULL in the database
- For a PATCH:
    - Not allowed to set any REQUIRED column to NULL
    - Skipping any non required column leaves it UNALTERED in the database
    - Setting any non required column to NULL sets it to NULL in the database

The main difference between PUT and PATCH is that since PUT is meant to perform a FULL update, any omissions are interpreted as "wipe out this column's value", while with PATCH any omissions are interpreted as "skip altering this column's value".

We don't allow nullifying any REQUIRED column's value as it is considered integral for the record and the database would return an error anyways.

### DTOs
- CreateUserDTO:
    - Includes all properties we can manipulate when creating a new user
    - Defines which properties are REQUIRED
    - Used with POST /Users
- UpdateUserDTO:
    - Includes all properties we can modify when updating a user
    - PUT and PATCH behave differently, per JSON's rules section
    - used with PUT and PATCH on /Users
- ReponseUserDTO:
    - Declares all properties that we as the client are allowed to see from a user record
    - Is the entity that the server always returns back as the user's representation
    - Lets us omit any sensitive properties we might not want to include from the User model (Example: password hash)

### `Optional<T>`
Generic class that allows us to better write JSON in the request. Thanks to this we can set things to null, skip properties and easily implement PUT and PATCH.

A JsonConverter takes care of serializing and deserializing the JSON to and from an `Optional<T>` value