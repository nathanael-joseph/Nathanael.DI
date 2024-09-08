namespace Nathanael.DI;

/// <summary>
/// This enumeration represents different lifetimes for objects managed by a dependency injection (DI) container. 
/// </summary>
public enum Lifetime
{ 
    /// <summary>
    /// Indicates that only one instance of the service should be created and shared throughout the application.
    /// </summary>
    Singleton,
    /// <summary>
    /// Indicates that a single instance of the object is created once per request or scope (typically used in web applications where each HTTP request can be considered as a scope).
    /// </summary>
    Scoped,
    /// <summary>
    /// Indicates that a new instance of the object should be created every time it is requested.
    /// </summary>
    Transient
}
