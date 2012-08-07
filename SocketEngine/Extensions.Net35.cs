using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;

namespace SuperSocket.SocketEngine
{
    /// <summary>
    /// Extensions for .Net 3.5
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Creates a new instance of the specified type defined in the specified assembly file.
        /// </summary>
        /// <param name="appDomain">The app domain.</param>
        /// <param name="assemblyFile">The assembly file.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <param name="bindingAttr">The binding attr.</param>
        /// <param name="binder">The binder.</param>
        /// <param name="args">The args.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="activationAttributes">The activation attributes.</param>
        /// <returns></returns>
        public static object CreateInstanceAndUnwrap(this AppDomain appDomain, string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            return appDomain.CreateInstanceAndUnwrap(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, AppDomain.CurrentDomain.Evidence);
        }

        /// <summary>
        /// Creates the instance from.
        /// </summary>
        /// <param name="appDomain">The app domain.</param>
        /// <param name="assemblyFile">The assembly file.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="ignoreCase">if set to <c>true</c> [ignore case].</param>
        /// <param name="bindingAttr">The binding attr.</param>
        /// <param name="binder">The binder.</param>
        /// <param name="args">The args.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="activationAttributes">The activation attributes.</param>
        /// <returns></returns>
        public static object CreateInstanceFrom(this AppDomain appDomain, string assemblyFile, string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes)
        {
            return appDomain.CreateInstanceFrom(assemblyFile, typeName, ignoreCase, bindingAttr, binder, args, culture, activationAttributes, AppDomain.CurrentDomain.Evidence);
        }
    }
}
