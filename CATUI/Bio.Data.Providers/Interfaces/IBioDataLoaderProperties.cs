using System;

namespace Bio.Data.Providers.Interfaces
{
    /// <summary>
    /// This interface is used to alter data loader properties
    /// </summary>
    public interface IBioDataLoaderProperties
    {
        /// <summary>
        /// This method is used to change the properties of this loader.
        /// It is assumed that a GUI will be provided by the implementation.
        /// </summary>
        /// <returns>True if properties were changed</returns>
        bool ChangeProperties();

        /// <summary>
        /// This event should be raised by the loader when it changes properties
        /// that affect the sequence of data.
        /// </summary>
        event EventHandler PropertiesChanged;
    }
}