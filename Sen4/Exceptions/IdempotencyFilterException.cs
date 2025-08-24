namespace Sen4.Exceptions;

public class IdempotencyFilterException: Exception
{
    public  IdempotencyFilterException(string message): base(message){}
}