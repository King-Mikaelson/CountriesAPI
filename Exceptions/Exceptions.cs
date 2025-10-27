namespace StringAnalyzer.Exceptions;

// 400 Bad Request
public class InvalidStringException : Exception
{
    public InvalidStringException(string message) : base(message) { }
}

// 409 Conflict
public class StringAlreadyExistsException : Exception
{
    public StringAlreadyExistsException(string message) : base(message) { }
}

// 422 Unprocessable Entity
public class InvalidStringTypeException : Exception
{
    public InvalidStringTypeException(string message) : base(message) { }
}

// 404 Not Found
public class StringNotFoundException : Exception
{
    public StringNotFoundException(string message) : base(message) { }
}
