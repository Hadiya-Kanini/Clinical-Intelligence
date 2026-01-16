---
trigger: glob
globs: "**/*.sql"
---

# SQL Development

## Database Schema Generation
- Table names in singular form
- Column names in singular form
- All tables have primary key column named `id`
- All tables have `created_at` for creation timestamp
- All tables have `updated_at` for last update timestamp

## Database Schema Design
- All tables have primary key constraint
- All foreign key constraints have a name
- Foreign key constraints defined inline
- Foreign key constraints have `ON DELETE CASCADE` option
- Foreign key constraints have `ON UPDATE CASCADE` option
- Foreign key constraints reference primary key of parent table

## SQL Coding Style
- Uppercase for SQL keywords (SELECT, FROM, WHERE)
- Consistent indentation for nested queries and conditions
- Comments to explain complex logic
- Break long queries into multiple lines for readability
- Organize clauses consistently (SELECT, FROM, JOIN, WHERE, GROUP BY, HAVING, ORDER BY)

## SQL Query Structure
- Explicit column names in SELECT (instead of SELECT *)
- Qualify column names with table name/alias when using multiple tables
- Limit subqueries when joins can be used instead
- Include LIMIT/TOP clauses to restrict result sets
- Use appropriate indexing for frequently queried columns
- Avoid functions on indexed columns in WHERE clauses

## Stored Procedure Naming
- Prefix with 'usp_'
- Use PascalCase
- Descriptive names indicating purpose (e.g., usp_GetCustomerOrders)
- Plural noun for multiple records (e.g., usp_GetProducts)
- Singular noun for single record (e.g., usp_GetProduct)

## Parameter Handling
- Prefix parameters with '@'
- Use camelCase for parameter names
- Provide default values for optional parameters
- Validate parameter values before use
- Document parameters with comments
- Arrange parameters: required first, optional later

## Stored Procedure Structure
- Include header comment block: description, parameters, return values
- Return standardized error codes/messages
- Return result sets with consistent column order
- Use OUTPUT parameters for returning status information
- Prefix temporary tables with 'tmp_'

## SQL Security
- Parameterize all queries to prevent SQL injection
- Use prepared statements when executing dynamic SQL
- Avoid embedding credentials in SQL scripts
- Proper error handling without exposing system details
- Avoid dynamic SQL within stored procedures

## Transaction Management
- Explicitly begin and commit transactions
- Use appropriate isolation levels based on requirements
- Avoid long-running transactions that lock tables
- Use batch processing for large data operations
- Include SET NOCOUNT ON for stored procedures that modify data
