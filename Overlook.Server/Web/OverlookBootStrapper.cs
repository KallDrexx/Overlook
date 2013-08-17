using Nancy;
using Nancy.TinyIoc;
using Overlook.Server.Storage;

namespace Overlook.Server.Web
{
    public class OverlookBootStrapper : DefaultNancyBootstrapper
    {
        private readonly IStorageEngine _storageEngine;

        public OverlookBootStrapper(IStorageEngine storageEngine)
        {
            _storageEngine = storageEngine;
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            container.Register(_storageEngine);
        }
    }
}
