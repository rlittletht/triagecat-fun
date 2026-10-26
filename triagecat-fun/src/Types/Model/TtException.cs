using TCore.Exceptions;

namespace tt.Types.Model;

// Basic exception for our WebApi to allow us to differentiate exceptions
public class TtException : TcException
{
    public TtException() : base(Guid.Empty) { }
    public TtException(Guid crids) : base(crids) { }
    public TtException(string errorMessage) : base(errorMessage) { }
    public TtException(string errorMessage, Exception innerException) : base(errorMessage, innerException) { }
    public TtException(Guid crids, string errorMessage) : base(crids, errorMessage) { }
    public TtException(Guid crids, Exception innerException, string errorMessage) : base(crids, innerException, errorMessage) { }
}

public class TtExceptionAuthenticationFailure : TtException
{
    public TtExceptionAuthenticationFailure() : base(Guid.Empty) { }
    public TtExceptionAuthenticationFailure(Guid crids) : base(crids) { }
    public TtExceptionAuthenticationFailure(string errorMessage) : base(errorMessage) { }
    public TtExceptionAuthenticationFailure(string errorMessage, Exception innerException) : base(errorMessage, innerException) { }
    public TtExceptionAuthenticationFailure(Guid crids, string errorMessage) : base(crids, errorMessage) { }
    public TtExceptionAuthenticationFailure(Guid crids, Exception innerException, string errorMessage) : base(crids, innerException, errorMessage) { }
}

public class TtExceptionUnauthorized : TtException
{
    public TtExceptionUnauthorized() : base(Guid.Empty) { }
    public TtExceptionUnauthorized(Guid crids) : base(crids) { }
    public TtExceptionUnauthorized(string errorMessage) : base(errorMessage) { }
    public TtExceptionUnauthorized(string errorMessage, Exception innerException) : base(errorMessage, innerException) { }
    public TtExceptionUnauthorized(Guid crids, string errorMessage) : base(crids, errorMessage) { }
    public TtExceptionUnauthorized(Guid crids, Exception innerException, string errorMessage) : base(crids, innerException, errorMessage) { }
}

public class TtExceptionInvalidArgument : TtException
{
    public TtExceptionInvalidArgument() : base(Guid.Empty) { }
    public TtExceptionInvalidArgument(Guid crids) : base(crids) { }
    public TtExceptionInvalidArgument(string errorMessage) : base(errorMessage) { }
    public TtExceptionInvalidArgument(string errorMessage, Exception innerException) : base(errorMessage, innerException) { }
    public TtExceptionInvalidArgument(Guid crids, string errorMessage) : base(crids, errorMessage) { }
    public TtExceptionInvalidArgument(Guid crids, Exception innerException, string errorMessage) : base(crids, innerException, errorMessage) { }
}

public class TtExceptionDataValidationError : TtException
{
    public TtExceptionDataValidationError() : base(Guid.Empty) { }
    public TtExceptionDataValidationError(Guid crids) : base(crids) { }
    public TtExceptionDataValidationError(string errorMessage) : base(errorMessage) { }
    public TtExceptionDataValidationError(string errorMessage, Exception innerException) : base(errorMessage, innerException) { }
    public TtExceptionDataValidationError(Guid crids, string errorMessage) : base(crids, errorMessage) { }
    public TtExceptionDataValidationError(Guid crids, Exception innerException, string errorMessage) : base(crids, innerException, errorMessage) { }
}
public class TtExceptionNotInitializedError : TtException
{
    public TtExceptionNotInitializedError() : base(Guid.Empty) { }
    public TtExceptionNotInitializedError(Guid crids) : base(crids) { }
    public TtExceptionNotInitializedError(string errorMessage) : base(errorMessage) { }
    public TtExceptionNotInitializedError(string errorMessage, Exception innerException) : base(errorMessage, innerException) { }
    public TtExceptionNotInitializedError(Guid crids, string errorMessage) : base(crids, errorMessage) { }
    public TtExceptionNotInitializedError(Guid crids, Exception innerException, string errorMessage) : base(crids, innerException, errorMessage) { }
}