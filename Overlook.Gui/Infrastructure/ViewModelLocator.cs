using Overlook.Gui.ViewModels;
using TinyIoC;

namespace Overlook.Gui.Infrastructure
{
    public class ViewModelLocator
    {
        private readonly TinyIoCContainer _container;

        public ViewModelLocator()
        {
            _container = new TinyIoCContainer();
        }

        // Available View Models
        public WorkAreaViewModel WorkAreaViewModel { get { return _container.Resolve<WorkAreaViewModel>(); } }
    }
}
