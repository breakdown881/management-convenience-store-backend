namespace ConvenienceStore.Domain.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key) : base($"Entity '{entityName}' with identifier ({key}) was not found.")
    {
    }
}
