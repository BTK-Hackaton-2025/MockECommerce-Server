using System.Runtime.Serialization;

namespace MockECommerce.BusinessLayer.Exceptions;

[Serializable]
public class BusinessException : Exception
{
    public string? ErrorCode { get; }

    public BusinessException() { }

    public BusinessException(string message)
        : base(message) { }

    public BusinessException(string message, string errorCode)
        : base(message) => ErrorCode = errorCode;

    public BusinessException(string message, Exception innerException)
        : base(message, innerException) { }

    // Deserialization ctor (gerektiğinde)
    protected BusinessException(SerializationInfo info, StreamingContext ctx)
        : base(info, ctx)
    {
        ErrorCode = info.GetString(nameof(ErrorCode));
    }

    // Eğer ErrorCode ekliyorsan serialize et
    public override void GetObjectData(SerializationInfo info, StreamingContext ctx)
    {
        base.GetObjectData(info, ctx);
        info.AddValue(nameof(ErrorCode), ErrorCode);
    }
}