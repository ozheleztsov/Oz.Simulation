namespace Oz.SimulationLib.Contracts;

public interface ISimObject : ISimEntity
{
    /// <summary>
    ///     Collection of object properties
    /// </summary>
    IEnumerable<KeyValuePair<string, object>> Properties { get; }

    /// <summary>
    ///     Set object property. Replace property if such property already exists
    /// </summary>
    /// <param name="key">Property key</param>
    /// <param name="value">Property value</param>
    void SetProperty(string key, object? value);

    /// <summary>
    ///     Returns property by key
    /// </summary>
    /// <param name="key">Property key</param>
    /// <typeparam name="T">Type of property</typeparam>
    /// <returns>Returns property by key or null if there is no such property</returns>
    T? GetProperty<T>(string key);

    /// <summary>
    ///     Create and add component to the object
    /// </summary>
    /// <param name="name">Optional component name. If there is no name default name is used</param>
    /// <typeparam name="T">Type of new component</typeparam>
    /// <returns>New component</returns>
    Task<T> AddComponentAsync<T>(string? name = null) where T : ISimComponent;

    /// <summary>
    ///     Add component instance to the object. If that instance already added to some other object or to this object then
    ///     throws exception
    /// </summary>
    /// <param name="instance">Component instance to be added to the object</param>
    /// <typeparam name="T">Type of the component</typeparam>
    Task AddComponentAsync<T>(T instance) where T : ISimComponent;

    /// <summary>
    ///     Get all components on the object
    /// </summary>
    /// <returns>Collection of components</returns>
    IEnumerable<ISimComponent> GetComponents();

    /// <summary>
    ///     Get components of specific type attached to the object
    /// </summary>
    /// <typeparam name="T">Type of components</typeparam>
    /// <returns>Collection of components</returns>
    IEnumerable<T> GetComponents<T>() where T : ISimComponent;

    /// <summary>
    ///     Get any component of the specified type
    /// </summary>
    /// <typeparam name="T">Component type</typeparam>
    /// <returns>Component on the object or null</returns>
    T? GetComponent<T>() where T : ISimComponent;

    /// <summary>
    ///     Get component by id. If such component is not found returns null
    /// </summary>
    /// <param name="id">Id of component</param>
    /// <returns>Component with specified id</returns>
    ISimComponent? GetComponent(Guid id);

    /// <summary>
    ///     Get all components with specified name. Components can have duplicate names.
    /// </summary>
    /// <param name="name">Name of component</param>
    /// <returns>Collection of components with specified name</returns>
    IEnumerable<ISimComponent> GetComponents(string name);

    /// <summary>
    ///     Get components of specified type that conform to predicate
    /// </summary>
    /// <param name="predicate">Predicate that is checked for component</param>
    /// <typeparam name="T">Type of component</typeparam>
    /// <returns>Collection of components</returns>
    IEnumerable<T> GetComponents<T>(Func<T, bool> predicate);

    /// <summary>
    ///     Get components of any type that conforms to specified predicate
    /// </summary>
    /// <param name="predicate">Predicate that is evaluated for component</param>
    /// <returns>Collection of components</returns>
    IEnumerable<ISimComponent> GetComponents(Func<ISimComponent, bool> predicate);

    /// <summary>
    ///     Remove component by id
    /// </summary>
    /// <param name="id">Id of component</param>
    /// <returns>True - if component was removed successfully</returns>
    Task<bool> RemoveComponentAsync(Guid id);

    /// <summary>
    ///     Remove all components of specified type
    /// </summary>
    /// <typeparam name="T">Type of components</typeparam>
    Task RemoveComponentsAsync<T>() where T : ISimComponent;

    Task TryInitializeAsync();
}