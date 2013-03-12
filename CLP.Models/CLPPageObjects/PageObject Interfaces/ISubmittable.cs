
namespace CLP.Models
{
    public interface ISubmittable
    {
        void BeforeSubmit(bool isGroupSubmit, CLPNotebook notebook);
        void AfterSubmit(bool isGroupSubmit, CLPNotebook notebook);
    }
}
