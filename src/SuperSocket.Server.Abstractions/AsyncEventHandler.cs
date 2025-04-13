using System;
using System.Threading.Tasks;

namespace SuperSocket.Server.Abstractions
{
    /// <summary>
    /// Represents an asynchronous event handler.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public delegate ValueTask AsyncEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Represents an asynchronous event handler with a specific event argument type.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of the event data.</typeparam>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public delegate ValueTask AsyncEventHandler<TEventArgs>(object sender, TEventArgs e)
        where TEventArgs : EventArgs;

    /// <summary>
    /// Represents an asynchronous event handler with specific sender and event argument types.
    /// </summary>
    /// <typeparam name="TSender">The type of the event sender.</typeparam>
    /// <typeparam name="TEventArgs">The type of the event data.</typeparam>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public delegate ValueTask AsyncEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e)
        where TSender : class
        where TEventArgs : EventArgs;
}