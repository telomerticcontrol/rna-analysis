using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

namespace BioBrowser.Models
{
    /// <summary>
    /// This wraps the MEF composition container and provides access to it to all parties.
    /// </summary>
    class MefManager
    {
        /// <summary>
        /// MEF composition container
        /// </summary>
        private readonly CompositionContainer _mefContainer;

        /// <summary>
        /// Singleton access
        /// </summary>
        readonly static Lazy<MefManager> _instance = new Lazy<MefManager>(() => new MefManager(), true);
        public static MefManager Current
        {
            get { return _instance.Value; }            
        }

        private MefManager()
        {
            // An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            // Add all the parts found in our assembly.
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(MefManager).Assembly));

            // Add all the parts in our app directory
            string directory = Path.GetDirectoryName(Assembly.GetEntryAssembly().CodeBase.Substring(8));
            catalog.Catalogs.Add(new DirectoryCatalog(directory));

            // Create the CompositionContainer with the parts in the catalog
            _mefContainer = new CompositionContainer(catalog);
        }

        public void ComposeParts(params object[] containers)
        {
            // Fill the imports of this object
            _mefContainer.ComposeParts(containers);
        }
    }
}
