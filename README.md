# TransactionAPI - Transaction Management API

TransactionAPI is a API for handling transactions. It allows users to import transactions from an Excel file, update transaction status, and export a filtered list of transactions to a CSV file.

## Functionalities

1. User authentication is required to access the service. The type and sequence of actions are at the discretion of the executor.

2. All methods are protected from unauthorized users. Only authenticated users can access them.

3. Upon uploading an Excel file, the .NET backend processes the content and adds the data to the database based on the transaction_id found in the Excel file. If a record with the same transaction_id already exists in the database, the transaction status is updated; otherwise, a new record is added.

4. When requesting an export to CSV, the API will provide a file with essential transaction information (columns chosen by the executor) based on selected filters (transaction type, status).

5. Users can filter transactions by type (possibly multiple types simultaneously), status (one status), and search for a client by their name.

6. Users have the ability to update the status of a transaction based on its ID.

## API Request

| Method | URL                                      | Description                                                     |
|-------|------------------------------------------|------------------------------------------------------------------|
| POST  | /api/account/login                       | User login and JWT token retrieval.                              |
| POST  | /api/account/registration                | User registration.                                              |
| POST  | /api/account/refresh                     | Refresh JWT token using refresh token.                          |
| POST  | /api/file/upload                         | Upload an Excel file for adding transactions to the database.    |
| GET   | /api/file/export                         | Export transactions in CSV format.                              |
| GET   | /api/transaction/alltransactions        | Get a list of transactions with filtering options.             |
| GET   | /api/transaction/updatetransactionstatus/{id} | Update transaction status by its identifier.            |

## Middlewares

### CachingMiddleware

The caching middleware represented by the `CachingMiddleware` class plays an important role in optimizing the performance of our server. This middleware allows you to save and use cached versions of HTTP responses for repeated requests, which improves performance and reduces the load on the server.

### RequestLoggingMiddleware

Request Logging middleware implemented as `RequestLoggingMiddleware` class. It records important information about each request and the responses to them. This middleware serves as an important tool for monitoring, debugging and optimizing our server operations.

## Technologies Used
The TransactionAPI project is built using the following technologies:

- ASP.NET Core: A cross-platform, high-performance framework for building modern web applications using C#.
- ADO.NET: A comprehensive data access technology in the .NET framework that includes classes like SqlConnection, SqlCommand, and SqlDataReader to manage SQL Server database interactions securely and efficiently.
- Entity Framework Core: An Object-Relational Mapping (ORM) tool that simplifies database interactions and provides data access to the application.
- SQL Server: The database management system used to store transaction data.
- Swagger: A tool for generating interactive API documentation, allowing users to explore and test the API endpoints.
- JWT (JSON Web Tokens): A secure method for token-based authentication, used to protect API endpoints from unauthorized access.
- NUnit: A popular unit testing framework for .NET applications, used to write and execute unit tests to ensure the correctness of the application's code and functionality.

## License

TransactionAPI is released under the [MIT License](LICENSE).
