using System.Runtime.Serialization;

namespace MockECommerce.BusinessLayer.Exceptions;

[Serializable]
public class NotFoundException : BusinessException
{
    public NotFoundException()
        : base("Kaynak bulunamadı.") { }

    public NotFoundException(string message)
        : base(message) { }

    public NotFoundException(string message, string errorCode)
        : base(message, errorCode) { }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }

    protected NotFoundException(SerializationInfo info, StreamingContext ctx)
        : base(info, ctx) { }
}