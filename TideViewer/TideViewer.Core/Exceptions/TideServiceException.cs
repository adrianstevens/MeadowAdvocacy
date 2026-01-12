using System;

namespace TideViewer.Exceptions;

/// <summary>
/// Exception thrown when tide service operations fail
/// </summary>
public class TideServiceException : Exception
{
    /// <summary>
    /// Name of the service that threw the exception
    /// </summary>
    public string? ServiceName { get; }

    /// <summary>
    /// API endpoint that was called, if applicable
    /// </summary>
    public string? Endpoint { get; }

    /// <summary>
    /// Creates a new TideServiceException
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="serviceName">Name of the service</param>
    public TideServiceException(string message, string? serviceName = null)
        : base(message)
    {
        ServiceName = serviceName;
    }

    /// <summary>
    /// Creates a new TideServiceException with an inner exception
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="innerException">Inner exception</param>
    /// <param name="serviceName">Name of the service</param>
    public TideServiceException(string message, Exception innerException, string? serviceName = null)
        : base(message, innerException)
    {
        ServiceName = serviceName;
    }

    /// <summary>
    /// Creates a new TideServiceException with endpoint information
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="serviceName">Name of the service</param>
    /// <param name="endpoint">API endpoint</param>
    public TideServiceException(string message, string serviceName, string endpoint)
        : base(message)
    {
        ServiceName = serviceName;
        Endpoint = endpoint;
    }
}
