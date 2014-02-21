using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLP.Models.Services
{
    public interface INotebookSerializationService
    {
        CLPNotebook LoadNotebook();
        void SaveNotebook(CLPNotebook notebook);
    }
}
