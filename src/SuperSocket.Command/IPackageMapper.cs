using System;
using System.Threading.Tasks;

namespace SuperSocket.Command
{
    /// <summary>
    /// Defines a method for mapping one package type to another.
    /// </summary>
    /// <typeparam name="PackageFrom">The type of the source package.</typeparam>
    /// <typeparam name="PackageTo">The type of the target package.</typeparam>
    public interface IPackageMapper<PackageFrom, PackageTo>
    {
        /// <summary>
        /// Maps a package of type <typeparamref name="PackageFrom"/> to a package of type <typeparamref name="PackageTo"/>.
        /// </summary>
        /// <param name="package">The source package to map.</param>
        /// <returns>The mapped package of type <typeparamref name="PackageTo"/>.</returns>
        PackageTo Map(PackageFrom package);
    }
}
