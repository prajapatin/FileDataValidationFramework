# FileDataValidationFramework
CSV File data validation

The solution parses multiple csv files, each containing rows of data (each of a particular type T) into a .NET collection. It performs three kind of validations of data, whichever files pass the validation have to be persisted in a database. 

Types of validations:
1. File validations - covers standard issues (File doesn't exist, File is empty, File has only header etc.)
2. Format validations - which shall check for issues like Incorrect datatype, Null or Empty value etc.
3. Business validations - Cross-file references would be checked here or any business logic validation

Note: You can use files from 'Tested Files' folder for testing the solution
